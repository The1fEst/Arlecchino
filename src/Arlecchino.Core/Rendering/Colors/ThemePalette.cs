namespace Arlecchino.Rendering.Colors;

/// <summary>
/// The colors behind the roles in <see cref="Theme"/>. Every role has a default, so a palette that
/// overrides two of them is a valid palette — and what it does not override is the framework's own
/// colors, described on <see cref="Arlecchino"/>.
/// </summary>
public sealed class ThemePalette
{
    private static readonly Rgb Crimson = new(0xC9, 0x38, 0x2B);
    private static readonly Rgb Bone = new(0xED, 0xE6, 0xD9);
    private static readonly Rgb Ink = new(0x14, 0x13, 0x17);
    private static readonly Rgb Hairline = new(0x2E, 0x2B, 0x33);
    private static readonly Rgb Ash = new(0x8A, 0x81, 0x89);
    private static readonly Rgb Amber = new(0xD0, 0x8A, 0x2C);

    /// <summary>
    /// The framework's own colors, on whatever background the terminal has. It is what a palette starts
    /// from, and <see cref="Basic"/> is the way back to the sixteen plain colors.
    /// </summary>
    public static ThemePalette Arlecchino { get; } = new();

    /// <summary>
    /// The terminal's own sixteen colors, with nothing exact behind them. It was the default before 2.0.
    /// </summary>
    public static ThemePalette Basic { get; } = new()
    {
        Header = new() { Foreground = TerminalColor.BrightMagenta, Style = TextStyle.Bold },
        TableHeader = new() { Foreground = TerminalColor.BrightBlue, Style = TextStyle.Bold },
        Accent = new() { Foreground = TerminalColor.BrightWhite },
        Info = new() { Foreground = TerminalColor.Cyan },
        Muted = new() { Foreground = TerminalColor.BrightBlack },
        Input = new() { Foreground = TerminalColor.Black, Background = TerminalColor.Blue },
        Caret = new() { Foreground = TerminalColor.Black, Background = TerminalColor.White },
        Selected = new() { Background = TerminalColor.BrightBlack },
        Active = new() { Foreground = TerminalColor.Green },
        ActiveSelected = new() { Foreground = TerminalColor.Black, Background = TerminalColor.Green },
        Warning = new() { Foreground = TerminalColor.Black, Background = TerminalColor.Yellow },
        Error = new() { Foreground = TerminalColor.Black, Background = TerminalColor.Red },
    };

    /// <summary>Ordinary text. The terminal's own foreground and background.</summary>
    public TermColor Default { get; init; } = new();

    /// <summary>Screen titles. Bold crimson.</summary>
    public TermColor Header { get; init; } = new()
    {
        Foreground = TerminalColor.BrightRed,
        ExactForeground = Crimson,
        Style = TextStyle.Bold,
    };

    /// <summary>Column headers. Bold bone.</summary>
    public TermColor TableHeader { get; init; } = new()
    {
        Foreground = TerminalColor.White,
        ExactForeground = Bone,
        Style = TextStyle.Bold,
    };

    /// <summary>Text that stands out without being alarming. Bone.</summary>
    public TermColor Accent { get; init; } = new()
        { Foreground = TerminalColor.BrightWhite, ExactForeground = Bone };

    /// <summary>Borders and structural lines. Ash.</summary>
    public TermColor Info { get; init; } = new()
        { Foreground = TerminalColor.BrightBlack, ExactForeground = Ash };

    /// <summary>Secondary text such as hints and footers. Ash.</summary>
    public TermColor Muted { get; init; } = new()
        { Foreground = TerminalColor.BrightBlack, ExactForeground = Ash };

    /// <summary>The editable part of a text field. Ink on bone.</summary>
    public TermColor Input { get; init; } = new()
    {
        Foreground = TerminalColor.Black,
        ExactForeground = Ink,
        Background = TerminalColor.White,
        ExactBackground = Bone,
    };

    /// <summary>
    /// The symbol the caret stands on, which is written the other way round rather than beside. Bone on
    /// crimson, so the caret is found at a glance without the text shifting to make room for it.
    /// </summary>
    public TermColor Caret { get; init; } = new()
    {
        Foreground = TerminalColor.White,
        ExactForeground = Bone,
        Background = TerminalColor.BrightRed,
        ExactBackground = Crimson,
    };

    /// <summary>The cursor row of an unfocused pane. Bone on the hairline gray.</summary>
    public TermColor Selected { get; init; } = new()
    {
        Foreground = TerminalColor.White,
        ExactForeground = Bone,
        Background = TerminalColor.BrightBlack,
        ExactBackground = Hairline,
    };

    /// <summary>Something switched on or available. Crimson.</summary>
    public TermColor Active { get; init; } = new()
        { Foreground = TerminalColor.BrightRed, ExactForeground = Crimson };

    /// <summary>The cursor row of the focused pane. Ink on ash, so it is never read as a failure.</summary>
    public TermColor ActiveSelected { get; init; } = new()
    {
        Foreground = TerminalColor.Black,
        ExactForeground = Ink,
        Background = TerminalColor.White,
        ExactBackground = Ash,
    };

    /// <summary>Something worth noticing. Ink on amber.</summary>
    public TermColor Warning { get; init; } = new()
    {
        Foreground = TerminalColor.Black,
        ExactForeground = Ink,
        Background = TerminalColor.Yellow,
        ExactBackground = Amber,
    };

    /// <summary>Failures and validation messages. Bone on crimson.</summary>
    public TermColor Error { get; init; } = new()
    {
        Foreground = TerminalColor.BrightWhite,
        ExactForeground = Bone,
        Background = TerminalColor.Red,
        ExactBackground = Crimson,
    };
}
