using System;
using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Modals;

/// <summary>
/// Following a click or a drag along a track. Pressing anywhere on it jumps the value there rather than
/// nudging it, so the track can be aimed at.
/// </summary>
internal static class Tracking
{
    /// <summary>Sets a value from where the pointer landed on a track.</summary>
    /// <param name="track">Where the track was drawn.</param>
    /// <param name="mouse">The event that arrived.</param>
    /// <param name="apply">What to do with how far along it landed.</param>
    public static void Follow(SurfaceRegion track, MouseEvent mouse, Action<decimal> apply)
    {
        if (track.IsEmpty ||
            mouse.Action is not (MouseAction.Pressed or MouseAction.Moved) ||
            !track.Contains(mouse.Row, mouse.Column))
        {
            return;
        }

        var (_, column) = track.ToLocal(mouse.Row, mouse.Column);

        apply(track.Width <= 1 ? 0m : (decimal)column / (track.Width - 1));
    }
}
