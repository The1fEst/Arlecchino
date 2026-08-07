using System;
using Arlecchino.Input;

namespace Arlecchino.Hosting;

/// <summary>
/// Every key the framework itself reacts to, in one place. Replace the whole map through
/// <c>UseKeymap</c>, or one binding at a time with <c>with</c>; each binding also relabels itself in
/// the hints box and the palette.
/// </summary>
public sealed record ArlecchinoKeymap
{
    /// <summary>Goes back in the history. <c>Alt+←</c> by default.</summary>
    public KeyBinding Back { get; init; } = new(ConsoleKey.LeftArrow, KeyModifiers.Alt);

    /// <summary>Retraces a step back. <c>Alt+→</c> by default.</summary>
    public KeyBinding Forward { get; init; } = new(ConsoleKey.RightArrow, KeyModifiers.Alt);

    /// <summary>Accepts a dialog, opens a field, activates a row. <c>Enter</c> by default.</summary>
    public KeyBinding Confirm { get; init; } = new(ConsoleKey.Enter);

    /// <summary>Dismisses a dialog or leaves a screen. <c>Esc</c> by default.</summary>
    public KeyBinding Cancel { get; init; } = new(ConsoleKey.Escape);

    /// <summary>Moves to the next pane, segment or channel. <c>Tab</c> by default.</summary>
    public KeyBinding NextField { get; init; } = new(ConsoleKey.Tab);

    /// <summary>Moves to the previous one. <c>Shift+Tab</c> by default.</summary>
    public KeyBinding PreviousField { get; init; } = new(ConsoleKey.Tab, KeyModifiers.Shift);

    /// <summary>Moves the cursor up, or steps a number up. <c>↑</c> by default.</summary>
    public KeyBinding MoveUp { get; init; } = new(ConsoleKey.UpArrow);

    /// <summary>Moves the cursor down, or steps a number down. <c>↓</c> by default.</summary>
    public KeyBinding MoveDown { get; init; } = new(ConsoleKey.DownArrow);

    /// <summary>Moves left: a slider down, a tree node closed, out of a folder. <c>←</c> by default.</summary>
    public KeyBinding MoveLeft { get; init; } = new(ConsoleKey.LeftArrow);

    /// <summary>Moves right: a slider up, a tree node open, into a folder. <c>→</c> by default.</summary>
    public KeyBinding MoveRight { get; init; } = new(ConsoleKey.RightArrow);

    /// <summary>A long stride upward, or a page of rows. <c>PgUp</c> by default.</summary>
    public KeyBinding JumpUp { get; init; } = new(ConsoleKey.PageUp);

    /// <summary>A long stride downward, or a page of rows. <c>PgDn</c> by default.</summary>
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
    public KeyBinding EraseWord { get; init; } = new(ConsoleKey.Backspace, KeyModifiers.Control);

    /// <summary>Deletes everything before the caret. <c>Ctrl+U</c> by default, as in a shell.</summary>
    public KeyBinding EraseToStart { get; init; } = new(ConsoleKey.U, KeyModifiers.Control);

    /// <summary>Moves the caret to the previous word. <c>Ctrl+←</c> by default.</summary>
    public KeyBinding WordLeft { get; init; } = new(ConsoleKey.LeftArrow, KeyModifiers.Control);

    /// <summary>Moves the caret past the next word. <c>Ctrl+→</c> by default.</summary>
    public KeyBinding WordRight { get; init; } = new(ConsoleKey.RightArrow, KeyModifiers.Control);

    /// <summary>
    /// Copies what is being edited to the clipboard. Two combinations, because the habits differ:
    /// <c>Ctrl+Insert</c> and <c>Ctrl+Shift+C</c>. Plain <c>Ctrl+C</c> is left alone — that is how the
    /// user stops the application.
    /// </summary>
    public KeyBinding Copy { get; init; } = new KeyBinding(ConsoleKey.Insert, KeyModifiers.Control)
        .AddAlternative(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);

    /// <summary>Shows or hides the log overlay. <c>Ctrl+L</c> by default.</summary>
    public KeyBinding ToggleLog { get; init; } = new(ConsoleKey.L, KeyModifiers.Control);

    /// <summary>Opens the screen listing what the application has said lately.</summary>
    public KeyBinding Notifications { get; init; } = new(ConsoleKey.N, KeyModifiers.Control);

    /// <summary>Opens the screen listing every key. <c>F1</c> by default.</summary>
    public KeyBinding Help { get; init; } = new(ConsoleKey.F1);

    /// <summary>
    /// Accepts a dialog where <c>Enter</c> means something else — the multi-line text area, where it
    /// starts a new line. <c>Ctrl+Enter</c> by default.
    /// </summary>
    public KeyBinding Submit { get; init; } = new(ConsoleKey.Enter, KeyModifiers.Control);

    /// <summary>Marks a row or flips a toggle. <c>Space</c> by default.</summary>
    public KeyBinding Mark { get; init; } = new(ConsoleKey.Spacebar);

    /// <summary>Picks the folder open in the file picker. <c>Ctrl+Enter</c> by default.</summary>
    public KeyBinding PickCurrentFolder { get; init; } = new(ConsoleKey.Enter, KeyModifiers.Control);

    /// <summary>
    /// The whole map with one modifier put in place of another. A keyboard that cannot send a modifier
    /// makes every key built on it unreachable, and rewriting thirty bindings by hand to say so is how
    /// an application ends up with twenty-eight of them rewritten. A Mac terminal is the case this was
    /// written for: Option is spoken for by the characters it types, so <c>Alt</c> never arrives and
    /// Command is what that keyboard has going spare.
    /// </summary>
    /// <param name="from">The modifier to take out.</param>
    /// <param name="to">The modifier to put in its place.</param>
    /// <returns>A new map, with every binding rewritten.</returns>
    /// <example>
    /// <code>
    /// builder.UseKeymap(new ArlecchinoKeymap().Replacing(KeyModifiers.Alt, KeyModifiers.Super));
    /// </code>
    /// </example>
    public ArlecchinoKeymap Replacing(KeyModifiers from, KeyModifiers to) => new()
    {
        Back = Back.Replacing(from, to),
        Forward = Forward.Replacing(from, to),
        Confirm = Confirm.Replacing(from, to),
        Cancel = Cancel.Replacing(from, to),
        NextField = NextField.Replacing(from, to),
        PreviousField = PreviousField.Replacing(from, to),
        MoveUp = MoveUp.Replacing(from, to),
        MoveDown = MoveDown.Replacing(from, to),
        MoveLeft = MoveLeft.Replacing(from, to),
        MoveRight = MoveRight.Replacing(from, to),
        JumpUp = JumpUp.Replacing(from, to),
        JumpDown = JumpDown.Replacing(from, to),
        First = First.Replacing(from, to),
        Last = Last.Replacing(from, to),
        Erase = Erase.Replacing(from, to),
        DeleteForward = DeleteForward.Replacing(from, to),
        EraseWord = EraseWord.Replacing(from, to),
        EraseToStart = EraseToStart.Replacing(from, to),
        WordLeft = WordLeft.Replacing(from, to),
        WordRight = WordRight.Replacing(from, to),
        Copy = Copy.Replacing(from, to),
        ToggleLog = ToggleLog.Replacing(from, to),
        Notifications = Notifications.Replacing(from, to),
        Help = Help.Replacing(from, to),
        Submit = Submit.Replacing(from, to),
        Mark = Mark.Replacing(from, to),
        PickCurrentFolder = PickCurrentFolder.Replacing(from, to),
    };
}
