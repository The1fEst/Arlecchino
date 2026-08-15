using System;
using System.Globalization;
using Arlecchino.Input;

namespace Arlecchino.Modals.Setting;

/// <summary>
/// A value edited as a row of fixed-width number segments, the way dates and times are. Digits are
/// collected per segment and only applied once the segment fills up, so a half-typed segment never
/// produces a nonsensical intermediate value.
/// </summary>
public abstract class SegmentedModal : Modal
{
    private string _typedDigits = "";

    /// <summary>Which segment the arrows and digits act on.</summary>
    public int Segment { get; private set; }

    /// <summary>How many segments the value is made of.</summary>
    protected abstract int SegmentCount { get; }

    /// <summary>What is drawn between the segments.</summary>
    public abstract string Separator { get; }

    /// <summary>The stored value as one padded string per segment.</summary>
    /// <returns>A fresh array the caller may modify.</returns>
    protected abstract string[] SegmentTexts();

    /// <summary>Steps the active segment, carrying into the neighbors as that value type requires.</summary>
    /// <param name="delta">How far to step; negative goes down.</param>
    protected abstract void Add(int delta);

    /// <summary>How many digits a segment holds, which is also when typing moves on to the next one.</summary>
    /// <param name="segment">Index of the segment.</param>
    /// <returns>Its digit count.</returns>
    protected abstract int SegmentLength(int segment);

    /// <summary>Stores what was typed into a segment, keeping the whole value legal.</summary>
    /// <param name="segment">Index of the segment that was typed into.</param>
    /// <param name="value">The digits, parsed.</param>
    protected abstract void ApplyTypedValue(int segment, int value);

    /// <summary>
    /// What to draw: the stored value, but with half-typed digits shown in place of the segment being
    /// edited so typing is visible before it takes effect.
    /// </summary>
    /// <returns>One string per segment.</returns>
    public string[] EditedSegmentTexts()
    {
        var texts = SegmentTexts();
        if (_typedDigits.Length > 0)
        {
            texts[Segment] = _typedDigits.PadLeft(SegmentLength(Segment), '0');
        }

        return texts;
    }

    /// <summary>Moves between segments, stopping at the ends. Anything half-typed is applied first.</summary>
    /// <param name="delta">How far to move; negative goes left.</param>
    public void MoveSegment(int delta)
    {
        CommitTypedDigits();
        Segment = Math.Clamp(Segment + delta, 0, SegmentCount - 1);
    }

    /// <summary>
    /// Adds a digit to the active segment. A full segment is applied and left behind, and typing past
    /// it starts the next one over rather than being dropped.
    /// </summary>
    /// <param name="digit">The digit that was typed.</param>
    public void TypeDigit(char digit)
    {
        var length = SegmentLength(Segment);
        _typedDigits = _typedDigits.Length >= length ? digit.ToString() : _typedDigits + digit;

        if (_typedDigits.Length < length)
        {
            return;
        }

        CommitTypedDigits();

        if (Segment < SegmentCount - 1)
        {
            Segment++;
        }
    }

    /// <summary>
    /// Applies a partly typed segment, padding it with leading zeroes. Called before anything that
    /// reads the value, so confirming the dialog keeps what was typed.
    /// </summary>
    protected void CommitTypedDigits()
    {
        if (_typedDigits.Length == 0)
        {
            return;
        }

        var typed = _typedDigits;
        _typedDigits = "";
        ApplyTypedValue(Segment, int.Parse(typed, CultureInfo.InvariantCulture));
    }

    /// <summary>Throws away a partly typed segment, restoring what was stored.</summary>
    public void ClearTypedDigits() => _typedDigits = "";

    /// <summary>Hands the value over to whoever asked for it, once the segments have been committed.</summary>
    protected abstract void Submit();

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key)
    {
        if (frame.Keymap.Cancel.Matches(key))
        {
            frame.Close();

            return;
        }

        if (frame.Keymap.Confirm.Matches(key))
        {
            CommitTypedDigits();
            frame.Close();
            Submit();

            return;
        }

        if (frame.Keymap.MoveLeft.Matches(key) || frame.Keymap.PreviousField.Matches(key))
        {
            MoveSegment(-1);

            return;
        }

        if (frame.Keymap.MoveRight.Matches(key) || frame.Keymap.NextField.Matches(key))
        {
            MoveSegment(1);

            return;
        }

        if (frame.Keymap.MoveUp.Matches(key))
        {
            Add(1);

            return;
        }

        if (frame.Keymap.MoveDown.Matches(key))
        {
            Add(-1);

            return;
        }

        if (frame.Keymap.Erase.Matches(key))
        {
            ClearTypedDigits();

            return;
        }

        if (frame.Keys.Resolve(key) is { } typed && char.IsAsciiDigit(typed))
        {
            TypeDigit(typed);
        }
    }
}
