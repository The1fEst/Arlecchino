using System;
using Arlecchino.Rendering;

using Arlecchino.Input;

namespace Arlecchino.Modals.Setting;

/// <summary>A yes-or-no answer, flipped with the arrows or picked by clicking one of the two chips.</summary>
public sealed class ToggleModal : Modal
{
    /// <summary>The answer as it stands.</summary>
    public bool Value { get; set; }

    /// <summary>Called with the answer that was confirmed.</summary>
    public required Action<bool> OnSubmit { get; init; }

    /// <summary>Where the affirmative chip was drawn last frame, used to turn a click into an answer.</summary>
    public SurfaceRegion YesChip { get; set; }

    /// <summary>Where the negative chip was drawn last frame.</summary>
    public SurfaceRegion NoChip { get; set; }

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) => frame.Values.Toggle(this);

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.Keymap.Cancel.Matches(key))
        {
            frame.Close();

            return;
        }

        if (frame.Keymap.Confirm.Matches(key))
        {
            frame.Close();
            OnSubmit(Value);

            return;
        }

        if (frame.Keymap.MoveLeft.Matches(key) ||
            frame.Keymap.MoveRight.Matches(key) ||
            frame.Keymap.NextField.Matches(key) ||
            frame.Keymap.Mark.Matches(key))
        {
            Value = !Value;
        }
    }

    /// <inheritdoc/>
    public override void HandleMouse(ModalFrame frame, MouseEvent mouse)
    {
        if (mouse.Action != MouseAction.Pressed || mouse.Button != MouseButton.Left)
        {
            return;
        }

        if (YesChip.Contains(mouse.Row, mouse.Column))
        {
            Value = true;
        }
        else if (NoChip.Contains(mouse.Row, mouse.Column))
        {
            Value = false;
        }
    }
}
