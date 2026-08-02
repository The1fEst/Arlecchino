using System;
using Arlecchino.Input;
using Arlecchino.Rendering;

namespace Arlecchino.Modals;

/// <summary>
/// A dialog the application draws and drives itself.
///
/// The dialogs above this one are the framework's: it knows what a number looks like and what a
/// choice looks like, and an application that wants one gets it for a line of code. An application
/// with a look of its own wants neither, and the answer is not for it to keep a dialog of its own
/// beside <c>Modal</c> — two things that both take every key are two things that will disagree about
/// which of them has it. This is the same slot, the same stack and the same rules; only the drawing
/// and the keys are handed back.
///
/// Close it the way any dialog is closed, with <c>ArlecchinoState.CloseModal</c>.
/// </summary>
public abstract class CustomModal : Modal
{
    /// <summary>Draws it, over whatever the view has already drawn.</summary>
    /// <param name="screen">The whole content area, which the dialog places itself in.</param>
    public abstract void Draw(SurfaceRegion screen);

    /// <summary>
    /// Takes a key. Every key reaches this while the dialog is open, and none of them reach the view
    /// behind it — a dialog that let the view move underneath it would be asking about one thing while
    /// something else happened.
    /// </summary>
    /// <param name="key">The key that arrived.</param>
    public abstract void Handle(ConsoleKeyInfo key);

    /// <summary>Takes a click. Clicks outside the dialog are not offered.</summary>
    /// <param name="mouse">The event that arrived.</param>
    public virtual void HandleMouse(MouseEvent mouse)
    {
    }
}
