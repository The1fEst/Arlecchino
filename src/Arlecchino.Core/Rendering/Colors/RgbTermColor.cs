using System.Text;
using Arlecchino.Rendering.Terminals;

namespace Arlecchino.Rendering.Colors;

/// <summary>
/// A style built from exact colors, for where the color itself is the point rather than a role in
/// <see cref="Theme"/>. It falls back to the nearest palette color where the terminal cannot do 24-bit.
/// </summary>
public sealed class RgbTermColor : IArlecchinoColor
{
    private ColorSupport _ansiSupport;

    /// <summary>Color of the glyphs, or <c>null</c> to leave the foreground alone.</summary>
    public Rgb? Foreground { get; init; }

    /// <summary>Color behind the glyphs, or <c>null</c> to leave the background alone.</summary>
    public Rgb? Background { get; init; }

    /// <summary>Bold, italic, underline and dim, in any combination.</summary>
    public TextStyle Style { get; init; } = TextStyle.None;

    /// <summary>
    /// The escape sequence for this style: 24-bit where the terminal supports it, the nearest
    /// palette color where it does not, and empty when color is off.
    /// </summary>
    public string Ansi
    {
        get
        {
            if (field is not null && _ansiSupport == TerminalCapabilities.Color)
            {
                return field;
            }

            _ansiSupport = TerminalCapabilities.Color;
            field = BuildAnsi(_ansiSupport);

            return field;
        }
    }

    /// <summary>Writes the style as the sequence that puts it in force.</summary>
    /// <returns><see cref="Ansi"/>.</returns>
    public override string ToString() => Ansi;

    private string BuildAnsi(ColorSupport support)
    {
        if (support == ColorSupport.None)
        {
            return "";
        }

        var builder = new StringBuilder("\e[0");

        if (Style.HasFlag(TextStyle.Bold))
        {
            builder.Append(";1");
        }

        if (Style.HasFlag(TextStyle.Dim))
        {
            builder.Append(";2");
        }

        if (Style.HasFlag(TextStyle.Italic))
        {
            builder.Append(";3");
        }

        if (Style.HasFlag(TextStyle.Underline))
        {
            builder.Append(";4");
        }

        if (Foreground is { } foreground)
        {
            builder.Append(support == ColorSupport.TrueColor
                ? $";38;2;{foreground.Red};{foreground.Green};{foreground.Blue}"
                : $";{TermColor.ForegroundCode(TerminalCapabilities.NearestPaletteColor(foreground))}");
        }

        if (Background is { } background)
        {
            builder.Append(support == ColorSupport.TrueColor
                ? $";48;2;{background.Red};{background.Green};{background.Blue}"
                : $";{TermColor.BackgroundCode(TerminalCapabilities.NearestPaletteColor(background))}");
        }

        return builder.Append('m').ToString();
    }
}
