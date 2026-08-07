using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.State;
using Arlecchino.Modals.Choosing;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// The dialogs that are a list to pick from, whether one thing is being picked or several.
///
/// Typing narrows the list rather than jumping to a letter. Jumping works on a list of five and nothing else,
/// while narrowing works on a list of five and on a list of five hundred. A list that behaves one way at one
/// size and another way at another is one nobody comes to trust.
/// </summary>
internal sealed class ListKeys
{
    private readonly ArlecchinoState _state;
    private readonly ArlecchinoKeymap _keymap;
    private readonly KeyText _keyText;

    /// <summary>Reads keys for the list dialogs.</summary>
    /// <param name="state">Where the dialog on top lives, so that it can be closed.</param>
    /// <param name="keymap">Keys to obey.</param>
    /// <param name="keyText">Turns a key press into the character it stands for.</param>
    public ListKeys(ArlecchinoState state, ArlecchinoKeymap keymap, KeyText keyText)
    {
        _state = state;
        _keymap = keymap;
        _keyText = keyText;
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
            var picked = matching[Math.Clamp(modal.Index, 0, matching.Count - 1)];

            _state.CloseModal();
            modal.OnPicked(picked);

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
            var picked = modal.SelectedInOptionOrder();

            _state.CloseModal();
            modal.OnSubmit(picked);

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
}
