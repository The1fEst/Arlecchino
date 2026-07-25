using System;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Xunit;

namespace Arlecchino.Tests;

public sealed class SurfaceTests
{
    private const int Width = 40;
    private const int Height = 10;

    private static (Surface Surface, FakeTerminal Terminal) CreateSurface(int horizontalPadding, int verticalPadding)
    {
        var terminal = new FakeTerminal(Width, Height);
        var surface = new Surface(terminal)
        {
            HorizontalPadding = horizontalPadding,
            VerticalPadding = verticalPadding,
        };

        surface.StartFrame();
        return (surface, terminal);
    }

    private static string[] Render(Surface surface, FakeTerminal terminal)
    {
        surface.Build();
        return FrameText.Lines(terminal.Written);
    }

    [Fact]
    public void AppendLineHonoursHorizontalPadding()
    {
        var (surface, terminal) = CreateSurface(2, 0);

        surface.AppendLine("left", Theme.Default);

        Assert.StartsWith("  left", Render(surface, terminal)[0]);
    }

    [Fact]
    public void AppendLineCentersWithinContentWidth()
    {
        var (surface, terminal) = CreateSurface(2, 0);

        surface.AppendLine("abcd", Theme.Default, Align.Center);

        var contentWidth = Width - 4;
        var expectedColumn = 2 + (contentWidth - 4) / 2;

        Assert.Equal(expectedColumn, Render(surface, terminal)[0].IndexOf("abcd", StringComparison.Ordinal));
    }

    [Fact]
    public void AppendLineClipsTextToContentWidth()
    {
        var (surface, terminal) = CreateSurface(2, 0);

        surface.AppendLine(new('x', Width * 2), Theme.Default);

        var line = Render(surface, terminal)[0];
        Assert.Equal(Width, line.Length);
        Assert.Equal(Width - 4, line.Trim().Length);
    }

    [Fact]
    public void FlowStopsWhenFrameIsFull()
    {
        var (surface, terminal) = CreateSurface(2, 0);

        for (var i = 0; i < Height * 2; i++)
        {
            surface.AppendLine("row", Theme.Default);
        }

        Assert.Equal(Height, Render(surface, terminal).Length);
    }

    [Fact]
    public void VerticalPaddingSkipsLeadingLines()
    {
        var (surface, terminal) = CreateSurface(0, 2);

        surface.AppendLine("first", Theme.Default);

        var lines = Render(surface, terminal);
        Assert.Equal("", lines[0].Trim());
        Assert.Equal("", lines[1].Trim());
        Assert.Equal("first", lines[2].Trim());
    }

    [Fact]
    public void WriteTableRowPadsColumnsBySign()
    {
        var (surface, terminal) = CreateSurface(0, 0);

        surface.WriteTableRow(["a", "b"], [-4, 4], Theme.Default);

        Assert.Equal("a      b", Render(surface, terminal)[0].TrimEnd());
    }

    [Fact]
    public void WriteAtIgnoresPositionsOutsideTheFrame()
    {
        var (surface, terminal) = CreateSurface(0, 0);

        surface.WriteAt(-1, 0, "above", Theme.Default);
        surface.WriteAt(Height, 0, "below", Theme.Default);
        surface.WriteAt(0, Width - 2, "clipped", Theme.Default);

        var lines = Render(surface, terminal);
        Assert.Equal(Height, lines.Length);
        Assert.Equal("cl", lines[0].Trim());
    }

    [Fact]
    public void ListWindowLeavesRoomForChrome()
    {
        var (surface, _) = CreateSurface(2, 1);

        Assert.True(surface.ListWindow() >= 4);
        Assert.True(surface.ListWindow() < Height);
    }

    [Fact]
    public void FixedSizeIgnoresTerminalSize()
    {
        var surface = new Surface(new FakeTerminal(Width, Height));

        surface.SetFixedSize(11, 5);
        surface.StartFrame();

        Assert.Equal(11, surface.FrameWidth);
        Assert.Equal(5, surface.FrameHeight);
    }
}
