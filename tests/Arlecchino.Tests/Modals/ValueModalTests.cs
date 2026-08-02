using System;
using Xunit;
using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Setting;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

public sealed class ValueModalTests
{
    [Fact]
    public void ArrowsStepTheNumberAndPageKeysJump()
    {
        using var app = new TestApplication();

        app.State.Modal = new NumberModal
        {
            Title = "Price",
            Text = "10",
            Step = 5,
            LargeStep = 50,
            OnSubmit = static _ => { },
        };

        app.Press(ConsoleKey.UpArrow);
        Assert.Equal("15", ((NumberModal)app.State.Modal!).Text);

        app.Press(ConsoleKey.PageDown);
        Assert.Equal("-35", ((NumberModal)app.State.Modal!).Text);
    }

    [Fact]
    public void SteppingClampsToTheBounds()
    {
        using var app = new TestApplication();

        app.State.RequestNumber("Weight", 5, 0, 10, static _ => { });

        app.Press(ConsoleKey.PageUp);
        Assert.Equal("10", ((NumberModal)app.State.Modal!).Text);

        app.Press(ConsoleKey.PageDown);
        app.Press(ConsoleKey.PageDown);
        Assert.Equal("0", ((NumberModal)app.State.Modal!).Text);
    }

    [Fact]
    public void LettersAreIgnoredAndSeparatorNeedsDecimals()
    {
        using var app = new TestApplication();

        app.State.RequestNumber("Weight", 1, 0, 10, static _ => { });
        app.Type("a.2");

        Assert.Equal("12", ((NumberModal)app.State.Modal!).Text);
    }

    [Fact]
    public void DecimalSeparatorIsAcceptedWhenDecimalsAreAllowed()
    {
        using var app = new TestApplication();
        decimal submitted = 0;

        app.State.Modal = new NumberModal
        {
            Title = "Price",
            Decimals = 2,
            Maximum = 100,
            OnSubmit = value => submitted = value,
        };

        app.Type("2,50");
        app.Press(ConsoleKey.Enter);

        Assert.Equal(2.5m, submitted);
    }

    [Fact]
    public void OutOfRangeIsReportedBeforeTheUserValidator()
    {
        using var app = new TestApplication();
        var validatorRan = false;

        app.State.Modal = new NumberModal
        {
            Title = "Weight",
            Text = "50",
            Minimum = 0,
            Maximum = 10,
            Validate = _ =>
            {
                validatorRan = true;
                return null;
            },
            OnSubmit = static _ => { },
        };

        app.Press(ConsoleKey.Enter);

        Assert.NotNull(app.State.Modal);
        Assert.False(validatorRan);
        Assert.Contains(app.Options.Strings.OutOfRange("0", "10"), app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void UnparsableNumberIsReported()
    {
        using var app = new TestApplication();

        app.State.Modal = new NumberModal { Title = "Weight", OnSubmit = static _ => { } };
        app.Press(ConsoleKey.Enter);

        Assert.Contains(app.Options.Strings.NotANumber(), app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void SliderMovesWithHorizontalArrowsAndEnds()
    {
        using var app = new TestApplication();
        decimal submitted = 0;

        app.State.RequestSlider("Volume", 50, 0, 100, value => submitted = value);

        app.Press(ConsoleKey.RightArrow);
        Assert.Equal(51m, ((SliderModal)app.State.Modal!).Value);

        app.Press(ConsoleKey.Home);
        Assert.Equal(0m, ((SliderModal)app.State.Modal!).Value);

        app.Press(ConsoleKey.End);
        app.Press(ConsoleKey.Enter);
        Assert.Equal(100m, submitted);
    }

    [Fact]
    public void SliderTrackFillsWithTheValue()
    {
        using var app = new TestApplication();

        app.State.RequestSlider("Volume", 0, 0, 100, static _ => { });
        var empty = app.FrameLineContaining("░");
        Assert.DoesNotContain('█', empty);

        app.State.RequestSlider("Volume", 50, 0, 100, static _ => { });
        var half = app.FrameLineContaining("█");
        Assert.Contains('░', half);

        app.State.RequestSlider("Volume", 100, 0, 100, static _ => { });
        var full = app.FrameLineContaining("█");
        Assert.DoesNotContain('░', full);
    }

    [Fact]
    public void ToggleSwitchesAndSubmits()
    {
        using var app = new TestApplication();
        var submitted = true;

        app.State.RequestToggle("Fullscreen", true, value => submitted = value);

        app.Press(ConsoleKey.LeftArrow);
        app.Press(ConsoleKey.Enter);

        Assert.False(submitted);
    }

    [Fact]
    public void ToggleShowsBothLabels()
    {
        using var app = new TestApplication();

        app.State.RequestToggle("Fullscreen", true, static _ => { });

        var frame = app.Frame();
        Assert.Contains(app.Options.Strings.Yes(), frame, StringComparison.Ordinal);
        Assert.Contains(app.Options.Strings.No(), frame, StringComparison.Ordinal);
    }
}
