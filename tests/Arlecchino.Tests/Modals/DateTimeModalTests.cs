using System;
using Xunit;
using Arlecchino.Modals.Setting;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

public sealed class DateTimeModalTests
{
    [Fact]
    public void DateIsShownAsSegments()
    {
        using var app = new TestApplication();

        app.State.RequestDate("Release", new(2026, 7, 25), static _ => { });

        Assert.Contains("2026-07-25", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void ArrowsChangeTheSegmentUnderTheCursor()
    {
        using var app = new TestApplication();
        var submitted = default(DateOnly);

        app.State.RequestDate("Release", new(2026, 7, 25), value => submitted = value);

        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(2027, 8, 25), submitted);
    }

    [Fact]
    public void MonthStepKeepsTheDayValid()
    {
        using var app = new TestApplication();
        var submitted = default(DateOnly);

        app.State.RequestDate("Release", new(2026, 1, 31), value => submitted = value);

        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(2026, 2, 28), submitted);
    }

    [Fact]
    public void TypingDigitsFillsSegmentsLeftToRight()
    {
        using var app = new TestApplication();
        var submitted = default(DateOnly);

        app.State.RequestDate("Release", new(2026, 7, 25), value => submitted = value);

        app.Type("20240103");
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(2024, 1, 3), submitted);
    }

    [Fact]
    public void BoundsClampTypedAndSteppedValues()
    {
        using var app = new TestApplication();
        var submitted = default(DateOnly);

        app.State.Modal = new DateModal
        {
            Title = "Release",
            Value = new(2026, 7, 25),
            Minimum = new(2026, 1, 1),
            Maximum = new(2026, 12, 31),
            OnSubmit = value => submitted = value,
        };

        app.Type("2030");
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(2026, 12, 31), submitted);
    }

    [Fact]
    public void BackspaceDiscardsAHalfTypedSegment()
    {
        using var app = new TestApplication();

        app.State.RequestDate("Release", new(2026, 7, 25), static _ => { });

        app.Type("19");
        app.Press(ConsoleKey.Backspace);

        Assert.Contains("2026-07-25", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TimeIsShownAsHoursAndMinutes()
    {
        using var app = new TestApplication();

        app.State.RequestTime("Start", new(9, 41), static _ => { });

        Assert.Contains("09:41", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void HourStepWrapsAroundMidnight()
    {
        using var app = new TestApplication();
        var submitted = default(TimeOnly);

        app.State.RequestTime("Start", new(23, 30), value => submitted = value);

        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(0, 30), submitted);
    }

    [Fact]
    public void TypedMinutesWrapWithinTheHour()
    {
        using var app = new TestApplication();
        var submitted = default(TimeOnly);

        app.State.RequestTime("Start", new(9, 41), value => submitted = value);

        app.Type("1075");
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(10, 15), submitted);
    }

    [Fact]
    public void EscapeCancelsTheSegmentEditor()
    {
        using var app = new TestApplication();
        var submitted = false;

        app.State.RequestTime("Start", new(9, 41), _ => submitted = true);
        app.Press(ConsoleKey.Escape);

        Assert.Null(app.State.Modal);
        Assert.False(submitted);
    }
}
