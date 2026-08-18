using System;
using Arlecchino.Input;
using Xunit;
using Arlecchino.Modals.Choosing;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

public sealed class ChoiceModalTests
{
    private static readonly string[] Options = ["alpha", "beta", "gamma"];

    [Fact]
    public void ArrowsMoveAndEnterPicks()
    {
        using var app = new TestApplication();
        var choice = "";

        app.State.RequestChoice("Pick", Options, value => choice = value);

        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal("beta", choice);
    }

    [Fact]
    public void CurrentOptionStartsSelected()
    {
        using var app = new TestApplication();
        var choice = "";

        app.State.RequestChoice("Pick", Options, value => choice = value, current: "gamma");
        app.Press(ConsoleKey.Enter);

        Assert.Equal("gamma", choice);
    }

    [Fact]
    public void TypingFiltersAndResetsTheSelection()
    {
        using var app = new TestApplication();
        var choice = "";

        app.State.RequestChoice("Pick", Options, value => choice = value);

        app.Press(ConsoleKey.DownArrow);
        app.Type("ga");
        app.Press(ConsoleKey.Enter);

        Assert.Equal("gamma", choice);
    }

    [Fact]
    public void BackspaceShortensTheFilter()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });

        app.Type("ga");
        app.Press(ConsoleKey.Backspace);

        Assert.Equal("g", ((ChoiceModal)app.State.Modal!).Text);
    }

    /// <summary>
    ///     What is typed to narrow a list is edited the way any other line is: a symbol goes in one press and
    ///     comes out in one, and the word key takes the whole word.
    /// </summary>
    [Fact]
    public void BackspaceTakesAWholeSymbolOffTheFilter()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });
        var modal = (ChoiceModal)app.State.Modal!;

        modal.Text = "a😀";
        app.Press(ConsoleKey.Backspace);

        Assert.Equal("a", modal.Text);
    }

    [Fact]
    public void EmptyResultShowsTheNoticeAndEnterDoesNothing()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });
        app.Type("zzz");

        Assert.Contains(app.Options.Strings.NothingMatches(), app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.Enter);
        Assert.NotNull(app.State.Modal);
    }

    [Fact]
    public void SpaceMarksAndEnterReturnsSelectionInOptionOrder()
    {
        using var app = new TestApplication();
        string[] choice = [];

        app.State.RequestMultiChoice("Columns", Options, [], value => choice = [.. value]);

        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Spacebar);
        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.UpArrow);
        app.Press(ConsoleKey.Spacebar);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(["alpha", "gamma"], choice);
    }

    [Fact]
    public void PreselectedOptionsAreMarkedAndCounted()
    {
        using var app = new TestApplication();

        app.State.RequestMultiChoice("Columns", Options, ["beta"], static _ => { });

        var frame = app.Frame();
        Assert.Contains("[×] beta", frame, StringComparison.Ordinal);
        Assert.Contains("[ ] alpha", frame, StringComparison.Ordinal);
        Assert.Contains(app.Options.Strings.SelectedCount(1), frame, StringComparison.Ordinal);
    }

    [Fact]
    public void MarksSurviveFiltering()
    {
        using var app = new TestApplication();
        string[] choice = [];

        app.State.RequestMultiChoice("Columns", Options, [], value => choice = [.. value]);

        app.Press(ConsoleKey.Spacebar);
        app.Type("ga");
        app.Press(ConsoleKey.Spacebar);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(["alpha", "gamma"], choice);
    }

    [Fact]
    public void CommandPaletteOpensRunsAndReportsUnknownKeys()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<ProbeCommand>());

        app.Press(ConsoleKey.Oem1, KeyModifiers.Shift);
        Assert.IsType<CommandModal>(app.State.Modal);

        app.Press(ConsoleKey.Z);
        Assert.Null(app.State.Modal);
        Assert.Contains(app.Options.Strings.CommandUnknown("Z"), app.State.Output, StringComparison.Ordinal);

        app.Press(ConsoleKey.Oem1, KeyModifiers.Shift);
        app.Press(ConsoleKey.P);
        Assert.Equal("probe command", app.State.Output);
    }

    [Fact]
    public void AListLongerThanTheBoxShowsWhereTheCursorIs()
    {
        using var app = new TestApplication();
        var options = new string[40];

        for (var i = 0; i < options.Length; i++)
        {
            options[i] = $"option {i}";
        }

        app.State.RequestChoice("Pick", options, static _ => { });
        app.Press(ConsoleKey.DownArrow);

        var frame = app.Frame();
        Assert.Contains(app.Options.Strings.ListPosition(2, 40), frame, StringComparison.Ordinal);
        Assert.Contains('█', frame);
    }

    [Fact]
    public void AListThatFitsShowsNoPosition()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["alpha", "beta"], static _ => { });

        Assert.DoesNotContain(app.Options.Strings.ListPosition(1, 2), app.Frame(), StringComparison.Ordinal);
    }
}
