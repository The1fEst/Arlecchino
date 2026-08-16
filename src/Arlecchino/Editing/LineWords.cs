using System;
using System.Buffers;
using System.Globalization;
using System.Text;

namespace Arlecchino.Editing;

/// <summary>
/// Where a word starts and ends, for the keys that move and rub out a word at a time. A word runs over
/// letters, digits, their marks and underscores, so <c>Arlecchino.Commander</c> is two words.
/// </summary>
internal static class LineWords
{
    /// <summary>The start of the word behind the caret, over anything it is sitting after.</summary>
    /// <param name="text">The line as it stands.</param>
    /// <param name="caret">Where the caret is.</param>
    /// <returns>Where the word starts.</returns>
    public static int Start(string text, int caret)
    {
        var index = Math.Clamp(caret, 0, text.Length);

        while (index > 0 && !IsWordBefore(text, index))
        {
            index -= LengthBefore(text, index);
        }

        while (index > 0 && IsWordBefore(text, index))
        {
            index -= LengthBefore(text, index);
        }

        return index;
    }

    /// <summary>Past the end of the word ahead of the caret, over anything before it.</summary>
    /// <param name="text">The line as it stands.</param>
    /// <param name="caret">Where the caret is.</param>
    /// <returns>Where the word ends.</returns>
    public static int End(string text, int caret)
    {
        var index = Math.Clamp(caret, 0, text.Length);

        while (index < text.Length && !IsWordAt(text, index))
        {
            index += LengthAt(text, index);
        }

        while (index < text.Length && IsWordAt(text, index))
        {
            index += LengthAt(text, index);
        }

        return index;
    }

    private static bool IsWordAt(string text, int index) =>
        Rune.TryGetRuneAt(text, index, out var rune) && IsWord(rune);

    private static bool IsWordBefore(string text, int index) =>
        Rune.DecodeLastFromUtf16(text.AsSpan(0, index), out var rune, out _) == OperationStatus.Done &&
        IsWord(rune);

    private static int LengthAt(string text, int index) =>
        Rune.TryGetRuneAt(text, index, out var rune) ? rune.Utf16SequenceLength : 1;

    private static int LengthBefore(string text, int index)
    {
        Rune.DecodeLastFromUtf16(text.AsSpan(0, index), out _, out var length);

        return Math.Max(length, 1);
    }

    private static bool IsWord(Rune rune) =>
        Rune.IsLetterOrDigit(rune) ||
        Rune.GetUnicodeCategory(rune) is UnicodeCategory.NonSpacingMark or
            UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark or
            UnicodeCategory.ConnectorPunctuation;
}
