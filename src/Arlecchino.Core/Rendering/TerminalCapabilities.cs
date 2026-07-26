using System;

namespace Arlecchino.Rendering;

/// <summary>
/// What the terminal can actually show. Detected once at startup and consulted by every style when
/// it builds its escape sequence; assign <see cref="Color"/> to override the guess.
/// </summary>
public static class TerminalCapabilities
{
    private static readonly (TerminalColor Color, Rgb Value)[] PaletteColors =
    [
        (TerminalColor.Black, new(0, 0, 0)),
        (TerminalColor.Red, new(205, 0, 0)),
        (TerminalColor.Green, new(0, 205, 0)),
        (TerminalColor.Yellow, new(205, 205, 0)),
        (TerminalColor.Blue, new(0, 0, 238)),
        (TerminalColor.Magenta, new(205, 0, 205)),
        (TerminalColor.Cyan, new(0, 205, 205)),
        (TerminalColor.White, new(229, 229, 229)),
        (TerminalColor.BrightBlack, new(127, 127, 127)),
        (TerminalColor.BrightRed, new(255, 0, 0)),
        (TerminalColor.BrightGreen, new(0, 255, 0)),
        (TerminalColor.BrightYellow, new(255, 255, 0)),
        (TerminalColor.BrightBlue, new(92, 92, 255)),
        (TerminalColor.BrightMagenta, new(255, 0, 255)),
        (TerminalColor.BrightCyan, new(0, 255, 255)),
        (TerminalColor.BrightWhite, new(255, 255, 255)),
    ];

    /// <summary>
    /// How much colour styles may emit. Detected on first use; a terminal that refuses virtual
    /// terminal mode lowers it to <see cref="ColorSupport.None"/> at startup.
    ///
    /// Process-wide, like <see cref="Theme.Palette"/>: one terminal per process is the assumption the
    /// framework makes, and tests that change this share it with everything else running.
    /// </summary>
    public static ColorSupport Color { get; set; } = DetectColor();

    /// <summary>Reads the environment and decides what the terminal can show.</summary>
    /// <returns>The detected level of colour support.</returns>
    public static ColorSupport DetectColor() => DetectColor(
        Environment.GetEnvironmentVariable("NO_COLOR"),
        Environment.GetEnvironmentVariable("TERM"),
        Environment.GetEnvironmentVariable("COLORTERM"),
        Environment.GetEnvironmentVariable("WT_SESSION"));

    /// <summary>
    /// The same decision made from explicit values, which is what makes it testable.
    /// <c>NO_COLOR</c> or <c>TERM=dumb</c> mean no colour at all; <c>truecolor</c>, <c>24bit</c> or a
    /// Windows Terminal session mean 24-bit; everything else falls back to the palette.
    /// </summary>
    /// <param name="noColor">Value of <c>NO_COLOR</c>.</param>
    /// <param name="term">Value of <c>TERM</c>.</param>
    /// <param name="colorTerm">Value of <c>COLORTERM</c>.</param>
    /// <param name="windowsTerminalSession">Value of <c>WT_SESSION</c>.</param>
    /// <returns>The level of colour support those values imply.</returns>
    public static ColorSupport DetectColor(string? noColor, string? term, string? colorTerm, string? windowsTerminalSession)
    {
        if (!string.IsNullOrEmpty(noColor) || string.Equals(term, "dumb", StringComparison.OrdinalIgnoreCase))
        {
            return ColorSupport.None;
        }

        if (string.Equals(colorTerm, "truecolor", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(colorTerm, "24bit", StringComparison.OrdinalIgnoreCase) ||
            !string.IsNullOrEmpty(windowsTerminalSession))
        {
            return ColorSupport.TrueColor;
        }

        return ColorSupport.Palette;
    }

    /// <summary>
    /// Picks the palette colour closest to an exact one. This is the conversion
    /// <see cref="RgbTermColor"/> uses when the terminal cannot do 24-bit, available for your own
    /// rendering.
    /// </summary>
    /// <param name="color">The colour to approximate.</param>
    /// <returns>The nearest of the sixteen ANSI colours.</returns>
    public static TerminalColor NearestPaletteColor(Rgb color)
    {
        var nearest = TerminalColor.Default;
        var nearestDistance = int.MaxValue;

        foreach (var (candidate, value) in PaletteColors)
        {
            var red = color.Red - value.Red;
            var green = color.Green - value.Green;
            var blue = color.Blue - value.Blue;
            var distance = red * red + green * green + blue * blue;

            if (distance >= nearestDistance)
            {
                continue;
            }

            nearest = candidate;
            nearestDistance = distance;
        }

        return nearest;
    }
}
