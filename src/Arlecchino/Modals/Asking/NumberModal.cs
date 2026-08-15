using System;
using System.Globalization;
using Arlecchino.Input;
using Arlecchino.Modals.Setting;

namespace Arlecchino.Modals.Asking;

/// <summary>
/// A number that can be both typed and stepped. Bounds are checked before your validator runs, and
/// the message reports them with affixes, so the user sees the same form they are editing.
/// </summary>
public sealed class NumberModal : NumericModal, ITextEntryModal, IBoundedModal
{
    private string _text = "";
    private int _caret;
    private int _anchor;

    /// <summary>
    /// Whatever has been typed so far, which may not parse yet. Assigning it puts the caret at the end,
    /// which is what makes stepping leave the caret after the new number.
    /// </summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _caret = value.Length;
            _anchor = value.Length;
        }
    }

    /// <summary>Where the caret sits, pulled into the text when it would fall outside.</summary>
    public int Caret
    {
        get => Math.Clamp(_caret, 0, _text.Length);
        set => _caret = Math.Clamp(value, 0, _text.Length);
    }

    /// <summary>Where the selection was started from, on the caret while nothing is selected.</summary>
    public int Anchor
    {
        get => Math.Clamp(_anchor, 0, _text.Length);
        set => _anchor = Math.Clamp(value, 0, _text.Length);
    }

    /// <summary>Lowest value allowed. A negative bound is also what allows a minus sign to be typed.</summary>
    public decimal Minimum { get; init; } = decimal.MinValue;

    /// <summary>Highest value allowed.</summary>
    public decimal Maximum { get; init; } = decimal.MaxValue;

    /// <summary>Your own check, run after parsing and bounds. Return a message to keep the dialog open.</summary>
    public Func<decimal, string?>? Validate { get; init; }

    /// <summary>Called with the accepted number.</summary>
    public required Action<decimal> OnSubmit { get; init; }

    /// <summary>Validation message shown under the field.</summary>
    public string? Message { get; set; }

    /// <summary>Numbers are never masked.</summary>
    public bool Masked => false;

    /// <summary>Reads what has been typed as a number. Both <c>.</c> and <c>,</c> are accepted.</summary>
    /// <param name="value">The parsed value, when the text is a number.</param>
    /// <returns><c>true</c> when the text parses.</returns>
    public bool TryGetValue(out decimal value) =>
        decimal.TryParse(Text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    /// <summary>
    /// Whether a character belongs in a number here: digits always, a separator only when decimals
    /// are allowed, a minus only when the range goes below zero.
    /// </summary>
    /// <param name="character">The character resolved from the key press.</param>
    /// <returns><c>true</c> when it should be inserted.</returns>
    public bool AcceptsCharacter(char character) =>
        char.IsAsciiDigit(character) ||
        (character is '.' or ',' && Decimals > 0) ||
        (character == '-' && Minimum < 0);

    /// <summary>
    /// Steps the value and rewrites the text with it. Text that does not parse is treated as zero,
    /// so stepping always leaves a valid number behind.
    /// </summary>
    /// <param name="delta">How far to move; negative goes down.</param>
    public void Add(decimal delta)
    {
        var current = TryGetValue(out var value) ? value : 0m;
        Text = FormatNumber(Math.Clamp(current + delta, Minimum, Maximum));
    }

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) =>
        frame.Paint.Entry(this, Title, frame.Strings.ModalNumberHints());

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key) => frame.Fields.Number(this, key);
}
