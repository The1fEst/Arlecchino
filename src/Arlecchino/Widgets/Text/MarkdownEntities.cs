using System;
using System.Collections.Generic;
using System.Globalization;

namespace Arlecchino.Widgets.Text;

/// <summary>
/// What a document writes as <c>&amp;rarr;</c> for want of the arrow itself, drawn here as the symbol it
/// stands for. A name this does not know is left as the document wrote it.
/// </summary>
internal static class MarkdownEntities
{
    private const int Longest = 12;
    private const int Surrogates = 0xD800;
    private const int PastSurrogates = 0xE000;
    private const int Highest = 0x10FFFF;

    /// <summary>
    /// A space that holds the words either side of it together, which is what the name says and is why a
    /// document reaches for it. Wrapping breaks on the ordinary space and leaves this one alone.
    /// </summary>
    private const string HardSpace = " ";

    private static readonly Dictionary<string, string> NamedEntities = new(StringComparer.Ordinal)
    {
        ["amp"] = "&",
        ["lt"] = "<",
        ["gt"] = ">",
        ["quot"] = "\"",
        ["apos"] = "'",
        ["nbsp"] = HardSpace,
        ["ensp"] = HardSpace,
        ["emsp"] = HardSpace,
        ["thinsp"] = HardSpace,
        ["shy"] = "",
        ["ndash"] = "–",
        ["mdash"] = "—",
        ["hellip"] = "…",
        ["middot"] = "·",
        ["bull"] = "•",
        ["dagger"] = "†",
        ["sect"] = "§",
        ["para"] = "¶",
        ["copy"] = "©",
        ["reg"] = "®",
        ["trade"] = "™",
        ["deg"] = "°",
        ["plusmn"] = "±",
        ["times"] = "×",
        ["divide"] = "÷",
        ["ne"] = "≠",
        ["le"] = "≤",
        ["ge"] = "≥",
        ["infin"] = "∞",
        ["permil"] = "‰",
        ["laquo"] = "«",
        ["raquo"] = "»",
        ["ldquo"] = "“",
        ["rdquo"] = "”",
        ["lsquo"] = "‘",
        ["rsquo"] = "’",
        ["larr"] = "←",
        ["uarr"] = "↑",
        ["rarr"] = "→",
        ["darr"] = "↓",
        ["harr"] = "↔",
        ["check"] = "✓",
        ["cross"] = "✗",
        ["star"] = "☆",
    };

    /// <summary>The symbol an entity stands for, where a line has one at this point.</summary>
    /// <param name="source">The line.</param>
    /// <param name="index">Where the ampersand is.</param>
    /// <returns>The symbol and where the line carries on, or nothing where this is an ampersand as such.</returns>
    public static (string Symbol, int Next)? At(string source, int index)
    {
        var closing = source.IndexOf(';', index + 1);

        if (closing < 0 || closing - index > Longest)
        {
            return null;
        }

        var name = source[(index + 1)..closing];

        if (name.Length == 0)
        {
            return null;
        }

        if (name[0] == '#')
        {
            return Numbered(name[1..]) is { } symbol ? (symbol, closing + 1) : null;
        }

        return NamedEntities.TryGetValue(name, out var found) ? (found, closing + 1) : null;
    }

    /// <summary>The symbol an entity stands for, where it is written with digits rather than a name.</summary>
    /// <param name="digits">What follows the hash.</param>
    /// <returns>The symbol, or nothing where the digits are not a symbol at all.</returns>
    private static string? Numbered(string digits)
    {
        var hexadecimal = digits.Length > 1 && digits[0] is 'x' or 'X';
        var read = hexadecimal
            ? int.TryParse(digits[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value)
            : int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out value);

        if (!read || value is <= 0 or > Highest || value is >= Surrogates and < PastSurrogates)
        {
            return null;
        }

        return char.ConvertFromUtf32(value);
    }
}
