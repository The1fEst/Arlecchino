using Arlecchino.Hosting;
using Arlecchino.Input;

namespace Arlecchino.Editing;

/// <summary>
/// The keys that take the caret about a field of one line, by a symbol, by a word or to either end. Any of
/// them drops whatever was selected, which is what tells them from the keys with Shift held.
/// </summary>
public static class CaretKeys
{
    /// <summary>Moves the caret where the key says, leaving the text as it is.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="keymap">The keys the application obeys.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these and has been dealt with.</returns>
    public static bool Moved(ITextEntry entry, ArlecchinoKeymap keymap, KeyPress key)
    {
        if (keymap.WordLeft.Matches(key))
        {
            TextEditing.MoveWord(entry, -1);

            return true;
        }

        if (keymap.WordRight.Matches(key))
        {
            TextEditing.MoveWord(entry, 1);

            return true;
        }

        if (keymap.MoveLeft.Matches(key))
        {
            TextEditing.MoveCaret(entry, -1);

            return true;
        }

        if (keymap.MoveRight.Matches(key))
        {
            TextEditing.MoveCaret(entry, 1);

            return true;
        }

        if (keymap.First.Matches(key))
        {
            TextEditing.MoveToStart(entry);

            return true;
        }

        if (!keymap.Last.Matches(key))
        {
            return false;
        }

        TextEditing.MoveToEnd(entry);

        return true;
    }
}
