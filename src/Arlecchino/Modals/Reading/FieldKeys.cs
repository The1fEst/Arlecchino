using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.State;

using Arlecchino.Modals.Asking;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// A field of one line, whatever it is asking for. Moving the caret, rubbing out, copying and typing
/// are the same everywhere, so a dialog that asks for a name and one that asks for a number differ
/// only in what they will accept and what they do when the answer is given.
/// </summary>
internal sealed class FieldKeys
{
    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;
    private readonly KeyText _keyText;
    private readonly IArlecchinoTerminal _terminal;
    private readonly ArlecchinoStrings _strings;
    private readonly StepKeys _steps;

    /// <summary>Reads keys for the fields of one line.</summary>
    /// <param name="state">Where the dialog on top lives, so that it can be closed.</param>
    /// <param name="keymap">Keys to obey.</param>
    /// <param name="keyText">Turns a key press into the character it stands for.</param>
    /// <param name="terminal">Reached for the clipboard when a field is copied.</param>
    /// <param name="strings">The words the application says things in.</param>
    /// <param name="steps">The keys that move a number without typing it.</param>
    public FieldKeys(
        ArlecchinoState state,
        ArlecchinoKeymap keymap,
        KeyText keyText,
        IArlecchinoTerminal terminal,
        ArlecchinoStrings strings,
        StepKeys steps)
    {
        _state = state;
        _keymap = keymap;
        _keyText = keyText;
        _terminal = terminal;
        _strings = strings;
        _steps = steps;
    }

    /// <summary>Reads a key for a field of text.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    public void Text(TextModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();

            return;
        }

        if (!_keymap.Confirm.Matches(key))
        {
            Edit(modal, key);
            Recheck(modal);

            return;
        }

        if ((Complaints.AboutFormat(modal, _strings) ?? modal.Validate?.Invoke(modal.Text)) is { } error)
        {
            modal.Message = error;

            return;
        }

        _state.CloseModal();
        modal.OnSubmit(modal.Text);
    }

    /// <summary>Reads a key for a field that asks for a number, which can be typed or stepped.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    public void Number(NumberModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();

            return;
        }

        if (_keymap.Confirm.Matches(key))
        {
            Submit(modal);

            return;
        }

        if (!_steps.Stepped(modal, key))
        {
            Edit(modal, key);
        }

        Recheck(modal);
    }

    /// <summary>Copying, moving, rubbing out and typing, for any field of one line.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    public void Edit(ITextEntryModal modal, ConsoleKeyInfo key)
    {
        if (_keymap.Copy.Matches(key))
        {
            _terminal.CopyToClipboard(modal.Text);

            return;
        }

        if (Moved(modal, key) || Erased(modal, key))
        {
            return;
        }

        if (_keyText.Resolve(key) is not { } typed || !modal.AcceptsCharacter(typed))
        {
            return;
        }

        TextEditing.Insert(modal, typed);
    }

    /// <summary>
    /// Keeps a message that is already showing up to date as the field is edited, clearing it the
    /// moment what is typed becomes valid.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    public void Recheck(ITextEntryModal modal)
    {
        if (modal.Message is not null)
        {
            modal.Message = Complaints.About(modal, _strings);
        }
    }

    private void Submit(NumberModal modal)
    {
        if (Complaints.AboutNumber(modal, _strings) is { } problem)
        {
            modal.Message = problem;

            return;
        }

        modal.TryGetValue(out var value);

        _state.CloseModal();
        modal.OnSubmit(value);
    }

    private bool Moved(ITextEntryModal modal, ConsoleKeyInfo key)
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

        if (!_keymap.Last.Matches(key))
        {
            return false;
        }

        TextEditing.MoveToEnd(modal);

        return true;
    }

    private bool Erased(ITextEntryModal modal, ConsoleKeyInfo key)
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

        if (!_keymap.DeleteForward.Matches(key))
        {
            return false;
        }

        TextEditing.Delete(modal);

        return true;
    }
}
