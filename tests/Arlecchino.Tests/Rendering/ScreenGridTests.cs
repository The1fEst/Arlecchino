using System;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Testing;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Rendering;

public sealed class ScreenGridTests
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

    private static void DrawCorners(Surface surface, IArlecchinoColor style)
    {
        surface.StartFrame();
        surface.WriteAt(0, 0, "top", style);
        surface.WriteAt(2, 14, "end", style);
        surface.Build();
    }

    [Fact]
    public void DiffedFramesLeaveWhatAWholeRepaintWouldHaveDrawn()
    {
        var (diffed, diffedTerminal) = CreateSurface();
        DrawLine(diffed, "hello");
        DrawLine(diffed, "hellp");
        DrawLine(diffed, "a longer line here");

        var (whole, wholeTerminal) = CreateSurface();
        DrawLine(whole, "a longer line here");

        Assert.Equal(wholeTerminal.Screen.ToString(), diffedTerminal.Screen.ToString());
        Assert.True(diffedTerminal.Screen.Matches(wholeTerminal.Screen));
    }

    [Fact]
    public void ADiffedStyleChangeLeavesWhatAWholeRepaintWouldHaveDrawn()
    {
        using var colors = new ColorSupportScope(ColorSupport.TrueColor);

        var (diffed, diffedTerminal) = CreateSurface();
        DrawCorners(diffed, Theme.Error);
        DrawCorners(diffed, Theme.Accent);

        var (whole, wholeTerminal) = CreateSurface();
        DrawCorners(whole, Theme.Accent);

        Assert.True(diffedTerminal.Screen.Matches(wholeTerminal.Screen));
    }

    [Fact]
    public void ACellCarriesTheStyleItWasDrawnIn()
    {
        using var colors = new ColorSupportScope(ColorSupport.TrueColor);

        var (surface, terminal) = CreateSurface();
        surface.StartFrame();
        surface.WriteAt(0, 0, "!", Theme.Error);
        surface.Build();

        Assert.Equal(Theme.Error.Ansi, terminal.Screen.StyleAt(0, 0));
        Assert.Equal(Theme.Default.Ansi, terminal.Screen.StyleAt(0, 1));
    }

    [Fact]
    public void AWideSymbolTakesTwoColumns()
    {
        var (surface, terminal) = CreateSurface();
        surface.StartFrame();
        surface.WriteAt(0, 0, "日本", Theme.Default);
        surface.Build();

        Assert.Equal("日", terminal.Screen.CellAt(0, 0));
        Assert.Equal("", terminal.Screen.CellAt(0, 1));
        Assert.Equal("本", terminal.Screen.CellAt(0, 2));
        Assert.Equal("", terminal.Screen.CellAt(0, 3));
    }

    [Fact]
    public void OverwritingHalfOfAWideSymbolBlanksTheOtherHalf()
    {
        var grid = new ScreenGrid(6, 1);

        grid.Apply("日本");
        grid.Apply("\e[1;2Hx");

        Assert.Equal(" x本  ", grid.Line(0));
    }

    [Fact]
    public void TextPastTheRightEdgeWrapsOntoTheNextRow()
    {
        var grid = new ScreenGrid(4, 2);

        grid.Apply("abcdef");

        Assert.Equal("abcd", grid.Line(0));
        Assert.Equal("ef  ", grid.Line(1));
    }

    [Fact]
    public void TextPastTheRightEdgeStaysOnTheRowWhenWrappingIsOff()
    {
        var grid = new ScreenGrid(4, 2);

        grid.Apply("\e[?7labcdef");

        Assert.Equal("abcf", grid.Line(0));
        Assert.Equal("    ", grid.Line(1));
    }

    [Fact]
    public void ResizingKeepsWhatStillFits()
    {
        var (surface, terminal) = CreateSurface();
        DrawLine(surface, "hello");

        terminal.Width = 8;

        Assert.Equal(8, terminal.Screen.Width);
        Assert.Equal(Height, terminal.Screen.Height);
        Assert.Equal("hello   ", terminal.Screen.Line(0));
    }

    [Fact]
    public void TheCursorEndsWhereTheOutputLeftIt()
    {
        var grid = new ScreenGrid(10, 3);

        grid.Apply("\e[?25l\e[2;4Hx");

        Assert.Equal(1, grid.CursorRow);
        Assert.Equal(4, grid.CursorColumn);
        Assert.False(grid.IsCursorVisible);
    }

    /// <summary>
    /// One of the two things a terminal does here: tmux drops the symbol and kitty shifts it a column
    /// inwards. A frame asks for neither, so this pins the reading rather than claiming it is the only one.
    /// </summary>
    [Fact]
    public void AWideSymbolPastTheRightEdgeIsDroppedWhenWrappingIsOff()
    {
        var grid = new ScreenGrid(5, 2);

        grid.Apply("\e[?7l日本語");

        Assert.Equal("日本 ", grid.Line(0));
        Assert.Equal("     ", grid.Line(1));
    }

    [Fact]
    public void ASymbolInTheLastColumnLeavesTheCursorWaitingToWrap()
    {
        var grid = new ScreenGrid(4, 2);

        grid.Apply("abcd");

        Assert.Equal(0, grid.CursorRow);
        Assert.Equal(4, grid.CursorColumn);
    }

    /// <summary>
    /// The reading tmux gives, where kitty leaves the cursor one past the edge. Nothing visible turns on
    /// which, since the next symbol lands in the last column either way.
    /// </summary>
    [Fact]
    public void TheCursorStopsAtTheLastColumnWhenWrappingIsOff()
    {
        var grid = new ScreenGrid(4, 2);

        grid.Apply("\e[?7labcd");

        Assert.Equal(3, grid.CursorColumn);
    }

    [Fact]
    public void ABackspaceFromTheEdgeStepsOntoTheLastColumn()
    {
        var grid = new ScreenGrid(6, 1);

        grid.Apply("abcdef\bX");

        Assert.Equal("abcdeX", grid.Line(0));
    }

    [Fact]
    public void ATabMovesToTheNextStop()
    {
        var grid = new ScreenGrid(32, 2);

        grid.Apply("a\tb\tc\e[2;1H\tz");

        Assert.Equal("a       b       c", grid.Line(0).TrimEnd());
        Assert.Equal("        z", grid.Line(1).TrimEnd());
    }

    [Fact]
    public void ATabStopsAtTheLastColumn()
    {
        var grid = new ScreenGrid(6, 1);

        grid.Apply("ab\t\tz");

        Assert.Equal("ab   z", grid.Line(0));
    }

    [Fact]
    public void TakingTheScreenOverStartsItBlank()
    {
        var grid = new ScreenGrid(8, 2);

        grid.Apply("first\r\nsecond\e[?1049h");

        Assert.Equal("        ", grid.Line(0));
        Assert.Equal("        ", grid.Line(1));
    }

    [Fact]
    public void GivingTheScreenBackLeavesWhatWasUnderIt()
    {
        var grid = new ScreenGrid(8, 2);

        grid.Apply("first\r\nsecond\e[?1049h\e[1;1Hover\e[?1049l");

        Assert.Equal("first   ", grid.Line(0));
        Assert.Equal("second  ", grid.Line(1));
        Assert.Equal(1, grid.CursorRow);
        Assert.Equal(6, grid.CursorColumn);
    }

    [Fact]
    public void AFeedOnTheLastRowScrollsTheScreen()
    {
        var grid = new ScreenGrid(3, 2);

        grid.Apply("ab\r\ncd\r\nef");

        Assert.Equal("cd ", grid.Line(0));
        Assert.Equal("ef ", grid.Line(1));
    }

    [Theory]
    [InlineData("\e_Gf=100,a=T;AAAA\e\\")]
    [InlineData("\eP0;1;0q#0;2;0;0;0#0~~\e\\")]
    [InlineData("\e]1337;File=inline=1:AAAA\a")]
    public void APassthroughPayloadNeverLandsOnACell(string payload)
    {
        var (surface, terminal) = CreateSurface();
        surface.StartFrame();
        surface.WriteAt(0, 0, "ok", Theme.Default);
        surface.Passthrough(1, 0, payload);
        surface.Build();

        Assert.Equal("ok", terminal.Screen.Line(0).TrimEnd());
        Assert.Equal("", terminal.Screen.Line(1).TrimEnd());
    }

    [Fact]
    public void ForgettingWhatWasWrittenLeavesTheScreenStanding()
    {
        var (surface, terminal) = CreateSurface();
        DrawLine(surface, "hello");

        terminal.Clear();

        Assert.Equal("", terminal.Written);
        Assert.Equal("hello", terminal.Screen.Line(0).TrimEnd());
    }

    [Fact]
    public void AnIdleFrameLeavesTheScreenAsItWas()
    {
        var (surface, terminal) = CreateSurface();
        DrawLine(surface, "hello");
        var before = terminal.Screen.ToString();
        terminal.Clear();

        DrawLine(surface, "hello");

        Assert.Equal("", terminal.Written);
        Assert.Equal(before, terminal.Screen.ToString());
    }

    [Fact]
    public void TheScreenReadsWholeWhileTheFrameOnlySaysWhatChanged()
    {
        var (surface, terminal) = CreateSurface();
        DrawLine(surface, "hello");
        terminal.Clear();

        DrawLine(surface, "hellp");

        Assert.DoesNotContain("hell", terminal.Written, StringComparison.Ordinal);
        Assert.Equal("hellp", terminal.Screen.Line(0).TrimEnd());
        Assert.Equal(Height, terminal.Screen.Lines().Length);
    }

    [Fact]
    public void TheHostDrawsDiffedFramesTheScreenStillReadsWhole()
    {
        using var app = new TestApplication(40, 10);
        app.Frame();
        var whole = app.Terminal.Written.Length;

        app.Navigator.Apply(ViewKind.Other);
        var lines = app.FrameLines();

        Assert.True(app.Terminal.Written.Length < whole);
        Assert.Equal(10, lines.Length);
        Assert.Contains(lines, line => line.Contains("other", StringComparison.Ordinal));
        Assert.DoesNotContain(lines, line => line.Contains("probe", StringComparison.Ordinal));
    }
}
