using System;
using System.Collections.Generic;
using Arlecchino.Layout;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;

namespace Arlecchino.Tests;

public sealed class PaneTreeTests
{
    [Fact]
    public void OnePaneIsHandedTheWholeRegion()
    {
        var only = new Probe();

        PaneTree.Pane(only.Draw).Draw(Frame(80, 24));

        Assert.Equal(80, only.Region.Width);
        Assert.Equal(24, only.Region.Height);
    }

    [Fact]
    public void AShareOfTheWidthGoesToTheLeftHalf()
    {
        var tree = new Probe();
        var editor = new Probe();

        PaneTree.Columns(0.25, PaneTree.Pane(tree.Draw), PaneTree.Pane(editor.Draw)).Draw(Frame(80, 24));

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

        PaneTree.Rows(0.5, PaneTree.Pane(editor.Draw), PaneTree.Pane(log.Draw)).Draw(Frame(80, 24));

        Assert.Equal(12, editor.Region.Height);
        Assert.Equal(12, log.Region.Top);
        Assert.Equal(80, log.Region.Width);
    }

    [Fact]
    public void ACountOfCellsIsTheSameAtEverySize()
    {
        var toolbar = new Probe();
        var body = new Probe();
        var layout = PaneTree.Rows(3, PaneTree.Pane(toolbar.Draw), PaneTree.Pane(body.Draw));

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

        PaneTree
            .Rows(PaneSize.CellsFromEnd(1), PaneTree.Pane(body.Draw), PaneTree.Pane(status.Draw))
            .Draw(Frame(80, 24));

        Assert.Equal(23, body.Region.Height);
        Assert.Equal(23, status.Region.Top);
        Assert.Equal(1, status.Region.Height);
    }

    [Fact]
    public void NestedSplitsBuildAWholeScreen()
    {
        var toolbar = new Probe();
        var tree = new Probe();
        var editor = new Probe();
        var log = new Probe();

        PaneTree.Rows(
                3,
                PaneTree.Pane(toolbar.Draw),
                PaneTree.Columns(
                    0.25,
                    PaneTree.Pane(tree.Draw),
                    PaneTree.Rows(0.75, PaneTree.Pane(editor.Draw), PaneTree.Pane(log.Draw))))
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

        PaneTree.Columns(
                0.3,
                PaneTree.Rows(4, PaneTree.Pane(panes[0].Draw), PaneTree.Pane(panes[1].Draw)),
                PaneTree.Rows(
                    PaneSize.CellsFromEnd(2),
                    PaneTree.Columns(0.5, PaneTree.Pane(panes[2].Draw), PaneTree.Pane(panes[3].Draw)),
                    PaneTree.Pane(panes[4].Draw)))
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
    public void AGapIsTakenOutBetweenTheHalves()
    {
        var left = new Probe();
        var right = new Probe();

        PaneTree.Columns(0.5, PaneTree.Pane(left.Draw), PaneTree.Pane(right.Draw)).Draw(Frame(80, 24), gap: 2);

        Assert.Equal(0, left.Region.Left);
        Assert.Equal(39, left.Region.Width);
        Assert.Equal(41, right.Region.Left);
        Assert.Equal(39, right.Region.Width);
    }

    [Fact]
    public void TheSameTreeFitsAnyTerminal()
    {
        var left = new Probe();
        var layout = PaneTree.Columns(0.5, PaneTree.Pane(left.Draw), PaneTree.Empty());

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

        PaneTree.Rows(3, PaneTree.Empty(), PaneTree.Pane(body.Draw)).Draw(surface.Frame);

        surface.Build();

        Assert.True(body.Region.IsEmpty);
        Assert.DoesNotContain("invisible", FrameText.WithoutStyles(terminal.Written), StringComparison.Ordinal);
    }

    [Fact]
    public void AShareIsClampedToWhatAShareCanBe()
    {
        var left = new Probe();
        var right = new Probe();

        PaneTree
            .Columns(PaneSize.Fraction(4), PaneTree.Pane(left.Draw), PaneTree.Pane(right.Draw))
            .Draw(Frame(80, 24));

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

        PaneTree.Rows(PaneSize.CellsFromEnd(1), PaneTree.Empty(), PaneTree.Pane(bar)).Draw(surface.Frame);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.Contains("ready", lines[^1], StringComparison.Ordinal);
    }

    [Fact]
    public void ATreeCountsThePanesItHolds()
    {
        var layout = PaneTree.Rows(
            3,
            PaneTree.Empty(),
            PaneTree.Columns(0.5, PaneTree.Empty(), PaneTree.Rows(0.5, PaneTree.Empty(), PaneTree.Empty())));

        Assert.Equal(4, layout.Count);
    }

    [Fact]
    public void APaneNeedsSomethingToDraw()
    {
        Assert.Throws<ArgumentNullException>(static () => PaneTree.Pane((IArlecchinoWidget)null!));
        Assert.Throws<ArgumentNullException>(static () => PaneTree.Pane((Action<SurfaceRegion>)null!));
        Assert.Throws<ArgumentNullException>(static () => PaneTree.Rows(0.5, PaneTree.Empty(), null!));
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
