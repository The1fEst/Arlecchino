using System;
using System.Text;

namespace Arlecchino.Editing;

/// <summary>
/// What a block of pasted text comes to where it lands. A line of one row takes the first line of it, since
/// what was on the clipboard does not turn one row into several.
/// </summary>
public static class PastedText
{
    /// <summary>The first line of what was pasted, with the line breaks and everything after them gone.</summary>
    /// <param name="text">What was pasted.</param>
    /// <returns>The first line, or the whole text when it holds no line break.</returns>
    public static string FirstLine(string text)
    {
        var end = text.IndexOfAny(['\r', '\n']);

        return end < 0 ? text : text[..end];
    }

    /// <summary>
    /// The first line of what was pasted, with the characters the line refuses left out. What was on the
    /// clipboard does not widen what a field accepts.
    /// </summary>
    /// <param name="text">What was pasted.</param>
    /// <param name="accepts">Whether a character may be typed here at all.</param>
    /// <returns>What is left of the first line.</returns>
    public static string FirstLine(string text, Func<char, bool> accepts)
    {
        var line = FirstLine(text);
        var kept = new StringBuilder(line.Length);

        foreach (var character in line)
        {
            if (accepts(character))
            {
                kept.Append(character);
            }
        }

        return kept.ToString();
    }
}
