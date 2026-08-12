using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Modals;

/// <summary>
/// A dialog waiting for an answer, assigned to <c>ArlecchinoState.Modal</c>. It draws itself and reads its
/// own keys, so a kind an application writes is another subclass and nothing more.
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
    public virtual void HandleMouse(ModalFrame frame, MouseEvent mouse) { }
}
