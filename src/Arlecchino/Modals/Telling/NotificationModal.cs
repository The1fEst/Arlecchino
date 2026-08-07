using System;
using System.Collections.Generic;
using Arlecchino.Diagnostics;
using Arlecchino.Rendering;

using Arlecchino.Input;

namespace Arlecchino.Modals.Telling;

/// <summary>
/// One notification, read in full. The output row and the notifications screen have one line each to give a
/// message, which is not enough for the errors a copy collected or the output of a command. Opening the entry
/// shows the whole of it, and offers whatever the entry said could be done about it.
///
/// The notifications screen opens this itself, so an application only fills in
/// <see cref="Notification.Detail"/> and <see cref="Notification.Actions"/> when it raises the entry.
/// </summary>
public sealed class NotificationModal : Modal
{
    /// <summary>The entry being read.</summary>
    public required Notification Entry { get; init; }

    /// <summary>The whole text, wrapped by the renderer to the width of the box.</summary>
    public string Text => Entry.Whole();

    /// <summary>What can be done about it. Empty for a message that is only to be read.</summary>
    public IReadOnlyList<NotificationAction> Actions => Entry.Actions;

    /// <summary>Which action is selected, moved with the left and right keys.</summary>
    public int Index { get; set; }

    /// <summary>
    /// Where each action was drawn last frame, filled in by the renderer, so a click can be resolved
    /// to the action under it.
    /// </summary>
    public IReadOnlyList<SurfaceRegion> Chips { get; set; } = [];

    /// <summary>Moves the selection along the actions, stopping at both ends.</summary>
    /// <param name="delta">How far to move; negative goes left.</param>
    public void Move(int delta) => Index = Math.Clamp(Index + delta, 0, Math.Max(0, Actions.Count - 1));

    /// <summary>Runs the selected action, if there is one.</summary>
    public void Run()
    {
        if (Actions.Count > 0)
        {
            Actions[Math.Clamp(Index, 0, Actions.Count - 1)].Run();
        }
    }

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) => frame.Tells.Notification(this);

    /// <summary>
    /// The arrows walk its actions, confirming runs the one selected and cancelling only closes. The
    /// dialog is closed before the action runs, so an action is free to open one of its own.
    /// </summary>
    /// <param name="frame">How to close.</param>
    /// <param name="key">The key that arrived.</param>
    public override void Handle(ModalFrame frame, KeyPress key)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.Keymap.MoveLeft.Matches(key))
        {
            Move(-1);

            return;
        }

        if (frame.Keymap.MoveRight.Matches(key))
        {
            Move(1);

            return;
        }

        if (frame.Keymap.Cancel.Matches(key))
        {
            frame.Close();

            return;
        }

        if (!frame.Keymap.Confirm.Matches(key))
        {
            return;
        }

        frame.Close();
        Run();
    }

    /// <inheritdoc/>
    public override void HandleMouse(ModalFrame frame, MouseEvent mouse)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (mouse.Action != MouseAction.Pressed || mouse.Button != MouseButton.Left)
        {
            return;
        }

        for (var index = 0; index < Chips.Count; index++)
        {
            if (!Chips[index].Contains(mouse.Row, mouse.Column))
            {
                continue;
            }

            Index = index;

            frame.Close();
            Run();

            return;
        }
    }
}
