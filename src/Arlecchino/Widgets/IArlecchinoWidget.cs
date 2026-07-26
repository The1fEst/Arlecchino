using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// A reusable piece of a screen: it draws into the region it is handed and holds no coordinates of
/// its own, so the same widget works in a pane, in a column or across the whole frame. This is the
/// contract every built-in widget answers, and the one to implement for a widget of your own.
///
/// A widget that also takes keys or the mouse implements
/// <see cref="IArlecchinoInteractiveWidget"/> instead.
/// </summary>
public interface IArlecchinoWidget
{
    /// <summary>
    /// Draws the widget. Called once per frame with the region it may paint; anything written outside
    /// is clipped rather than spilled onto a neighbour.
    /// </summary>
    /// <param name="region">Where to draw, in its own coordinates.</param>
    void Draw(SurfaceRegion region);
}
