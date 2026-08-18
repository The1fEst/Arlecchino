using System;
using Arlecchino.Rendering.Colors;
using Arlecchino.Input;
using Arlecchino.Modals.Choosing;
using Arlecchino.Tests.Support;
using Xunit;

namespace Arlecchino.Tests.Editing;

/// <summary>
/// What is typed to narrow a list is a line of text like any other. All of it starts once something is being
/// filtered by, so a list with nothing typed still answers the keys that walk its rows.
/// </summary>
public sealed class FilterEditingTests
{
    private static readonly string[] Options = ["alpha", "beta", "gamma"];

    [Fact]
    public void TheCaretWalksTheFilterOnceSomethingIsTyped()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });

        app.Type("ba");
        app.Press(ConsoleKey.LeftArrow);
        app.Type("et");

        Assert.Equal("beta", ((ChoiceModal)app.State.Modal!).Text);
    }

    [Fact]
    public void ShiftSelectsInTheFilterAndTypingReplacesTheSelection()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });

        app.Type("beta");
        app.Press(ConsoleKey.Home, KeyModifiers.Shift);
        app.Type("g");

        Assert.Equal("g", ((ChoiceModal)app.State.Modal!).Text);
    }

    [Fact]
    public void CopyingTheFilterReachesTheClipboard()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });

        app.Type("gam");
        app.Press(ConsoleKey.Insert, KeyModifiers.Control);

        Assert.Equal("gam", app.Terminal.CopiedText);
    }

    [Fact]
    public void CuttingTakesTheSelectionOutOfTheFilter()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });

        app.Type("gamma");
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Press(ConsoleKey.Delete, KeyModifiers.Shift);

        Assert.Equal("ma", app.Terminal.CopiedText);
        Assert.Equal("gam", ((ChoiceModal)app.State.Modal!).Text);
    }

    /// <summary>
    /// The word key takes a word of the filter, which is the same key the field of a dialog rubs a word
    /// out with.
    /// </summary>
    [Fact]
    public void TheWordKeyRubsOutAWordOfTheFilter()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", ["one two"], static _ => { });

        app.Type("one two");
        app.Press(ConsoleKey.Backspace, KeyModifiers.Control);

        Assert.Equal("one ", ((ChoiceModal)app.State.Modal!).Text);
    }

    /// <summary>
    /// With nothing typed the rows are what the keys are for: walking the list still picks the row the
    /// arrows landed on rather than being swallowed by an empty filter.
    /// </summary>
    [Fact]
    public void AnEmptyFilterLeavesTheRowsTheirKeys()
    {
        using var app = new TestApplication();
        var choice = "";

        app.State.RequestChoice("Pick", Options, value => choice = value);

        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal("beta", choice);
    }

    /// <summary>
    /// The filter is drawn the way a field is: the caret is written over the symbol it stands on rather than
    /// wedged beside it, so moving it never shifts the text.
    /// </summary>
    [Fact]
    public void TheFilterIsDrawnWithItsCaret()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });

        app.Type("ga");

        Assert.Contains($"{app.Options.Strings.Filter()} ga", app.Frame(), StringComparison.Ordinal);
        Assert.Equal(Theme.Caret.Ansi, app.StyleAfter("ga"));

        app.Press(ConsoleKey.LeftArrow);

        Assert.Contains($"{app.Options.Strings.Filter()} ga", app.Frame(), StringComparison.Ordinal);
        Assert.Equal(Theme.Caret.Ansi, app.StyleAfter("g"));
    }

    [Fact]
    public void PastingIntoTheFilterLandsAtTheCaret()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });

        app.Type("ba");
        app.Press(ConsoleKey.LeftArrow);
        app.ReadFromTerminal("\e[200~et\e[201~");

        Assert.Equal("beta", ((ChoiceModal)app.State.Modal!).Text);
    }
}
