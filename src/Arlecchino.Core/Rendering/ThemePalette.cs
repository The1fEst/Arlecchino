namespace Arlecchino.Rendering;

/// <summary>
/// The colours behind the roles in <see cref="Theme"/>. Every role has a default, so a palette that
/// overrides two of them is a valid palette.
/// </summary>
public sealed class ThemePalette
{
    private static readonly Rgb Crimson = new(0xC9, 0x38, 0x2B);
    private static readonly Rgb Bone = new(0xED, 0xE6, 0xD9);
    private static readonly Rgb Ink = new(0x14, 0x13, 0x17);
    private static readonly Rgb Hairline = new(0x2E, 0x2B, 0x33);
    private static readonly Rgb Ash = new(0x8A, 0x81, 0x89);

    /// <summary>
    /// The framework's own colours — the crimson, bone and ink of the harlequin mask. The background
    /// stays whatever the terminal has, so it sits on a light theme as readily as a dark one; only the
    /// two cursor rows paint behind their text, because a selection has to be visible.
    ///
    /// Each entry carries an exact colour and a palette colour behind it, so a terminal without 24-bit
    /// draws the nearest thing the author picked rather than the nearest thing arithmetic found.
    /// Apply it with <c>UseTheme(ThemePalette.Arlecchino)</c>.
    /// </summary>
    public static ThemePalette Arlecchino { get; } = new()
    {
        Header = new()
        {
            Foreground = TerminalColor.BrightRed,
            ExactForeground = Crimson,
            Style = TextStyle.Bold,
        },
        TableHeader = new()
        {
            Foreground = TerminalColor.White,
            ExactForeground = Bone,
            Style = TextStyle.Bold,
        },
        Accent = new() { Foreground = TerminalColor.BrightWhite, ExactForeground = Bone },
        Info = new() { Foreground = TerminalColor.BrightBlack, ExactForeground = Ash },
        Muted = new() { Foreground = TerminalColor.BrightBlack, ExactForeground = Ash },
        Input = new()
        {
            Foreground = TerminalColor.Black,
            ExactForeground = Ink,
            Background = TerminalColor.White,
            ExactBackground = Bone,
        },
        Selected = new() { Background = TerminalColor.BrightBlack, ExactBackground = Hairline },
        Active = new() { Foreground = TerminalColor.BrightRed, ExactForeground = Crimson },
        ActiveSelected = new()
        {
            Foreground = TerminalColor.Black,
            ExactForeground = Ink,
            Background = TerminalColor.BrightRed,
            ExactBackground = Crimson,
        },
        Warning = new()
        {
            Foreground = TerminalColor.Black,
            ExactForeground = Ink,
            Background = TerminalColor.White,
            ExactBackground = Bone,
        },
        Error = new()
        {
            Foreground = TerminalColor.BrightWhite,
            ExactForeground = Bone,
            Background = TerminalColor.Red,
            ExactBackground = Crimson,
        },
    };

    /// <summary>Ordinary text. Defaults to the terminal's own foreground and background.</summary>
    public TermColor Default { get; init; } = new();

    /// <summary>Screen titles. Bold bright magenta by default.</summary>
    public TermColor Header { get; init; } = new()
        { Foreground = TerminalColor.BrightMagenta, Style = TextStyle.Bold };

    /// <summary>Column headers. Bold bright blue by default.</summary>
    public TermColor TableHeader { get; init; } = new()
        { Foreground = TerminalColor.BrightBlue, Style = TextStyle.Bold };

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
