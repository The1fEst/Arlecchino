using System;
using Arlecchino.Editing;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.State;
using Arlecchino.Modals.Choosing;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// The keys of the dialogs that are a list to pick from, one thing or several. Typing narrows the list rather
/// than jumping to a letter, which works the same at five rows and at five hundred.
/// </summary>
internal sealed class ListKeys
{
    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;
    private readonly KeyText _keyText;
    private readonly IArlecchinoTerminal _terminal;

    /// <summary>Reads keys for the list dialogs.</summary>
    /// <param name="state">Where the dialog on top lives, so that it can be closed.</param>
    /// <param name="keymap">Keys to obey.</param>
    /// <param name="keyText">Turns a key press into the character it stands for.</param>
    /// <param name="terminal">Reached for the clipboard when the filter is copied or cut.</param>
    public ListKeys(
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

    /// <summary>Reads a key for a list one thing is picked from.</summary>
    /// <param name="modal">The list.</param>
    /// <param name="key">The key that arrived.</param>
    public void One(ChoiceModal modal, KeyPress key)
    {
        var matching = modal.MatchingOptions();

        if (_keymap.Cancel.Matches(key))
        {
            _state.CloseModal();

            return;
        }

        if (_keymap.Confirm.Matches(key) && matching.Count > 0)
        {
            var option = matching[Math.Clamp(modal.Index, 0, matching.Count - 1)];

            _state.CloseModal();
            modal.OnPicked(option);

            return;
        }

        MoveOrFilter(modal, matching.Count, key);
    }

    /// <summary>Reads a key for a list several things are picked from.</summary>
    /// <param name="modal">The list.</param>
    /// <param name="key">The key that arrived.</param>
    public void Several(MultiChoiceModal modal, KeyPress key)
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
            var option = modal.SelectedInOptionOrder();

            _state.CloseModal();
            modal.OnSubmit(option);

            return;
        }

        MoveOrFilter(modal, matching.Count, key);
    }

    /// <summary>Walking the rows still showing, and narrowing which rows those are.</summary>
    /// <param name="modal">The list.</param>
    /// <param name="matching">How many rows are still showing.</param>
    /// <param name="key">The key that arrived.</param>
    private void MoveOrFilter(OptionListModal modal, int matching, KeyPress key)
    {
        if (_keymap.MoveUp.Matches(key))
        {
            modal.Index = Math.Max(0, modal.Index - 1);

            return;
        }

        if (_keymap.MoveDown.Matches(key))
        {
            modal.Index = Math.Min(Math.Max(0, matching - 1), modal.Index + 1);

            return;
        }

        var typedSoFar = modal.Text;

        if (!Filtering(modal, key) && _keyText.Resolve(key) is { } typed)
        {
            TextEditing.Insert(modal, typed);
        }

        if (modal.Text != typedSoFar)
        {
            modal.Index = 0;
        }
    }

    /// <summary>
    /// The keys that edit the filter rather than the list, which are read only once something is being
    /// filtered by. With nothing typed the rows are what the keys are for, so the filter leaves them alone.
    /// </summary>
    /// <param name="modal">The list.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the filter took the key.</returns>
    private bool Filtering(OptionListModal modal, KeyPress key) =>
        modal.Text.Length > 0 && EntryKeys.Handled(modal, _keymap, _terminal.CopyToClipboard, key);
}
