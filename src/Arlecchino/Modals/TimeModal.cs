using System;
using System.Globalization;

namespace Arlecchino.Modals;

/// <summary>
/// A time of day, edited as hours and minutes on a 24-hour clock. Everything wraps: stepping past
/// midnight comes back around instead of stopping.
/// </summary>
public sealed class TimeModal : SegmentedModal
{
    private const int HourSegment = 0;
    private const int HoursPerDay = 24;
    private const int MinutesPerHour = 60;

    /// <summary>The time as it stands. Defaults to midnight.</summary>
    public TimeOnly Value { get; set; }

    /// <summary>Called with the time that was confirmed.</summary>
    public required Action<TimeOnly> OnSubmit { get; init; }

    /// <summary>Hours and minutes; seconds are not edited here.</summary>
    public override int SegmentCount => 2;

    /// <summary>Segments are separated the way a clock is written.</summary>
    public override string Separator => ":";

    /// <summary>The time as two-digit hours and minutes.</summary>
    /// <returns>One string per segment.</returns>
    public override string[] SegmentTexts() =>
    [
        Value.Hour.ToString("D2", CultureInfo.InvariantCulture),
        Value.Minute.ToString("D2", CultureInfo.InvariantCulture),
    ];

    /// <summary>Steps by whole hours or minutes depending on the active segment, wrapping around the clock.</summary>
    /// <param name="delta">How far to step; negative goes back.</param>
    public override void Add(int delta)
    {
        CommitTypedDigits();
        Value = Segment == HourSegment ? Value.AddHours(delta) : Value.AddMinutes(delta);
    }

    /// <summary>Two digits for both hours and minutes.</summary>
    /// <param name="segment">Index of the segment.</param>
    /// <returns>Its digit count.</returns>
    protected override int SegmentLength(int segment) => 2;

    /// <summary>Stores a typed segment, wrapping input that is too large rather than refusing it.</summary>
    /// <param name="segment">Index of the segment that was typed into.</param>
    /// <param name="value">The digits, parsed.</param>
    protected override void ApplyTypedValue(int segment, int value)
    {
        var hour = segment == HourSegment ? value % HoursPerDay : Value.Hour;
        var minute = segment == HourSegment ? Value.Minute : value % MinutesPerHour;

        Value = new(hour, minute);
    }
}
