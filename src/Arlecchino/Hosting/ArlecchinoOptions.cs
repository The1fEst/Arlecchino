using System;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;

namespace Arlecchino.Hosting;

/// <summary>
/// Everything the framework can be told about an application. Configure it in the
/// <c>AddArlecchino</c> callback; most of it also has a builder call.
/// </summary>
public sealed class ArlecchinoOptions
{
    /// <summary>How often the loop may draw. Frames are only composed when something asked for one.</summary>
    public int TargetFramesPerSecond { get; set; } = 60;

    /// <summary>Below this width the view is replaced by a "make the window bigger" notice.</summary>
    public int MinimumWidth { get; set; } = 100;

    /// <summary>Below this height the view is replaced by a "make the window bigger" notice.</summary>
    public int MinimumHeight { get; set; } = 30;

    /// <summary>Cells kept free on the left and right of the content area.</summary>
    public int HorizontalPadding { get; set; } = 2;

    /// <summary>Rows kept free above and below the content area.</summary>
    public int VerticalPadding { get; set; } = 1;

    /// <summary>
    /// Whether to run on the alternate screen, which leaves the user's scrollback untouched on exit.
    /// </summary>
    public bool UseAlternateScreen { get; set; } = true;

    /// <summary>Whether to draw the hints box in the bottom-right corner.</summary>
    public bool ShowHints { get; set; } = true;

    /// <summary>Whether to keep the last row for <c>ArlecchinoState.Output</c>.</summary>
    public bool ShowOutputLine { get; set; } = true;

    /// <summary>
    /// Character that opens the command palette. A character rather than a binding, so it survives a
    /// layout where the key sits elsewhere.
    /// </summary>
    public char CommandPaletteKey { get; set; } = ':';

    /// <summary>How a key press becomes a character on a non-latin layout.</summary>
    public TextInputMode TextInput { get; set; } = TextInputMode.LatinOnly;

    /// <summary>
    /// Whether to report mouse events. Off by default, because with it on the terminal stops handling
    /// selection itself and copying text with the mouse no longer works the way the user expects.
    /// </summary>
    public bool MouseInput { get; set; }

    /// <summary>
    /// Whether pasted text arrives as one block. On by default: without it a paste reads as a burst of
    /// key presses, and a long one can trip validation or a shortcut halfway through.
    /// </summary>
    public bool BracketedPaste { get; set; } = true;

    /// <summary>
    /// How long to wait for the rest of an escape sequence before deciding there is none. Arrows and
    /// function keys arrive as several characters, and over a slow link they do not always arrive
    /// together; this is also the delay a lone <c>Esc</c> costs, so keep it short.
    /// </summary>
    public TimeSpan EscapeTimeout { get; set; } = TimeSpan.FromMilliseconds(25);

    /// <summary>Keys the framework itself reacts to.</summary>
    public ArlecchinoKeymap Keymap { get; set; } = new();

    /// <summary>Colours behind the roles. Installed into <see cref="Rendering.Theme"/> on resolve.</summary>
    public ThemePalette Theme { get; set; } = new();

    /// <summary>Every piece of text the framework draws.</summary>
    public ArlecchinoStrings Strings { get; set; } = new();

    /// <summary>Route shown on the first frame. For a start that depends on state, use a startup.</summary>
    public ViewRoute StartRoute { get; set; } = ViewRoute.None;

    /// <summary>How long the input loop sleeps when no key is waiting.</summary>
    public TimeSpan InputPollInterval { get; set; } = TimeSpan.FromMilliseconds(8);

    /// <summary>
    /// How long a notification stays on the output row. Once it is up the row goes quiet, and the
    /// message is only in the notifications screen.
    /// </summary>
    public TimeSpan NotificationTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// How long a notification stays in the list behind the output row — long enough to go and read
    /// what went past while the screen was busy.
    /// </summary>
    public TimeSpan NotificationLifetime { get; set; } = TimeSpan.FromMinutes(10);
}
