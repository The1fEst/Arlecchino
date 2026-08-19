using System;

namespace Arlecchino.Rendering.Colors;

/// <summary>
/// How far apart two colors read, by the ratio the accessibility guidelines are written in. It runs from
/// 1 for a color on itself to 21 for black on white.
/// </summary>
public static class Contrast
{
    /// <summary>
    /// The luminance at which a background is as far from white as it is from black. Below it a background
    /// is dark and wants light text; above it the other way.
    /// </summary>
    public static readonly double Pivot = Math.Sqrt(1.05d * 0.05d) - 0.05d;

    /// <summary>How much light a color sends back, weighted the way an eye weights the three channels.</summary>
    /// <param name="color">The color to measure.</param>
    /// <returns>Luminance, from 0 for black to 1 for white.</returns>
    public static double Luminance(Rgb color) =>
        (0.2126d * Linear(color.Red)) + (0.7152d * Linear(color.Green)) + (0.0722d * Linear(color.Blue));

    /// <summary>The ratio between two colors, whichever way round they are given.</summary>
    /// <param name="one">The first color.</param>
    /// <param name="other">The second color.</param>
    /// <returns>The ratio, from 1 to 21.</returns>
    public static double Between(Rgb one, Rgb other)
    {
        var first = Luminance(one);
        var second = Luminance(other);

        return (Math.Max(first, second) + 0.05d) / (Math.Min(first, second) + 0.05d);
    }

    /// <summary>Whether a background wants light text on it.</summary>
    /// <param name="background">The color behind the text.</param>
    /// <returns><c>true</c> when the background is the darker side of <see cref="Pivot"/>.</returns>
    public static bool IsDark(Rgb background) => Luminance(background) < Pivot;

    /// <summary>
    /// The most contrast this background can give in the one direction text goes on it. A mid-gray
    /// background reaches about 5, so a ladder written for black cannot be had on it at any lightness.
    /// </summary>
    /// <param name="background">The color behind the text.</param>
    /// <returns>The ratio against white or black, whichever this background is further from.</returns>
    public static double Reach(Rgb background) =>
        IsDark(background)
            ? Between(new(0xFF, 0xFF, 0xFF), background)
            : Between(new(0x00, 0x00, 0x00), background);

    private static double Linear(byte channel)
    {
        var level = channel / 255d;

        return level <= 0.04045d ? level / 12.92d : Math.Pow((level + 0.055d) / 1.055d, 2.4d);
    }
}
