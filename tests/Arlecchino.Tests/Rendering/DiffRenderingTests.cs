using System;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Testing;
using Arlecchino.Tests.Views;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Rendering;

public sealed class DiffRenderingTests
{
    private const int Width = 20;
    private const int Height = 4;

    private static (Surface Surface, FakeTerminal Terminal) CreateSurface()
    {
        var terminal = new FakeTerminal(Width, Height);
        return (new(terminal) { HorizontalPadding = 0, VerticalPadding = 0 }, terminal);
    }

    private static void DrawLine(Surface surface, string text)
    {
        surface.StartFrame();
        surface.AppendLine(text, Theme.Default);
        surface.Build();
    }

    [Fact]
    public void FirstFrameIsSentWhole()
    {
        var (surface, terminal) = CreateSurface();

        DrawLine(surface, "hello");

        Assert.Equal(Height, FrameText.Lines(terminal.Written).Length);
    }

    [Fact]
    public void UnchangedFrameSendsNothing()
    {
        var (surface, terminal) = CreateSurface();

        DrawLine(surface, "hello");
        terminal.Clear();
        DrawLine(surface, "hello");

        Assert.Equal("", terminal.Written);
    }

    [Fact]
    public void ChangedCellsAreSentWithACursorJump()
    {
        var (surface, terminal) = CreateSurface();

        DrawLine(surface, "hello");
        terminal.Clear();
        DrawLine(surface, "hellp");

        Assert.Contains("\e[1;5H", terminal.Written, StringComparison.Ordinal);
        Assert.Contains("p", terminal.Written, StringComparison.Ordinal);
        Assert.DoesNotContain("hell", terminal.Written, StringComparison.Ordinal);
    }

    [Fact]
    public void ChangedRunIsSentAsOneJumpWhenGapsAreShort()
    {
        var (surface, terminal) = CreateSurface();

        DrawLine(surface, "aaaaaaaa");
        terminal.Clear();
        DrawLine(surface, "baaaaaab");

        Assert.Single(FrameText.CursorJumpsIn(terminal.Written));
    }

    [Fact]
    public void DistantChangesAreSentAsSeparateJumps()
    {
        var (surface, terminal) = CreateSurface();

        surface.StartFrame();
        surface.WriteAt(0, 0, "a", Theme.Default);
        surface.WriteAt(3, 0, "b", Theme.Default);
        surface.Build();

        terminal.Clear();

        surface.StartFrame();
        surface.WriteAt(0, 0, "x", Theme.Default);
        surface.WriteAt(3, 0, "y", Theme.Default);
        surface.Build();

        Assert.Equal(2, FrameText.CursorJumpsIn(terminal.Written).Count);
    }

    [Fact]
    public void ResizeFallsBackToAWholeFrame()
    {
        var (surface, terminal) = CreateSurface();

        DrawLine(surface, "hello");
        terminal.Width = Width + 4;
        terminal.Clear();
        DrawLine(surface, "hello");

        Assert.Equal(Height, FrameText.Lines(terminal.Written).Length);
    }

    [Fact]
    public void ForgettingThePreviousFrameForcesAWholeFrame()
    {
        var (surface, terminal) = CreateSurface();

        DrawLine(surface, "hello");
        terminal.Clear();
        surface.ForgetPreviousFrame();
        DrawLine(surface, "hello");

        Assert.Equal(Height, FrameText.Lines(terminal.Written).Length);
    }

    [Fact]
    public void RepaintIsRequestedByInputNavigationAndState()
    {
        using var app = new TestApplication();
        var repaint = app.Repaint;

        repaint.TakeRequested();
        Assert.False(repaint.IsRequested);

        app.Press(ConsoleKey.DownArrow);
        Assert.True(repaint.TakeRequested());

        app.State.Output = "changed";
        Assert.True(repaint.TakeRequested());

        app.Navigator.Apply(ViewKind.Other);
        Assert.True(repaint.TakeRequested());

        app.State.RequestText("Name", "", null, static _ => { });
        Assert.True(repaint.TakeRequested());
    }
}
