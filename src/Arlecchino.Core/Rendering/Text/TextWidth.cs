using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Arlecchino.Rendering.Text;

/// <summary>
/// Measures text the way a terminal shows it: in columns, not in <c>char</c> values. CJK and emoji
/// take two columns, combining marks take none, and a surrogate pair is one symbol. Use these
/// instead of <c>string.Length</c>, <c>PadRight</c> and slicing whenever the result lands on screen.
/// </summary>
public static class TextWidth
{
    private const char VariationSelectorEmoji = '️';
    private const char ZeroWidthJoiner = '‍';

    private static readonly (int Start, int End)[] WideRanges =
    [
        (0x1100, 0x115F),
        (0x2E80, 0x303E),
        (0x3041, 0x33FF),
        (0x3400, 0x4DBF),
        (0x4E00, 0x9FFF),
        (0xA000, 0xA4CF),
        (0xA960, 0xA97F),
        (0xAC00, 0xD7A3),
        (0xF900, 0xFAFF),
        (0xFE10, 0xFE19),
        (0xFE30, 0xFE6F),
        (0xFF00, 0xFF60),
        (0xFFE0, 0xFFE6),
        (0x1F300, 0x1F64F),
        (0x1F680, 0x1F6FF),
        (0x1F900, 0x1F9FF),
        (0x20000, 0x2FFFD),
        (0x30000, 0x3FFFD),
    ];

    /// <summary>How many columns the text occupies.</summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>Width in terminal columns.</returns>
    public static int Of(string text)
    {
        var width = 0;
        var index = 0;

        while (index < text.Length)
        {
            var length = NextClusterLength(text, index);
            width += OfCluster(text.AsSpan(index, length));
            index += length;
        }

        return width;
    }

    /// <summary>Width of a single grapheme cluster — one symbol as the user sees it.</summary>
    /// <param name="cluster">The cluster, as returned by <see cref="NextClusterLength"/>.</param>
    /// <returns>0, 1 or 2 columns.</returns>
    public static int OfCluster(ReadOnlySpan<char> cluster)
    {
        if (cluster.IsEmpty)
        {
            return 0;
        }

        if (cluster.IndexOf(VariationSelectorEmoji) >= 0 || cluster.IndexOf(ZeroWidthJoiner) >= 0)
        {
            return 2;
        }

        return Rune.DecodeFromUtf16(cluster, out var rune, out _) == OperationStatus.Done ? OfRune(rune) : 1;
    }

    /// <summary>Width of a single code point, before combining marks are taken into account.</summary>
    /// <param name="rune">The code point to measure.</param>
    /// <returns>0 for marks and control characters, 2 for wide ranges, 1 otherwise.</returns>
    public static int OfRune(Rune rune)
    {
        if (rune.Value == 0)
        {
            return 0;
        }

        var category = Rune.GetUnicodeCategory(rune);
        if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark or UnicodeCategory.Format)
        {
            return 0;
        }

        if (category == UnicodeCategory.Control)
        {
            return 0;
        }

        foreach (var (start, end) in WideRanges)
        {
            if (rune.Value >= start && rune.Value <= end)
            {
                return 2;
            }
        }

        return 1;
    }

    /// <summary>How many <c>char</c> values the next symbol occupies, starting at an index.</summary>
    /// <param name="text">The text being walked.</param>
    /// <param name="index">Where the symbol starts.</param>
    /// <returns>Length of the cluster in <c>char</c> values.</returns>
    public static int NextClusterLength(string text, int index) =>
        StringInfo.GetNextTextElementLength(text.AsSpan(index));

    /// <summary>
    /// Where the symbol before an index starts. This is what a backspace has to remove: deleting one
    /// <c>char</c> would cut an emoji or a letter with a combining mark in half.
    /// </summary>
    /// <param name="text">The text being walked.</param>
    /// <param name="index">Position to look back from.</param>
    /// <returns>The boundary before the index, or <c>0</c> at the start of the text.</returns>
    public static int PreviousClusterStart(string text, int index)
    {
        var start = 0;
        var walk = 0;

        while (walk < text.Length && walk < index)
        {
            start = walk;
            walk += NextClusterLength(text, walk);
        }

        return start;
    }

    /// <summary>
    /// Where the symbol at an index ends. This is what a forward delete has to remove.
    /// </summary>
    /// <param name="text">The text being walked.</param>
    /// <param name="index">Position to look forward from.</param>
    /// <returns>The next boundary, or the length of the text at its end.</returns>
    public static int NextClusterEnd(string text, int index)
    {
        var start = SnapToCluster(text, index);
        return start >= text.Length ? text.Length : start + NextClusterLength(text, start);
    }

    /// <summary>
    /// Pulls a position back to the start of the symbol it lands in, so an index that came from
    /// somewhere else never points into the middle of one.
    /// </summary>
    /// <param name="text">The text being walked.</param>
    /// <param name="index">Position to snap.</param>
    /// <returns>The boundary at or before the index.</returns>
    public static int SnapToCluster(string text, int index)
    {
        if (index <= 0)
        {
            return 0;
        }

        if (index >= text.Length)
        {
            return text.Length;
        }

        var walk = 0;

        while (walk < text.Length)
        {
            var next = walk + NextClusterLength(text, walk);

            if (next > index)
            {
                return walk;
            }

            walk = next;
        }

        return text.Length;
    }

    /// <summary>
    /// Cuts the text down to a column width on a symbol boundary, so a wide character or a surrogate
    /// pair is never split in half.
    /// </summary>
    /// <param name="text">The text to cut.</param>
    /// <param name="maxWidth">Columns available.</param>
    /// <returns>The longest prefix that fits.</returns>
    public static string Truncate(string text, int maxWidth)
    {
        if (maxWidth <= 0)
        {
            return "";
        }

        var width = 0;
        var index = 0;

        while (index < text.Length)
        {
            var length = NextClusterLength(text, index);
            var clusterWidth = OfCluster(text.AsSpan(index, length));

            if (width + clusterWidth > maxWidth)
            {
                return text[..index];
            }

            width += clusterWidth;
            index += length;
        }

        return text;
    }

    /// <summary>
    /// Cuts the text down to a column width from the other end, keeping the tail rather than the head.
    /// That is what a field scrolled to the right shows.
    /// </summary>
    /// <param name="text">The text to cut.</param>
    /// <param name="maxWidth">Columns available.</param>
    /// <returns>The longest suffix that fits.</returns>
    public static string TruncateStart(string text, int maxWidth)
    {
        if (maxWidth <= 0)
        {
            return "";
        }

        var width = 0;
        var start = text.Length;

        while (start > 0)
        {
            var previous = PreviousClusterStart(text, start);
            var clusterWidth = OfCluster(text.AsSpan(previous, start - previous));

            if (width + clusterWidth > maxWidth)
            {
                break;
            }

            width += clusterWidth;
            start = previous;
        }

        return text[start..];
    }

    /// <summary>How many symbols the text is made of, as the user would count them.</summary>
    /// <param name="text">The text to count.</param>
    /// <returns>The number of grapheme clusters.</returns>
    public static int CountClusters(string text)
    {
        var count = 0;
        var index = 0;

        while (index < text.Length)
        {
            index += NextClusterLength(text, index);
            count++;
        }

        return count;
    }

    /// <summary>
    /// Breaks text into lines that fit a column width, at spaces where there is one and mid-word only
    /// when a single word is wider than the space. Line breaks already in the text are kept, so a
    /// paragraph stays a paragraph.
    /// </summary>
    /// <param name="text">The text to break up.</param>
    /// <param name="width">Columns available; anything below one is treated as one.</param>
    /// <returns>The lines, in order.</returns>
    public static List<string> Wrap(string text, int width)
    {
        var columns = Math.Max(1, width);
        var lines = new List<string>();

        foreach (var paragraph in text.Replace("\r", "").Split('\n'))
        {
            var rest = paragraph;

            while (Of(rest) > columns)
            {
                var head = Truncate(rest, columns);
                var breakAt = head.LastIndexOf(' ');

                if (breakAt <= 0)
                {
                    breakAt = head.Length;
                }

                lines.Add(rest[..breakAt].TrimEnd());
                rest = rest[breakAt..].TrimStart();
            }

            lines.Add(rest);
        }

        return lines;
    }

    /// <summary>Pads the text with spaces on the right until it fills the given column width.</summary>
    /// <param name="text">The text to pad.</param>
    /// <param name="width">Columns to fill.</param>
    /// <returns>The padded text, unchanged when it is already that wide.</returns>
    public static string PadRight(string text, int width)
    {
        var missing = width - Of(text);
        return missing > 0 ? text + new string(' ', missing) : text;
    }

    /// <summary>Pads the text with spaces on the left, which right-aligns it in that width.</summary>
    /// <param name="text">The text to pad.</param>
    /// <param name="width">Columns to fill.</param>
    /// <returns>The padded text, unchanged when it is already that wide.</returns>
    public static string PadLeft(string text, int width)
    {
        var missing = width - Of(text);
        return missing > 0 ? new string(' ', missing) + text : text;
    }
}
