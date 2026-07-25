namespace Arlecchino.Rendering;

/// <summary>
/// The colours behind the roles in <see cref="Theme"/>. Every role has a default, so a palette that
/// overrides two of them is a valid palette.
/// </summary>
public sealed class ThemePalette
{
    /// <summary>Ordinary text. Defaults to the terminal's own foreground and background.</summary>
    public TermColor Default { get; init; } = new();

    /// <summary>Screen titles. Bold bright magenta by default.</summary>
    public TermColor Header { get; init; } = new()
        { Foreground = TerminalColor.BrightMagenta, Style = FontStyle.Bold };

    /// <summary>Column headers. Bold bright blue by default.</summary>
    public TermColor TableHeader { get; init; } = new()
        { Foreground = TerminalColor.BrightBlue, Style = FontStyle.Bold };

    /// <summary>Text that stands out without being alarming. Bright white by default.</summary>
    public TermColor Accent { get; init; } = new() { Foreground = TerminalColor.BrightWhite };

    /// <summary>Borders and structural lines. Cyan by default.</summary>
    public TermColor Info { get; init; } = new() { Foreground = TerminalColor.Cyan };

    /// <summary>Secondary text such as hints and footers. Grey by default.</summary>
    public TermColor Muted { get; init; } = new() { Foreground = TerminalColor.BrightBlack };

    /// <summary>The editable part of a text field. Black on blue by default.</summary>
    public TermColor Input { get; init; } = new()
        { Foreground = TerminalColor.Black, Background = TerminalColor.Blue };

    /// <summary>The cursor row of an unfocused pane. A grey background by default.</summary>
    public TermColor Selected { get; init; } = new() { Background = TerminalColor.BrightBlack };

    /// <summary>Something switched on or available. Green by default.</summary>
    public TermColor Active { get; init; } = new() { Foreground = TerminalColor.Green };

    /// <summary>The cursor row of the focused pane. Black on green by default.</summary>
    public TermColor ActiveSelected { get; init; } = new()
        { Foreground = TerminalColor.Black, Background = TerminalColor.Green };

    /// <summary>Something worth noticing. Black on yellow by default.</summary>
    public TermColor Warning { get; init; } = new()
        { Foreground = TerminalColor.Black, Background = TerminalColor.Yellow };

    /// <summary>Failures and validation messages. Black on red by default.</summary>
    public TermColor Error { get; init; } = new()
        { Foreground = TerminalColor.Black, Background = TerminalColor.Red };
}
