using System;
using System.Linq;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;

namespace Arlecchino.Tests;

public sealed class ResizeTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    [Fact]
    public void AListKeepsItsSelectionOnScreenWhenTheWindowShrinks()
    {
        var terminal = new FakeTerminal(30, 20);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var list = new ListBox<string>(Keymap)
        {
            Render = static item => item,
            Items = Enumerable.Range(0, 60).Select(static index => $"item {index}").ToArray(),
            Selected = 55,
        };

        Draw(surface, list);
        Assert.Contains("item 55", Lines(terminal), StringComparison.Ordinal);

        terminal.Height = 6;
        surface.ForgetPreviousFrame();
        terminal.Clear();

        Draw(surface, list);

        Assert.Contains("item 55", Lines(terminal), StringComparison.Ordinal);
        Assert.Equal(55, list.Selected);
    }

    [Fact]
    public void AListWiderThanTheWindowIsCutRatherThanWrapped()
    {
        var terminal = new FakeTerminal(40, 8);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var list = new ListBox<string>(Keymap)
        {
            Render = static item => item,
            Items = ["a line far longer than the window it is drawn in"],
        };

        terminal.Width = 12;
        surface.ForgetPreviousFrame();

        Draw(surface, list);

        Assert.All(FrameText.Lines(terminal.Written), line => Assert.True(TextWidth.Of(line) <= 12));
    }

    [Fact]
    public void AScrolledPaneComesBackIntoRangeWhenTheWindowShrinks()
    {
        var terminal = new FakeTerminal(24, 20);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var pane = new ScrollPane(Keymap)
        {
            ContentHeight = static () => 40,
            Content = static region =>
            {
                for (var row = 0; row < 40; row++)
                {
                    region.WriteLine(row, $"line {row}", Theme.Default);
                }
            },
            Offset = 30,
        };

        Draw(surface, pane);
        Assert.Contains("line 30", Lines(terminal), StringComparison.Ordinal);

        terminal.Height = 4;
        surface.ForgetPreviousFrame();
        terminal.Clear();

        Draw(surface, pane);

        Assert.True(pane.Offset <= 36);
        Assert.NotEqual("", Lines(terminal).Trim());
    }

    [Fact]
    public void TextReflowsWhenTheWindowNarrows()
    {
        const string paragraph = "a paragraph long enough that the width it is given decides how many " +
                                 "lines it takes on screen";

        var terminal = new FakeTerminal(60, 12);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };
        var view = new TextView(Keymap) { Text = paragraph };

        Draw(surface, view);
        var wide = NonEmptyLines(terminal);

        terminal.Width = 24;
        surface.ForgetPreviousFrame();
        terminal.Clear();

        Draw(surface, view);
        var narrow = NonEmptyLines(terminal);

        Assert.True(narrow > wide);
        Assert.All(FrameText.Lines(terminal.Written), line => Assert.True(TextWidth.Of(line) <= 24));
    }

    [Fact]
    public void AWindowTooSmallForTheViewIsAnswerWithTheNotice()
    {
        using var app = new TestApplication(80, 24, static builder =>
        {
            builder.Options.MinimumWidth = 40;
            builder.Options.MinimumHeight = 10;
        });

        Assert.DoesNotContain(app.Options.Strings.TerminalTooSmall(), app.Frame(), StringComparison.Ordinal);

        app.Terminal.Width = 34;
        var narrow = app.Frame();

        Assert.Equal(34, app.Surface.FrameWidth);
        Assert.Contains(app.Options.Strings.TerminalTooSmall(), narrow, StringComparison.Ordinal);

        app.Terminal.Width = 80;

        Assert.DoesNotContain(app.Options.Strings.TerminalTooSmall(), app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void AWindowOfOneCellStillDraws()
    {
        using var app = new TestApplication(80, 24, static builder =>
        {
            builder.Options.MinimumWidth = 1;
            builder.Options.MinimumHeight = 1;
        });

        app.Terminal.Width = 1;
        app.Terminal.Height = 1;

        var frame = app.Frame();

        Assert.NotNull(frame);
    }

    private static void Draw(Surface surface, IArlecchinoWidget widget)
    {
        surface.StartFrame();
        widget.Draw(surface.Frame);
        surface.Build();
    }

    private static string Lines(FakeTerminal terminal) => FrameText.WithoutStyles(terminal.Written);

    private static int NonEmptyLines(FakeTerminal terminal) =>
        FrameText.Lines(terminal.Written).Count(static line => line.Trim().Length > 0);
}
