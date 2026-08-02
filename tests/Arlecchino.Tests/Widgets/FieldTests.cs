using System;
using System.Collections.Generic;
using Arlecchino.Atoms.Local;
using Arlecchino.Forms;
using Arlecchino.Navigation;
using Arlecchino.Rendering.Colors;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Widgets;

public sealed class FieldTests
{
    private static readonly string[] _options = ["alpha", "beta", "gamma"];

    [Fact]
    public void ASecretIsShownAsDotsAndTypedInFull()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<string>("hunter2");
        var field = Field.Secret(static () => "Password", value);

        Assert.Equal("•••••••", field.Value());

        field.Activate!(app.State);
        app.Type("opensesame");
        app.Press(ConsoleKey.Enter);

        Assert.Equal("opensesame", value.Value);
    }

    [Fact]
    public void ClearingASecretEmptiesIt()
    {
        var value = new LocalAtom<string>("hunter2");

        Field.Secret(static () => "Password", value).Reset!();

        Assert.Equal("", value.Value);
    }

    [Fact]
    public void ANumberIsSteppedAndWrittenBack()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<decimal>(12);
        var field = Field.Number(static () => "Count", value, 0, 100);

        Assert.Equal("12", field.Value());

        field.Activate!(app.State);
        app.Press(ConsoleKey.PageUp);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(22, value.Value);
    }

    [Fact]
    public void ClearingANumberPutsItBackToTheLowestAllowed()
    {
        var value = new LocalAtom<decimal>(80);

        Field.Number(static () => "Count", value, 5, 100).Reset!();

        Assert.Equal(5, value.Value);
    }

    [Fact]
    public void ASliderWritesWhatItWasDraggedTo()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<decimal>(50);
        var field = Field.Slider(static () => "Volume", value, 0, 100);

        field.Activate!(app.State);
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.Enter);

        Assert.True(value.Value > 50);
    }

    [Fact]
    public void AChoiceWritesWhatWasPicked()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<string>("alpha");
        var field = Field.Choice(static () => "Kind", _options, value);

        Assert.Equal("alpha", field.Value());

        field.Activate!(app.State);
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal("beta", value.Value);
    }

    [Fact]
    public void AMultiChoiceWritesEverythingThatWasTicked()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<IReadOnlyList<string>>(["alpha"]);
        var field = Field.MultiChoice(static () => "Columns",
            _options,
            value,
            static picked => string.Join(", ", picked));

        Assert.Equal("alpha", field.Value());

        field.Activate!(app.State);
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Spacebar);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(["alpha", "beta"], value.Value);
    }

    [Fact]
    public void ClearingAMultiChoiceLeavesNothingTicked()
    {
        var value = new LocalAtom<IReadOnlyList<string>>(["alpha", "beta"]);

        Field.MultiChoice(static () => "Columns", _options, value, static picked => string.Join(", ", picked))
            .Reset!();

        Assert.Empty(value.Value);
    }

    [Fact]
    public void ADateIsShownAndWritten()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<DateOnly>(new(2026, 7, 26));
        var field = Field.Date(static () => "When", value, static date => date.ToString("yyyy-MM-dd"));

        Assert.Contains("2026", field.Value(), StringComparison.Ordinal);

        field.Activate!(app.State);
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(2026, 7, 27), value.Value);
    }

    [Fact]
    public void ATimeIsShownAndWritten()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<TimeOnly>(new(9, 30));
        var field = Field.Time(static () => "At", value, static time => time.ToString("HH:mm"));

        Assert.Contains("30", field.Value(), StringComparison.Ordinal);

        field.Activate!(app.State);
        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.Enter);

        Assert.NotEqual(new(9, 30), value.Value);
    }

    [Fact]
    public void AColourIsShownAsItsHexAndWritten()
    {
        using var app = new TestApplication();
        var value = new LocalAtom<Rgb>(new(255, 0, 0));
        var field = Field.Color(static () => "Accent", value);

        Assert.Contains("#", field.Value(), StringComparison.Ordinal);

        field.Activate!(app.State);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(new(255, 0, 0), value.Value);
    }

    [Fact]
    public void HelpIsEmptyUnlessItIsGiven()
    {
        var value = new LocalAtom<string>("");

        Assert.Equal("", Field.Text(static () => "Name", value).Help());
        Assert.Equal("the one on the badge",
            Field.Text(static () => "Name",
                    value,
                    help: static () => "the one on the badge")
                .Help());
    }

    [Fact]
    public void AnActionFieldIsMarkedAsOneAndCarriesNoValue()
    {
        var field = Field.Action(static () => "Apply", static () => ViewRoute.None);

        Assert.True(field.IsAction);
        Assert.Equal("", field.Value());
        Assert.Null(field.Reset);
    }
}
