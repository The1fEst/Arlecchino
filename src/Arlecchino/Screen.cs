using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Commands;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Modals;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.State;
using Arlecchino.Widgets;
using Microsoft.Extensions.Logging;

namespace Arlecchino;

/// <summary>
/// Draws the frames: the current view first, then the output line, the hints and any dialog on top.
/// A view that throws while drawing is reported on the output line instead of taking the application
/// down, since a half-drawn frame is easier to recover from than a dead process.
/// </summary>
public class Screen
{
    private readonly ArlecchinoState _state;
    private readonly Surface _surface;
    private readonly Navigator _navigator;
    private readonly ArlecchinoOptions _options;
    private readonly ArlecchinoStrings _strings;
    private readonly ILogger<Screen> _logger;
    private readonly IArlecchinoTerminal _terminal;
    private readonly Repaint _repaint;
    private readonly Ticker _ticker;
    private readonly LogOverlay _log;
    private readonly PendingInput _pending;
    private readonly InputRouter _router;
    private readonly CommandRegistry _commands;
    private readonly ModalFrame _frame;
    private readonly LogPaint _logPaint;

    private int _lastWidth;
    private int _lastHeight;
    private int _forgetFrame;

    /// <summary>Creates the screen.</summary>
    /// <param name="state">Supplies the output line and the dialog to draw.</param>
    /// <param name="surface">The cell grid frames are built in.</param>
    /// <param name="navigator">Draws the current view.</param>
    /// <param name="options">Settings gathered at startup.</param>
    /// <param name="terminal">Watched for a change of size.</param>
    /// <param name="repaint">Says when a frame is actually needed.</param>
    /// <param name="ticker">Runs scheduled work between frames.</param>
    /// <param name="log">Drawn over the view while it is open.</param>
    /// <param name="pending">Input read since the last frame, routed on this thread before drawing.</param>
    /// <param name="router">Where that input goes.</param>
    /// <param name="commands">The registered commands, for offering the palette in the hints box.</param>
    /// <param name="frame">What a dialog is handed while it is on screen.</param>
    /// <param name="logger">Where drawing failures are reported.</param>
    internal Screen(
        ArlecchinoState state,
        Surface surface,
        Navigator navigator,
        ArlecchinoOptions options,
        IArlecchinoTerminal terminal,
        Repaint repaint,
        Ticker ticker,
        LogOverlay log,
        PendingInput pending,
        InputRouter router,
        CommandRegistry commands,
        ModalFrame frame,
        ILogger<Screen> logger)
    {
        _commands = commands;
        _log = log;
        _pending = pending;
        _router = router;
        _state = state;
        _surface = surface;
        _navigator = navigator;
        _options = options;
        _strings = options.Strings;
        _terminal = terminal;
        _repaint = repaint;
        _ticker = ticker;
        _logger = logger;
        _frame = frame;
        _logPaint = new(surface, _strings);
    }

    /// <summary>
    /// Draws whatever dialogs are open, oldest first and each a little below and to the right of the
    /// one under it. What each of them looks like is its own to say.
    /// </summary>
    private void DrawModals()
    {
        for (var depth = 0; depth < _state.Modals.Count; depth++)
        {
            _frame.Depth = depth;

            _state.Modals[depth].Draw(_frame);
        }

        _frame.Depth = 0;
    }

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
    public Task Run(CancellationToken stoppingToken)
    {
        var finished = new TaskCompletionSource();

        var thread = new Thread(() =>
        {
            try
            {
                Loop(stoppingToken);
                finished.SetResult();
            }
            catch (OperationCanceledException)
            {
                finished.SetResult();
            }
            catch (Exception exception)
            {
                finished.SetException(exception);
            }
        })
        {
            Name = "arlecchino-frames",
            IsBackground = true,
        };

        thread.Start();
        return finished.Task;
    }

    private void Loop(CancellationToken stoppingToken)
    {
        using var drawing = FrameThread.Claim(_repaint.Request);

        var interval = TimeSpan.FromSeconds(1d / _options.TargetFramesPerSecond);

        while (!stoppingToken.IsCancellationRequested)
        {
            var started = Stopwatch.GetTimestamp();

            _pending.Drain(_router);
            _ticker.Run(TickFailed);

            if (_repaint.TakeRequested() || TerminalWasResized())
            {
                DrawFrame();
            }

            var left = interval - Stopwatch.GetElapsedTime(started);

            if (left > TimeSpan.Zero)
            {
                stoppingToken.WaitHandle.WaitOne(left);
            }
        }
    }

    private void TickFailed(Exception exception)
    {
        Log.TickFailed(_logger, exception);
        _state.Output = _strings.ViewFailed(exception.Message);
    }

    private void RunFailed(Exception exception)
    {
        Log.PostedWorkFailed(_logger, exception);
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

    /// <summary>
    /// Draws one frame the way the loop does, as the difference from the last one. Reachable from the
    /// testing package so that a test drives the same path a running application takes, rather than
    /// the whole-frame path <see cref="DrawOnce"/> forces.
    /// </summary>
    internal void DrawFrame()
    {
        FrameThread.RunPending(RunFailed);

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
            Log.DrawFailed(_logger, exception, _navigator.CurrentRoute);
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
            _logPaint.Draw(_log);
        }

        DrawModals();

        _surface.Build();

        if (DrawFaults.TakeSkippedRows() is var skipped and > 0)
        {
            Log.RowsVanished(_logger, _navigator.CurrentRoute, skipped);
        }
    }

    private void DrawOutput()
    {
        var outputStyle = string.IsNullOrEmpty(_state.Output) ? Theme.Default : Theme.Warning;

        _surface.FillLineAt(_surface.FrameHeight - 2);
        _surface.WriteLineAt(_surface.FrameHeight - 1, _state.Output, outputStyle);
    }

    private void DrawHints()
    {
        if (_state.Modal != null)
        {
            return;
        }

        var hints = new List<(string Key, string Description)>(_navigator.CurrentHints);

        if (_commands.Commands.Count > 0)
        {
            hints.Add((_options.CommandPaletteKey.ToString(), _strings.HintCommands()));
        }

        if (hints.Count == 0)
        {
            return;
        }

        var keyWidth = 0;
        foreach (var (key, _) in hints)
        {
            keyWidth = Math.Max(keyWidth, TextWidth.Of(key));
        }

        var rows = new string[hints.Count];
        var inner = 8;
        for (var i = 0; i < hints.Count; i++)
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
