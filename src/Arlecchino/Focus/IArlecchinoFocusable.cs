using Arlecchino.Input;
using Arlecchino.Navigation;

namespace Arlecchino.Focus;

/// <summary>
/// What a focusable element did with an event: whether it claimed it, and whether that means going
/// somewhere.
/// </summary>
/// <param name="WasHandled">Whether the element claimed the event.</param>
/// <param name="Route">Where to navigate, if anywhere.</param>
public readonly record struct FocusResult(bool WasHandled, ViewRoute Route)
{
    /// <summary>The event was not mine; whoever asked should keep looking.</summary>
    public static FocusResult Ignored => new(false, ViewRoute.None);

    /// <summary>The event was mine and nothing else should see it.</summary>
    public static FocusResult Handled => new(true, ViewRoute.None);

    /// <summary>The event was mine and the screen should navigate.</summary>
    /// <param name="route">Where to go.</param>
    /// <returns>The result to return.</returns>
    public static FocusResult Navigate(ViewRoute route) => new(true, route);
}

/// <summary>
/// Something inside a view that can hold the cursor: a pane, a list, a form. Put them in a
/// <see cref="FocusRing"/> and the cycling, the routing and the mouse are handled for you.
/// </summary>
public interface IArlecchinoFocusable
{
    /// <summary>Set by the ring. Draw the element differently while it is false.</summary>
    bool IsFocused { get; set; }

    /// <summary>Handles a key while this element has the focus.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>What was done with it.</returns>
    FocusResult Handle(KeyPress key);

    /// <summary>
    /// Handles a mouse event wherever it landed. Claiming one also moves the focus here, so a click
    /// selects the pane it hit.
    /// </summary>
    /// <param name="mouse">The event, in frame coordinates.</param>
    /// <returns>What was done with it.</returns>
    FocusResult HandleMouse(MouseEvent mouse) => FocusResult.Ignored;

    /// <summary>
    /// Moves the focus one step inside this element, for something that holds focusable parts of its
    /// own — a <see cref="FocusRing"/> nested in another one, or a widget made of several fields.
    /// Answering <c>false</c> hands the step back to the surrounding ring, which is what lets
    /// <c>Tab</c> walk into a composite element, through its parts and out the far side.
    ///
    /// An element with nothing inside it leaves this alone: the default says the step was not taken,
    /// and the ring moves to the next element as it always has.
    /// </summary>
    /// <param name="direction">Which way the focus is going.</param>
    /// <returns>Whether the focus moved inside this element.</returns>
    bool MoveFocus(FocusDirection direction) => false;

    /// <summary>
    /// Keys this element reacts to while it holds the focus. The screen collects them down the chain
    /// of focus, so the hints box follows the cursor instead of listing the same keys everywhere.
    /// </summary>
    /// <returns>Pairs of key and description, empty when the element has nothing to say.</returns>
    (string Key, string Description)[] Hints() => [];
}
