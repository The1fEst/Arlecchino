using System;
using System.Linq;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Xunit;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Tests;

public sealed class PaneFlowTests
{
    [Fact]
    public void LinesFollowOneAnotherDownTheRegion()
    {
        var terminal = new FakeTerminal(20, 5);
        var surface = Frame(terminal);

        var flow = surface.Frame.Rows(1, 3).Flow();

        flow.AppendLine("first", Theme.Default);
        flow.AppendLine("second", Theme.Default);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.Equal("", lines[0].Trim());
        Assert.StartsWith("first", lines[1], StringComparison.Ordinal);
        Assert.StartsWith("second", lines[2], StringComparison.Ordinal);
    }

    [Fact]
    public void AFlowStaysInsideItsPaneInsteadOfWritingOverTheBorder()
    {
        var terminal = new FakeTerminal(40, 8);
        var surface = Frame(terminal);

        Branch(
                Columns,
                0.5,
                Leaf(
                    static region =>
                    {
                        var flow = region.Flow();

                        flow.AppendLine("PLAYERS", Theme.TableHeader);
                        flow.AppendLine("fEst", Theme.Default);
                    },
                    static () => "left"),
                Leaf(static region => region.WriteLine(0, "right", Theme.Muted), static () => "right"))
            .Draw(surface.Frame);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.Contains("left", lines[0], StringComparison.Ordinal);
        Assert.Contains("right", lines[0], StringComparison.Ordinal);
        Assert.DoesNotContain("PLAYERS", lines[0], StringComparison.Ordinal);

        Assert.Contains("PLAYERS", lines[1], StringComparison.Ordinal);
        Assert.Contains("fEst", lines[2], StringComparison.Ordinal);
    }

    [Fact]
    public void ALoopLongerThanThePaneStopsAtItsEdge()
    {
        var terminal = new FakeTerminal(20, 6);
        var surface = Frame(terminal);
        var region = surface.Frame.Rows(0, 3);
        var flow = region.Flow();

        foreach (var index in Enumerable.Range(0, 20))
        {
            flow.AppendLine($"row {index}", Theme.Default);
        }

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.StartsWith("row 2", lines[2], StringComparison.Ordinal);
        Assert.Equal("", lines[3].Trim());
        Assert.True(flow.IsFull);
        Assert.Equal(0, flow.FreeLines);
    }

    [Fact]
    public void SkippingAndRulesMoveTheCursorToo()
    {
        var terminal = new FakeTerminal(10, 6);
        var surface = Frame(terminal);
        var flow = surface.Frame.Flow();

        flow.AppendLine("title", Theme.Default);
        flow.FillLine();
        flow.SkipLine();
        flow.AppendLine("body", Theme.Default);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.StartsWith("title", lines[0], StringComparison.Ordinal);
        Assert.Equal("----------", lines[1]);
        Assert.Equal("", lines[2].Trim());
        Assert.StartsWith("body", lines[3], StringComparison.Ordinal);
        Assert.Equal(4, flow.Row);
    }

    [Fact]
    public void WhatIsLeftIsARegionOfItsOwn()
    {
        var surface = Frame(new(20, 10));
        var flow = surface.Frame.Flow();

        flow.AppendLine("header", Theme.Default);
        flow.SkipLine();

        var rest = flow.Rest();

        Assert.Equal(2, rest.Top);
        Assert.Equal(8, rest.Height);
        Assert.Equal(20, rest.Width);
    }

    [Fact]
    public void RewindingWritesOverWhatWasThere()
    {
        var terminal = new FakeTerminal(12, 3);
        var surface = Frame(terminal);
        var flow = surface.Frame.Flow();

        flow.AppendLine("first", Theme.Default);
        flow.Rewind();
        flow.AppendLine("second", Theme.Default);

        surface.Build();

        Assert.StartsWith("second", FrameText.Lines(terminal.Written)[0], StringComparison.Ordinal);
        Assert.Equal(1, flow.Row);
    }

    [Fact]
    public void TwoFlowsOverOneRegionDoNotShareACursor()
    {
        var surface = Frame(new(20, 6));
        var region = surface.Frame;

        var first = region.Flow();
        first.AppendLine("one", Theme.Default);

        var second = region.Flow();

        Assert.Equal(1, first.Row);
        Assert.Equal(0, second.Row);
        Assert.Equal(region, second.Region);
    }

    [Fact]
    public void AlignmentIsTheRegionsOwn()
    {
        var terminal = new FakeTerminal(11, 3);
        var surface = Frame(terminal);
        var flow = surface.Frame.Rows(0, 2).Flow();

        flow.AppendLine("mid", Theme.Default, Align.Center);
        flow.AppendLine("end", Theme.Default, Align.Right);

        surface.Build();

        var lines = FrameText.Lines(terminal.Written);

        Assert.Equal("    mid", lines[0].TrimEnd());
        Assert.Equal("        end", lines[1]);
    }

    private static Surface Frame(FakeTerminal terminal)
    {
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();

        return surface;
    }
}
