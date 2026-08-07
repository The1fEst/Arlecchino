using System;
using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Navigation;

/// <summary>
///     The frame every view is drawn inside: a band along the top, a bar along the bottom, whatever a
///     screen of this application always has around it.
///     It is one object for the whole application rather than one per screen, so what it holds outlives the
///     view. A row of tabs keeps its scroll position when a screen is left and come back to, which is the whole
///     reason a header is worth having in one place instead of drawn again by every view.
///     <see cref="Draw" /> is handed the room there is and a delegate that draws the view. Where that delegate
///     is called is where the view goes, and how much it is given is what the view thinks its screen is. A view
///     asks the <see cref="Surface" /> for its content and gets the region the layout left it, so no view has
///     to know it is inside one.
/// </summary>
public interface IArlecchinoLayout
{
    /// <summary>Draws the frame, and the view inside it.</summary>
    /// <param name="frame">Everything there is to draw in.</param>
    /// <param name="body">Draws the view into the region it is given. Call it once.</param>
    void Draw(SurfaceRegion frame, Action<SurfaceRegion> body);

    /// <summary>
    ///     Reads a mouse event before the view does, for a header that answers to one. Keys are not here:
    ///     a key that works on every screen is an <see cref="Arlecchino.Commands.IArlecchinoCommand" />,
    ///     which the framework already had, and two ways to say the same thing is one too many.
    /// </summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns><c>true</c> when the layout took it and the view should not see it.</returns>
    bool HandleMouse(MouseEvent mouse)
    {
        return false;
    }
}
