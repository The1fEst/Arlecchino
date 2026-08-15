using Arlecchino.Editing;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.State;
using Arlecchino.Modals.Asking;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// The keys of the field of many lines. The caret moves up and down as well as along, and Enter starts a new
/// line, so submitting takes a key of its own.
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
    public void Handle(TextAreaModal modal, KeyPress key)
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

        if (Clipped(modal, key) || Selected(modal, key) || Moved(modal, key) || Edited(modal, key))
        {
            return;
        }

        if (_keyText.Resolve(key) is { } typed)
        {
            TextEditing.Insert(modal, typed);
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

    private bool Clipped(TextAreaModal modal, KeyPress key)
    {
        var selected = TextEditing.Selected(modal);

        if (_keymap.Copy.Matches(key))
        {
            _terminal.CopyToClipboard(selected.Length > 0 ? selected : modal.Text);

            return true;
        }

        if (!_keymap.Cut.Matches(key) || selected.Length == 0)
        {
            return false;
        }

        _terminal.CopyToClipboard(selected);
        TextEditing.EraseSelection(modal);

        return true;
    }

    /// <summary>
    /// Taking the selection about. The keys that go by rows and to either end of a row are this field's own;
    /// the rest are the ones every line being typed into obeys.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these.</returns>
    private bool Selected(TextAreaModal modal, KeyPress key) =>
        RowKeys.Selected(modal, _keymap, key) || SelectKeys.Handled(modal, _keymap, key);

    private bool Moved(TextAreaModal modal, KeyPress key)
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

        if (!_keymap.MoveRight.Matches(key))
        {
            return RowKeys.Moved(modal, _keymap, key);
        }

        TextEditing.MoveCaret(modal, 1);

        return true;
    }

    private bool Edited(TextAreaModal modal, KeyPress key)
    {
        if (_keymap.Confirm.Matches(key))
        {
            TextEditing.Insert(modal, '\n');

            return true;
        }

        if (_keymap.EraseWord.Matches(key))
        {
            TextEditing.EraseWord(modal);

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
