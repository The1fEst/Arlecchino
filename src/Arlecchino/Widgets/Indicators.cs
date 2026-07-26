using System;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>A filled bar showing how far along something is, with an optional readout beside it.</summary>
public sealed class ProgressBar
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

    /// <summary>Draws the bar across the region, leaving room for the caption when there is one.</summary>
    /// <param name="region">Where to draw; only its first row is used.</param>
    /// <param name="style">Colour of the filled part. Defaults to the theme's active colour.</param>
    public void Draw(SurfaceRegion region, ITermColor? style = null)
    {
        if (region.IsEmpty)
        {
            return;
        }

        var caption = Caption?.Invoke(Value) ?? "";
        var trackWidth = Math.Max(0, region.Width - (caption.Length == 0 ? 0 : TextWidth.Of(caption) + 1));
        var filled = (int)Math.Round(Fraction * trackWidth);

        region.Write(0, 0, new(FilledCell, filled), style ?? Theme.Active);
        region.Write(0, filled, new(EmptyCell, Math.Max(0, trackWidth - filled)), Theme.Muted);

        if (caption.Length > 0)
        {
            region.Write(0, trackWidth + 1, caption, Theme.Accent);
        }
    }
}

/// <summary>
/// A one-cell animation for work of unknown length. It does not run on its own: something has to step
/// it, which keeps the framework free of timers the application did not ask for.
/// </summary>
public sealed class Spinner
{
    private static readonly string[] DefaultFrames = ["⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"];

    private int _frame;

    /// <summary>The frames cycled through. Braille dots by default, which most terminals render in one cell.</summary>
    public string[] Frames { get; init; } = DefaultFrames;

    /// <summary>The frame to draw right now.</summary>
    public string Current => Frames[_frame % Frames.Length];

    /// <summary>Moves to the next frame, wrapping at the end.</summary>
    public void Advance() => _frame = (_frame + 1) % Frames.Length;

    /// <summary>Draws the current frame at one spot.</summary>
    /// <param name="region">The region the position is relative to.</param>
    /// <param name="row">Row within the region.</param>
    /// <param name="column">Column within the region.</param>
    /// <param name="style">Colour to draw in. Defaults to the theme's informational colour.</param>
    public void Draw(SurfaceRegion region, int row, int column, ITermColor? style = null) =>
        region.Write(row, column, Current, style ?? Theme.Info);
}
