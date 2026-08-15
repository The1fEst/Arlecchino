using System;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Editing;

/// <summary>
/// Editing a line of text: where the caret goes and what each edit does to it, apart from whatever holds the
/// line. A symbol is a grapheme cluster rather than a <c>char</c>, so an emoji is rubbed out whole.
/// </summary>
public static class TextEditing
{
    /// <summary>What is selected, as the place it starts and the place it ends.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <returns>The two places, the smaller first; they are equal while nothing is selected.</returns>
    public static (int Start, int End) Selection(ITextEntry entry)
    {
        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);
        var anchor = TextWidth.SnapToCluster(entry.Text, entry.Anchor);

        return caret < anchor ? (caret, anchor) : (anchor, caret);
    }

    /// <summary>Whatever is selected, for putting on the clipboard.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <returns>The selected text, or an empty string while nothing is selected.</returns>
    public static string Selected(ITextEntry entry)
    {
        var (start, end) = Selection(entry);

        return entry.Text[start..end];
    }

    /// <summary>Selects the whole line, leaving the caret at the end of it.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void SelectAll(ITextEntry entry)
    {
        entry.Anchor = 0;
        entry.Caret = entry.Text.Length;
    }

    /// <summary>Removes whatever is selected, which is what typing over a selection comes to.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <returns><c>true</c> when something was selected and has gone.</returns>
    public static bool EraseSelection(ITextEntry entry)
    {
        var (start, end) = Selection(entry);

        if (start == end)
        {
            return false;
        }

        Cut(entry, start, end);

        return true;
    }

    /// <summary>Puts a character in at the caret and steps past it, over whatever was selected.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="character">The character to insert.</param>
    public static void Insert(ITextEntry entry, char character)
    {
        EraseSelection(entry);

        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);

        entry.Text = entry.Text[..caret] + character + entry.Text[caret..];
        Put(entry, caret + 1);
    }

    /// <summary>
    /// Removes the symbol before the caret, or whatever is selected. A symbol, not a <c>char</c>: an emoji
    /// or a letter with a combining mark goes in one press rather than being left as half a surrogate pair.
    /// </summary>
    /// <param name="entry">The line being edited.</param>
    public static void Backspace(ITextEntry entry)
    {
        if (EraseSelection(entry))
        {
            return;
        }

        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);

        if (caret == 0)
        {
            return;
        }

        Cut(entry, TextWidth.PreviousClusterStart(entry.Text, caret), caret);
    }

    /// <summary>Removes the symbol after the caret, or whatever is selected.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void Delete(ITextEntry entry)
    {
        if (EraseSelection(entry))
        {
            return;
        }

        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);

        if (caret >= entry.Text.Length)
        {
            return;
        }

        Cut(entry, caret, TextWidth.NextClusterEnd(entry.Text, caret));
    }

    /// <summary>Removes the word before the caret, or whatever is selected.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void EraseWord(ITextEntry entry)
    {
        if (EraseSelection(entry))
        {
            return;
        }

        var caret = entry.Caret;

        Cut(entry, WordStart(entry.Text, caret), caret);
    }

    /// <summary>Removes everything before the caret, or whatever is selected.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void EraseToStart(ITextEntry entry)
    {
        if (EraseSelection(entry))
        {
            return;
        }

        Cut(entry, 0, entry.Caret);
    }

    /// <summary>Moves the caret by whole symbols, stopping at either end and dropping the selection.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="delta">How many symbols to move by; negative goes left.</param>
    public static void MoveCaret(ITextEntry entry, int delta) => Put(entry, Stepped(entry, delta));

    /// <summary>Takes the caret the same way, dragging the selection along behind it.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="delta">How many symbols to move by; negative goes left.</param>
    public static void SelectCaret(ITextEntry entry, int delta) => Drag(entry, Stepped(entry, delta));

    /// <summary>Moves the caret to the start of the line.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void MoveToStart(ITextEntry entry) => Put(entry, 0);

    /// <summary>Selects from the caret back to the start of the line.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void SelectToStart(ITextEntry entry) => Drag(entry, 0);

    /// <summary>Moves the caret past the last character.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void MoveToEnd(ITextEntry entry) => Put(entry, entry.Text.Length);

    /// <summary>Selects from the caret to the end of the line.</summary>
    /// <param name="entry">The line being edited.</param>
    public static void SelectToEnd(ITextEntry entry) => Drag(entry, entry.Text.Length);

    /// <summary>
    /// Moves the caret a word at a time: to the start of the word behind it, or past the end of the
    /// word ahead of it.
    /// </summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="direction">Negative to go left, positive to go right.</param>
    public static void MoveWord(ITextEntry entry, int direction) => Put(entry, Word(entry, direction));

    /// <summary>Takes the caret a word the same way, dragging the selection along behind it.</summary>
    /// <param name="entry">The line being edited.</param>
    /// <param name="direction">Negative to go left, positive to go right.</param>
    public static void SelectWord(ITextEntry entry, int direction) => Drag(entry, Word(entry, direction));

    private static int Word(ITextEntry entry, int direction) =>
        direction < 0 ? WordStart(entry.Text, entry.Caret) : WordEnd(entry.Text, entry.Caret);

    private static int Stepped(ITextEntry entry, int delta)
    {
        var caret = TextWidth.SnapToCluster(entry.Text, entry.Caret);

        for (var step = 0; step < Math.Abs(delta); step++)
        {
            caret = delta < 0
                ? TextWidth.PreviousClusterStart(entry.Text, caret)
                : TextWidth.NextClusterEnd(entry.Text, caret);
        }

        return caret;
    }

    private static void Put(ITextEntry entry, int caret)
    {
        entry.Caret = caret;
        entry.Anchor = entry.Caret;
    }

    private static void Drag(ITextEntry entry, int caret)
    {
        var anchor = entry.Anchor;

        entry.Caret = caret;
        entry.Anchor = anchor;
    }

    private static void Cut(ITextEntry entry, int start, int end)
    {
        entry.Text = entry.Text[..start] + entry.Text[end..];
        Put(entry, start);
    }

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
