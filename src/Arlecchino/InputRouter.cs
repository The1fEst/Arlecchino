using System;
using Arlecchino.Commands;
using Arlecchino.Diagnostics;
using Arlecchino.Editing;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Modals;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Choosing;
using Arlecchino.Navigation;
using Arlecchino.State;
using Microsoft.Extensions.Logging;

namespace Arlecchino;

/// <summary>
/// Decides who gets a key or a mouse event, in order: an open dialog, the palette key, the view's commands,
/// the commands available everywhere, then the view. A handler that throws is reported on the output line.
/// </summary>
public class InputRouter
{
    private readonly ArlecchinoState _state;
    private readonly Navigator _navigator;
    private readonly IArlecchinoTerminal _terminal;
    private readonly LogOverlay _log;
    private readonly ArlecchinoOptions _options;
    private readonly ArlecchinoKeymap _keymap;
    private readonly Repaint _repaint;
    private readonly ILogger<InputRouter> _logger;
    private readonly ModalFrame _frame;
    private readonly CommandPalette _palette;
    private readonly CommandKeys _keys;
    private readonly IArlecchinoLayout? _layout;

    /// <summary>Creates the router.</summary>
    /// <param name="state">Holds the open dialog and the output line.</param>
    /// <param name="navigator">Supplies the current view and applies routes.</param>
    /// <param name="terminal">Reached for the clipboard when a field is copied.</param>
    /// <param name="log">Shown, hidden and scrolled from here.</param>
    /// <param name="commands">Commands available everywhere, which the palette lists.</param>
    /// <param name="options">Settings gathered at startup.</param>
    /// <param name="keyText">Turns a key press into the character it stands for.</param>
    /// <param name="repaint">Asked for a frame after anything is handled.</param>
    /// <param name="frame">What a dialog is handed while the screen shows it.</param>
    /// <param name="logger">Where handler failures are reported.</param>
    /// <param name="keys">Finds the command a key belongs to, and holds a chord that is half typed.</param>
    /// <param name="layout">The frame around every view, which sees a click before the view does.</param>
    internal InputRouter(
        ArlecchinoState state,
        Navigator navigator,
        IArlecchinoTerminal terminal,
        LogOverlay log,
        CommandRegistry commands,
        ArlecchinoOptions options,
        KeyText keyText,
        Repaint repaint,
        ModalFrame frame,
        ILogger<InputRouter> logger,
        CommandKeys keys,
        IArlecchinoLayout? layout = null)
    {
        _keys = keys;
        _layout = layout;
        _state = state;
        _navigator = navigator;
        _terminal = terminal;
        _log = log;
        _options = options;
        _keymap = options.Keymap;
        _repaint = repaint;
        _logger = logger;
        _frame = frame;

        _palette = new(state, navigator, commands, options, keyText);
    }

    /// <summary>Routes one key press and asks for a frame, whether anything took it.</summary>
    /// <param name="key">The key that was pressed.</param>
    public void ProcessKey(KeyPress key)
    {
        try
        {
            Route(key);
            _repaint.Request();
        }
        catch (Exception exception)
        {
            Log.KeyFailed(_logger, exception, key.Key, _navigator.CurrentRoute);
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
            Log.MouseFailed(_logger, exception, _navigator.CurrentRoute);
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
            Log.PasteFailed(_logger, exception, _navigator.CurrentRoute);
            _state.Output = _options.Strings.ViewFailed(exception.Message);
        }
    }

    private void Route(KeyPress key)
    {
        if (_state.Modal is { } modal)
        {
            modal.Handle(_frame, key);

            return;
        }

        if (_keys.IsWaiting)
        {
            _keys.Finish(key);

            return;
        }

        if (_keymap.ToggleLog.Matches(key))
        {
            _log.Toggle();

            return;
        }

        if (_keymap.Notifications.Matches(key) && _navigator.CurrentRoute != Routes.Notifications)
        {
            _navigator.Apply(Routes.Notifications);

            return;
        }

        if (_keymap.Help.Matches(key) && _navigator.CurrentRoute != Routes.Help)
        {
            _navigator.Apply(Routes.Help);

            return;
        }

        if (_log.IsVisible && _log.Handle(key, _keymap))
        {
            return;
        }

        if (_palette.Opens(key))
        {
            _palette.Open();

            return;
        }

        if (_keymap.Back.Matches(key) && !_navigator.CurrentIsTyping)
        {
            _navigator.Back();

            return;
        }

        if (_keymap.Forward.Matches(key) && !_navigator.CurrentIsTyping)
        {
            _navigator.Forward();

            return;
        }

        if (_keys.Ran(key))
        {
            return;
        }

        _navigator.Handle(key);
    }

    /// <summary>
    /// Where a mouse event goes: the dialog on top, then the output row, then the layout, then the view. The
    /// layout is asked first of those two, since what it draws is around the view rather than under it.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    private void RouteMouse(MouseEvent mouse)
    {
        switch (_state.Modal)
        {
            case null when ClickedOutputRow(mouse):
                _navigator.Apply(Routes.Notifications);
                return;
            case null when _layout is not null && _navigator.CurrentUsesLayout && _layout.HandleMouse(mouse):
                return;
            case null:
                _navigator.HandleMouse(mouse);
                return;
            case { } modal:
                modal.HandleMouse(_frame, mouse);
                return;
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
            case TextAreaModal area:
                area.InsertText(text);
                return;
            case ITextEntryModal entry:
                PasteIntoField(entry, text);
                return;
            case OptionListModal list:
                foreach (var character in FirstLine(text))
                {
                    TextEditing.Insert(list, character);
                }

                list.Index = 0;
                return;
            case null:
                _navigator.HandlePaste(text);
                return;
        }
    }

    /// <summary>
    /// Pastes into a field of one line: the first line of what was pasted, and only the characters the field
    /// will take. What was on the clipboard does not widen what a field accepts.
    /// </summary>
    /// <param name="modal">The field.</param>
    /// <param name="text">What was pasted.</param>
    private void PasteIntoField(ITextEntryModal modal, string text)
    {
        foreach (var character in FirstLine(text))
        {
            if (modal.AcceptsCharacter(character))
            {
                TextEditing.Insert(modal, character);
            }
        }

        _frame.Fields.Recheck(modal);
    }

    private static string FirstLine(string text)
    {
        var end = text.IndexOfAny(['\r', '\n']);

        return end < 0 ? text : text[..end];
    }

    private bool ClickedOutputRow(MouseEvent mouse) =>
        _options.ShowOutputLine &&
        mouse.IsLeftClick &&
        mouse.Row == _terminal.Height - 1 &&
        _navigator.CurrentRoute != Routes.Notifications;
}
