using System;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>A filled bar showing how far along something is, with an optional readout beside it.</summary>
public sealed class ProgressBar : IArlecchinoWidget
{
    private const char FilledCell = '█';
    private const char EmptyCell = '░';

    /// <summary>Value at which the bar is empty.</summary>
    public decimal Minimum { get; init; }

    /// <summary>Value at which the bar is full.</summary>
    public decimal Maximum { get; init; } = 100;

    /// <summary>How far along it is now.</summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Builds the text drawn after the bar, given the value. Supplied as a delegate so the wording and
    /// units stay with the application rather than the widget.
    /// </summary>
    public Func<decimal, string>? Caption { get; init; }

    /// <summary>How full the bar is, from <c>0</c> to <c>1</c>. An empty range reads as <c>0</c>.</summary>
    public decimal Fraction => Maximum > Minimum
        ? Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0m, 1m)
        : 0m;

    /// <summary>Colour of the filled part. The theme's active colour when left alone.</summary>
    public IArlecchinoColor? Style { get; init; }

    /// <summary>
    /// Draws the bar across the first row of the region, leaving room for the caption when there is
    /// one, and returns the rows below it.
    /// </summary>
    /// <param name="region">Where to draw; only its first row is used.</param>
    /// <returns>The region below the bar.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        var caption = Caption?.Invoke(Value) ?? "";
        var trackWidth = Math.Max(0, region.Width - (caption.Length == 0 ? 0 : TextWidth.Of(caption) + 1));
        var filled = (int)Math.Round(Fraction * trackWidth);

        region.Write(0, 0, new(FilledCell, filled), Style ?? Theme.Active);
        region.Write(0, filled, new(EmptyCell, Math.Max(0, trackWidth - filled)), Theme.Muted);

        if (caption.Length > 0)
        {
            region.Write(0, trackWidth + 1, caption, Theme.Accent);
        }

        return region.Rows(1, region.Height - 1);
    }
}

/// <summary>
/// A one-cell animation for work of unknown length. It does not run on its own: something has to step
/// it, which keeps the framework free of timers the application did not ask for.
/// </summary>
public sealed class Spinner : IArlecchinoWidget
{
    private static readonly string[] DefaultFrames = ["⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"];

    private int _frame;

    /// <summary>The frames cycled through. Braille dots by default, which most terminals render in one cell.</summary>
    public string[] Frames { get; init; } = DefaultFrames;

    /// <summary>The frame to draw right now.</summary>
    public string Current => Frames[_frame % Frames.Length];

    /// <summary>Moves to the next frame, wrapping at the end.</summary>
    public void Advance() => _frame = (_frame + 1) % Frames.Length;

    /// <summary>Colour to draw in. The theme's informational colour when left alone.</summary>
    public IArlecchinoColor? Style { get; init; }

    /// <summary>
    /// Draws the current frame in the first cell of the region and returns the rows below it. One cell
    /// is all a spinner needs, so hand it the cell it belongs in — <c>region.Rows(0, 1)</c>, a column
    /// split, or whatever the layout gives.
    /// </summary>
    /// <param name="region">Where to draw; the top-left cell is used.</param>
    /// <returns>The region below the spinner's row.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        region.Write(0, 0, Current, Style ?? Theme.Info);

        return region.Rows(1, region.Height - 1);
    }
}
