using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.State;
using Arlecchino.Widgets;
using Arlecchino.Modals;
using Arlecchino.Input;

namespace Arlecchino.Rendering;

/// <summary>
/// Draws the frames: the current view first, then the output line, the hints and any dialog on top.
/// A view that throws while drawing is reported on the output line instead of taking the application
/// down, since a half-drawn frame is easier to recover from than a dead process.
/// </summary>
public class Screen
{
    private const int SliderTrackCells = 24;
    private const int SwatchRows = 2;
    private const int StackedRows = 1;
    private const int StackedColumns = 3;
    private const int BoxChromeColumns = 8;
    private const int SmallestFieldColumns = 12;
    private const string ScrollMarker = "…";

    private readonly ArlecchinoState _state;
    private readonly Surface _surface;
    private readonly Navigator _navigator;
    private readonly ArlecchinoOptions _options;
    private readonly ArlecchinoStrings _strings;
    private readonly ILogger<Screen> _logger;
    private readonly IArlecchinoTerminal _terminal;
    private readonly Repaint _repaint;
    private readonly UiDispatcher _dispatcher;
    private readonly Ticker _ticker;
    private readonly LogOverlay _log;

    private int _lastWidth;
    private int _lastHeight;
    private int _stackDepth;
    private int _forgetFrame;

    /// <summary>Creates the screen.</summary>
    /// <param name="state">Supplies the output line and the dialog to draw.</param>
    /// <param name="surface">The cell grid frames are built in.</param>
    /// <param name="navigator">Draws the current view.</param>
    /// <param name="options">Settings gathered at startup.</param>
    /// <param name="terminal">Watched for a change of size.</param>
    /// <param name="repaint">Says when a frame is actually needed.</param>
    /// <param name="dispatcher">Runs work posted from background threads, just before drawing.</param>
    /// <param name="ticker">Runs scheduled work between frames.</param>
    /// <param name="log">Drawn over the view while it is open.</param>
    /// <param name="logger">Where drawing failures are reported.</param>
    internal Screen(
        ArlecchinoState state,
        Surface surface,
        Navigator navigator,
        ArlecchinoOptions options,
        IArlecchinoTerminal terminal,
        Repaint repaint,
        UiDispatcher dispatcher,
        Ticker ticker,
        LogOverlay log,
        ILogger<Screen> logger)
    {
        _log = log;
        _state = state;
        _surface = surface;
        _navigator = navigator;
        _options = options;
        _strings = options.Strings;
        _terminal = terminal;
        _repaint = repaint;
        _dispatcher = dispatcher;
        _ticker = ticker;
        _logger = logger;
    }

    private readonly record struct Span(string Text, IArlecchinoColor Style);

    /// <summary>
    /// Draws one full frame, forgetting what was on screen first. Redrawing everything is what makes
    /// this usable outside the loop — in tests, or after something else has written to the terminal.
    /// </summary>
    public void DrawOnce()
    {
        _surface.ForgetPreviousFrame();
        DrawFrame();
    }

    /// <summary>
    /// Asks for the next frame to be drawn from scratch rather than as a difference. Safe from any
    /// thread, and needed whenever something outside the framework has written over the screen —
    /// coming back from a suspended process, for one.
    /// </summary>
    public void RedrawEverything()
    {
        Interlocked.Exchange(ref _forgetFrame, 1);
        _repaint.Request();
    }

    /// <summary>
    /// Draws until stopped, at the configured rate. A frame is only built when something asked for one
    /// or the terminal changed size, so an idle application costs nothing.
    /// </summary>
    /// <param name="stoppingToken">Cancelled when the application is shutting down.</param>
    /// <returns>A task that completes once drawing has stopped.</returns>
    public async Task Run(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1d / _options.TargetFramesPerSecond));

        while (!stoppingToken.IsCancellationRequested)
        {
            _ticker.Run(TickFailed);

            if (_repaint.TakeRequested() || TerminalWasResized())
            {
                DrawFrame();
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }

    private void TickFailed(Exception exception)
    {
        _logger.LogError(exception, "Scheduled work failed.");
        _state.Output = _strings.ViewFailed(exception.Message);
    }

    private void RunFailed(Exception exception)
    {
        _logger.LogError(exception, "A posted action failed before the frame was drawn.");
        _state.Output = _strings.ViewFailed(exception.Message);
    }

    private bool TerminalWasResized()
    {
        var width = _terminal.Width;
        var height = _terminal.Height;

        if (width == _lastWidth && height == _lastHeight)
        {
            return false;
        }

        _lastWidth = width;
        _lastHeight = height;
        return true;
    }

    private void DrawFrame()
    {
        _dispatcher.RunPending(RunFailed);

        if (Interlocked.Exchange(ref _forgetFrame, 0) == 1)
        {
            _surface.ForgetPreviousFrame();
        }

        _surface.StartFrame();

        if (_surface.FrameWidth < _options.MinimumWidth || _surface.FrameHeight < _options.MinimumHeight)
        {
            DrawSizeNotice();
            _surface.Build();
            return;
        }

        try
        {
            _navigator.Draw();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "The view at route {Route} failed to draw.", _navigator.CurrentRoute);
            _state.Output = _strings.ViewFailed(exception.Message);
        }

        if (_options.ShowOutputLine)
        {
            DrawOutput();
        }

        if (_options.ShowHints)
        {
            DrawHints();
        }

        if (_log.IsVisible)
        {
            DrawLogOverlay();
        }

        DrawModals();

        _surface.Build();
    }

    private void DrawOutput()
    {
        var outputStyle = string.IsNullOrEmpty(_state.Output) ? Theme.Default : Theme.Warning;

        _surface.FillLineAt(_surface.FrameHeight - 2);
        _surface.WriteLineAt(_surface.FrameHeight - 1, _state.Output, outputStyle);
    }

    private void DrawLogOverlay()
    {
        var entries = _log.Buffer.Snapshot();
        var height = Math.Clamp(_surface.FrameHeight / 2, 4, Math.Max(4, _surface.FrameHeight - 2));
        var box = _surface.Frame.Rows(_surface.FrameHeight - height, height);
        var inside = box.Border(Theme.Info, _strings.LogTitle(entries.Count)).Inset(new Margin(1, 0, 1, 0));
        var rows = Math.Max(0, inside.Height - 1);

        if (entries.Count == 0)
        {
            inside.WriteLine(0, _strings.LogEmpty(), Theme.Muted);
        }

        _log.Scroll = Math.Min(_log.Scroll, Math.Max(0, entries.Count - rows));

        var last = entries.Count - _log.Scroll;
        var first = Math.Max(0, last - rows);

        for (var row = 0; first + row < last; row++)
        {
            var entry = entries[first + row];
            var line = $"{entry.Time:HH:mm:ss} {LevelName(entry.Level)} {entry.Category}: {entry.Message}";

            inside.Write(row, 0, TextWidth.PadRight(TextWidth.Truncate(line, inside.Width), inside.Width),
                LevelStyle(entry.Level));
        }

        inside.WriteLine(inside.Height - 1, TextWidth.Truncate(_strings.LogHints(), inside.Width), Theme.Muted);
    }

    private static string LevelName(LogLevel level) => level switch
    {
        LogLevel.Trace => "trce",
        LogLevel.Debug => "dbug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "fail",
        LogLevel.Critical => "crit",
        _ => "none",
    };

    private static IArlecchinoColor LevelStyle(LogLevel level) => level switch
    {
        LogLevel.Warning => Theme.Warning,
        LogLevel.Error or LogLevel.Critical => Theme.Error,
        LogLevel.Trace or LogLevel.Debug => Theme.Muted,
        _ => Theme.Default,
    };

    private void DrawModals()
    {
        for (var depth = 0; depth < _state.Modals.Count; depth++)
        {
            _stackDepth = depth;
            DrawModal(_state.Modals[depth]);
        }

        _stackDepth = 0;
    }

    private void DrawModal(Modal? open)
    {
        switch (open)
        {
            case ChoiceModal modal:
                DrawChoiceModal(modal);
                return;
            case MultiChoiceModal modal:
                DrawMultiChoiceModal(modal);
                return;
            case CommandModal modal:
                DrawCommandModal(modal);
                return;
            case NumberModal modal:
                DrawTextEntryModal(modal, modal.Title, _strings.ModalNumberHints());
                return;
            case SliderModal modal:
                DrawSliderModal(modal);
                return;
            case ToggleModal modal:
                DrawToggleModal(modal);
                return;
            case MessageModal modal:
                DrawMessageModal(modal);
                return;
            case DateModal modal:
                DrawSegmentedModal(modal, _strings.ModalDateHints());
                return;
            case TimeModal modal:
                DrawSegmentedModal(modal, _strings.ModalTimeHints());
                return;
            case ColorModal modal:
                DrawColorModal(modal);
                return;
            case TextModal modal:
                DrawTextEntryModal(modal, modal.Title, _strings.ModalTextHints());
                return;
        }
    }

    private void DrawTextEntryModal(ITextEntryModal modal, string title, string hints)
    {
        var caretIndex = TextWidth.SnapToCluster(modal.Text, modal.Caret);
        var shown = modal.Masked ? Dots(modal.Text) : modal.Text;
        var caret = modal.Masked ? TextWidth.CountClusters(modal.Text[..caretIndex]) : caretIndex;

        var room = Math.Max(SmallestFieldColumns,
            _surface.FrameWidth - BoxChromeColumns - TextWidth.Of(modal.Prefix) - TextWidth.Of(modal.Suffix));
        var (before, after) = FieldWindow(shown, caret, room);

        List<Span[]> body =
        [
            [
                new(modal.Prefix, Theme.Muted),
                new(before.Length < caret ? ScrollMarker : "", Theme.Muted),
                new(before, Theme.Input),
                new("▏", Theme.Accent),
                new(after, Theme.Input),
                new(caret + after.Length < shown.Length ? ScrollMarker : "", Theme.Muted),
                new(modal.Suffix, Theme.Muted),
            ],
        ];

        if (!string.IsNullOrEmpty(modal.Message))
        {
            body.Add([new(modal.Message, Theme.Error)]);
        }

        DrawBox(title, body, hints);
    }

    /// <summary>
    /// The slice of a value that fits beside the caret. A value longer than the terminal is scrolled
    /// rather than cut off, so the caret stays on screen wherever it is in the text.
    /// </summary>
    private static (string Before, string After) FieldWindow(string text, int caret, int room)
    {
        var before = text[..caret];
        var after = text[caret..];

        if (TextWidth.Of(text) < room)
        {
            return (before, after);
        }

        var forCaretAndMarkers = 3;
        var visible = Math.Max(1, room - forCaretAndMarkers);
        var trailing = TextWidth.Truncate(after, visible / 2);

        return (TextWidth.TruncateStart(before, visible - TextWidth.Of(trailing)), trailing);
    }

    private static string Dots(string text) => new('•', TextWidth.CountClusters(text));

    private void DrawSliderModal(SliderModal modal)
    {
        var filled = Math.Clamp((int)Math.Round(modal.Fraction * SliderTrackCells), 0, SliderTrackCells);
        var track = $"[{new string('█', filled)}{new string('░', SliderTrackCells - filled)}]";

        List<Span[]> body =
        [
            [
                new(track, Theme.Active),
                new($"  {modal.Display(modal.Value)}", Theme.Accent),
            ],
        ];

        var inside = DrawBox(modal.Title, body, _strings.ModalSliderHints());
        modal.Box = inside.Box;
        modal.Track = inside.Content.Rows(0, 1).Inset(new Margin(1, 0, inside.Content.Width - SliderTrackCells - 1, 0));
    }

    private void DrawToggleModal(ToggleModal modal)
    {
        var yes = $" {_strings.Yes()} ";
        var no = $" {_strings.No()} ";

        List<Span[]> body =
        [
            [
                new(yes, modal.Value ? Theme.ActiveSelected : Theme.Muted),
                new("   ", Theme.Default),
                new(no, modal.Value ? Theme.Muted : Theme.ActiveSelected),
            ],
        ];

        var (box, inside) = DrawBox(modal.Title, body, _strings.ModalToggleHints());
        var yesWidth = TextWidth.Of(yes);

        modal.Box = box;
        modal.YesChip = inside.Rows(0, 1).Inset(new Margin(0, 0, inside.Width - yesWidth, 0));
        modal.NoChip = inside.Rows(0, 1).Inset(new Margin(yesWidth + 3, 0, Math.Max(0, inside.Width - yesWidth - 3 - TextWidth.Of(no)), 0));
    }

    private void DrawMessageModal(MessageModal modal)
    {
        var width = Math.Max(SmallestFieldColumns, _surface.FrameWidth / 2);
        var body = new List<Span[]>();

        foreach (var line in WrapText(modal.Text, width))
        {
            body.Add([new(line, Theme.Default)]);
        }

        var (box, _) = DrawBox(modal.Title, body, _strings.ModalMessageHints());
        modal.Box = box;
    }

    private static List<string> WrapText(string text, int width)
    {
        var lines = new List<string>();

        foreach (var paragraph in text.Split('\n'))
        {
            var rest = paragraph.Replace("\r", "");

            if (rest.Length == 0)
            {
                lines.Add("");
                continue;
            }

            while (TextWidth.Of(rest) > width)
            {
                var head = TextWidth.Truncate(rest, width);
                var breakAt = head.LastIndexOf(' ');

                if (breakAt <= 0)
                {
                    breakAt = head.Length;
                }

                lines.Add(rest[..breakAt].TrimEnd());
                rest = rest[breakAt..].TrimStart();
            }

            lines.Add(rest);
        }

        return lines;
    }

    private void DrawSegmentedModal(SegmentedModal modal, string hints)
    {
        var texts = modal.EditedSegmentTexts();
        var spans = new List<Span>();

        for (var i = 0; i < texts.Length; i++)
        {
            if (i > 0)
            {
                spans.Add(new(modal.Separator, Theme.Muted));
            }

            spans.Add(new(texts[i], i == modal.Segment ? Theme.Input : Theme.Default));
        }

        DrawBox(modal.Title, [spans.ToArray()], hints);
    }

    private void DrawColorModal(ColorModal modal)
    {
        string[] labels = [_strings.ColorHue(), _strings.ColorSaturation(), _strings.ColorLightness()];

        var labelWidth = 0;
        foreach (var label in labels)
        {
            labelWidth = Math.Max(labelWidth, TextWidth.Of(label));
        }

        var preview = new RgbTermColor { Background = modal.Value };

        List<Span[]> body =
        [
            [
                new(new(' ', SliderTrackCells + 2), preview),
                new($"  {modal.Value.Hex}", Theme.Accent),
            ],
            [],
        ];

        for (var channel = ColorChannel.Hue; channel <= ColorChannel.Lightness; channel++)
        {
            var value = modal.ValueOf(channel);
            var maximum = ColorModal.MaximumOf(channel);
            var filled = maximum > 0 ? value * SliderTrackCells / maximum : 0;
            var track = $"[{new string('█', filled)}{new string('░', SliderTrackCells - filled)}]";
            var isActive = channel == modal.Channel;

            body.Add(
            [
                new(TextWidth.PadRight(labels[(int)channel], labelWidth + 2), isActive ? Theme.Accent : Theme.Muted),
                new(track, isActive ? Theme.Active : Theme.Muted),
                new($"  {value,3}", isActive ? Theme.Accent : Theme.Muted),
            ]);
        }

        var (box, inside) = DrawBox(modal.Title, body, _strings.ModalColorHints());
        var trackStart = labelWidth + 3;

        modal.Box = box;
        modal.ChannelRows = new SurfaceRegion[3];
        modal.ChannelTracks = new SurfaceRegion[3];

        for (var channel = 0; channel < 3; channel++)
        {
            var row = inside.Rows(SwatchRows + channel, 1);
            modal.ChannelRows[channel] = row;
            modal.ChannelTracks[channel] =
                row.Inset(new Margin(trackStart, 0, Math.Max(0, inside.Width - trackStart - SliderTrackCells), 0));
        }
    }

    private void DrawCommandModal(CommandModal modal)
    {
        var keyWidth = 0;
        foreach (var (key, _) in modal.Commands)
        {
            keyWidth = Math.Max(keyWidth, TextWidth.Of(key));
        }

        var body = new List<Span[]>();
        foreach (var (key, label) in modal.Commands)
        {
            body.Add([new($"{TextWidth.PadLeft(key, keyWidth)}  {label}", Theme.Default)]);
        }

        var (box, inside) = DrawBox(modal.Title, body, _strings.ModalCommandHints());

        modal.Box = box;
        modal.Rows = inside.Rows(0, modal.Commands.Count);
    }

    private void DrawChoiceModal(ChoiceModal modal)
    {
        DrawOptionList(modal, modal.Title, _strings.ModalChoiceHints(), static option => option);
    }

    private void DrawMultiChoiceModal(MultiChoiceModal modal)
    {
        var title = $"{modal.Title} — {_strings.SelectedCount(modal.Selected.Count)}";
        DrawOptionList(modal, title, _strings.ModalMultiChoiceHints(),
            option => $"[{(modal.IsSelected(option) ? '×' : ' ')}] {option}");
    }

    private void DrawOptionList(OptionListModal modal, string title, string hints, Func<string, string> formatRow)
    {
        var matchingOptions = modal.MatchingOptions();
        modal.Index = Math.Clamp(modal.Index, 0, Math.Max(0, matchingOptions.Count - 1));

        var maxRows = Math.Min(12, Math.Max(3, _surface.FrameHeight - 10));
        var visible = Math.Min(matchingOptions.Count, maxRows);
        var start = Math.Clamp(modal.Index - maxRows / 2, 0, Math.Max(0, matchingOptions.Count - maxRows));

        var rows = new string[visible];
        var content = Math.Max(TextWidth.Of(title) + 4, 34);
        content = Math.Max(content, TextWidth.Of(hints));
        for (var i = 0; i < visible; i++)
        {
            rows[i] = formatRow(matchingOptions[start + i]);
            content = Math.Max(content, TextWidth.Of(rows[i]));
        }

        var emptyNotice = matchingOptions.Count == 0 ? 1 : 0;
        var box = Centered(content, visible + emptyNotice + 4);
        var inside = box.Border(Theme.Info, title).Inset(new Margin(1, 0, 1, 0));

        modal.Box = box;
        modal.Rows = inside.Rows(2 + emptyNotice, visible);
        modal.FirstVisible = start;

        inside.WriteLine(0, _strings.Filter(modal.Filter), Theme.Info);
        DrawDivider(box, 1);

        if (emptyNotice == 1)
        {
            inside.WriteLine(2, _strings.NothingMatches(), Theme.Muted);
        }

        var scrolled = ScrollBar.IsNeeded(matchingOptions.Count, visible);
        var rowWidth = scrolled ? Math.Max(0, inside.Width - 1) : inside.Width;

        for (var i = 0; i < visible; i++)
        {
            var style = start + i == modal.Index ? Theme.Selected : Theme.Default;
            inside.Write(2 + emptyNotice + i, 0,
                TextWidth.PadRight(TextWidth.Truncate(rows[i], rowWidth), rowWidth), style);
        }

        if (scrolled)
        {
            ScrollBar.Draw(modal.Rows, start, matchingOptions.Count);
            DrawListPosition(inside, modal.Index + 1, matchingOptions.Count);
        }

        var footerRow = 2 + emptyNotice + visible;
        DrawDivider(box, footerRow);
        inside.WriteLine(footerRow + 1, hints, Theme.Muted);
    }

    private void DrawListPosition(SurfaceRegion inside, int position, int count)
    {
        var text = _strings.ListPosition(position, count);
        var column = inside.Width - TextWidth.Of(text);

        if (column > 0)
        {
            inside.Write(0, column, text, Theme.Muted);
        }
    }

    private static void DrawDivider(SurfaceRegion box, int insideRow) =>
        box.Write(insideRow + 1, 0, $"├{new string('─', Math.Max(0, box.Width - 2))}┤", Theme.Info);

    private SurfaceRegion Centered(int contentWidth, int contentHeight)
    {
        var width = Math.Min(contentWidth + 4, _surface.FrameWidth - 4);
        var height = Math.Min(contentHeight + 2, _surface.FrameHeight - 2);

        var left = Math.Max(0, (_surface.FrameWidth - width) / 2) + _stackDepth * StackedColumns;
        var top = Math.Max(0, (_surface.FrameHeight - height) / 2) + _stackDepth * StackedRows;

        return new(
            _surface,
            Math.Clamp(left, 0, Math.Max(0, _surface.FrameWidth - width)),
            Math.Clamp(top, 0, Math.Max(0, _surface.FrameHeight - height)),
            width,
            height);
    }

    private (SurfaceRegion Box, SurfaceRegion Content) DrawBox(string title, IReadOnlyList<Span[]> body, string footer)
    {
        var inner = Math.Max(TextWidth.Of(title) + 4, 34);
        foreach (var line in body)
        {
            inner = Math.Max(inner, LineWidth(line) + 2);
        }

        inner = Math.Max(inner, TextWidth.Of(footer) + 2);

        var box = Centered(inner, body.Count + 2);
        var inside = box.Border(Theme.Info, title).Inset(new Margin(1, 0, 1, 0));

        for (var row = 0; row < body.Count; row++)
        {
            var offset = 0;
            foreach (var span in body[row])
            {
                if (offset >= inside.Width)
                {
                    break;
                }

                var text = TextWidth.Truncate(span.Text, inside.Width - offset);
                inside.Write(row, offset, text, span.Style);
                offset += TextWidth.Of(text);
            }
        }

        DrawDivider(box, body.Count);
        inside.WriteLine(body.Count + 1, footer, Theme.Muted);

        return (box, inside);
    }

    private static int LineWidth(Span[] line)
    {
        var width = 0;
        foreach (var span in line)
        {
            width += TextWidth.Of(span.Text);
        }

        return width;
    }

    private void DrawHints()
    {
        if (_state.Modal != null)
        {
            return;
        }

        var hints = _navigator.CurrentHints;
        if (hints.Length == 0)
        {
            return;
        }

        var keyWidth = 0;
        foreach (var (key, _) in hints)
        {
            keyWidth = Math.Max(keyWidth, TextWidth.Of(key));
        }

        var rows = new string[hints.Length];
        var inner = 8;
        for (var i = 0; i < hints.Length; i++)
        {
            rows[i] = $"{TextWidth.PadLeft(hints[i].Key, keyWidth)} → {hints[i].Description}";
            inner = Math.Max(inner, TextWidth.Of(rows[i]));
        }

        var title = _strings.KeysTitle();
        var box = new List<string> { $"╭─ {title} {new('─', Math.Max(0, inner - TextWidth.Of(title) - 1))}╮" };
        foreach (var row in rows)
        {
            box.Add($"│ {TextWidth.PadRight(row, inner)} │");
        }

        box.Add($"╰{new string('─', inner + 2)}╯");

        _surface.WriteBlock(box, Theme.Info, Align.Right | Align.Bottom, new(0, 0, 2, 3));
    }

    private void DrawSizeNotice()
    {
        var top = Math.Max(0, (_surface.FrameHeight - 5) / 2);

        _surface.AppendLine(_strings.TerminalTooSmall(), Theme.Selected, Align.Center, new(0, top, 0, 0));
        _surface.AppendLine(_strings.TerminalSize(_surface.FrameWidth, _surface.FrameHeight), Theme.Error, Align.Center, new(0, 0, 0, 1));
        _surface.AppendLine(_strings.TerminalNeeded(), Theme.Default, Align.Center);
        _surface.AppendLine(_strings.TerminalSize(_options.MinimumWidth, _options.MinimumHeight), Theme.Default, Align.Center);
    }
}
