using System;
using Arlecchino.Input;

namespace Arlecchino.Hosting;

/// <summary>
/// Every key the framework itself reacts to, in one place. Replace the whole map through
/// <c>UseKeymap</c>; each binding also relabels itself in the hints box and the palette.
/// </summary>
public sealed class ArlecchinoKeymap
{
    /// <summary>Goes back in the history. <c>Alt+←</c> by default.</summary>
    public KeyBinding Back { get; init; } = new(ConsoleKey.LeftArrow, ConsoleModifiers.Alt);

    /// <summary>Retraces a step back. <c>Alt+→</c> by default.</summary>
    public KeyBinding Forward { get; init; } = new(ConsoleKey.RightArrow, ConsoleModifiers.Alt);

    /// <summary>Accepts a dialog, opens a field, activates a row. <c>Enter</c> by default.</summary>
    public KeyBinding Confirm { get; init; } = new(ConsoleKey.Enter);

    /// <summary>Dismisses a dialog or leaves a screen. <c>Esc</c> by default.</summary>
    public KeyBinding Cancel { get; init; } = new(ConsoleKey.Escape);

    /// <summary>Moves to the next pane, segment or channel. <c>Tab</c> by default.</summary>
    public KeyBinding NextField { get; init; } = new(ConsoleKey.Tab);

    /// <summary>Moves to the previous one. <c>Shift+Tab</c> by default.</summary>
    public KeyBinding PreviousField { get; init; } = new(ConsoleKey.Tab, ConsoleModifiers.Shift);

    /// <summary>Moves the cursor up, or steps a number up. <c>↑</c> by default.</summary>
    public KeyBinding MoveUp { get; init; } = new(ConsoleKey.UpArrow);

    /// <summary>Moves the cursor down, or steps a number down. <c>↓</c> by default.</summary>
    public KeyBinding MoveDown { get; init; } = new(ConsoleKey.DownArrow);

    /// <summary>Moves left: a slider down, a tree node closed, out of a folder. <c>←</c> by default.</summary>
    public KeyBinding MoveLeft { get; init; } = new(ConsoleKey.LeftArrow);

    /// <summary>Moves right: a slider up, a tree node open, into a folder. <c>→</c> by default.</summary>
    public KeyBinding MoveRight { get; init; } = new(ConsoleKey.RightArrow);

    /// <summary>A large step up, or a page of rows. <c>PgUp</c> by default.</summary>
    public KeyBinding JumpUp { get; init; } = new(ConsoleKey.PageUp);

    /// <summary>A large step down, or a page of rows. <c>PgDn</c> by default.</summary>
    public KeyBinding JumpDown { get; init; } = new(ConsoleKey.PageDown);

    /// <summary>Goes to the start of a list or the minimum of a range. <c>Home</c> by default.</summary>
    public KeyBinding First { get; init; } = new(ConsoleKey.Home);

    /// <summary>Goes to the end of a list or the maximum of a range. <c>End</c> by default.</summary>
    public KeyBinding Last { get; init; } = new(ConsoleKey.End);

    /// <summary>Deletes: a character, a filter, a typed segment, a field value. <c>Backspace</c> by default.</summary>
    public KeyBinding Erase { get; init; } = new(ConsoleKey.Backspace);

    /// <summary>Deletes the character after the caret. <c>Delete</c> by default.</summary>
    public KeyBinding DeleteForward { get; init; } = new(ConsoleKey.Delete);

    /// <summary>Deletes the word before the caret. <c>Ctrl+Backspace</c> by default.</summary>
    public KeyBinding EraseWord { get; init; } = new(ConsoleKey.Backspace, ConsoleModifiers.Control);

    /// <summary>Deletes everything before the caret. <c>Ctrl+U</c> by default, as in a shell.</summary>
    public KeyBinding EraseToStart { get; init; } = new(ConsoleKey.U, ConsoleModifiers.Control);

    /// <summary>Moves the caret to the previous word. <c>Ctrl+←</c> by default.</summary>
    public KeyBinding WordLeft { get; init; } = new(ConsoleKey.LeftArrow, ConsoleModifiers.Control);

    /// <summary>Moves the caret past the next word. <c>Ctrl+→</c> by default.</summary>
    public KeyBinding WordRight { get; init; } = new(ConsoleKey.RightArrow, ConsoleModifiers.Control);

    /// <summary>
    /// Copies the field being edited to the clipboard. <c>Ctrl+Insert</c> by default, because
    /// <c>Ctrl+C</c> is how the user stops the application.
    /// </summary>
    public KeyBinding Copy { get; init; } = new(ConsoleKey.Insert, ConsoleModifiers.Control);

    /// <summary>Shows or hides the log overlay. <c>Ctrl+L</c> by default.</summary>
    public KeyBinding ToggleLog { get; init; } = new(ConsoleKey.L, ConsoleModifiers.Control);

    /// <summary>Marks a row or flips a toggle. <c>Space</c> by default.</summary>
    public KeyBinding Mark { get; init; } = new(ConsoleKey.Spacebar);

    /// <summary>Picks the folder currently open in the file picker. <c>Ctrl+Enter</c> by default.</summary>
    public KeyBinding PickCurrentFolder { get; init; } = new(ConsoleKey.Enter, ConsoleModifiers.Control);
}
