using System.Text;

namespace Arlecchino.Rendering;

/// <summary>
/// A style built from the sixteen-colour palette. This is what the roles on <see cref="Theme"/> are
/// made of and what chrome should use, because those colours follow the terminal's own theme.
/// </summary>
public sealed class TermColor : IArlecchinoColor
{
    private string? _ansi;
    private ColorSupport _ansiSupport;

    /// <summary>Colour of the glyphs. <see cref="TerminalColor.Default"/> leaves it to the terminal.</summary>
    public TerminalColor Foreground { get; init; } = TerminalColor.Default;

    /// <summary>Colour behind the glyphs. <see cref="TerminalColor.Default"/> leaves it to the terminal.</summary>
    public TerminalColor Background { get; init; } = TerminalColor.Default;

    /// <summary>
    /// An exact colour for the glyphs, used where the terminal can do 24-bit. Elsewhere
    /// <see cref="Foreground"/> is drawn instead, so a palette written in brand colours still says
    /// what it wants on a terminal with sixteen — set both, and the palette entry is the fallback the
    /// author chose rather than the nearest one arithmetic found.
    /// </summary>
    public Rgb? ExactForeground { get; init; }

    /// <summary>The same for what is behind the glyphs, falling back to <see cref="Background"/>.</summary>
    public Rgb? ExactBackground { get; init; }

    /// <summary>Bold, italic, underline and dim, in any combination.</summary>
    public TextStyle Style { get; init; } = TextStyle.None;

    /// <summary>
    /// The escape sequence for this style, built once and rebuilt only if
    /// <see cref="TerminalCapabilities.Color"/> changes. Empty when colour is turned off.
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
            _ansi = _ansiSupport == ColorSupport.None ? "" : BuildAnsi();

            return _ansi;
        }
    }

    /// <summary>Returns <see cref="Ansi"/>, so a style can be appended to a string builder directly.</summary>
    public override string ToString() => Ansi;

    private string BuildAnsi()
    {
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

        var exact = _ansiSupport == ColorSupport.TrueColor;

        builder.Append(exact && ExactForeground is { } foreground
            ? $";38;2;{foreground.Red};{foreground.Green};{foreground.Blue}"
            : $";{ForegroundCode(Foreground)}");

        builder.Append(exact && ExactBackground is { } background
            ? $";48;2;{background.Red};{background.Green};{background.Blue}"
            : $";{BackgroundCode(Background)}");

        return builder.Append('m').ToString();
    }

    internal static int ForegroundCode(TerminalColor color)
    {
        return color switch
        {
            TerminalColor.Default => 39,
            <= TerminalColor.White => 29 + (int)color,
            _ => 81 + (int)color,
        };
    }

    internal static int BackgroundCode(TerminalColor color)
    {
        return color switch
        {
            TerminalColor.Default => 49,
            <= TerminalColor.White => 39 + (int)color,
            _ => 91 + (int)color,
        };
    }
}
