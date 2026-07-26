using System;

namespace Arlecchino.Input;

/// <summary>What the mouse did.</summary>
public enum MouseAction : byte
{
    /// <summary>A button went down.</summary>
    Pressed,

    /// <summary>A button came back up.</summary>
    Released,

    /// <summary>The pointer moved with a button held — a drag.</summary>
    Moved,

    /// <summary>The wheel turned away from the user.</summary>
    ScrolledUp,

    /// <summary>The wheel turned towards the user.</summary>
    ScrolledDown,
}

/// <summary>Which button an event belongs to.</summary>
public enum MouseButton : byte
{
    /// <summary>No button, which is what wheel events carry.</summary>
    None,

    /// <summary>Left button.</summary>
    Left,

    /// <summary>Middle button.</summary>
    Middle,

    /// <summary>Right button.</summary>
    Right,
}

/// <summary>
/// A mouse report from the terminal. Coordinates are frame cells — the same ones
/// <see cref="Rendering.Surface.WriteAt"/> and <see cref="Rendering.SurfaceRegion.Contains"/> use, so
/// hit-testing is comparing numbers.
/// </summary>
/// <param name="Action">What the mouse did.</param>
/// <param name="Button">Which button, or <see cref="MouseButton.None"/> for the wheel.</param>
/// <param name="Row">Zero-based row in the frame.</param>
/// <param name="Column">Zero-based column in the frame.</param>
/// <param name="Modifiers">Modifiers held at the time.</param>
public readonly record struct MouseEvent(
    MouseAction Action,
    MouseButton Button,
    int Row,
    int Column,
    ConsoleModifiers Modifiers)
{
    /// <summary>Whether this is the left button going down — the usual "click" test.</summary>
    public bool IsLeftClick => Action == MouseAction.Pressed && Button == MouseButton.Left;

    /// <summary>Whether this is a wheel event in either direction.</summary>
    public bool IsScroll => Action is MouseAction.ScrolledUp or MouseAction.ScrolledDown;
}
