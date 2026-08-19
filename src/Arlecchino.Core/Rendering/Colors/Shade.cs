using System;

namespace Arlecchino.Rendering.Colors;

/// <summary>
/// Colors worked out against the background they will be read on, so a palette is written as how far
/// apart things should read rather than as a list of colors that only suit one terminal.
/// </summary>
public static class Shade
{
    private const int Steps = 20;

    /// <summary>
    /// A color of this hue and chroma, as light or as dark as it must be to reach the wanted contrast
    /// against the background. The hue is kept whatever happens, and the chroma is cut only to stay in <c>sRGB</c>.
    /// </summary>
    /// <param name="background">The color it will be read on.</param>
    /// <param name="hue">Degrees around the wheel, which the answer keeps.</param>
    /// <param name="chroma">How vivid to be, cut back where <c>sRGB</c> cannot hold it.</param>
    /// <param name="contrast">The contrast to reach, or as near as the background allows.</param>
    /// <returns>The color to write in.</returns>
    public static Rgb Against(Rgb background, double hue, double chroma, double contrast)
    {
        var away = Contrast.IsDark(background);
        var edge = Oklch.Of(background).Lightness;
        var lower = away ? edge : 0d;
        var upper = away ? 1d : edge;
        var answer = new Oklch(edge, chroma, hue);

        for (var step = 0; step < Steps; step++)
        {
            var middle = (lower + upper) / 2d;

            answer = new Oklch(middle, chroma, hue).Trimmed();

            if (Contrast.Between(answer.ToRgb(), background) < contrast)
            {
                if (away)
                {
                    lower = middle;
                }
                else
                {
                    upper = middle;
                }
            }
            else if (away)
            {
                upper = middle;
            }
            else
            {
                lower = middle;
            }
        }

        return answer.ToRgb();
    }

    /// <summary>The same, taking the hue and chroma off a color that already has them.</summary>
    /// <param name="background">The color it will be read on.</param>
    /// <param name="sample">The color whose hue and chroma to keep.</param>
    /// <param name="contrast">The contrast to reach.</param>
    /// <returns>The color to write in.</returns>
    public static Rgb Against(Rgb background, Rgb sample, double contrast)
    {
        var (_, chroma, hue) = Oklch.Of(sample);

        return Against(background, hue, chroma, contrast);
    }

    /// <summary>
    /// The background lifted off itself by a step of lightness, which is what a raised surface is. The step
    /// goes away from the background on a dark terminal and toward it on a light one.
    /// </summary>
    /// <param name="background">The surface to lift off.</param>
    /// <param name="step">How far, in lightness, a whole surface being about 0.07.</param>
    /// <returns>The raised surface.</returns>
    public static Rgb Lifted(Rgb background, double step)
    {
        var surface = Oklch.Of(background);
        var toward = Contrast.IsDark(background) ? step : -step;

        return (surface with
        {
            Lightness = Math.Clamp(surface.Lightness + toward, 0d, 1d),
            Chroma = Math.Min(surface.Chroma, Tint),
        }).ToRgb();
    }

    /// <summary>
    /// A wanted contrast brought down to what the background can actually give, keeping the order of a
    /// ladder that would otherwise flatten. A background near the middle reaches about 5 and no more.
    /// </summary>
    /// <param name="contrast">The contrast the design asks for.</param>
    /// <param name="lowest">The least contrast in the ladder, which is left where it is.</param>
    /// <param name="highest">The most contrast in the ladder.</param>
    /// <param name="background">The color everything is read on.</param>
    /// <returns>The contrast to ask <see cref="Against(Rgb,double,double,double)"/> for.</returns>
    public static double Scaled(double contrast, double lowest, double highest, Rgb background)
    {
        var room = Contrast.Reach(background) * Headroom;

        if (room >= highest || highest <= lowest)
        {
            return contrast;
        }

        return lowest + ((contrast - lowest) * (Math.Max(room, lowest) - lowest) / (highest - lowest));
    }

    /// <summary>
    /// The most color a raised surface takes from the background it sits on. A terminal set to a vivid
    /// color would otherwise have that color amplified rather than raised.
    /// </summary>
    private const double Tint = 0.04d;

    private const double Headroom = 0.92d;
}
