using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Testing;
using Xunit;

namespace Arlecchino.Tests.Rendering;

public sealed class RegionTests
{
    private const int Width = 20;
    private const int Height = 8;

    private static (Surface Surface, FakeTerminal Terminal) CreateSurface()
    {
        var terminal = new FakeTerminal(Width, Height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        surface.StartFrame();
        return (surface, terminal);
    }

    private static string[] Render(Surface surface, FakeTerminal terminal)
    {
        surface.Build();
        return FrameText.Lines(terminal.WrittenText);
    }

    [Fact]
    public void WritingIsClippedToTheRegion()
    {
        var (surface, terminal) = CreateSurface();
        var region = new SurfaceRegion(surface, 4, 1, 6, 2);

        region.Write(0, 0, "abcdefghij", Theme.Default);
        region.Write(5, 0, "below", Theme.Default);
        region.Write(0, -2, "xy", Theme.Default);

        var lines = Render(surface, terminal);
        Assert.Equal("    abcdef", lines[1].TrimEnd());
        Assert.Equal("", lines[5].Trim());
    }

    [Fact]
    public void CoordinatesAreLocalToTheRegion()
    {
        var (surface, terminal) = CreateSurface();
        var region = new SurfaceRegion(surface, 3, 2, 10, 3);

        region.Write(1, 2, "here", Theme.Default);

        var lines = Render(surface, terminal);
        Assert.Equal("     here", lines[3].TrimEnd());
    }

    [Fact]
    public void InsetShrinksOnEverySide()
    {
        var (surface, _) = CreateSurface();
        var region = new SurfaceRegion(surface, 0, 0, 10, 6).Inset(new Margin(1, 2, 3, 4));

        Assert.Equal(1, region.Left);
        Assert.Equal(2, region.Top);
        Assert.Equal(6, region.Width);
        Assert.Equal(0, region.Height);
    }

    [Fact]
    public void SplittingKeepsTheTotalSize()
    {
        var (surface, _) = CreateSurface();
        var (left, right) = new SurfaceRegion(surface, 0, 0, 20, 5).SplitLeft(7);

        Assert.Equal(7, left.Width);
        Assert.Equal(13, right.Width);
        Assert.Equal(7, right.Left);

        var (top, bottom) = new SurfaceRegion(surface, 0, 0, 20, 5).SplitTop(2);
        Assert.Equal(2, top.Height);
        Assert.Equal(3, bottom.Height);
        Assert.Equal(2, bottom.Top);
    }

    [Fact]
    public void SplittingClampsToTheRegion()
    {
        var (surface, _) = CreateSurface();
        var (left, right) = new SurfaceRegion(surface, 0, 0, 10, 5).SplitLeft(50);

        Assert.Equal(10, left.Width);
        Assert.True(right.IsEmpty);
    }

    [Fact]
    public void BorderDrawsAFrameAndReturnsTheInside()
    {
        var (surface, terminal) = CreateSurface();
        var content = new SurfaceRegion(surface, 0, 0, 12, 4).Border(Theme.Info, "Hi");

        content.Write(0, 0, "body", Theme.Default);

        var lines = Render(surface, terminal);
        Assert.Equal("╭─ Hi ─────╮", lines[0].TrimEnd());
        Assert.Equal("│body      │", lines[1].TrimEnd());
        Assert.Equal("╰──────────╯", lines[3].TrimEnd());
        Assert.Equal(12, TextWidth.Of(lines[0].TrimEnd()));
        Assert.Equal(12, TextWidth.Of(lines[3].TrimEnd()));
        Assert.Equal(10, content.Width);
        Assert.Equal(2, content.Height);
    }

    [Fact]
    public void ContainsAndToLocalAnswerHitTests()
    {
        var (surface, _) = CreateSurface();
        var region = new SurfaceRegion(surface, 5, 2, 4, 3);

        Assert.True(region.Contains(2, 5));
        Assert.True(region.Contains(4, 8));
        Assert.False(region.Contains(5, 8));
        Assert.False(region.Contains(2, 9));
        Assert.Equal((1, 2), region.ToLocal(3, 7));
    }

    [Fact]
    public void AlignmentIsMeasuredInsideTheRegion()
    {
        var (surface, terminal) = CreateSurface();
        var region = new SurfaceRegion(surface, 4, 0, 10, 1);

        region.WriteLine(0, "ab", Theme.Default, Align.Center);

        Assert.Equal("        ab", Render(surface, terminal)[0].TrimEnd());
    }

    [Fact]
    public void SurfaceExposesItsFrameAndPaddedContent()
    {
        var terminal = new FakeTerminal(Width, Height);
        var surface = new Surface(terminal) { HorizontalPadding = 2, VerticalPadding = 1 };
        surface.StartFrame();

        Assert.Equal(Width, surface.Frame.Width);
        Assert.Equal(Width - 4, surface.Content.Width);
        Assert.Equal(Height - 2, surface.Content.Height);
        Assert.Equal(2, surface.Content.Left);
    }
}
