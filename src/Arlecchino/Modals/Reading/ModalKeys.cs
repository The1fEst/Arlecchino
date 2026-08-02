using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.State;

using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Choosing;
using Arlecchino.Modals.Setting;
using Arlecchino.Modals.Telling;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// How every dialog the framework brings reads a key.
///
/// The router decides who gets the key; this decides what the key means once a dialog has it. They
/// are apart because the order — dialog, palette, view commands, view — is one decision made in one
/// place, and it has nothing to teach a slider about which way is up.
/// </summary>
internal sealed class ModalKeys
{
    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;
    private readonly KeyText _keyText;
    private readonly FieldKeys _fields;
    private readonly TextAreaKeys _areas;
    private readonly StepKeys _steps;
    private readonly ListKeys _lists;

    /// <summary>Reads keys for the dialogs the framework brings.</summary>
    /// <param name="state">Where the dialog on top lives, so that it can be closed.</param>
    /// <param name="keymap">Keys to obey.</param>
    /// <param name="keyText">Turns a key press into the character it stands for.</param>
    /// <param name="terminal">Reached for the clipboard when a field is copied.</param>
    /// <param name="strings">The words the application says things in.</param>
    public ModalKeys(
        ArlecchinoState state,
        ArlecchinoKeymap keymap,
        KeyText keyText,
        IArlecchinoTerminal terminal,
        ArlecchinoStrings strings)
    {
        _state = state;
        _keymap = keymap;
        _keyText = keyText;
        _steps = new(state, keymap);
        _fields = new(state, keymap, keyText, terminal, strings, _steps);
        _areas = new(state, keymap, keyText, terminal);
        _lists = new(state, keymap, keyText);
    }

    /// <summary>Hands a key to whichever kind of dialog is open.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>false</c> when it is a kind this does not know, which the router deals with itself.</returns>
    public bool Handle(Modal modal, ConsoleKeyInfo key)
    {
        switch (modal)
        {
            case ChoiceModal choice:
                _lists.One(choice, key);
                return true;
            case MultiChoiceModal multiChoice:
                _lists.Several(multiChoice, key);
                return true;
            case NumberModal number:
                _fields.Number(number, key);
                return true;
            case SliderModal slider:
                _steps.Slider(slider, key);
                return true;
            case ToggleModal toggle:
                Toggle(toggle, key);
                return true;
            case MessageModal message:
                Message(message, key);
                return true;
            case NotificationModal opened:
                Notification(opened, key);
                return true;
            case TextAreaModal area:
                _areas.Handle(area, key);
                return true;
            case SegmentedModal segmented:
                Segmented(segmented, key);
                return true;
            case ColorModal color:
                _steps.Color(color, key);
                return true;
            case TextModal text:
                _fields.Text(text, key);
                return true;
            default:
                return false;
        }
    }

    /// <summary>Keeps a complaint a field is already showing up to date after it has been edited.</summary>
    /// <param name="modal">The field.</param>
    public void Recheck(ITextEntryModal modal) => _fields.Recheck(modal);

    private void Toggle(ToggleModal modal, ConsoleKeyInfo key)
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

        if (_keymap.MoveLeft.Matches(key) ||
            _keymap.MoveRight.Matches(key) ||
            _keymap.NextField.Matches(key) ||
            _keymap.Mark.Matches(key))
        {
            modal.Value = !modal.Value;
        }
    }

    private void Message(MessageModal modal, ConsoleKeyInfo key)
    {
        if (!_keymap.Cancel.Matches(key) && !_keymap.Confirm.Matches(key))
        {
            return;
        }

        _state.CloseModal();
        modal.OnClosed?.Invoke();
    }

    /// <summary>
    /// Reads an opened notification: the arrows walk its actions, confirming runs the one selected and
    /// cancelling only closes. The dialog is closed before the action runs, so an action is free to
    /// open one of its own.
    /// </summary>
    /// <param name="modal">The notification.</param>
    /// <param name="key">The key that arrived.</param>
    private void Notification(NotificationModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.MoveLeft.Matches(key))
        {
            modal.Move(-1);

            return;
        }

        if (_keymap.MoveRight.Matches(key))
        {
            modal.Move(1);

            return;
        }

        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();

            return;
        }

        if (!_keymap.Confirm.Matches(key))
        {
            return;
        }

        _state.CloseModal();
        modal.Run();
    }

    private void Segmented(SegmentedModal modal, ConsoleKeyInfo key)
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
            Submit(modal);

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

    private static void Submit(SegmentedModal modal)
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
}
