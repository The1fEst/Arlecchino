using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.State;

using Arlecchino.Modals.Asking;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// The field of many lines. It differs from the one of one line in the two things that follow from
/// having rows: the caret moves up and down as well as along, and Enter is a new line rather than the
/// answer — which is why submitting it takes a key of its own.
/// </summary>
internal sealed class TextAreaKeys
{
    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;
    private readonly KeyText _keyText;
    private readonly IArlecchinoTerminal _terminal;

    /// <summary>Reads keys for the field of many lines.</summary>
    /// <param name="state">Where the dialog on top lives, so that it can be closed.</param>
    /// <param name="keymap">Keys to obey.</param>
    /// <param name="keyText">Turns a key press into the character it stands for.</param>
    /// <param name="terminal">Reached for the clipboard when the field is copied.</param>
    public TextAreaKeys(
        ArlecchinoState state,
        ArlecchinoKeymap keymap,
        KeyText keyText,
        IArlecchinoTerminal terminal)
    {
        _state = state;
        _keymap = keymap;
        _keyText = keyText;
        _terminal = terminal;
    }

    /// <summary>Reads a key.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    public void Handle(TextAreaModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();

            return;
        }

        if (_keymap.Submit.Matches(key))
        {
            Submit(modal);

            return;
        }

        if (_keymap.Copy.Matches(key))
        {
            _terminal.CopyToClipboard(modal.Text);

            return;
        }

        if (Moved(modal, key) || Edited(modal, key))
        {
            return;
        }

        if (_keyText.Resolve(key) is { } typed)
        {
            modal.Insert(typed);
        }
    }

    private void Submit(TextAreaModal modal)
    {
        var text = modal.Text;

        if (modal.Validate?.Invoke(text) is { } failure)
        {
            modal.Message = failure;

            return;
        }

        _state.CloseModal();
        modal.OnSubmit(text);
    }

    private bool Moved(TextAreaModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.MoveLeft.Matches(key))
        {
            modal.MoveLeft();

            return true;
        }

        if (_keymap.MoveRight.Matches(key))
        {
            modal.MoveRight();

            return true;
        }

        if (_keymap.MoveUp.Matches(key))
        {
            modal.MoveRows(-1);

            return true;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            modal.MoveRows(1);

            return true;
        }

        if (_keymap.JumpUp.Matches(key))
        {
            modal.MoveRows(-modal.VisibleRows);

            return true;
        }

        if (_keymap.JumpDown.Matches(key))
        {
            modal.MoveRows(modal.VisibleRows);

            return true;
        }

        if (_keymap.First.Matches(key))
        {
            modal.MoveToLineStart();

            return true;
        }

        if (!_keymap.Last.Matches(key))
        {
            return false;
        }

        modal.MoveToLineEnd();

        return true;
    }

    private bool Edited(TextAreaModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Confirm.Matches(key))
        {
            modal.Break();

            return true;
        }

        if (_keymap.Erase.Matches(key))
        {
            modal.Erase();

            return true;
        }

        if (!_keymap.DeleteForward.Matches(key))
        {
            return false;
        }

        modal.DeleteForward();

        return true;
    }
}
