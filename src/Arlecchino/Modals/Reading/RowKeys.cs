using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Modals.Asking;

namespace Arlecchino.Modals.Reading;

/// <summary>
/// The keys that go by rows in the field of many lines, and to either end of the row the caret is on. They
/// are the field's own: a line of text has nowhere to go up to.
/// </summary>
internal static class RowKeys
{
    /// <summary>Moves the caret by rows or to an end of its row, dropping the selection.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="keymap">The keys the application obeys.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these and has been dealt with.</returns>
    public static bool Moved(TextAreaModal modal, ArlecchinoKeymap keymap, KeyPress key)
    {
        if (keymap.MoveUp.Matches(key))
        {
            modal.MoveRows(-1);

            return true;
        }

        if (keymap.MoveDown.Matches(key))
        {
            modal.MoveRows(1);

            return true;
        }

        if (keymap.JumpUp.Matches(key))
        {
            modal.MoveRows(-modal.VisibleRows);

            return true;
        }

        if (keymap.JumpDown.Matches(key))
        {
            modal.MoveRows(modal.VisibleRows);

            return true;
        }

        if (keymap.First.Matches(key))
        {
            modal.MoveToLineStart();

            return true;
        }

        if (!keymap.Last.Matches(key))
        {
            return false;
        }

        modal.MoveToLineEnd();

        return true;
    }

    /// <summary>Takes the selection by rows or to an end of the row the caret is on.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="keymap">The keys the application obeys.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these and has been dealt with.</returns>
    public static bool Selected(TextAreaModal modal, ArlecchinoKeymap keymap, KeyPress key)
    {
        if (keymap.SelectUp.Matches(key))
        {
            modal.SelectRows(-1);

            return true;
        }

        if (keymap.SelectDown.Matches(key))
        {
            modal.SelectRows(1);

            return true;
        }

        if (keymap.SelectToStart.Matches(key))
        {
            modal.SelectToLineStart();

            return true;
        }

        if (!keymap.SelectToEnd.Matches(key))
        {
            return false;
        }

        modal.SelectToLineEnd();

        return true;
    }
}
