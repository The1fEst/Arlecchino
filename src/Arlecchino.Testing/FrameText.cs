using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Arlecchino.Testing;

/// <summary>
/// Pulls apart what was written to a terminal. A frame is text with escape sequences woven through
/// it, which is unreadable in an assertion message, so these separate the content from the styling
/// and let a test assert on either.
/// </summary>
public static partial class FrameText
{
    /// <summary>Strips every escape sequence, leaving what the user would actually read.</summary>
    /// <param name="text">The frame.</param>
    /// <returns>The frame as plain text.</returns>
    public static string WithoutStyles(string text) => AnsiSequence().Replace(text, "");

    /// <summary>The frame as plain rows.</summary>
    /// <param name="text">The frame.</param>
    /// <returns>One string per row.</returns>
    public static string[] Lines(string text) => WithoutStyles(text).Split("\r\n");

    /// <summary>The color sequences in order, for asserting that something was drawn as a warning.</summary>
    /// <param name="text">The frame.</param>
    /// <returns>The sequences as they appeared.</returns>
    public static List<string> StylesIn(string text)
    {
        var styles = new List<string>();
        foreach (Match match in StyleSequence().Matches(text))
        {
            styles.Add(match.Value);
        }

        return styles;
    }

    /// <summary>
    /// The cursor moves in order. Since only what changed is redrawn, counting them is how a test
    /// shows that a frame touched a few cells rather than the whole screen.
    /// </summary>
    /// <param name="text">The frame.</param>
    /// <returns>The sequences as they appeared.</returns>
    public static List<string> CursorJumpsIn(string text)
    {
        var jumps = new List<string>();
        foreach (Match match in CursorJump().Matches(text))
        {
            jumps.Add(match.Value);
        }

        return jumps;
    }

    /// <summary>
    /// How wide a box is on one row, measured between its border characters. Useful for checking that a
    /// dialog grew to fit its content.
    /// </summary>
    /// <param name="line">A plain row, with styles already stripped.</param>
    /// <returns>The width in columns, or <c>-1</c> when the row holds no box.</returns>
    public static int BoxWidth(string line)
    {
        var first = line.IndexOfAny(['╭', '│', '├', '╰']);
        var last = line.LastIndexOfAny(['╮', '│', '┤', '╯']);

        return first < 0 || last <= first ? -1 : last - first + 1;
    }

    /// <summary>Matches any escape sequence.</summary>
    /// <returns>The expression.</returns>
    [GeneratedRegex(@"\x1b\[[0-9;?]*[a-zA-Z]")]
    public static partial Regex AnsiSequence();

    /// <summary>Matches a color or attribute sequence.</summary>
    /// <returns>The expression.</returns>
    [GeneratedRegex(@"\x1b\[[0-9;]*m")]
    public static partial Regex StyleSequence();

    /// <summary>Matches a cursor move to a row and column.</summary>
    /// <returns>The expression.</returns>
    [GeneratedRegex(@"\x1b\[\d+;\d+H")]
    public static partial Regex CursorJump();
}
