using System;
using Arlecchino.Input;

namespace Arlecchino.Modals.Telling;

/// <summary>
/// Something the user only has to read: a result, a warning, an explanation of what just failed. It
/// takes no input beyond the key that closes it, which is what separates it from every other dialog
/// here.
/// </summary>
public sealed class MessageModal : Modal
{
    /// <summary>The message, drawn under the title. Long text wraps to the width of the box.</summary>
    public required string Text { get; init; }

    /// <summary>Called once it is dismissed, for a screen that wants to carry on afterward.</summary>
    public Action? OnClosed { get; init; }

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) => frame.Tells.Message(this);

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key)
    {
        if (!frame.Keymap.Cancel.Matches(key) && !frame.Keymap.Confirm.Matches(key))
        {
            return;
        }

        frame.Close();
        OnClosed?.Invoke();
    }
}
