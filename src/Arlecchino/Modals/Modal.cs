using Arlecchino.Rendering;

namespace Arlecchino.Modals;

/// <summary>
/// A dialog waiting for an answer. Assign one to <c>ArlecchinoState.Modal</c> — while it is open it takes
/// every key, draws over the view and suppresses the hints box.
/// </summary>
public abstract class Modal
{
    /// <summary>Title written into the top edge of the box.</summary>
    public required string Title { get; init; }

    /// <summary>
    /// Where the box was drawn last frame. Filled in by the renderer and used to tell a click on the
    /// dialog from a click outside it.
    /// </summary>
    public SurfaceRegion Box { get; set; }
}
