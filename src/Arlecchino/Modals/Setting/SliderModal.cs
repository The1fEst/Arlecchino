using System;
using Arlecchino.Rendering;

using Arlecchino.Modals.Asking;

using Arlecchino.Input;

namespace Arlecchino.Modals.Setting;

/// <summary>
/// A value inside a range, adjusted by arrows or by dragging the track. There is nothing to type, so
/// the value is always valid and the dialog never reports an error.
/// </summary>
public sealed class SliderModal : NumericModal, IBoundedModal
{
    /// <summary>Where the handle currently sits.</summary>
    public decimal Value { get; set; }

    /// <summary>Value at the left end of the track.</summary>
    public decimal Minimum { get; init; }

    /// <summary>Value at the right end of the track.</summary>
    public decimal Maximum { get; init; } = 100m;

    /// <summary>Called with the value the handle was left at.</summary>
    public required Action<decimal> OnSubmit { get; init; }

    /// <summary>Where the track was drawn last frame, used to turn a click into a value.</summary>
    public SurfaceRegion Track { get; set; }

    /// <summary>Places the handle at a position along the track.</summary>
    /// <param name="fraction">Position from <c>0</c> at the left end to <c>1</c> at the right; anything outside is pulled in.</param>
    public void SetFromFraction(decimal fraction) =>
        Value = Math.Clamp(Minimum + (Maximum - Minimum) * Math.Clamp(fraction, 0m, 1m), Minimum, Maximum);

    /// <summary>How far along the track the handle sits, as <c>0</c> to <c>1</c>. An empty range reads as <c>0</c>.</summary>
    public decimal Fraction => Maximum > Minimum
        ? (Value - Minimum) / (Maximum - Minimum)
        : 0m;

    /// <summary>Moves the handle, stopping at the ends of the range.</summary>
    /// <param name="delta">How far to move; negative goes left.</param>
    public void Add(decimal delta) => Value = Math.Clamp(Value + delta, Minimum, Maximum);

    /// <summary>Jumps to the left end.</summary>
    public void MoveToMinimum() => Value = Minimum;

    /// <summary>Jumps to the right end.</summary>
    public void MoveToMaximum() => Value = Maximum;

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) => frame.Values.Slider(this);

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, ConsoleKeyInfo key) => frame.Steps.Slider(this, key);

    /// <inheritdoc/>
    public override void HandleMouse(ModalFrame frame, MouseEvent mouse) =>
        Tracking.Follow(Track, mouse, SetFromFraction);
}
