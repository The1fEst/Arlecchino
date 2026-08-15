using System;
using System.Collections.Generic;
using System.Linq;
using Arlecchino.Focus;
using Arlecchino.Input;
using Arlecchino.Layout;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Arlecchino.Widgets.Readouts;
using Xunit;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Rendering;

public sealed class PaneTreeTests
{
    [Fact]
    public void OnePaneIsHandedTheWholeRegion()
    {
        var only = new Probe();

        Leaf(only.Draw).Draw(Frame(80, 24));

        Assert.Equal(80, only.Region.Width);
        Assert.Equal(24, only.Region.Height);
    }

    [Fact]
    public void AShareOfTheWidthGoesToTheLeftHalf()
    {
        var tree = new Probe();
        var editor = new Probe();

        Branch(Columns, 0.25, Leaf(tree.Draw), Leaf(editor.Draw)).Draw(Frame(80, 24));

        Assert.Equal(0, tree.Region.Left);
        Assert.Equal(20, tree.Region.Width);
        Assert.Equal(20, editor.Region.Left);
        Assert.Equal(60, editor.Region.Width);
        Assert.Equal(24, editor.Region.Height);
    }

    [Fact]
    public void AShareOfTheHeightGoesToTheTopHalf()
    {
        var editor = new Probe();
        var log = new Probe();

        Branch(Rows, 0.5, Leaf(editor.Draw), Leaf(log.Draw)).Draw(Frame(80, 24));

        Assert.Equal(12, editor.Region.Height);
        Assert.Equal(12, log.Region.Top);
        Assert.Equal(80, log.Region.Width);
    }

    [Fact]
    public void ABranchOfTwoLeavesHalvesTheSpaceItIsGiven()
    {
        var left = new Probe();
        var right = new Probe();

        Branch(Leaf(left.Draw), Leaf(right.Draw)).Draw(Frame(80, 24));

        Assert.Equal(40, left.Region.Width);
        Assert.Equal(40, right.Region.Width);
    }

    [Fact]
    public void ABranchThatWasNotToldWhichWayCutsAlongTheLongerSide()
    {
        var first = new Probe();
        var second = new Probe();
        var layout = Branch(Leaf(first.Draw), Leaf(second.Draw));

        layout.Draw(Frame(80, 24));

        Assert.Equal(40, first.Region.Width);
        Assert.Equal(24, first.Region.Height);

        layout.Draw(Frame(40, 24));

        Assert.Equal(40, first.Region.Width);
        Assert.Equal(12, first.Region.Height);
    }

    [Fact]
    public void TheLongerSideIsWhatTheEyeSeesRatherThanTheCellCount()
    {
        var first = new Probe();

        Branch(Leaf(first.Draw), Leaf()).Draw(Frame(48, 24));

        Assert.Equal(24, first.Region.Width);
        Assert.Equal(24, first.Region.Height);

        Branch(Leaf(first.Draw), Leaf()).Draw(Frame(47, 24));

        Assert.Equal(47, first.Region.Width);
        Assert.Equal(12, first.Region.Height);
    }

    [Fact]
    public void ASizeWithoutADirectionStillCutsAlongTheLongerSide()
    {
        var first = new Probe();
        var second = new Probe();

        Branch(0.25, Leaf(first.Draw), Leaf(second.Draw)).Draw(Frame(80, 24));

        Assert.Equal(20, first.Region.Width);
        Assert.Equal(24, first.Region.Height);
        Assert.Equal(60, second.Region.Width);
    }

    [Fact]
    public void ADirectionWithoutASizeHalvesTheSpace()
    {
        var top = new Probe();
        var bottom = new Probe();

        Branch(Rows, Leaf(top.Draw), Leaf(bottom.Draw)).Draw(Frame(80, 24));

        Assert.Equal(12, top.Region.Height);
        Assert.Equal(12, bottom.Region.Top);
    }

    [Fact]
    public void ACountOfCellsIsTheSameAtEverySize()
    {
        var toolbar = new Probe();
        var body = new Probe();
        var layout = Branch(Rows, 3, Leaf(toolbar.Draw), Leaf(body.Draw));

        foreach (var height in new[] { 10, 24, 60 })
        {
            layout.Draw(Frame(80, height));

            Assert.Equal(3, toolbar.Region.Height);
            Assert.Equal(height - 3, body.Region.Height);
        }
    }

    [Fact]
    public void CellsFromTheEndPutAStatusBarOnTheLastRow()
    {
        var body = new Probe();
        var status = new Probe();

        Branch(Rows, PaneSize.CellsFromEnd(1), Leaf(body.Draw), Leaf(status.Draw)).Draw(Frame(80, 24));

        Assert.Equal(23, body.Region.Height);
        Assert.Equal(23, status.Region.Top);
        Assert.Equal(1, status.Region.Height);
    }

    [Fact]
    public void NestedBranchesBuildAWholeScreen()
    {
        var toolbar = new Probe();
        var tree = new Probe();
        var editor = new Probe();
        var log = new Probe();

        Branch(
                Rows,
                3,
                Leaf(toolbar.Draw),
                Branch(
                    Columns,
                    0.25,
                    Leaf(tree.Draw),
                    Branch(Rows, 0.75, Leaf(editor.Draw), Leaf(log.Draw))))
            .Draw(Frame(100, 40));

        Assert.Equal(100, toolbar.Region.Width);
        Assert.Equal(3, toolbar.Region.Height);

        Assert.Equal(3, tree.Region.Top);
        Assert.Equal(25, tree.Region.Width);
        Assert.Equal(37, tree.Region.Height);

        Assert.Equal(25, editor.Region.Left);
        Assert.Equal(75, editor.Region.Width);
        Assert.Equal(28, editor.Region.Height);

        Assert.Equal(31, log.Region.Top);
        Assert.Equal(9, log.Region.Height);
    }

    [Fact]
    public void PanesCoverTheRegionCompletelyAndNeverOverlap()
    {
        var panes = new[] { new Probe(), new Probe(), new Probe(), new Probe(), new Probe() };

        Branch(
                Columns,
                0.3,
                Branch(Rows, 4, Leaf(panes[0].Draw), Leaf(panes[1].Draw)),
                Branch(
                    Rows,
                    PaneSize.CellsFromEnd(2),
                    Branch(Leaf(panes[2].Draw), Leaf(panes[3].Draw)),
                    Leaf(panes[4].Draw)))
            .Draw(Frame(100, 30));

        var covered = new HashSet<(int Row, int Column)>();

        foreach (var pane in panes)
        {
            for (var row = pane.Region.Top; row < pane.Region.Bottom; row++)
            {
                for (var column = pane.Region.Left; column < pane.Region.Right; column++)
                {
                    Assert.True(covered.Add((row, column)), $"cell {row},{column} belongs to two panes");
                }
            }
        }

        Assert.Equal(100 * 30, covered.Count);
    }

    [Fact]
    public void TheInnerGapIsTakenOutBetweenTheHalvesOfEveryBranch()
    {
        var left = new Probe();
        var right = new Probe();

        Branch(Columns, 0.5, Leaf(left.Draw), Leaf(right.Draw)).Gaps(inner: 2).Draw(Frame(80, 24));

        Assert.Equal(0, left.Region.Left);
        Assert.Equal(39, left.Region.Width);
        Assert.Equal(41, right.Region.Left);
        Assert.Equal(39, right.Region.Width);
    }

    [Fact]
    public void TheOuterGapIsTakenOutAroundEverything()
    {
        var left = new Probe();
        var right = new Probe();

        Branch(Columns, 0.5, Leaf(left.Draw), Leaf(right.Draw)).Gaps(inner: 0, outer: 2).Draw(Frame(80, 24));

        Assert.Equal(2, left.Region.Left);
        Assert.Equal(2, left.Region.Top);
        Assert.Equal(20, left.Region.Height);
        Assert.Equal(38, left.Region.Width);
        Assert.Equal(38, right.Region.Width);
        Assert.Equal(78, right.Region.Right);
    }

    [Fact]
    public void GapsBelongToTheTreeRatherThanToACall()
    {
        var left = new Probe();
        var layout = Branch(Columns, 0.5, Leaf(left.Draw), Leaf());

        Assert.Same(layout, layout.Gaps(inner: 1, outer: 3));

        Assert.Equal(1, layout.InnerGap);
        Assert.Equal(3, layout.OuterGap);

        layout.Draw(Frame(80, 24));

        Assert.Equal(3, left.Region.Left);
        Assert.Equal(36, left.Region.Width);
    }

    [Fact]
    public void TheSameTreeFitsAnyTerminal()
    {
        var left = new Probe();
        var layout = Branch(Columns, 0.5, Leaf(left.Draw), Leaf());

        layout.Draw(Frame(40, 10));
        Assert.Equal(20, left.Region.Width);

        layout.Draw(Frame(120, 10));
        Assert.Equal(60, left.Region.Width);
    }

    [Fact]
    public void APaneThatDidNotFitIsHandedNothingAndDrawsNothing()
    {
        var terminal = new FakeTerminal(20, 2);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var body = new Probe { Text = "invisible" };

        surface.StartFrame();

        Branch(Rows, 3, Leaf(), Leaf(body.Draw)).Draw(surface.Frame);

        surface.Build();

        Assert.True(body.Region.IsEmpty);
        Assert.DoesNotContain("invisible", FrameText.WithoutStyles(terminal.Written), StringComparison.Ordinal);
    }

    [Fact]
    public void AShareIsClampedToWhatAShareCanBe()
    {
        var left = new Probe();
        var right = new Probe();

        Branch(Columns, PaneSize.Fraction(4), Leaf(left.Draw), Leaf(right.Draw)).Draw(Frame(80, 24));

        Assert.Equal(80, left.Region.Width);
        Assert.True(right.Region.IsEmpty);
    }

    [Fact]
    public void AWidgetIsDrawnWhereTheTreePutIt()
    {
        var terminal = new FakeTerminal(40, 6);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var bar = new StatusBar { Left = [static () => "ready"] };

        surface.StartFrame();

        Branch(Rows, PaneSize.CellsFromEnd(1), Leaf(), Leaf(bar)).Draw(surface.Frame);

        surface.Build();

        Assert.Contains("ready", FrameText.Lines(terminal.Written)[^1], StringComparison.Ordinal);
    }

    [Fact]
    public void ATreeCountsThePanesItHolds()
    {
        var layout = Branch(
            Rows,
            3,
            Leaf(),
            Branch(Columns, 0.5, Leaf(), Branch(Leaf(), Leaf())));

        Assert.Equal(4, layout.Count);
    }

    [Fact]
    public void ATitleDrawsABoxAndLeavesTheRoomInsideToThePane()
    {
        var terminal = new FakeTerminal(20, 5);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var body = new Probe { Text = "inside" };

        surface.StartFrame();

        Leaf(body.Draw, static () => "log").Draw(surface.Frame);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.Contains("log", lines[0], StringComparison.Ordinal);
        Assert.Contains("inside", lines[1], StringComparison.Ordinal);

        Assert.Equal(1, body.Region.Top);
        Assert.Equal(1, body.Region.Left);
        Assert.Equal(18, body.Region.Width);
    }

    [Fact]
    public void TheBoxOfTheFocusedPaneIsDrawnDifferently()
    {
        using var colours = new ColorSupportScope(ColorSupport.TrueColor);

        var focused = Styles(new() { IsFocused = true });
        var unfocused = Styles(new() { IsFocused = false });

        Assert.Contains(Theme.Active.Ansi, focused, StringComparison.Ordinal);
        Assert.DoesNotContain(Theme.Active.Ansi, unfocused, StringComparison.Ordinal);
        Assert.Contains(Theme.Info.Ansi, unfocused, StringComparison.Ordinal);
    }

    [Fact]
    public void TheRingWalksThePanesInLayoutOrder()
    {
        using var app = new TestApplication();

        var first = new Badge();
        var second = new Badge();
        var third = new Badge();

        var ring = Branch(
                Rows,
                3,
                Leaf(first),
                Branch(Columns, 0.5, Leaf(second), Branch(Leaf(new StatusBar()), Leaf(third))))
            .AsFocusRing(app.Options.Keymap);

        Assert.Equal([first, second, third], ring.Items);
    }

    [Fact]
    public void APaneThatCannotTakeFocusIsNotInTheRing()
    {
        using var app = new TestApplication();

        var ring = Branch(Leaf(new StatusBar()), Leaf(static _ => { })).AsFocusRing(app.Options.Keymap);

        Assert.Empty(ring.Items);
    }

    [Fact]
    public void ATreeBuildsTheFocusRingOfTheScreen()
    {
        using var app = new TestApplication();

        var first = new Badge();
        var second = new Badge();

        var ring = Branch(Leaf(first), Leaf(second)).AsFocusRing(app.Options.Keymap);

        Assert.Equal([first, second], ring.Items);
        Assert.True(first.IsFocused);
        Assert.False(second.IsFocused);
    }

    [Fact]
    public void TheSameWidgetCannotBeTwoPanes()
    {
        var badge = new Badge();

        var failure = Assert.Throws<ArgumentException>(() => Branch(Leaf(badge), Leaf(badge)));

        Assert.Contains("Badge", failure.Message, StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() =>
            Branch(Rows, 3, Leaf(badge), Branch(Columns, 0.5, Leaf(new Badge()), Leaf(badge))));
    }

    [Fact]
    public void PanesWithNoGapBetweenThemShareOneLine()
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();

        Branch(Columns, 0.5, Leaf(static _ => { }, static () => "a"), Leaf(static _ => { }, static () => "b"))
            .Gaps(inner: 0)
            .Draw(surface.Frame);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.Contains('┬', lines[0]);
        Assert.Contains('┴', lines[3]);
        Assert.DoesNotContain("╮╭", lines[0]);
    }

    [Fact]
    public void AGapLeavesThemStandingApart()
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();

        Branch(Columns, 0.5, Leaf(static _ => { }, static () => "a"), Leaf(static _ => { }, static () => "b"))
            .Gaps(inner: 1)
            .Draw(surface.Frame);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.DoesNotContain('┬', lines[0]);
        Assert.Equal(2, lines[0].Count(static symbol => symbol == '╭'));
    }

    [Fact]
    public void APaneWithoutABoxIsNotPulledOntoItsNeighbour()
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var body = new Probe();

        surface.StartFrame();

        Branch(Columns, 0.5, Leaf(static _ => { }, static () => "a"), Leaf(body.Draw))
            .Gaps(inner: 0)
            .Draw(surface.Frame);

        surface.Build();

        Assert.Equal(10, body.Region.Left);
        Assert.Equal(10, body.Region.Width);
    }

    [Fact]
    public void ThePaneWithTheFocusOwnsTheEdgeItShares()
    {
        using var app = new TestApplication();

        var first = new Badge();
        var second = new Badge();
        var layout = Branch(Columns, 0.5, Leaf(first, static () => "a"), Leaf(second, static () => "b"))
            .Gaps(inner: 0);

        var ring = layout.AsFocusRing(app.Options.Keymap);

        Assert.True(first.IsFocused);
        Assert.Equal(Theme.Active.Ansi, EdgeStyle(layout));

        ring.FocusNext();

        Assert.True(second.IsFocused);
        Assert.Equal(Theme.Active.Ansi, EdgeStyle(layout));
    }

    private static string EdgeStyle(PaneTree layout)
    {
        var terminal = new FakeTerminal(20, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        layout.Draw(surface.Frame);
        surface.Build();

        var written = terminal.Written;
        var shared = written.IndexOf('┬');

        return FrameText.StylesIn(written[..shared]).LastOrDefault() ?? "";
    }

    private static SurfaceRegion Frame(int width, int height)
    {
        var surface = new Surface(new FakeTerminal(width, height))
        {
            HorizontalPadding = 0,
            VerticalPadding = 0,
        };

        surface.StartFrame();

        return surface.Frame;
    }

    private static string Styles(Badge badge)
    {
        var terminal = new FakeTerminal(20, 5);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();

        Leaf(badge, static () => "pane").Draw(surface.Frame);

        surface.Build();

        return terminal.Written;
    }

    private sealed class Badge : IArlecchinoInteractiveWidget
    {
        public bool IsFocused { get; set; }

        public SurfaceRegion Draw(SurfaceRegion region) => region;

        public FocusResult Handle(KeyPress key) => FocusResult.Ignored;

        public FocusResult HandleMouse(MouseEvent mouse) => FocusResult.Ignored;
    }

    private sealed class Probe
    {
        public SurfaceRegion Region { get; private set; }

        public string Text { get; init; } = "";

        public void Draw(SurfaceRegion region)
        {
            Region = region;

            if (Text.Length > 0 && !region.IsEmpty)
            {
                region.WriteLine(0, Text, Theme.Default);
            }
        }
    }
}
