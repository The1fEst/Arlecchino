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
    /// Draws the widget and answers what is left of the region underneath it, so the caller can stack
    /// the next thing without knowing how tall this one is. Called once per frame with the region it
    /// may paint; anything written outside is clipped rather than spilled onto a neighbour.
    ///
    /// A widget that fills whatever it is given — a list, a pane, a tree — returns an empty region. One
    /// that occupies a known number of rows returns the rest, which is what makes
    /// <c>var rest = header.Draw(surface.Content);</c> replace a hand-counted <c>SplitTop</c>.
    /// </summary>
    /// <param name="region">Where to draw, in its own coordinates.</param>
    /// <returns>The part of <paramref name="region"/> the widget did not paint, empty when none is.</returns>
    SurfaceRegion Draw(SurfaceRegion region);
}
