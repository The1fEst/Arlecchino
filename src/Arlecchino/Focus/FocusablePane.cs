using System;
using Arlecchino.Input;

namespace Arlecchino.Focus;

/// <summary>
/// Wraps delegates as a focusable element, for a view that keeps its logic in methods rather than in
/// objects. That is how the file picker holds its list and the sidebar of places.
/// </summary>
public sealed class FocusablePane : IArlecchinoFocusable
{
    private readonly Func<KeyPress, FocusResult> _handle;
    private readonly Func<MouseEvent, FocusResult>? _handleMouse;

    /// <summary>Creates the element.</summary>
    /// <param name="handle">What to do with a key while focused.</param>
    /// <param name="handleMouse">What to do with a mouse event; omit to ignore the mouse.</param>
    public FocusablePane(Func<KeyPress, FocusResult> handle, Func<MouseEvent, FocusResult>? handleMouse = null)
    {
        _handle = handle;
        _handleMouse = handleMouse;
    }

    /// <summary>Whether this element holds the focus.</summary>
    public bool IsFocused { get; set; }

    /// <summary>Passes the key to the delegate.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>What the delegate decided.</returns>
    public FocusResult Handle(KeyPress key) => _handle(key);

    /// <summary>Passes the mouse event to the delegate, if one was given.</summary>
    /// <param name="mouse">The event, in frame coordinates.</param>
    /// <returns>What the delegate decided, or ignored.</returns>
    public FocusResult HandleMouse(MouseEvent mouse) => _handleMouse?.Invoke(mouse) ?? FocusResult.Ignored;
}
