using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Modals;

/// <summary>
/// A dialog waiting for an answer. Assign one to <c>ArlecchinoState.Modal</c> — while it is open it
/// takes every key, draws over the view and suppresses the hints box.
///
/// A dialog draws itself and reads its own keys. The framework used to do both for it, from a switch over every
/// kind it knew, which meant an application could not add a kind at all. A kind it wrote would match no branch,
/// never be drawn, and swallow every key. Now the kinds the framework brings are nothing more than the first
/// few subclasses, and one an application writes is the next.
/// </summary>
public abstract class Modal
{
    /// <summary>Title written into the top edge of the box.</summary>
    public required string Title { get; init; }

    /// <summary>
    /// Where the box was drawn last frame. Set as the dialog draws itself, and used to tell a click on
    /// it from a click outside.
    /// </summary>
    public SurfaceRegion Box { get; set; }

    /// <summary>Draws it.</summary>
    /// <param name="frame">Where to draw, and the words to draw in.</param>
    public abstract void Draw(ModalFrame frame);

    /// <summary>Reads one key, which reaches no one else while this dialog is on top.</summary>
    /// <param name="frame">The keys to obey, and how to close.</param>
    /// <param name="key">The key that arrived.</param>
    public abstract void Handle(ModalFrame frame, KeyPress key);

    /// <summary>Reads one mouse event. Dialogs that cannot be clicked leave it alone.</summary>
    /// <param name="frame">The keys to obey, and how to close.</param>
    /// <param name="mouse">The event that arrived.</param>
    public virtual void HandleMouse(ModalFrame frame, MouseEvent mouse)
    {
    }
}
