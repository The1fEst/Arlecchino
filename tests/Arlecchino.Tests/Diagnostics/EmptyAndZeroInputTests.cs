using System;
using System.IO;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Tracked;
using Arlecchino.Hosting;
using Arlecchino.Modals.Asking;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Text;
using Arlecchino.Testing;
using Arlecchino.Widgets.Lists;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Diagnostics;

public sealed class EmptyAndZeroInputTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    [Fact]
    public void WrapWithNoRoomAtAll()
    {
        var lines = TextWidth.Wrap("some text that has to go somewhere", 0);

        Assert.NotNull(lines);
    }

    [Fact]
    public void WrapWithNegativeWidth()
    {
        var lines = TextWidth.Wrap("some text", -5);

        Assert.NotNull(lines);
    }

    [Fact]
    public void TruncateToNothing()
    {
        Assert.NotNull(TextWidth.Truncate("some text", 0));
        Assert.NotNull(TextWidth.Truncate("some text", -3));
    }

    [Fact]
    public void ATickerAskedToRunEveryNoTime()
    {
        using var app = new TestApplication();
        var runs = 0;

        using var tick = app.Services.GetService(typeof(Ticker)) is Ticker ticker
            ? ticker.Every(TimeSpan.Zero, () => runs++)
            : null;

        app.Advance(TimeSpan.FromSeconds(1));

        Assert.True(runs <= 2);
    }

    [Fact]
    public void ChoiceWithNothingToChoose()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", [], static _ => { });

        var frame = app.Frame();
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.Enter);

        Assert.NotNull(frame);
    }

    [Fact]
    public void AnEmptyListTakesKeys()
    {
        var terminal = new FakeTerminal(20, 6);
        var surface = new Surface(terminal);
        var list = new ListBox<string>(Keymap) { Render = static item => item, Items = [] };

        list.Handle(new(ConsoleKey.DownArrow));
        list.Handle(new(ConsoleKey.End));
        list.Handle(new(ConsoleKey.Enter, default, '\r'));

        surface.StartFrame();
        list.Draw(surface.Frame);
        surface.Build();

        Assert.Null(list.SelectedItem);
    }

    [Fact]
    public void APaneWhoseContentIsEmpty()
    {
        var terminal = new FakeTerminal(20, 6);
        var surface = new Surface(terminal);
        var pane = new ScrollPane(Keymap)
        {
            ContentHeight = static () => 0,
            Content = static _ => { },
            Offset = 40,
        };

        surface.StartFrame();
        pane.Draw(surface.Frame);
        surface.Build();

        Assert.Equal(0, pane.Offset);
    }

    [Fact]
    public void AHistoryAskedToKeepNothingKeepsOneStep()
    {
        using var app = new TestApplication();
        var value = new TrackedAtom<int>(0);

        app.History.Capacity = 0;
        value.Value = 1;
        value.Value = 2;

        Assert.True(app.History.CanUndo);
        Assert.Equal(1, app.History.Depth);
    }

    [Fact]
    public void AComputedThatReadsItselfStopsRatherThanRecursing()
    {
        var box = new Computed<int>?[1];
        box[0] = new(() => box[0]!.Value + 1);

        Assert.Equal(1, box[0]!.Value);
    }

    [Fact]
    public void APickerPointedAtNowhere()
    {
        using var app = new TestApplication(100, 26);

        app.State.FilePicker = new("Pick",
            PickFolder: true,
            Path.Combine(Path.GetTempPath(), "no-such-folder-here"),
            ViewRoute.None,
            static _ => { });
        app.Navigator.Apply(Routes.FilePicker);

        Assert.NotNull(app.Frame());
    }

    [Fact]
    public void ATextAreaCaretMovingThroughEmoji()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "👩‍👩‍👧 family", static _ => { });

        var modal = (TextAreaModal)app.State.Modal!;

        for (var step = 0; step < 20; step++)
        {
            app.Press(ConsoleKey.RightArrow);
        }

        for (var step = 0; step < 20; step++)
        {
            app.Press(ConsoleKey.Backspace);
        }

        Assert.NotNull(modal.Lines);
        Assert.NotNull(app.Frame());
    }

    [Fact]
    public void ATableWithNoColumns()
    {
        var terminal = new FakeTerminal(30, 8);
        var surface = new Surface(terminal);
        var table = new Table<string>(Keymap) { Rows = ["a", "b"], Columns = [] };

        surface.StartFrame();
        table.Draw(surface.Frame);
        surface.Build();

        Assert.NotNull(FrameText.Lines(terminal.WrittenText));
    }

    [Fact]
    public void ATreeWithNoRoots()
    {
        var terminal = new FakeTerminal(30, 8);
        var surface = new Surface(terminal);
        var tree = new Tree<string>(Keymap)
        {
            Roots = [],
            Render = static item => item,
        };

        tree.Handle(new(ConsoleKey.RightArrow));

        surface.StartFrame();
        tree.Draw(surface.Frame);
        surface.Build();

        Assert.NotNull(FrameText.Lines(terminal.WrittenText));
    }
}
