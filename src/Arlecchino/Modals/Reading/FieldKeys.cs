using Arlecchino.Editing;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.State;
using Arlecchino.Modals.Asking;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// The keys of a field of one line, whatever it asks for. Two such dialogs differ only in what they accept
/// and in what they do with the answer.
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
    public void Text(TextModal modal, KeyPress key)
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
    public void Number(NumberModal modal, KeyPress key)
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

    /// <summary>Copying, selecting, moving, rubbing out and typing, for any field of one line.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="key">The key that arrived.</param>
    public void Edit(ITextEntryModal modal, KeyPress key)
    {
        if (_keymap.Copy.Matches(key))
        {
            _terminal.CopyToClipboard(Taken(modal));

            return;
        }

        if (_keymap.Cut.Matches(key))
        {
            _terminal.CopyToClipboard(Taken(modal));
            TextEditing.EraseSelection(modal);

            return;
        }

        if (SelectKeys.Handled(modal, _keymap, key) ||
            CaretKeys.Moved(modal, _keymap, key) ||
            EraseKeys.Erased(modal, _keymap, key))
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

    /// <summary>
    /// What the clipboard keys work on: the selection where there is one, the whole value where there is not.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    /// <returns>The text to put on the clipboard.</returns>
    private static string Taken(ITextEntryModal modal) =>
        TextEditing.Selected(modal) is { Length: > 0 } selected ? selected : modal.Text;

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
}
