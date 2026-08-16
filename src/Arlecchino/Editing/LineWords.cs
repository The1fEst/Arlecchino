using System;

namespace Arlecchino.Editing;

/// <summary>
/// Where a word starts and where it ends, for the keys that move and rub out a word at a time. Words are
/// told apart by the spaces between them, which is how a line of anything typed reads.
/// </summary>
internal static class LineWords
{
    /// <summary>The start of the word behind the caret, over any spaces it is sitting after.</summary>
    /// <param name="text">The line as it stands.</param>
    /// <param name="caret">Where the caret is.</param>
    /// <returns>Where the word starts.</returns>
    public static int Start(string text, int caret)
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

    /// <summary>Past the end of the word ahead of the caret, over any spaces before it.</summary>
    /// <param name="text">The line as it stands.</param>
    /// <param name="caret">Where the caret is.</param>
    /// <returns>Where the word ends.</returns>
    public static int End(string text, int caret)
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
