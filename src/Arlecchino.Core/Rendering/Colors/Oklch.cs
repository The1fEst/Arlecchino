using System;
using System.Globalization;

namespace Arlecchino.Rendering.Colors;

/// <summary>
/// A color as lightness, chroma and hue, in the space where those three come apart. Moving the lightness
/// of an <see cref="Rgb"/> here leaves the color recognizably itself, which moving it in HSL does not.
/// </summary>
/// <param name="Lightness">Perceived lightness, 0 for black and 1 for white.</param>
/// <param name="Chroma">How far the color stands from gray, up to about 0.4 within <c>sRGB</c>.</param>
/// <param name="Hue">Degrees around the wheel.</param>
public readonly record struct Oklch(double Lightness, double Chroma, double Hue)
{
    private const double Slack = 1e-4;
    private const int Steps = 24;

    /// <summary>Splits a color into the three.</summary>
    /// <param name="color">The color to read.</param>
    /// <returns>The same color, said the other way round.</returns>
    public static Oklch Of(Rgb color)
    {
        var red = Linear(color.Red);
        var green = Linear(color.Green);
        var blue = Linear(color.Blue);

        var one = Root((0.4122214708 * red) + (0.5363325363 * green) + (0.0514459929 * blue));
        var two = Root((0.2119034982 * red) + (0.6806995451 * green) + (0.1073969566 * blue));
        var three = Root((0.0883024619 * red) + (0.2817188376 * green) + (0.6299787005 * blue));

        var lightness = (0.2104542553 * one) + (0.7936177850 * two) - (0.0040720468 * three);
        var greenRed = (1.9779984951 * one) - (2.4285922050 * two) + (0.4505937099 * three);
        var blueYellow = (0.0259040371 * one) + (0.7827717662 * two) - (0.8086757660 * three);

        var hue = Math.Atan2(blueYellow, greenRed) * 180d / Math.PI;
        var chroma = Math.Sqrt((greenRed * greenRed) + (blueYellow * blueYellow));

        return new(lightness, chroma, (hue + 360d) % 360d);
    }

    /// <summary>Whether <c>sRGB</c> holds this color, or whether showing it would want channels it has not got.</summary>
    public bool FitsScreen
    {
        get
        {
            var (red, green, blue) = Channels();

            return red is >= -Slack and <= 1d + Slack &&
                   green is >= -Slack and <= 1d + Slack &&
                   blue is >= -Slack and <= 1d + Slack;
        }
    }

    /// <summary>
    /// The same lightness and hue with the chroma cut back to the most <c>sRGB</c> holds. Cutting the chroma keeps
    /// the hue, where clamping the channels turns a color that is too vivid into a different color.
    /// </summary>
    /// <returns>A color the screen can show.</returns>
    public Oklch Trimmed()
    {
        if (FitsScreen)
        {
            return this;
        }

        var lower = 0d;
        var upper = Chroma;

        for (var step = 0; step < Steps; step++)
        {
            var middle = (lower + upper) / 2d;

            if ((this with { Chroma = middle }).FitsScreen)
            {
                lower = middle;
            }
            else
            {
                upper = middle;
            }
        }

        return this with { Chroma = lower };
    }

    /// <summary>Writes the three parts out, which is what a log of a derived palette holds.</summary>
    /// <returns>Lightness, chroma and hue.</returns>
    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"L {Lightness:F3} C {Chroma:F3} H {Hue:F1}");

    /// <summary>The color as the screen shows it, trimmed to <c>sRGB</c> on the way.</summary>
    /// <returns>The 24-bit color.</returns>
    public Rgb ToRgb()
    {
        var (red, green, blue) = Trimmed().Channels();

        return new(Channel(red), Channel(green), Channel(blue));
    }

    private static double Cube(double value) => value * value * value;

    private static double Root(double value) => value > 0d ? Math.Cbrt(value) : 0d;

    private static double Linear(byte channel)
    {
        var level = channel / 255d;

        return level <= 0.04045d ? level / 12.92d : Math.Pow((level + 0.055d) / 1.055d, 2.4d);
    }

    private static byte Channel(double linear)
    {
        var level = Math.Clamp(linear, 0d, 1d);
        var gamma = level <= 0.0031308d ? 12.92d * level : (1.055d * Math.Pow(level, 1d / 2.4d)) - 0.055d;

        return (byte)Math.Clamp(Math.Round(gamma * 255d), 0d, 255d);
    }

    private (double Red, double Green, double Blue) Channels()
    {
        var greenRed = Chroma * Math.Cos(Hue * Math.PI / 180d);
        var blueYellow = Chroma * Math.Sin(Hue * Math.PI / 180d);

        var one = Cube(Lightness + (0.3963377774 * greenRed) + (0.2158037573 * blueYellow));
        var two = Cube(Lightness - (0.1055613458 * greenRed) - (0.0638541728 * blueYellow));
        var three = Cube(Lightness - (0.0894841775 * greenRed) - (1.2914855480 * blueYellow));

        return (
            (4.0767416621 * one) - (3.3077115913 * two) + (0.2309699292 * three),
            (-1.2684380046 * one) + (2.6097574011 * two) - (0.3413193965 * three),
            (-0.0041960863 * one) - (0.7034186147 * two) + (1.7076147010 * three));
    }
}
