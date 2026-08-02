using System;
using Arlecchino.Commands;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Modals;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Choosing;
using Arlecchino.Modals.Reading;
using Arlecchino.Navigation;
using Arlecchino.State;
using Microsoft.Extensions.Logging;

namespace Arlecchino;

/// <summary>
/// Decides who gets a key or a mouse event. The order is what keeps the application predictable: an
/// open dialog takes everything, then the palette key, then the view's own commands, then commands
/// available everywhere, and only then the view itself. A handler that throws is reported on the
/// output line rather than allowed to stop the loop.
///
/// What a key means once a dialog has it is not decided here. This file is the order and nothing
/// else, so that the order can be read at a sitting.
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
    private readonly Repaint _repaint;
    private readonly ILogger<InputRouter> _logger;
    private readonly ModalKeys _modalKeys;
    private readonly ModalClicks _modalClicks;
    private readonly CommandPalette _palette;

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
        _repaint = repaint;
        _logger = logger;

        _modalKeys = new(state, _keymap, keyText, terminal, options.Strings);
        _modalClicks = new(state);
        _palette = new(state, navigator, commands, options, keyText);
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

    private void Route(ConsoleKeyInfo key)
    {
        if (_state.Modal is { } modal)
        {
            RouteModal(modal, key);

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

        if (RanCommand(key))
        {
            return;
        }

        _navigator.Handle(key);
    }

    /// <summary>
    /// The view's own commands first, then the ones available everywhere. A command available
    /// everywhere is only reached with a modifier held, so an unmodified letter always belongs to the
    /// view.
    /// </summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when something was bound to it.</returns>
    private bool RanCommand(ConsoleKeyInfo key)
    {
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

            return true;
        }

        if (key.Modifiers == default || !_commands.TryFind(key, out var command))
        {
            return false;
        }

        _navigator.Apply(command.Execute());

        return true;
    }

    /// <summary>
    /// Hands a key to the open dialog. The two the framework does not read itself are the application's
    /// own, which reads its keys for itself, and the palette, whose keys are every other key there is.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    private void RouteModal(Modal modal, ConsoleKeyInfo key)
    {
        switch (modal)
        {
            case CustomModal custom:
                custom.Handle(key);
                return;
            case CommandModal:
                _palette.Handle(key);
                return;
            default:
                _modalKeys.Handle(modal, key);
                return;
        }
    }

    private void RouteMouse(MouseEvent mouse)
    {
        switch (_state.Modal)
        {
            case null when ClickedOutputRow(mouse):
                _navigator.Apply(Routes.Notifications);
                return;
            case null:
                _navigator.HandleMouse(mouse);
                return;
            case CustomModal custom:
                custom.HandleMouse(mouse);
                return;
            case CommandModal commands:
                _palette.Click(commands, mouse);
                return;
            case { } modal:
                _modalClicks.Handle(modal, mouse);
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
                list.Filter += FirstLine(text);
                list.Index = 0;
                return;
            case null:
                _navigator.HandlePaste(text);
                return;
        }
    }

    /// <summary>
    /// Pastes into a field of one line: the first line of what was pasted, and only the characters the
    /// field will take. A field asking for a number does not become a field asking for anything because
    /// something else was on the clipboard.
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

        _modalKeys.Recheck(modal);
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
