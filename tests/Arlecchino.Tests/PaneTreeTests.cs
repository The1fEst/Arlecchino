using System;
using System.Collections.Generic;
using Arlecchino.Layout;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Tests;

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
    public void APaneNeedsSomethingToDraw()
    {
        Assert.Throws<ArgumentNullException>(static () => Leaf((IArlecchinoWidget)null!));
        Assert.Throws<ArgumentNullException>(static () => Leaf((Action<SurfaceRegion>)null!));
        Assert.Throws<ArgumentNullException>(static () => Branch(Leaf(), null!));
        Assert.Throws<ArgumentNullException>(static () => Branch(Rows, 0.5, null!, Leaf()));
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
