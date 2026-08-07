using System;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Modals.Asking;

/// <summary>
/// Editing a line of text: where the caret goes and what each edit does to it. Kept apart from the
/// fields themselves so the text field, the number field and anything added later behave identically,
/// and so the behavior can be tested without a terminal. Editing never touches the validation
/// message — that is the router's job, which re-checks the field and clears the message only once the
/// input is actually valid.
/// </summary>
public static class TextEditing
{
    /// <summary>Puts a character in at the caret and steps past it.</summary>
    /// <param name="modal">The field being edited.</param>
    /// <param name="character">The character to insert.</param>
    public static void Insert(ITextEntryModal modal, char character)
    {
        var caret = TextWidth.SnapToCluster(modal.Text, modal.Caret);

        modal.Text = modal.Text[..caret] + character + modal.Text[caret..];
        modal.Caret = caret + 1;
    }

    /// <summary>
    /// Removes the symbol before the caret, doing nothing at the start of the line. A symbol, not a
    /// <c>char</c>: an emoji or a letter with a combining mark goes in one press rather than being
    /// left as half a surrogate pair.
    /// </summary>
    /// <param name="modal">The field being edited.</param>
    public static void Backspace(ITextEntryModal modal)
    {
        var caret = TextWidth.SnapToCluster(modal.Text, modal.Caret);
        if (caret == 0)
        {
            return;
        }

        var start = TextWidth.PreviousClusterStart(modal.Text, caret);

        modal.Text = modal.Text[..start] + modal.Text[caret..];
        modal.Caret = start;
    }

    /// <summary>Removes the symbol after the caret, leaving the caret where it is.</summary>
    /// <param name="modal">The field being edited.</param>
    public static void Delete(ITextEntryModal modal)
    {
        var caret = TextWidth.SnapToCluster(modal.Text, modal.Caret);
        if (caret >= modal.Text.Length)
        {
            return;
        }

        var end = TextWidth.NextClusterEnd(modal.Text, caret);

        modal.Text = modal.Text[..caret] + modal.Text[end..];
        modal.Caret = caret;
    }

    /// <summary>Removes everything from the start of the word before the caret up to the caret.</summary>
    /// <param name="modal">The field being edited.</param>
    public static void EraseWord(ITextEntryModal modal)
    {
        var caret = modal.Caret;
        var start = WordStart(modal.Text, caret);

        if (start == caret)
        {
            return;
        }

        modal.Text = modal.Text[..start] + modal.Text[caret..];
        modal.Caret = start;
    }

    /// <summary>Removes everything before the caret, which is how a field is retyped from scratch.</summary>
    /// <param name="modal">The field being edited.</param>
    public static void EraseToStart(ITextEntryModal modal)
    {
        var caret = modal.Caret;
        if (caret == 0)
        {
            return;
        }

        modal.Text = modal.Text[caret..];
        modal.Caret = 0;
    }

    /// <summary>Moves the caret by whole symbols, stopping at either end.</summary>
    /// <param name="modal">The field being edited.</param>
    /// <param name="delta">How many symbols to move by; negative goes left.</param>
    public static void MoveCaret(ITextEntryModal modal, int delta)
    {
        var caret = TextWidth.SnapToCluster(modal.Text, modal.Caret);

        for (var step = 0; step < Math.Abs(delta); step++)
        {
            caret = delta < 0
                ? TextWidth.PreviousClusterStart(modal.Text, caret)
                : TextWidth.NextClusterEnd(modal.Text, caret);
        }

        modal.Caret = caret;
    }

    /// <summary>Moves the caret to the start of the line.</summary>
    /// <param name="modal">The field being edited.</param>
    public static void MoveToStart(ITextEntryModal modal) => modal.Caret = 0;

    /// <summary>Moves the caret past the last character.</summary>
    /// <param name="modal">The field being edited.</param>
    public static void MoveToEnd(ITextEntryModal modal) => modal.Caret = modal.Text.Length;

    /// <summary>
    /// Moves the caret a word at a time: to the start of the word behind it, or past the end of the
    /// word ahead of it.
    /// </summary>
    /// <param name="modal">The field being edited.</param>
    /// <param name="direction">Negative to go left, positive to go right.</param>
    public static void MoveWord(ITextEntryModal modal, int direction) =>
        modal.Caret = direction < 0 ? WordStart(modal.Text, modal.Caret) : WordEnd(modal.Text, modal.Caret);

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
