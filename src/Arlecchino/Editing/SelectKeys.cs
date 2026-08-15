using Arlecchino.Hosting;
using Arlecchino.Input;

namespace Arlecchino.Editing;

/// <summary>
/// The keys that take the selection about rather than the caret, which are the moving keys with Shift
/// held. They are read before the moving keys.
/// </summary>
public static class SelectKeys
{
    /// <summary>Takes the selection where the key says, leaving the text as it is.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="keymap">The keys the application obeys.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these and has been dealt with.</returns>
    public static bool Handled(ITextEntry entry, ArlecchinoKeymap keymap, KeyPress key)
    {
        if (keymap.SelectAll.Matches(key))
        {
            TextEditing.SelectAll(entry);

            return true;
        }

        if (keymap.SelectWordLeft.Matches(key))
        {
            TextEditing.SelectWord(entry, -1);

            return true;
        }

        if (keymap.SelectWordRight.Matches(key))
        {
            TextEditing.SelectWord(entry, 1);

            return true;
        }

        if (keymap.SelectLeft.Matches(key))
        {
            TextEditing.SelectCaret(entry, -1);

            return true;
        }

        if (keymap.SelectRight.Matches(key))
        {
            TextEditing.SelectCaret(entry, 1);

            return true;
        }

        if (keymap.SelectToStart.Matches(key))
        {
            TextEditing.SelectToStart(entry);

            return true;
        }

        if (!keymap.SelectToEnd.Matches(key))
        {
            return false;
        }

        TextEditing.SelectToEnd(entry);

        return true;
    }
}
