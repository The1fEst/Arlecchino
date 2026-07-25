using System;
using System.Globalization;

namespace Arlecchino.Rendering;

/// <summary>
/// A 24-bit colour. Shown exactly only where the terminal supports true colour; otherwise it is
/// mapped to the nearest palette entry — see <see cref="TerminalCapabilities"/>.
/// </summary>
/// <param name="Red">Red channel.</param>
/// <param name="Green">Green channel.</param>
/// <param name="Blue">Blue channel.</param>
public readonly record struct Rgb(byte Red, byte Green, byte Blue)
{
    /// <summary>The colour as <c>#RRGGBB</c>.</summary>
    public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}";

    /// <summary>Returns <see cref="Hex"/>.</summary>
    public override string ToString() => Hex;

    /// <summary>
    /// Builds a colour from hue, saturation and lightness — the form the colour modal edits.
    /// </summary>
    /// <param name="hue">Degrees around the wheel; values outside 0..359 wrap.</param>
    /// <param name="saturation">Percent, clamped to 0..100.</param>
    /// <param name="lightness">Percent, clamped to 0..100.</param>
    /// <returns>The matching colour.</returns>
    public static Rgb FromHsl(int hue, int saturation, int lightness)
    {
        var turns = ((hue % 360) + 360) % 360 / 360d;
        var chroma = Math.Clamp(saturation, 0, 100) / 100d;
        var level = Math.Clamp(lightness, 0, 100) / 100d;

        if (chroma == 0d)
        {
            var gray = ToByte(level);
            return new(gray, gray, gray);
        }

        var upper = level < 0.5d ? level * (1d + chroma) : level + chroma - level * chroma;
        var lower = 2d * level - upper;

        return new(
            HueToChannel(lower, upper, turns + 1d / 3d),
            HueToChannel(lower, upper, turns),
            HueToChannel(lower, upper, turns - 1d / 3d));
    }

    /// <summary>
    /// Splits the colour back into hue, saturation and lightness. Channels are whole numbers, so a
    /// round trip through <see cref="FromHsl"/> can shift a colour by a unit or two.
    /// </summary>
    /// <returns>Hue in degrees, saturation and lightness in percent.</returns>
    public (int Hue, int Saturation, int Lightness) ToHsl()
    {
        var red = Red / 255d;
        var green = Green / 255d;
        var blue = Blue / 255d;

        var max = Math.Max(red, Math.Max(green, blue));
        var min = Math.Min(red, Math.Min(green, blue));
        var level = (max + min) / 2d;
        var span = max - min;

        if (span == 0d)
        {
            return (0, 0, (int)Math.Round(level * 100));
        }

        var chroma = level > 0.5d ? span / (2d - max - min) : span / (max + min);
        var turns = max == red
            ? (green - blue) / span + (green < blue ? 6d : 0d)
            : max == green
                ? (blue - red) / span + 2d
                : (red - green) / span + 4d;

        return ((int)Math.Round(turns * 60) % 360, (int)Math.Round(chroma * 100), (int)Math.Round(level * 100));
    }

    /// <summary>Reads a colour written as <c>#RRGGBB</c> or <c>RRGGBB</c>.</summary>
    /// <param name="text">The text to read.</param>
    /// <param name="color">The colour, or <c>default</c> when the text is not six hex digits.</param>
    /// <returns><c>true</c> when the text was a colour.</returns>
    public static bool TryParseHex(string text, out Rgb color)
    {
        var digits = text.StartsWith('#') ? text[1..] : text;
        color = default;

        if (digits.Length != 6 ||
            !int.TryParse(digits, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var packed))
        {
            return false;
        }

        color = new((byte)(packed >> 16), (byte)(packed >> 8), (byte)packed);
        return true;
    }

    private static byte HueToChannel(double lower, double upper, double turns)
    {
        if (turns < 0d)
        {
            turns += 1d;
        }

        if (turns > 1d)
        {
            turns -= 1d;
        }

        var value = turns switch
        {
            < 1d / 6d => lower + (upper - lower) * 6d * turns,
            < 1d / 2d => upper,
            < 2d / 3d => lower + (upper - lower) * (2d / 3d - turns) * 6d,
            _ => lower,
        };

        return ToByte(value);
    }

    private static byte ToByte(double value) => (byte)Math.Clamp(Math.Round(value * 255), 0, 255);
}
