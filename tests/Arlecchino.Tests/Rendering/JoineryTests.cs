using System;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Testing;
using Xunit;

namespace Arlecchino.Tests.Rendering;

public sealed class JoineryTests
{
    [Fact]
    public void ABoxOnItsOwnLooksLikeABorder()
    {
        var lines = Draw(12, 3, (surface, joinery) => joinery.Box(surface.Frame));

        Assert.Equal("╭──────────╮", lines[0]);
        Assert.Equal("│          │", lines[1]);
        Assert.Equal("╰──────────╯", lines[2]);
    }

    [Fact]
    public void TwoPanesThatTouchShareOneLine()
    {
        var lines = Draw(12,
            3,
            (surface, joinery) =>
            {
                var (left, right) = surface.Frame.SplitLeft(6);

                joinery.Box(left);
                joinery.Box(new(right.Surface, right.Left - 1, right.Top, right.Width + 1, right.Height));
            });

        Assert.Equal("╭────┬─────╮", lines[0]);
        Assert.Equal("│    │     │", lines[1]);
        Assert.Equal("╰────┴─────╯", lines[2]);
    }

    [Fact]
    public void FourPanesMeetInACross()
    {
        var lines = Draw(10,
            5,
            (surface, joinery) =>
            {
                joinery.Box(new(surface.Frame.Surface, 0, 0, 6, 3));
                joinery.Box(new(surface.Frame.Surface, 5, 0, 5, 3));
                joinery.Box(new(surface.Frame.Surface, 0, 2, 6, 3));
                joinery.Box(new(surface.Frame.Surface, 5, 2, 5, 3));
            });

        Assert.Equal("╭────┬───╮", lines[0]);
        Assert.Equal("├────┼───┤", lines[2]);
        Assert.Equal("╰────┴───╯", lines[4]);
    }

    [Fact]
    public void ARuleJoinsTheBoxAroundIt()
    {
        var lines = Draw(8,
            5,
            (surface, joinery) =>
            {
                joinery.Box(surface.Frame);
                joinery.Across(surface.Frame, 2);
            });

        Assert.Equal("╭──────╮", lines[0]);
        Assert.Equal("├──────┤", lines[2]);
        Assert.Equal("╰──────╯", lines[4]);
    }

    [Fact]
    public void ARuleDownJoinsItToo()
    {
        var lines = Draw(7,
            3,
            (surface, joinery) =>
            {
                joinery.Box(surface.Frame);
                joinery.Down(surface.Frame, 3);
            });

        Assert.Equal("╭──┬──╮", lines[0]);
        Assert.Equal("│  │  │", lines[1]);
        Assert.Equal("╰──┴──╯", lines[2]);
    }

    [Fact]
    public void ATitleGoesIntoTheTopEdge()
    {
        var lines = Draw(14, 3, (surface, joinery) => joinery.Box(surface.Frame, title: "files"));

        Assert.Equal("╭─ files ────╮", lines[0]);
    }

    [Fact]
    public void ATitleTooLongForTheEdgeIsCut()
    {
        var lines = Draw(10, 3, (surface, joinery) => joinery.Box(surface.Frame, title: "a very long name"));

        Assert.Equal(10, lines[0].Length);
        Assert.StartsWith("╭─ a ver", lines[0], StringComparison.Ordinal);
    }

    [Fact]
    public void WhatIsRecordedLastColoursWhatIsShared()
    {
        var terminal = new FakeTerminal(12, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var joinery = new Joinery();

        surface.StartFrame();

        var (left, right) = surface.Frame.SplitLeft(6);

        joinery.Box(left, Theme.Info);
        joinery.Box(new(right.Surface, right.Left - 1, right.Top, right.Width + 1, right.Height), Theme.Active);
        joinery.Draw(surface.Frame, Theme.Info);

        surface.Build();

        Assert.Contains(Theme.Active.Ansi, terminal.WrittenText, StringComparison.Ordinal);
    }

    [Fact]
    public void NothingIsDrawnUntilItIsAskedFor()
    {
        var terminal = new FakeTerminal(8, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var joinery = new Joinery();

        surface.StartFrame();
        joinery.Box(surface.Frame);
        surface.Build();

        Assert.Equal(18, joinery.Count);
        Assert.All(FrameText.Lines(terminal.WrittenText), line => Assert.Equal("", line.Trim()));
    }

    [Fact]
    public void WhatFallsOutsideIsLeftUndrawn()
    {
        var lines = Draw(6,
            3,
            (surface, joinery) =>
                joinery.Box(new(surface.Frame.Surface, -2, 0, 6, 3)));

        Assert.Equal("───╮  ", lines[0]);
        Assert.Equal("   │  ", lines[1]);
    }

    [Fact]
    public void ABoxWithNoRoomIsNotDrawn()
    {
        var lines = Draw(6, 3, (surface, joinery) => joinery.Box(surface.Frame.Rows(0, 1)));

        Assert.All(lines, line => Assert.Equal("", line.Trim()));
    }

    [Fact]
    public void TheInsideIsWhatComesBack()
    {
        var terminal = new FakeTerminal(10, 4);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var joinery = new Joinery();

        surface.StartFrame();

        var content = joinery.Box(surface.Frame, title: "files");

        Assert.Equal(1, content.Left);
        Assert.Equal(1, content.Top);
        Assert.Equal(8, content.Width);
        Assert.Equal(2, content.Height);
    }

    private static string[] Draw(int width, int height, Action<Surface, Joinery> record)
    {
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var joinery = new Joinery();

        surface.StartFrame();
        record(surface, joinery);
        joinery.Draw(surface.Frame, Theme.Info);
        surface.Build();

        return FrameText.Lines(terminal.WrittenText);
    }
}
