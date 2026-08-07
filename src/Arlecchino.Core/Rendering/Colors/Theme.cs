using System;
using Arlecchino.Atoms;

namespace Arlecchino.Rendering.Colors;

/// <summary>
/// The palette in use, reachable from anywhere that draws. Views pick a role here rather than a
/// color, so swapping <see cref="Palette"/> restyles the whole application, chrome included.
/// </summary>
public static class Theme
{
    private static readonly string SwappingPalette = FrameMembers.Of(typeof(Theme), nameof(Palette));

    /// <summary>
    /// The colors behind the roles. Assigned from <c>ArlecchinoOptions</c> when the container resolves
    /// them; set it directly when drawing without a host.
    ///
    /// This is process-wide on purpose — it is what lets a view write <c>Theme.Header</c> with no
    /// plumbing — so two hosts in one process share one palette, and the last one built wins.
    ///
    /// A frame reads it, so it is swapped on the drawing thread and asks for a frame by itself. Hand the
    /// change over with <see cref="FrameThread.Post"/> from anywhere else.
    /// </summary>
    /// <exception cref="InvalidOperationException">Assigned from off the drawing thread.</exception>
    public static ThemePalette Palette
    {
        get;

        set
        {
            FrameThread.Verify(SwappingPalette);
            field = value;
            AtomChanges.NotifyWritten();
        }
    } = new();

    /// <summary>Ordinary text on the terminal's own background.</summary>
    public static TermColor Default => Palette.Default;

    /// <summary>Screen titles.</summary>
    public static TermColor Header => Palette.Header;

    /// <summary>Column headers of a table.</summary>
    public static TermColor TableHeader => Palette.TableHeader;

    /// <summary>Text that should stand out from ordinary text.</summary>
    public static TermColor Accent => Palette.Accent;

    /// <summary>Box borders and other structural lines.</summary>
    public static TermColor Info => Palette.Info;

    /// <summary>Secondary text: hints, footers, disabled rows.</summary>
    public static TermColor Muted => Palette.Muted;

    /// <summary>The editable part of a text field.</summary>
    public static TermColor Input => Palette.Input;

    /// <summary>The row under the cursor while its pane is not focused.</summary>
    public static TermColor Selected => Palette.Selected;

    /// <summary>Something switched on, such as an enabled action.</summary>
    public static TermColor Active => Palette.Active;

    /// <summary>The row under the cursor in the focused pane.</summary>
    public static TermColor ActiveSelected => Palette.ActiveSelected;

    /// <summary>Something the user should notice, such as the output line.</summary>
    public static TermColor Warning => Palette.Warning;

    /// <summary>Validation messages and failures.</summary>
    public static TermColor Error => Palette.Error;
}
