using Arlecchino.Hosting;
using Arlecchino.Input;

namespace Arlecchino.Editing;

/// <summary>
/// The keys that rub text out of a line: a symbol, a word, or everything back to the start. Any of them
/// takes the selection instead while there is one.
/// </summary>
public static class EraseKeys
{
    /// <summary>Rubs out what the key says.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="keymap">The keys the application obeys.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these and has been dealt with.</returns>
    public static bool Erased(ITextEntry entry, ArlecchinoKeymap keymap, KeyPress key)
    {
        if (keymap.EraseWord.Matches(key))
        {
            TextEditing.EraseWord(entry);

            return true;
        }

        if (keymap.EraseToStart.Matches(key))
        {
            TextEditing.EraseToStart(entry);

            return true;
        }

        if (keymap.Erase.Matches(key))
        {
            TextEditing.Backspace(entry);

            return true;
        }

        if (!keymap.DeleteForward.Matches(key))
        {
            return false;
        }

        TextEditing.Delete(entry);

        return true;
    }
}
