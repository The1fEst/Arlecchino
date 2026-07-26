using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Arlecchino.Commands;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Modals;

namespace Arlecchino.Input;

/// <summary>
/// Decides who gets a key or a mouse event. The order is what keeps the application predictable: an
/// open dialog takes everything, then the palette key, then the view's own commands, then commands
/// available everywhere, and only then the view itself. A handler that throws is reported on the
/// output line rather than allowed to stop the loop.
/// </summary>
public class InputRouter
{
    private readonly ArlecchinoState _state;
    private readonly Navigator _navigator;
    private readonly IArlecchinoTerminal _terminal;
    private readonly LogOverlay _log;
    private readonly CommandRegistry _commands;
    private readonly ArlecchinoOptions _options;
    private readonly ArlecchinoKeymap _keymap;
    private readonly KeyText _keyText;
    private readonly Repaint _repaint;
    private readonly ILogger<InputRouter> _logger;

    /// <summary>Creates the router.</summary>
    /// <param name="state">Holds the open dialog and the output line.</param>
    /// <param name="navigator">Supplies the current view and applies routes.</param>
    /// <param name="terminal">Reached for the clipboard when a field is copied.</param>
    /// <param name="log">Shown, hidden and scrolled from here.</param>
    /// <param name="commands">Commands available everywhere and per view.</param>
    /// <param name="options">Settings gathered at startup.</param>
    /// <param name="keyText">Turns a key press into the character it stands for.</param>
    /// <param name="repaint">Asked for a frame after anything is handled.</param>
    /// <param name="logger">Where handler failures are reported.</param>
    internal InputRouter(
        ArlecchinoState state,
        Navigator navigator,
        IArlecchinoTerminal terminal,
        LogOverlay log,
        CommandRegistry commands,
        ArlecchinoOptions options,
        KeyText keyText,
        Repaint repaint,
        ILogger<InputRouter> logger)
    {
        _state = state;
        _navigator = navigator;
        _terminal = terminal;
        _log = log;
        _commands = commands;
        _options = options;
        _keymap = options.Keymap;
        _keyText = keyText;
        _repaint = repaint;
        _logger = logger;
    }

    /// <summary>Routes one key press and asks for a frame, whether or not anything took it.</summary>
    /// <param name="key">The key that was pressed.</param>
    public void ProcessKey(ConsoleKeyInfo key)
    {
        try
        {
            Route(key);
            _repaint.Request();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Handling {Key} failed at route {Route}.", key.Key, _navigator.CurrentRoute);
            _state.Output = _options.Strings.ViewFailed(exception.Message);
        }
    }

    /// <summary>Routes one mouse event and asks for a frame.</summary>
    /// <param name="mouse">The event that arrived.</param>
    public void ProcessMouse(MouseEvent mouse)
    {
        try
        {
            RouteMouse(mouse);
            _repaint.Request();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Handling a mouse event failed at route {Route}.", _navigator.CurrentRoute);
            _state.Output = _options.Strings.ViewFailed(exception.Message);
        }
    }

    /// <summary>
    /// Routes a block of pasted text and asks for a frame. It goes wherever typing would, but as one
    /// edit rather than one per character.
    /// </summary>
    /// <param name="text">What was pasted.</param>
    public void ProcessPaste(string text)
    {
        try
        {
            RoutePaste(text);
            _repaint.Request();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Handling a paste failed at route {Route}.", _navigator.CurrentRoute);
            _state.Output = _options.Strings.ViewFailed(exception.Message);
        }
    }

    private void RoutePaste(string text)
    {
        if (text.Length == 0)
        {
            return;
        }

        switch (_state.Modal)
        {
            case ITextEntryModal entry:
                PasteIntoField(entry, text);
                Recheck(entry);
                return;
            case OptionListModal list:
                list.Filter += FirstLine(text);
                list.Index = 0;
                return;
            case null:
                _navigator.HandlePaste(text);
                return;
        }
    }

    private static void PasteIntoField(ITextEntryModal modal, string text)
    {
        foreach (var character in FirstLine(text))
        {
            if (modal.AcceptsCharacter(character))
            {
                TextEditing.Insert(modal, character);
            }
        }
    }

    private static string FirstLine(string text)
    {
        var end = text.IndexOfAny(['\r', '\n']);
        return end < 0 ? text : text[..end];
    }

    private void RouteMouse(MouseEvent mouse)
    {
        switch (_state.Modal)
        {
            case null:
                _navigator.HandleMouse(mouse);
                return;
            case OptionListModal list:
                ClickOptionList(list, mouse);
                return;
            case CommandModal commands:
                ClickCommandModal(commands, mouse);
                return;
            case SliderModal slider:
                DragTrack(slider.Track, mouse, slider.SetFromFraction);
                return;
            case ToggleModal toggle:
                ClickToggle(toggle, mouse);
                return;
            case ColorModal color:
                ClickColor(color, mouse);
                return;
        }
    }

    private void ClickOptionList(OptionListModal modal, MouseEvent mouse)
    {
        var matching = modal.MatchingOptions();

        switch (mouse.Action)
        {
            case MouseAction.ScrolledUp:
                modal.Index = Math.Max(0, modal.Index - 1);
                return;
            case MouseAction.ScrolledDown:
                modal.Index = Math.Min(Math.Max(0, matching.Count - 1), modal.Index + 1);
                return;
            case MouseAction.Pressed when mouse.Button == MouseButton.Left && modal.Rows.Contains(mouse.Row, mouse.Column):
                var (row, _) = modal.Rows.ToLocal(mouse.Row, mouse.Column);
                var index = modal.FirstVisible + row;

                if (index < 0 || index >= matching.Count)
                {
                    return;
                }

                var wasSelected = index == modal.Index;
                modal.Index = index;

                if (wasSelected)
                {
                    Confirm(modal, matching);
                }

                return;
        }
    }

    private void Confirm(OptionListModal modal, List<string> matching)
    {
        var picked = matching[Math.Clamp(modal.Index, 0, matching.Count - 1)];

        switch (modal)
        {
            case ChoiceModal choice:
                _state.CloseModal();
                choice.OnPicked(picked);
                return;
            case MultiChoiceModal multiChoice:
                multiChoice.Toggle(picked);
                return;
        }
    }

    private void ClickCommandModal(CommandModal modal, MouseEvent mouse)
    {
        if (mouse.Action != MouseAction.Pressed || mouse.Button != MouseButton.Left ||
            !modal.Rows.Contains(mouse.Row, mouse.Column))
        {
            return;
        }

        var (row, _) = modal.Rows.ToLocal(mouse.Row, mouse.Column);
        var viewCommands = _navigator.CurrentCommands;

        if (row < 0 || row >= viewCommands.Count + _commands.Commands.Count)
        {
            return;
        }

        _state.CloseModal();

        if (row < viewCommands.Count)
        {
            var viewCommand = viewCommands[row];

            if (viewCommand.IsEnabled())
            {
                _navigator.Apply(viewCommand.Run());
            }

            return;
        }

        _navigator.Apply(_commands.Commands[row - viewCommands.Count].Execute());
    }

    private static void ClickToggle(ToggleModal modal, MouseEvent mouse)
    {
        if (mouse.Action != MouseAction.Pressed || mouse.Button != MouseButton.Left)
        {
            return;
        }

        if (modal.YesChip.Contains(mouse.Row, mouse.Column))
        {
            modal.Value = true;
        }
        else if (modal.NoChip.Contains(mouse.Row, mouse.Column))
        {
            modal.Value = false;
        }
    }

    private static void ClickColor(ColorModal modal, MouseEvent mouse)
    {
        if (mouse.Action is not (MouseAction.Pressed or MouseAction.Moved) || mouse.Button != MouseButton.Left)
        {
            return;
        }

        for (var channel = 0; channel < modal.ChannelRows.Length; channel++)
        {
            if (!modal.ChannelRows[channel].Contains(mouse.Row, mouse.Column))
            {
                continue;
            }

            modal.Channel = (ColorChannel)channel;
            DragTrack(modal.ChannelTracks[channel], mouse,
                fraction => modal.SetChannelFromFraction((ColorChannel)channel, fraction));
            return;
        }
    }

    private static void DragTrack(SurfaceRegion track, MouseEvent mouse, Action<decimal> apply)
    {
        if (track.IsEmpty || mouse.Action is not (MouseAction.Pressed or MouseAction.Moved) ||
            !track.Contains(mouse.Row, mouse.Column))
        {
            return;
        }

        var (_, column) = track.ToLocal(mouse.Row, mouse.Column);
        apply(track.Width <= 1 ? 0m : (decimal)column / (track.Width - 1));
    }

    private void Route(ConsoleKeyInfo key)
    {
        if (_state.Modal is { } modal)
        {
            ProcessModalKey(modal, key);
            return;
        }

        if (_keymap.ToggleLog.Matches(key))
        {
            _log.Toggle();
            return;
        }

        if (_log.IsVisible && ScrollLog(key))
        {
            return;
        }

        if (IsCommandPaletteKey(key))
        {
            OpenCommandPalette();
            return;
        }

        if (_keymap.Back.Matches(key))
        {
            _navigator.Back();
            return;
        }

        if (_keymap.Forward.Matches(key))
        {
            _navigator.Forward();
            return;
        }

        foreach (var viewCommand in _navigator.CurrentCommands)
        {
            if (!viewCommand.Binding.Matches(key))
            {
                continue;
            }

            if (viewCommand.IsEnabled())
            {
                _navigator.Apply(viewCommand.Run());
            }

            return;
        }

        if (key.Modifiers != default && _commands.TryFind(key, out var command))
        {
            _navigator.Apply(command.Execute());
            return;
        }

        _navigator.Handle(key);
    }

    /// <summary>
    /// Scrolling and closing the log. Only these keys are taken while it is open, so the screen behind
    /// it keeps working — the overlay is for reading, not a mode to get stuck in.
    /// </summary>
    private bool ScrollLog(ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _log.IsVisible = false;
            return true;
        }

        if (_keymap.MoveUp.Matches(key))
        {
            _log.Scroll++;
            return true;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            _log.Scroll--;
            return true;
        }

        if (_keymap.Last.Matches(key))
        {
            _log.Scroll = 0;
            return true;
        }

        if (_keymap.Erase.Matches(key))
        {
            _log.Buffer.Clear();
            _log.Scroll = 0;
            return true;
        }

        return false;
    }

    private bool IsCommandPaletteKey(ConsoleKeyInfo key)
    {
        return _commands.Commands.Count > 0 && _keyText.Resolve(key) == _options.CommandPaletteKey;
    }

    private void OpenCommandPalette()
    {
        _state.Output = string.Empty;
        _state.Modal = new CommandModal
        {
            Title = _options.Strings.CommandPaletteTitle(),
            Commands = PaletteEntries(),
        };
    }

    private (string Key, string Label)[] PaletteEntries()
    {
        var entries = new List<(string Key, string Label)>();

        foreach (var command in _navigator.CurrentCommands)
        {
            entries.Add((command.Binding.ToString(), command.Label()));
        }

        foreach (var command in _commands.Commands)
        {
            entries.Add((command.Binding.ToString(), command.Label));
        }

        return [.. entries];
    }

    private void ProcessModalKey(Modal modal, ConsoleKeyInfo key)
    {
        switch (modal)
        {
            case CommandModal:
                ProcessCommandModal(key);
                return;
            case ChoiceModal choice:
                ProcessChoiceModal(choice, key);
                return;
            case MultiChoiceModal multiChoice:
                ProcessMultiChoiceModal(multiChoice, key);
                return;
            case NumberModal number:
                ProcessNumberModal(number, key);
                return;
            case SliderModal slider:
                ProcessSliderModal(slider, key);
                return;
            case ToggleModal toggle:
                ProcessToggleModal(toggle, key);
                return;
            case SegmentedModal segmented:
                ProcessSegmentedModal(segmented, key);
                return;
            case ColorModal color:
                ProcessColorModal(color, key);
                return;
            case TextModal text:
                ProcessTextModal(text, key);
                return;
        }
    }

    private void ProcessCommandModal(ConsoleKeyInfo key)
    {
        _state.CloseModal();

        if (_keymap.Cancel.Matches(key) || _keymap.Confirm.Matches(key))
        {
            return;
        }

        foreach (var viewCommand in _navigator.CurrentCommands)
        {
            if (!viewCommand.Binding.Matches(key))
            {
                continue;
            }

            if (viewCommand.IsEnabled())
            {
                _navigator.Apply(viewCommand.Run());
            }

            return;
        }

        if (_commands.TryFind(key, out var command))
        {
            _navigator.Apply(command.Execute());
            return;
        }

        var shown = char.IsControl(key.KeyChar) || key.KeyChar == '\0'
            ? new KeyBinding(key.Key, key.Modifiers).ToString()
            : key.KeyChar.ToString();
        _state.Output = _options.Strings.CommandUnknown(shown);
    }

    private void ProcessChoiceModal(ChoiceModal modal, ConsoleKeyInfo key)
    {
        var matching = modal.MatchingOptions();

        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();
            return;
        }

        if (_keymap.Confirm.Matches(key) && matching.Count > 0)
        {
            var picked = matching[Math.Clamp(modal.Index, 0, matching.Count - 1)];
            _state.CloseModal();
            modal.OnPicked(picked);
            return;
        }

        MoveOrFilter(modal, matching.Count, key);
    }

    private void ProcessMultiChoiceModal(MultiChoiceModal modal, ConsoleKeyInfo key)
    {
        var matching = modal.MatchingOptions();

        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();
            return;
        }

        if (_keymap.Mark.Matches(key) && matching.Count > 0)
        {
            modal.Toggle(matching[Math.Clamp(modal.Index, 0, matching.Count - 1)]);
            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            var picked = modal.SelectedInOptionOrder();
            _state.CloseModal();
            modal.OnSubmit(picked);
            return;
        }

        MoveOrFilter(modal, matching.Count, key);
    }

    private void MoveOrFilter(OptionListModal modal, int matchingCount, ConsoleKeyInfo key)
    {
        if (_keymap.MoveUp.Matches(key))
        {
            modal.Index = Math.Max(0, modal.Index - 1);
            return;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            modal.Index = Math.Min(Math.Max(0, matchingCount - 1), modal.Index + 1);
            return;
        }

        if (_keymap.Erase.Matches(key) && modal.Filter.Length > 0)
        {
            modal.Filter = modal.Filter[..^1];
            modal.Index = 0;
            return;
        }

        if (_keyText.Resolve(key) is not { } typed)
        {
            return;
        }

        modal.Filter += typed;
        modal.Index = 0;
    }

    private void ProcessNumberModal(NumberModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();
            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            SubmitNumber(modal);
            return;
        }

        if (StepBounded(modal, key))
        {
            Recheck(modal);
            return;
        }

        EditText(modal, key);
        Recheck(modal);
    }

    /// <summary>
    /// Keeps a message that is already showing up to date as the field is edited, clearing it the
    /// moment the input becomes valid. Nothing is reported before the user has tried to submit once,
    /// so a field does not complain about being half-typed.
    /// </summary>
    private void Recheck(ITextEntryModal modal)
    {
        if (modal.Message is not null)
        {
            modal.Message = Problem(modal);
        }
    }

    private string? Problem(ITextEntryModal modal) => modal switch
    {
        NumberModal number => NumberProblem(number),
        TextModal text => FormatError(text) ?? text.Validate?.Invoke(text.Text),
        _ => null,
    };

    private string? NumberProblem(NumberModal modal)
    {
        if (!modal.TryGetValue(out var value))
        {
            return _options.Strings.NotANumber();
        }

        if (value < modal.Minimum || value > modal.Maximum)
        {
            return _options.Strings.OutOfRange(modal.Display(modal.Minimum), modal.Display(modal.Maximum));
        }

        return modal.Validate?.Invoke(value);
    }

    private bool StepBounded(IBoundedModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.MoveUp.Matches(key))
        {
            modal.Add(modal.Step);
            return true;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            modal.Add(-modal.Step);
            return true;
        }

        if (_keymap.JumpUp.Matches(key))
        {
            modal.Add(modal.LargeStep);
            return true;
        }

        if (_keymap.JumpDown.Matches(key))
        {
            modal.Add(-modal.LargeStep);
            return true;
        }

        return false;
    }

    private void EditText(ITextEntryModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Copy.Matches(key))
        {
            _terminal.CopyToClipboard(modal.Text);
            return;
        }

        if (MoveCaret(modal, key) || EraseText(modal, key))
        {
            return;
        }

        if (_keyText.Resolve(key) is not { } typed || !modal.AcceptsCharacter(typed))
        {
            return;
        }

        TextEditing.Insert(modal, typed);
    }

    private bool MoveCaret(ITextEntryModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.WordLeft.Matches(key))
        {
            TextEditing.MoveWord(modal, -1);
            return true;
        }

        if (_keymap.WordRight.Matches(key))
        {
            TextEditing.MoveWord(modal, 1);
            return true;
        }

        if (_keymap.MoveLeft.Matches(key))
        {
            TextEditing.MoveCaret(modal, -1);
            return true;
        }

        if (_keymap.MoveRight.Matches(key))
        {
            TextEditing.MoveCaret(modal, 1);
            return true;
        }

        if (_keymap.First.Matches(key))
        {
            TextEditing.MoveToStart(modal);
            return true;
        }

        if (_keymap.Last.Matches(key))
        {
            TextEditing.MoveToEnd(modal);
            return true;
        }

        return false;
    }

    private bool EraseText(ITextEntryModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.EraseWord.Matches(key))
        {
            TextEditing.EraseWord(modal);
            return true;
        }

        if (_keymap.EraseToStart.Matches(key))
        {
            TextEditing.EraseToStart(modal);
            return true;
        }

        if (_keymap.Erase.Matches(key))
        {
            TextEditing.Backspace(modal);
            return true;
        }

        if (_keymap.DeleteForward.Matches(key))
        {
            TextEditing.Delete(modal);
            return true;
        }

        return false;
    }

    private void SubmitNumber(NumberModal modal)
    {
        if (NumberProblem(modal) is { } problem)
        {
            modal.Message = problem;
            return;
        }

        modal.TryGetValue(out var value);
        _state.CloseModal();
        modal.OnSubmit(value);
    }

    private void ProcessSliderModal(SliderModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();
            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            _state.CloseModal();
            modal.OnSubmit(modal.Value);
            return;
        }

        if (_keymap.MoveRight.Matches(key))
        {
            modal.Add(modal.Step);
            return;
        }

        if (_keymap.MoveLeft.Matches(key))
        {
            modal.Add(-modal.Step);
            return;
        }

        if (_keymap.First.Matches(key))
        {
            modal.MoveToMinimum();
            return;
        }

        if (_keymap.Last.Matches(key))
        {
            modal.MoveToMaximum();
            return;
        }

        StepBounded(modal, key);
    }

    private void ProcessToggleModal(ToggleModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();
            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            _state.CloseModal();
            modal.OnSubmit(modal.Value);
            return;
        }

        if (_keymap.MoveLeft.Matches(key) || _keymap.MoveRight.Matches(key) ||
            _keymap.NextField.Matches(key) || _keymap.Mark.Matches(key))
        {
            modal.Value = !modal.Value;
        }
    }

    private void ProcessSegmentedModal(SegmentedModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();
            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            modal.CommitTypedDigits();
            _state.CloseModal();
            SubmitSegmented(modal);
            return;
        }

        if (_keymap.MoveLeft.Matches(key) || _keymap.PreviousField.Matches(key))
        {
            modal.MoveSegment(-1);
            return;
        }

        if (_keymap.MoveRight.Matches(key) || _keymap.NextField.Matches(key))
        {
            modal.MoveSegment(1);
            return;
        }

        if (_keymap.MoveUp.Matches(key))
        {
            modal.Add(1);
            return;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            modal.Add(-1);
            return;
        }

        if (_keymap.Erase.Matches(key))
        {
            modal.ClearTypedDigits();
            return;
        }

        if (_keyText.Resolve(key) is { } typed && char.IsAsciiDigit(typed))
        {
            modal.TypeDigit(typed);
        }
    }

    private static void SubmitSegmented(SegmentedModal modal)
    {
        switch (modal)
        {
            case DateModal date:
                date.OnSubmit(date.Value);
                return;
            case TimeModal time:
                time.OnSubmit(time.Value);
                return;
        }
    }

    private void ProcessColorModal(ColorModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();
            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            var picked = modal.Value;
            _state.CloseModal();
            modal.OnPicked(picked);
            return;
        }

        if (_keymap.MoveUp.Matches(key) || _keymap.PreviousField.Matches(key))
        {
            modal.MoveChannel(-1);
            return;
        }

        if (_keymap.MoveDown.Matches(key) || _keymap.NextField.Matches(key))
        {
            modal.MoveChannel(1);
            return;
        }

        if (_keymap.MoveLeft.Matches(key))
        {
            modal.Add(-modal.Step);
            return;
        }

        if (_keymap.MoveRight.Matches(key))
        {
            modal.Add(modal.Step);
            return;
        }

        if (_keymap.JumpUp.Matches(key))
        {
            modal.Add(modal.LargeStep);
            return;
        }

        if (_keymap.JumpDown.Matches(key))
        {
            modal.Add(-modal.LargeStep);
            return;
        }

        if (_keymap.First.Matches(key))
        {
            modal.MoveToMinimum();
            return;
        }

        if (_keymap.Last.Matches(key))
        {
            modal.MoveToMaximum();
        }
    }

    private void ProcessTextModal(TextModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();
            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            if ((FormatError(modal) ?? modal.Validate?.Invoke(modal.Text)) is { } error)
            {
                modal.Message = error;
                return;
            }

            _state.CloseModal();
            modal.OnSubmit(modal.Text);
            return;
        }

        EditText(modal, key);
        Recheck(modal);
    }

    private string? FormatError(TextModal modal)
    {
        return modal.Format switch
        {
            TextFormat.Email when !IsEmailAddress(modal.Text) => _options.Strings.NotAnEmail(),
            TextFormat.Url when !IsWebLink(modal.Text) => _options.Strings.NotAUrl(),
            _ => null,
        };
    }

    private static bool IsEmailAddress(string text)
    {
        var at = text.IndexOf('@');
        if (at <= 0 || at != text.LastIndexOf('@') || at == text.Length - 1 || text.Contains(' '))
        {
            return false;
        }

        var domain = text[(at + 1)..];
        return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
    }

    private static bool IsWebLink(string text) =>
        Uri.TryCreate(text, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
