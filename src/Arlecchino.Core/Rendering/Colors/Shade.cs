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
    /// The background lifted off itself by a step of lightness, which is what a raised surface is. It keeps
    /// the background's own hue and chroma, being the same surface raised rather than a different one.
    /// </summary>
    /// <param name="background">The surface to lift off.</param>
    /// <param name="step">How far, in lightness, a whole surface being about 0.07.</param>
    /// <returns>The raised surface.</returns>
    public static Rgb Lifted(Rgb background, double step) => Raised(background, Rise(background) * step);

    /// <summary>
    /// Which way surfaces are raised off this background: lighter on a dark terminal, darker on a light one,
    /// and the other way where that would take the tallest of them across <see cref="Contrast.Pivot"/>.
    /// </summary>
    /// <param name="background">The color behind the text.</param>
    /// <returns><c>1</c> where a raised surface is the lighter one, <c>-1</c> where it is the darker.</returns>
    public static double Rise(Rgb background)
    {
        var toward = Contrast.IsDark(background) ? 1d : -1d;
        var crossed = Contrast.IsDark(Raised(background, toward * Tallest)) != Contrast.IsDark(background);

        return crossed ? -toward : toward;
    }

    private static Rgb Raised(Rgb background, double step)
    {
        var surface = Oklch.Of(background);

        return (surface with { Lightness = Math.Clamp(surface.Lightness + step, 0d, 1d) }).ToRgb();
    }

    /// <summary>
    /// How much a background's own color should sway a design drawn for a gray one. It is nothing behind a
    /// terminal theme and everything behind a background someone chose a color for.
    /// </summary>
    /// <param name="background">The color behind the text.</param>
    /// <returns>How far to follow the background, from 0 to 1.</returns>
    public static double Pull(Rgb background) =>
        Math.Clamp((Oklch.Of(background).Chroma - Quiet) / (Vivid - Quiet), 0d, 1d);

    /// <summary>
    /// The turn to add to every hue of a design so it sits at an angle from a colored background rather
    /// than across from it. Turning the whole design at once keeps its colors spaced as they were drawn.
    /// </summary>
    /// <param name="background">The color behind the text.</param>
    /// <param name="anchor">The hue of the design's own accent, which the turn is measured from.</param>
    /// <param name="offset">How far from the background the accent is to end up.</param>
    /// <returns>Degrees to add to every hue, which is nothing behind a near-neutral terminal.</returns>
    public static double Turn(Rgb background, double anchor, double offset)
    {
        var hue = Oklch.Of(background).Hue;
        var degrees = ((((hue + offset) - anchor) % 360d) + 540d) % 360d - 180d;

        return degrees * Pull(background);
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
    /// The tallest step a surface is raised by, which is what decides the direction they all take. One step
    /// deciding for the rest keeps them in order, raised being further off whichever way that turned out.
    /// </summary>
    private const double Tallest = 0.11d;

    /// <summary>
    /// The chroma a background may have and still be left alone. Every terminal theme in wide use sits
    /// under it: <c>gruvbox</c> at 0, <c>dracula</c> and <c>nord</c> near 0.02, <c>solarized</c> at 0.049.
    /// </summary>
    private const double Quiet = 0.06d;

    /// <summary>
    /// The chroma at which a background sways a design as far as it ever will. A background someone chose
    /// a color for starts around here: a middling green is 0.177 and a full one 0.295.
    /// </summary>
    private const double Vivid = 0.16d;

    private const double Headroom = 0.92d;
}
