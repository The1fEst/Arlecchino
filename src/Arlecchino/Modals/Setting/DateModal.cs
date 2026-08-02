using System;
using System.Globalization;

namespace Arlecchino.Modals.Setting;

/// <summary>
/// A calendar date, edited as year, month and day. The value is kept inside the bounds and inside the
/// calendar at every step, so a day that the new month does not have is pulled back to its last day
/// rather than rejected.
/// </summary>
public sealed class DateModal : SegmentedModal
{
    private const int YearSegment = 0;
    private const int MonthSegment = 1;
    private const int DaySegment = 2;

    /// <summary>The date as it stands. Defaults to today.</summary>
    public DateOnly Value { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>Earliest date allowed.</summary>
    public DateOnly Minimum { get; init; } = DateOnly.MinValue;

    /// <summary>Latest date allowed.</summary>
    public DateOnly Maximum { get; init; } = DateOnly.MaxValue;

    /// <summary>Called with the date that was confirmed.</summary>
    public required Action<DateOnly> OnSubmit { get; init; }

    /// <summary>Year, month and day.</summary>
    public override int SegmentCount => 3;

    /// <summary>Segments are drawn in ISO order, so they are separated by a dash.</summary>
    public override string Separator => "-";

    /// <summary>The date as a four-digit year and two-digit month and day.</summary>
    /// <returns>One string per segment.</returns>
    public override string[] SegmentTexts() =>
    [
        Value.Year.ToString("D4", CultureInfo.InvariantCulture),
        Value.Month.ToString("D2", CultureInfo.InvariantCulture),
        Value.Day.ToString("D2", CultureInfo.InvariantCulture),
    ];

    /// <summary>
    /// Steps by whole years, months or days depending on the active segment, so stepping the month at
    /// the end of a long month lands on a date the shorter month actually has.
    /// </summary>
    /// <param name="delta">How far to step; negative goes back.</param>
    public override void Add(int delta)
    {
        CommitTypedDigits();

        var shifted = Segment switch
        {
            YearSegment => Value.AddYears(delta),
            MonthSegment => Value.AddMonths(delta),
            _ => Value.AddDays(delta),
        };

        Value = Clamp(shifted);
    }

    /// <summary>Four digits for the year, two for the rest.</summary>
    /// <param name="segment">Index of the segment.</param>
    /// <returns>Its digit count.</returns>
    protected override int SegmentLength(int segment) => segment == YearSegment ? 4 : 2;

    /// <summary>
    /// Stores a typed segment, pulling impossible input into range: month into 1-12 and day into the
    /// days the resulting month has.
    /// </summary>
    /// <param name="segment">Index of the segment that was typed into.</param>
    /// <param name="value">The digits, parsed.</param>
    protected override void ApplyTypedValue(int segment, int value)
    {
        var year = segment == YearSegment ? Math.Clamp(value, 1, 9999) : Value.Year;
        var month = segment == MonthSegment ? Math.Clamp(value, 1, 12) : Value.Month;
        var day = segment == DaySegment ? Math.Clamp(value, 1, 31) : Value.Day;

        Value = Clamp(new(year, month, Math.Min(day, DateTime.DaysInMonth(year, month))));
    }

    private DateOnly Clamp(DateOnly value)
    {
        if (value < Minimum)
        {
            return Minimum;
        }

        return value > Maximum ? Maximum : value;
    }

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) => frame.Values.Segmented(this, frame.Strings.ModalDateHints());

    /// <inheritdoc/>
    protected override void Submit() => OnSubmit(Value);
}
