using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>
/// A reusable piece of a screen: it draws into the region it is handed and holds no coordinates of its own.
/// A widget that also takes keys or the mouse implements <see cref="IArlecchinoInteractiveWidget"/> instead.
/// </summary>
public interface IArlecchinoWidget
{
    /// <summary>
    /// Draws the widget and answers what is left of the region underneath it, so the caller can stack the
    /// next thing. A widget that fills whatever it is given answers with an empty region.
    /// </summary>
    /// <param name="region">Where to draw, in its own coordinates.</param>
    /// <returns>The part of <paramref name="region"/> the widget did not paint, empty when none is.</returns>
    SurfaceRegion Draw(SurfaceRegion region);
}
