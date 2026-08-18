using System;
using Arlecchino.Hosting;
using Arlecchino.Input;

namespace Arlecchino.Editing;

/// <summary>
/// Every key a line of text answers to, in the order they are read: the clipboard, the selection, the caret,
/// then rubbing out. Whatever is typed into is offered these, so a filter is edited the way a field is.
/// </summary>
public static class EntryKeys
{
    /// <summary>Does what the key says to the line.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="keymap">The keys the application obeys.</param>
    /// <param name="copy">Puts text on the clipboard, for when the line is copied or cut.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these and has been dealt with.</returns>
    public static bool Handled(
        ITextEntry entry,
        ArlecchinoKeymap keymap,
        Action<string> copy,
        KeyPress key) =>
        Clipped(entry, keymap, copy, key) ||
        SelectKeys.Handled(entry, keymap, key) ||
        CaretKeys.Moved(entry, keymap, key) ||
        EraseKeys.Erased(entry, keymap, key);

    /// <summary>
    /// Copying and cutting. Copying takes the selection where there is one and the whole line where there is
    /// not; cutting takes the selection alone, since cutting a line nothing is selected on means nothing.
    /// </summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="keymap">The keys the application obeys.</param>
    /// <param name="copy">Puts text on the clipboard.</param>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these and has been dealt with.</returns>
    public static bool Clipped(
        ITextEntry entry,
        ArlecchinoKeymap keymap,
        Action<string> copy,
        KeyPress key)
    {
        var selectedText = TextEditing.Selected(entry);

        if (keymap.Copy.Matches(key))
        {
            copy(selectedText.Length > 0 ? selectedText : entry.Text);

            return true;
        }

        if (!keymap.Cut.Matches(key) || selectedText.Length == 0)
        {
            return false;
        }

        copy(selectedText);
        TextEditing.EraseSelection(entry);

        return true;
    }
}
