using System.Text;
using Arlecchino.Rendering.Terminals;

namespace Arlecchino.Rendering.Colors;

/// <summary>
/// A style built from exact colours. Use it where the colour itself is the point — a swatch, a
/// chart, syntax highlighting — and keep chrome on <see cref="Theme"/>, which follows the terminal
/// theme. Falls back to the nearest palette colour when the terminal cannot do 24-bit.
/// </summary>
public sealed class RgbTermColor : IArlecchinoColor
{
    private string? _ansi;
    private ColorSupport _ansiSupport;

    /// <summary>Colour of the glyphs, or <c>null</c> to leave the foreground alone.</summary>
    public Rgb? Foreground { get; init; }

    /// <summary>Colour behind the glyphs, or <c>null</c> to leave the background alone.</summary>
    public Rgb? Background { get; init; }

    /// <summary>Bold, italic, underline and dim, in any combination.</summary>
    public TextStyle Style { get; init; } = TextStyle.None;

    /// <summary>
    /// The escape sequence for this style: 24-bit where the terminal supports it, the nearest
    /// palette colour where it does not, and empty when colour is off.
    /// </summary>
    public string Ansi
    {
        get
        {
            if (_ansi is not null && _ansiSupport == TerminalCapabilities.Color)
            {
                return _ansi;
            }

            _ansiSupport = TerminalCapabilities.Color;
            _ansi = BuildAnsi(_ansiSupport);

            return _ansi;
        }
    }

    /// <summary>Returns <see cref="Ansi"/>.</summary>
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
