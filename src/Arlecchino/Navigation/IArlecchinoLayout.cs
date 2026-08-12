using System;
using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Navigation;

/// <summary>
/// The frame every view is drawn inside, from one object that outlives the views. A view asks the
/// <see cref="Surface"/> for its content and gets the region the layout left it, so it never knows.
/// </summary>
public interface IArlecchinoLayout
{
    /// <summary>Draws the frame, and the view inside it.</summary>
    /// <param name="frame">Everything there is to draw in.</param>
    /// <param name="body">Draws the view into the region it is given. Call it once.</param>
    void Draw(SurfaceRegion frame, Action<SurfaceRegion> body);

    /// <summary>
    /// Reads a mouse event before the view does, for a header that answers to one. A key that works on every
    /// screen is an <see cref="Arlecchino.Commands.IArlecchinoCommand"/> instead.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns><c>true</c> when the layout took it and the view should not see it.</returns>
    bool HandleMouse(MouseEvent mouse)
    {
        return false;
    }
}
