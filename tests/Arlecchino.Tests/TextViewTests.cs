using System;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;

namespace Arlecchino.Tests;

public sealed class TextViewTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    private static (Surface Surface, FakeTerminal Terminal) CreateSurface(int width = 20, int height = 6)
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

    [Fact]
    public void LongLinesWrapOnSpaces()
    {
        var (surface, terminal) = CreateSurface(12);
        var view = new TextView(Keymap) { Text = "one two three four", ShowScrollBar = false };

        view.Place(surface.Frame);
        var lines = Render(surface, terminal);

        Assert.Equal("one two", lines[0].TrimEnd());
        Assert.Equal("three four", lines[1].TrimEnd());
        Assert.Equal(2, view.LineCount);
    }

    [Fact]
    public void LineBreaksInTheTextAreKept()
    {
        var (surface, terminal) = CreateSurface();
        var view = new TextView(Keymap) { Text = "first\n\nthird", ShowScrollBar = false };

        view.Place(surface.Frame);
        var lines = Render(surface, terminal);

        Assert.Equal("first", lines[0].TrimEnd());
        Assert.Equal("", lines[1].TrimEnd());
        Assert.Equal("third", lines[2].TrimEnd());
    }

    [Fact]
    public void AWordWiderThanThePaneIsBrokenRatherThanLost()
    {
        var (surface, terminal) = CreateSurface(6);
        var view = new TextView(Keymap) { Text = "unbreakableword", ShowScrollBar = false };

        view.Place(surface.Frame);
        var lines = Render(surface, terminal);

        Assert.Equal("unbrea", lines[0].TrimEnd());
        Assert.Equal("kablew", lines[1].TrimEnd());
        Assert.Equal("ord", lines[2].TrimEnd());
    }

    [Fact]
    public void ScrollingShowsTheRestOfTheText()
    {
        var (surface, terminal) = CreateSurface(10, 2);
        var view = new TextView(Keymap) { Text = "a\nb\nc\nd", ShowScrollBar = false };

        view.Place(surface.Frame);
        Assert.Equal("a", Render(surface, terminal)[0].TrimEnd());

        view.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));

        var (second, terminalAgain) = CreateSurface(10, 2);
        view.Place(second.Frame);

        Assert.Equal("b", Render(second, terminalAgain)[0].TrimEnd());
    }

    [Fact]
    public void ChangingTheWidthReflowsInsteadOfCutting()
    {
        var (narrow, narrowTerminal) = CreateSurface(8);
        var view = new TextView(Keymap) { Text = "alpha beta gamma", ShowScrollBar = false };

        view.Place(narrow.Frame);
        var narrowLines = Render(narrow, narrowTerminal);

        Assert.Equal("alpha", narrowLines[0].TrimEnd());
        Assert.Equal(3, view.LineCount);

        var (wide, wideTerminal) = CreateSurface();
        view.Place(wide.Frame);

        Assert.Equal("alpha beta gamma", Render(wide, wideTerminal)[0].TrimEnd());
        Assert.Equal(1, view.LineCount);
    }

    [Fact]
    public void TextThatDoesNotFitGetsAScrollBar()
    {
        var (surface, terminal) = CreateSurface(10, 2);
        var view = new TextView(Keymap) { Text = "one\ntwo\nthree" };

        view.Place(surface.Frame);
        var frame = string.Join("", Render(surface, terminal));

        Assert.Contains("█", frame, StringComparison.Ordinal);
    }
}
