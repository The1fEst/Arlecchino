using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;

namespace Arlecchino.Tests;

public sealed class ScrollPaneTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    private static (Surface Surface, FakeTerminal Terminal) CreateSurface(int width = 24, int height = 12)
    {
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();
        return (surface, terminal);
    }

    private static string[] Render(Surface surface, FakeTerminal terminal)
    {
        surface.Build();
        return FrameText.Lines(terminal.Written);
    }

    private static ScrollPane PaneOf(int lines) => new(Keymap)
    {
        ContentHeight = () => lines,
        Content = region =>
        {
            for (var row = 0; row < lines; row++)
            {
                region.WriteLine(row, $"line {row}", Theme.Default);
            }
        },
    };

    [Fact]
    public void OnlyTheVisibleSliceIsDrawn()
    {
        var (surface, terminal) = CreateSurface();
        var pane = PaneOf(40);

        pane.Place(surface.Frame.Rows(0, 4));
        var lines = Render(surface, terminal);

        Assert.StartsWith("line 0", lines[0], StringComparison.Ordinal);
        Assert.StartsWith("line 3", lines[3], StringComparison.Ordinal);
        Assert.Equal("", lines[4].Trim());
    }

    [Fact]
    public void ScrollingMovesTheWindowOverTheContent()
    {
        var (surface, terminal) = CreateSurface();
        var pane = PaneOf(40);

        pane.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        pane.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        pane.Place(surface.Frame.Rows(0, 4));

        var lines = Render(surface, terminal);

        Assert.StartsWith("line 2", lines[0], StringComparison.Ordinal);
        Assert.StartsWith("line 5", lines[3], StringComparison.Ordinal);
    }

    [Fact]
    public void ContentOutsideThePaneNeverReachesItsNeighbours()
    {
        var (surface, terminal) = CreateSurface();
        var pane = PaneOf(40);
        var (top, bottom) = surface.Frame.SplitTop(3);

        bottom.WriteLine(0, "neighbour", Theme.Default);
        pane.Offset = 20;
        pane.Place(top);

        var lines = Render(surface, terminal);

        Assert.StartsWith("line 20", lines[0], StringComparison.Ordinal);
        Assert.StartsWith("line 22", lines[2], StringComparison.Ordinal);
        Assert.StartsWith("neighbour", lines[3], StringComparison.Ordinal);
    }

    [Fact]
    public void TheOffsetIsClampedToTheContent()
    {
        var (surface, _) = CreateSurface();
        var pane = PaneOf(6);

        pane.Offset = 100;
        pane.Place(surface.Frame.Rows(0, 4));

        Assert.Equal(2, pane.Offset);
    }

    [Fact]
    public void EndGoesToTheBottomAndHomeBack()
    {
        var (surface, _) = CreateSurface();
        var pane = PaneOf(20);
        var region = surface.Frame.Rows(0, 5);

        pane.Handle(new('\0', ConsoleKey.End, false, false, false));
        pane.Place(region);
        Assert.Equal(15, pane.Offset);

        pane.Handle(new('\0', ConsoleKey.Home, false, false, false));
        pane.Place(region);
        Assert.Equal(0, pane.Offset);
    }

    [Fact]
    public void ContentThatFitsNeedsNoScrollBar()
    {
        var (surface, terminal) = CreateSurface();
        var pane = PaneOf(2);

        pane.Place(surface.Frame.Rows(0, 5));
        var lines = Render(surface, terminal);

        Assert.StartsWith("line 1", lines[1], StringComparison.Ordinal);
        Assert.DoesNotContain("█", string.Join("", lines), StringComparison.Ordinal);
    }

    [Fact]
    public void TheWheelScrollsOnlyOverThePane()
    {
        var (surface, _) = CreateSurface();
        var pane = PaneOf(40);
        var region = surface.Frame.Rows(0, 4);

        pane.Place(region);

        var outside = pane.HandleMouse(
            new(MouseAction.ScrolledDown, MouseButton.None, region.Bottom + 1, region.Left, default));

        Assert.False(outside.WasHandled);
        Assert.Equal(0, pane.Offset);

        var inside = pane.HandleMouse(
            new(MouseAction.ScrolledDown, MouseButton.None, region.Top, region.Left, default));

        Assert.True(inside.WasHandled);
        Assert.Equal(1, pane.Offset);
    }
}
