using System;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Editing;

/// <summary>
/// Editing a line of text: where the caret goes and what each edit does to it, apart from whatever holds the
/// line. A symbol is a grapheme cluster rather than a <c>char</c>, so an emoji is rubbed out whole.
/// </summary>
public static class TextEditing
{
    /// <summary>Puts a character in at the caret and steps past it.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="character">The character to insert.</param>
    public static void Insert(ITextEntry entry, char character)
    {
        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);

        entry.Text = entry.Text[..caret] + character + entry.Text[caret..];
        entry.Caret = caret + 1;
    }

    /// <summary>
    /// Removes the symbol before the caret, doing nothing at the start of the line. A symbol, not a
    /// <c>char</c>: an emoji or a letter with a combining mark goes in one press rather than being
    /// left as half a surrogate pair.
    /// </summary>
    /// <param name="entry">The line being edited.</param>
    public static void Backspace(ITextEntry entry)
    {
        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);
        if (caret == 0)
        {
            return;
        }

        var start = TextWidth.PreviousClusterStart(entry.Text, caret);

        entry.Text = entry.Text[..start] + entry.Text[caret..];
        entry.Caret = start;
    }

    /// <summary>Removes the symbol after the caret, leaving the caret where it is.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void Delete(ITextEntry entry)
    {
        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);
        if (caret >= entry.Text.Length)
        {
            return;
        }

        var end = TextWidth.NextClusterEnd(entry.Text, caret);

        entry.Text = entry.Text[..caret] + entry.Text[end..];
        entry.Caret = caret;
    }

    /// <summary>Removes everything from the start of the word before the caret up to the caret.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void EraseWord(ITextEntry entry)
    {
        var caret = entry.Caret;
        var start = WordStart(entry.Text, caret);

        if (start == caret)
        {
            return;
        }

        entry.Text = entry.Text[..start] + entry.Text[caret..];
        entry.Caret = start;
    }

    /// <summary>Removes everything before the caret, which is how a field is retyped from scratch.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void EraseToStart(ITextEntry entry)
    {
        var caret = entry.Caret;
        if (caret == 0)
        {
            return;
        }

        entry.Text = entry.Text[caret..];
        entry.Caret = 0;
    }

    /// <summary>Moves the caret by whole symbols, stopping at either end.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="delta">How many symbols to move by; negative goes left.</param>
    public static void MoveCaret(ITextEntry entry, int delta)
    {
        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);

        for (var step = 0; step < Math.Abs(delta); step++)
        {
            caret = delta < 0
                ? TextWidth.PreviousClusterStart(entry.Text, caret)
                : TextWidth.NextClusterEnd(entry.Text, caret);
        }

        entry.Caret = caret;
    }

    /// <summary>Moves the caret to the start of the line.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void MoveToStart(ITextEntry entry) => entry.Caret = 0;

    /// <summary>Moves the caret past the last character.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void MoveToEnd(ITextEntry entry) => entry.Caret = entry.Text.Length;

    /// <summary>
    /// Moves the caret a word at a time: to the start of the word behind it, or past the end of the
    /// word ahead of it.
    /// </summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="direction">Negative to go left, positive to go right.</param>
    public static void MoveWord(ITextEntry entry, int direction) =>
        entry.Caret = direction < 0 ? WordStart(entry.Text, entry.Caret) : WordEnd(entry.Text, entry.Caret);

    private static int WordStart(string text, int caret)
    {
        var index = Math.Clamp(caret, 0, text.Length);

        while (index > 0 && char.IsWhiteSpace(text[index - 1]))
        {
            index--;
        }

        while (index > 0 && !char.IsWhiteSpace(text[index - 1]))
        {
            index--;
        }

        return index;
    }

    private static int WordEnd(string text, int caret)
    {
        var index = Math.Clamp(caret, 0, text.Length);

        while (index < text.Length && char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        while (index < text.Length && !char.IsWhiteSpace(text[index]))
        {
            index++;
        }

        return index;
    }
}
