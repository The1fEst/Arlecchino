using System;
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
    ///
    /// The default implementation forwards to <see cref="Place"/> and drops what it answers, so a
    /// widget written against 2.0's shape still satisfies a caller that has not migrated.
    /// </summary>
    /// <param name="region">Where to draw, in its own coordinates.</param>
    [Obsolete(
        "Draw is replaced by Place, which draws the same thing and returns the region left over. " +
        "Draw is removed in 2.0, where Place takes its name.",
        DiagnosticId = ArlecchinoDiagnostics.ObsoleteDraw)]
    void Draw(SurfaceRegion region) => Place(region);

    /// <summary>
    /// Draws the widget and answers what is left of the region underneath it, so the caller can stack
    /// the next thing without knowing how tall this one is.
    ///
    /// A widget that fills whatever it is given — a list, a pane, a tree — returns an empty region. One
    /// that occupies a known number of rows returns the rest, which is what makes
    /// <c>var rest = header.Place(surface.Content);</c> replace a hand-counted <c>SplitTop</c>.
    ///
    /// The default implementation is there so widgets written against 1.0 keep compiling: it draws
    /// through the obsolete <see cref="Draw"/> and conservatively reports nothing left, since a widget
    /// that has not been migrated cannot say how much it used. Override it.
    ///
    /// Both members carry a default so either one on its own satisfies the contract — which means a
    /// widget has to implement at least one of them. A type that implements neither says it draws and
    /// then has nothing to draw with: the two defaults call each other and the frame recurses until
    /// the stack ends.
    /// </summary>
    /// <param name="region">Where to draw, in its own coordinates.</param>
    /// <returns>The part of <paramref name="region"/> the widget did not paint, empty when none is.</returns>
    SurfaceRegion Place(SurfaceRegion region)
    {
#pragma warning disable ARL0001
        Draw(region);
#pragma warning restore ARL0001

        return region.Rows(region.Height, 0);
    }
}
