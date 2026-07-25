using System.Text;

namespace Arlecchino.Rendering;

/// <summary>
/// A style built from the sixteen-colour palette. This is what the roles on <see cref="Theme"/> are
/// made of and what chrome should use, because those colours follow the terminal's own theme.
/// </summary>
public sealed class TermColor : ITermColor
{
    private string? _ansi;
    private ColorSupport _ansiSupport;

    /// <summary>Colour of the glyphs. <see cref="TerminalColor.Default"/> leaves it to the terminal.</summary>
    public TerminalColor Foreground { get; init; } = TerminalColor.Default;

    /// <summary>Colour behind the glyphs. <see cref="TerminalColor.Default"/> leaves it to the terminal.</summary>
    public TerminalColor Background { get; init; } = TerminalColor.Default;

    /// <summary>Bold, italic, underline and dim, in any combination.</summary>
    public FontStyle Style { get; init; } = FontStyle.None;

    /// <summary>
    /// The escape sequence for this style, built once and rebuilt only if
    /// <see cref="TerminalCapabilities.Color"/> changes. Empty when colour is turned off.
    /// </summary>
    public string Ansi
    {
        get
        {
            if (_ansi is null || _ansiSupport != TerminalCapabilities.Color)
            {
                _ansiSupport = TerminalCapabilities.Color;
                _ansi = _ansiSupport == ColorSupport.None ? "" : BuildAnsi();
            }

            return _ansi;
        }
    }

    /// <summary>Returns <see cref="Ansi"/>, so a style can be appended to a string builder directly.</summary>
    public override string ToString() => Ansi;

    private string BuildAnsi()
    {
        var builder = new StringBuilder("\e[0");

        if (Style.HasFlag(FontStyle.Bold))
        {
            builder.Append(";1");
        }

        if (Style.HasFlag(FontStyle.Dim))
        {
            builder.Append(";2");
        }

        if (Style.HasFlag(FontStyle.Italic))
        {
            builder.Append(";3");
        }

        if (Style.HasFlag(FontStyle.Underline))
        {
            builder.Append(";4");
        }

        builder.Append($";{ForegroundCode(Foreground)};{BackgroundCode(Background)}m");
        return builder.ToString();
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
