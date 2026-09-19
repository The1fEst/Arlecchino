using System;
using System.Collections.Generic;
using System.Globalization;
using Arlecchino.Rendering.Colors;

namespace Arlecchino.Pictures.Formats.Svg;

/// <summary>
/// What a shape is filled and drawn with, and the numbers either is written with. A color this does not
/// know refuses the file rather than being guessed at.
/// </summary>
internal static class SvgPaint
{
    /// <summary>What a shape with nothing said about it is filled with, which is black.</summary>
    public static SvgColor Black { get; } = new(new(0, 0, 0), 1d);

    private static readonly Dictionary<string, RGB> NamedColors = new(StringComparer.Ordinal)
    {
        ["black"] = new(0x00, 0x00, 0x00),
        ["silver"] = new(0xC0, 0xC0, 0xC0),
        ["gray"] = new(0x80, 0x80, 0x80),
        ["grey"] = new(0x80, 0x80, 0x80),
        ["white"] = new(0xFF, 0xFF, 0xFF),
        ["maroon"] = new(0x80, 0x00, 0x00),
        ["red"] = new(0xFF, 0x00, 0x00),
        ["purple"] = new(0x80, 0x00, 0x80),
        ["fuchsia"] = new(0xFF, 0x00, 0xFF),
        ["magenta"] = new(0xFF, 0x00, 0xFF),
        ["green"] = new(0x00, 0x80, 0x00),
        ["lime"] = new(0x00, 0xFF, 0x00),
        ["olive"] = new(0x80, 0x80, 0x00),
        ["yellow"] = new(0xFF, 0xFF, 0x00),
        ["navy"] = new(0x00, 0x00, 0x80),
        ["blue"] = new(0x00, 0x00, 0xFF),
        ["teal"] = new(0x00, 0x80, 0x80),
        ["aqua"] = new(0x00, 0xFF, 0xFF),
        ["cyan"] = new(0x00, 0xFF, 0xFF),
        ["orange"] = new(0xFF, 0xA5, 0x00),
    };

    /// <summary>The color an attribute names.</summary>
    /// <param name="value">What the attribute said, or <c>null</c> where it was not written.</param>
    /// <param name="fallback">What a shape that says nothing is drawn in.</param>
    /// <param name="color">The color, which is nothing at all where the shape asked for none.</param>
    /// <returns><c>false</c> where the file names a color this does not know.</returns>
    public static bool Read(string? value, SvgColor? fallback, out SvgColor? color)
    {
        color = fallback;

        if (value is null)
        {
            return true;
        }

        var text = value.Trim();

        if (text.Length == 0)
        {
            return true;
        }

        if (text.Equals("none", StringComparison.OrdinalIgnoreCase) ||
            text.Equals("transparent", StringComparison.OrdinalIgnoreCase))
        {
            color = null;

            return true;
        }

        if (text[0] == '#')
        {
            if (!Hex(text[1..], out var mixed))
            {
                color = null;

                return false;
            }

            color = new(mixed, 1d);

            return true;
        }

        if (!NamedColors.TryGetValue(text.ToLowerInvariant(), out var found))
        {
            return false;
        }

        color = new(found, 1d);

        return true;
    }

    /// <summary>A number an attribute is written with, in the units a shape is measured in.</summary>
    /// <param name="value">What the attribute said, or <c>null</c> where it was not written.</param>
    /// <param name="fallback">What it is when it was not written.</param>
    /// <returns>The number.</returns>
    public static double Number(string? value, double fallback = 0d)
    {
        if (value is null)
        {
            return fallback;
        }

        var digits = value.Trim().TrimEnd('p', 'x', 'P', 'X').Trim();

        return double.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? number
            : fallback;
    }

    /// <summary>
    /// The numbers of an attribute that is written as a list of them, such as the points of a polyline or
    /// the dashes of a line. They are parted by spaces, by commas, or by both.
    /// </summary>
    /// <param name="value">What the attribute said.</param>
    /// <returns>The numbers, in the order they were written.</returns>
    public static List<double> Numbers(string? value)
    {
        var numbers = new List<double>();

        if (value is null)
        {
            return numbers;
        }

        foreach (var word in value.Split([' ', ',', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (double.TryParse(word, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                numbers.Add(number);
            }
        }

        return numbers;
    }

    /// <summary>The color a hexadecimal triple stands for, in three digits or in six.</summary>
    /// <param name="digits">What follows the hash.</param>
    /// <param name="color">The color, once it is read.</param>
    /// <returns><c>false</c> where these are not three or six digits.</returns>
    private static bool Hex(string digits, out RGB color)
    {
        color = default;

        if (digits.Length is not (3 or 6))
        {
            return false;
        }

        Span<byte> parts = stackalloc byte[3];

        for (var part = 0; part < 3; part++)
        {
            var pair = digits.Length == 3
                ? new(digits[part], 2)
                : digits.Substring(part * 2, 2);

            if (!byte.TryParse(pair, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out parts[part]))
            {
                return false;
            }
        }

        color = new(parts[0], parts[1], parts[2]);

        return true;
    }
}

/// <summary>One color as a shape asked for it.</summary>
/// <param name="Color">The color itself.</param>
/// <param name="Alpha">How much of it there is, from nought to one.</param>
internal readonly record struct SvgColor(RGB Color, double Alpha);
