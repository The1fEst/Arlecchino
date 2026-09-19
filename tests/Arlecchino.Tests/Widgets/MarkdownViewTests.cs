using System;
using System.Linq;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Terminals;
using Arlecchino.Testing;
using Arlecchino.Widgets.Pictures;
using Arlecchino.Widgets.Readouts;
using Xunit;

namespace Arlecchino.Tests.Widgets;

/// <summary>
///     A document as the view draws it. Every fact here is read off a frame rather than off the runs the
///     document was read into, the frame being the whole of what the view is for.
/// </summary>
public sealed class MarkdownViewTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    private static (Surface Surface, FakeTerminal Terminal) CreateSurface(int width = 20, int height = 6)
    {
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();
        return (surface, terminal);
    }

    private static string[] Drawn(string text, int width = 20, int height = 6)
    {
        var (surface, terminal) = CreateSurface(width, height);
        var view = new MarkdownView(Keymap) { Text = text, ShowScrollBar = false };

        view.Draw(surface.Frame);
        surface.Build();

        var lines = FrameText.Lines(terminal.WrittenText);

        for (var row = 0; row < lines.Length; row++)
        {
            lines[row] = lines[row].TrimEnd();
        }

        return lines;
    }

    private static string Styled(string text, int width = 20)
    {
        var (surface, terminal) = CreateSurface(width);
        var view = new MarkdownView(Keymap) { Text = text, ShowScrollBar = false };

        view.Draw(surface.Frame);
        surface.Build();

        return terminal.WrittenText;
    }

    [Fact]
    public void AHeadingIsWrittenWithoutItsHashesAndRuledOff()
    {
        var lines = Drawn("# Title\n\nwords");

        Assert.Equal("Title", lines[0]);
        Assert.Equal(new('─', 20), lines[1]);
        Assert.Equal("words", lines[3]);
    }

    [Fact]
    public void AHeadingWrittenAsAnUnderlineIsStillAHeading()
    {
        var lines = Drawn("Title\n=====\n\nwords");

        Assert.Equal("Title", lines[0]);
        Assert.Equal(new('─', 20), lines[1]);
    }

    [Fact]
    public void AHeadingIsSaidLouderThanTheProseUnderIt()
    {
        Assert.Contains("\e[0;1", Styled("# Title\n\nwords"), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryListIsBulletedTheSameWhicheverMarkTheDocumentUsed()
    {
        var lines = Drawn("- one\n* two\n+ three");

        Assert.Equal("• one", lines[0]);
        Assert.Equal("• two", lines[1]);
        Assert.Equal("• three", lines[2]);
    }

    [Fact]
    public void AnItemOfATaskListIsDrawnAsABox()
    {
        var lines = Drawn("- [ ] open\n- [x] done");

        Assert.Equal("☐ open", lines[0]);
        Assert.Equal("☑ done", lines[1]);
    }

    [Fact]
    public void ANumberedItemKeepsItsNumber()
    {
        var lines = Drawn("1. one\n2. two");

        Assert.Equal("1. one", lines[0]);
        Assert.Equal("2. two", lines[1]);
    }

    [Fact]
    public void TheRestOfAnItemIsSetUnderItsFirstWordRatherThanUnderItsBullet()
    {
        var lines = Drawn("- alpha beta gamma", 12);

        Assert.Equal("• alpha beta", lines[0]);
        Assert.Equal("  gamma", lines[1]);
    }

    [Fact]
    public void AListInsideAListIsSetFurtherIn()
    {
        var lines = Drawn("- one\n  - two");

        Assert.Equal("• one", lines[0]);
        Assert.Equal("  ◦ two", lines[1]);
    }

    [Fact]
    public void AQuoteCarriesItsBarDownEveryRowItTakes()
    {
        var lines = Drawn("> alpha beta gamma", 12);

        Assert.Equal("▌ alpha beta", lines[0]);
        Assert.Equal("▌ gamma", lines[1]);
    }

    [Fact]
    public void ACodeBlockKeepsItsLinesAsTheyStandAndLosesItsFence()
    {
        var lines = Drawn("```csharp\nvar total = one + two;\n```", 16);

        Assert.Equal("  var total = o›", lines[0]);
        Assert.Equal("", lines[1]);
    }

    [Fact]
    public void AStarredWordLosesItsStarsAndIsSaidLouder()
    {
        var lines = Drawn("a **loud** word");

        Assert.Equal("a loud word", lines[0]);
        Assert.Contains("\e[0;1", Styled("a **loud** word"), StringComparison.Ordinal);
    }

    [Fact]
    public void AStarredWordLosesItsStarsAndIsSlanted()
    {
        var lines = Drawn("a *slant* word");

        Assert.Equal("a slant word", lines[0]);
        Assert.Contains("\e[0;3", Styled("a *slant* word"), StringComparison.Ordinal);
    }

    [Fact]
    public void AStarThatOpensNothingIsLeftAsItWasWritten()
    {
        var lines = Drawn("2 * 3 * 4");

        Assert.Equal("2 * 3 * 4", lines[0]);
    }

    [Fact]
    public void ABacktickThatIsNeverClosedIsLeftAsItWasWritten()
    {
        var lines = Drawn("a ` b");

        Assert.Equal("a ` b", lines[0]);
    }

    [Fact]
    public void AnUnderscoreInsideAWordIsNotEmphasis()
    {
        var lines = Drawn("snake_case_name");

        Assert.Equal("snake_case_name", lines[0]);
    }

    [Fact]
    public void ACodeSpanIsDrawnWithoutItsBackticks()
    {
        var lines = Drawn("run `arlc` now");

        Assert.Equal("run arlc now", lines[0]);
    }

    [Fact]
    public void ALinkIsWrittenWithWhereItGoes()
    {
        var lines = Drawn("[docs](https://a.dev)", 30);

        Assert.Equal("docs https://a.dev", lines[0]);
    }

    [Fact]
    public void MarkupLeftInTheDocumentIsNotWrittenOut()
    {
        var lines = Drawn("<p align=\"center\">hello</p>", 30);

        Assert.Equal("hello", lines[0]);
    }

    [Fact]
    public void AParagraphBrokenOverSeveralLinesIsOneParagraphAgain()
    {
        var lines = Drawn("alpha\nbeta\ngamma", 12);

        Assert.Equal("alpha beta", lines[0]);
        Assert.Equal("gamma", lines[1]);
    }

    [Fact]
    public void ALineOfNothingButMarkupIsNotEvenAnEmptyRow()
    {
        var lines = Drawn("<p align=\"center\">\n</p>\n\nwords", 30);

        Assert.Equal("words", lines[0]);
    }

    /// <summary>
    ///     A picture is taken out of the line it was written in and given rows of its own. Where nothing
    ///     can draw it, what is left in those rows is what it was to be shown instead of.
    /// </summary>
    [Fact]
    public void APictureNothingCanDrawIsWrittenAsWhatItStandsFor()
    {
        Assert.Equal("recoil", Drawn("<img src=\"tables/recoil.svg\" alt=\"recoil\">", 30)[0]);
        Assert.Equal("the shape", Drawn("![the shape](chart.png)", 30)[0]);
        Assert.Equal("chart.png", Drawn("![](chart.png)", 30)[0]);
        Assert.Equal("one two", Drawn("![one](a.png)![two](b.png)", 30)[0]);
    }

    [Fact]
    public void ThePictureOfALineIsDrawnUnderWhatTheLineSays()
    {
        var lines = Drawn("<sub>as it ships</sub><br><img src=\"a.svg\" alt=\"shipped\">", 30);

        Assert.Equal("as it ships", lines[0]);
        Assert.Equal("shipped", lines[1]);
    }

    /// <summary>
    ///     An entity is what a document writes because a browser would not take the symbol itself. A pane
    ///     takes it, and a table of a hundred rows carrying <c>&amp;rarr;</c> is six columns wider for it.
    /// </summary>
    [Fact]
    public void AnEntityIsDrawnAsTheSymbolItStandsFor()
    {
        var lines = Drawn("2 &rarr; 3 &middot; &#8594; &#x2192; AT&amp;T", 40);

        Assert.Equal("2 → 3 · → → AT&T", lines[0]);
    }

    [Fact]
    public void AnAmpersandThatStandsForNothingIsLeftAsItWasWritten()
    {
        var lines = Drawn("one & two &notanentity; three", 40);

        Assert.Equal("one & two &notanentity; three", lines[0]);
    }

    [Fact]
    public void AnEntityInsideACodeSpanIsTheTextItself()
    {
        var lines = Drawn("write `&amp;` for it", 40);

        Assert.Equal("write &amp; for it", lines[0]);
    }

    [Fact]
    public void ARowThatRunsPastTheEdgeSaysThatItWasCut()
    {
        var lines = Drawn("| a | b |\n| --- | --- |\n| wide enough | to be cut |", 12);

        Assert.EndsWith("›", lines[0], StringComparison.Ordinal);
        Assert.EndsWith("›", lines[2], StringComparison.Ordinal);
        Assert.DoesNotContain("›", Drawn("| a | b |\n| --- | --- |\n| c | d |", 40)[0], StringComparison.Ordinal);
    }

    [Fact]
    public void ATableIsPaddedIntoColumns()
    {
        var lines = Drawn("| a | bb |\n| --- | --: |\n| ccc | d |");

        Assert.Equal("a   │ bb", lines[0]);
        Assert.Equal("────┼───", lines[1]);
        Assert.Equal("ccc │  d", lines[2]);
    }

    [Fact]
    public void RowsThatAreNotATableAreReadAsProse()
    {
        var lines = Drawn("| just | bars |", 30);

        Assert.Equal("| just | bars |", lines[0]);
    }

    [Fact]
    public void TheRoomADocumentLeavesAroundItsParagraphsIsKeptToOneRow()
    {
        var lines = Drawn("one\n\n\n\ntwo");

        Assert.Equal("one", lines[0]);
        Assert.Equal("", lines[1]);
        Assert.Equal("two", lines[2]);
    }

    [Fact]
    public void ScrollingShowsTheRestOfTheDocument()
    {
        var (surface, terminal) = CreateSurface(10, 2);
        var view = new MarkdownView(Keymap) { Text = "- one\n- two\n- three", ShowScrollBar = false };

        view.Draw(surface.Frame);
        surface.Build();

        Assert.Equal("• one", FrameText.Lines(terminal.WrittenText)[0].TrimEnd());

        view.Handle(new(ConsoleKey.DownArrow));

        var (second, terminalAgain) = CreateSurface(10, 2);
        view.Draw(second.Frame);
        second.Build();

        Assert.Equal("• two", FrameText.Lines(terminalAgain.WrittenText)[0].TrimEnd());
    }

    [Fact]
    public void ChangingTheWidthReflowsInsteadOfCutting()
    {
        var view = new MarkdownView(Keymap) { Text = "alpha beta gamma", ShowScrollBar = false };
        var (narrow, narrowTerminal) = CreateSurface(8);

        view.Draw(narrow.Frame);
        narrow.Build();

        Assert.Equal("alpha", FrameText.Lines(narrowTerminal.WrittenText)[0].TrimEnd());
        Assert.Equal(3, view.LineCount);

        var (wide, wideTerminal) = CreateSurface();
        view.Draw(wide.Frame);
        wide.Build();

        Assert.Equal("alpha beta gamma", FrameText.Lines(wideTerminal.WrittenText)[0].TrimEnd());
        Assert.Equal(1, view.LineCount);
    }

    [Fact]
    public void AWordWiderThanThePaneIsBrokenRatherThanLost()
    {
        var lines = Drawn("unbreakableword", 6);

        Assert.Equal("unbrea", lines[0]);
        Assert.Equal("kablew", lines[1]);
        Assert.Equal("ord", lines[2]);
    }

    [Fact]
    public void ADocumentThatDoesNotFitGetsAScrollBar()
    {
        var (surface, terminal) = CreateSurface(10, 2);
        var view = new MarkdownView(Keymap) { Text = "- one\n- two\n- three" };

        view.Draw(surface.Frame);
        surface.Build();

        Assert.Contains("█", terminal.WrittenText, StringComparison.Ordinal);
    }

    /// <summary>
    ///     A picture the application can hand over takes rows of its own and is drawn over them. The rows
    ///     are kept whether it is drawn on this frame or not, so the document does not jump as it scrolls.
    /// </summary>
    [Fact]
    public void APictureTheApplicationHandsOverIsDrawnInPlaceOfItsWords()
    {
        var (surface, terminal) = CreateSurface(20, 10);
        var picture = new Picture { Protocol = ImageProtocol.Blocks };
        var view = new MarkdownView(Keymap)
        {
            Text = "![shape](chart.png)",
            ShowScrollBar = false,
            Pictures = _ => picture,
            PictureRows = 4,
        };

        picture.Show(Enumerable.Repeat(new RGB(0xC9, 0x38, 0x2B), 40 * 400).ToArray(), 40, 400);

        view.Draw(surface.Frame);
        surface.Build();

        Assert.Equal(4, view.LineCount);
        Assert.DoesNotContain("shape", terminal.WrittenText, StringComparison.Ordinal);
        Assert.Contains("▀", terminal.WrittenText, StringComparison.Ordinal);
    }

    /// <summary>
    ///     A document that writes two pictures into one line means them beside one another — two columns
    ///     of a table, a before and an after — so they share the band and half its width each.
    /// </summary>
    [Fact]
    public void ThePicturesOfOneLineAreDrawnSideBySide()
    {
        var (surface, terminal) = CreateSurface(40, 8);
        var picture = new Picture { Protocol = ImageProtocol.Blocks };
        var view = new MarkdownView(Keymap)
        {
            Text = "<img src=\"one.svg\"><img src=\"two.svg\">",
            ShowScrollBar = false,
            Pictures = _ => picture,
            PictureRows = 4,
        };

        picture.Show(Enumerable.Repeat(new RGB(0xC9, 0x38, 0x2B), 40 * 80).ToArray(), 40, 80);

        view.Draw(surface.Frame);
        surface.Build();

        var row = FrameText.Lines(terminal.WrittenText)[0];

        Assert.InRange(row.IndexOf('▀'), 0, 19);
        Assert.InRange(row.LastIndexOf('▀'), 20, 39);
        Assert.Contains("▀ ", row, StringComparison.Ordinal);
    }

    /// <summary>
    ///     What reaches the terminal as a graphics protocol is placed at a cell rather than written into
    ///     the frame, which the pane cannot clip, so a picture half scrolled off the pane is not drawn.
    /// </summary>
    [Fact]
    public void APictureIsLeftUndrawnUntilTheWholeOfItIsOnScreen()
    {
        var (surface, terminal) = CreateSurface(20, 3);
        var picture = new Picture { Protocol = ImageProtocol.Blocks };
        var view = new MarkdownView(Keymap)
        {
            Text = "one\n\n![shape](chart.png)",
            ShowScrollBar = false,
            Pictures = _ => picture,
            PictureRows = 3,
        };

        picture.Show(Enumerable.Repeat(new RGB(0xC9, 0x38, 0x2B), 40 * 40).ToArray(), 40, 40);

        view.Draw(surface.Frame);
        surface.Build();

        Assert.DoesNotContain("▀", terminal.WrittenText, StringComparison.Ordinal);

        view.Offset = 2;

        var (second, again) = CreateSurface(20, 3);

        view.Draw(second.Frame);
        second.Build();

        Assert.Contains("▀", again.WrittenText, StringComparison.Ordinal);
    }

    [Fact]
    public void AnEmptyDocumentStillDraws()
    {
        var lines = Drawn("");

        Assert.Equal("", lines[0]);
    }
}
