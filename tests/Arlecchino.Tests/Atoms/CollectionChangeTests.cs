using System;
using System.Collections.Generic;
using Arlecchino.Diagnostics;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Arlecchino.Widgets.Lists;
using Xunit;
using Arlecchino.Tests.Support;
using Arlecchino.Input;

namespace Arlecchino.Tests.Atoms;

public sealed class CollectionChangeTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    [Fact]
    public void AListWhoseItemsAreClearedAfterSelecting()
    {
        var terminal = new FakeTerminal(30, 8);
        var surface = new Surface(terminal);
        var items = new List<string> { "alpha", "beta", "gamma" };
        var list = new ListBox<string>(Keymap) { Render = static item => item, Items = items };

        Draw(surface, list);
        list.Handle(new(ConsoleKey.End));

        items.Clear();

        Draw(surface, list);
        list.Handle(new(ConsoleKey.DownArrow));
        list.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, 1, 1, default));
        Draw(surface, list);

        Assert.Null(list.SelectedItem);
    }

    [Fact]
    public void ATableWhoseRowsAreClearedAfterSelecting()
    {
        var terminal = new FakeTerminal(40, 8);
        var surface = new Surface(terminal);
        var rows = new List<string> { "alpha", "beta" };
        var table = new Table<string>(Keymap)
        {
            Rows = rows,
            Columns = [new() { Header = static () => "Name", Cell = static item => item }],
        };

        Draw(surface, table);
        table.Handle(new(ConsoleKey.End));

        rows.Clear();

        Draw(surface, table);
        table.Handle(new(ConsoleKey.DownArrow));
        Draw(surface, table);

        Assert.NotNull(FrameText.Lines(terminal.Written));
    }

    [Fact]
    public void ATreeWhoseRootsAreClearedAfterSelecting()
    {
        var terminal = new FakeTerminal(40, 8);
        var surface = new Surface(terminal);
        var roots = new List<TreeNode<string>> { new() { Value = "alpha" }, new() { Value = "beta" } };
        var tree = new Tree<string>(Keymap) { Roots = roots, Render = static item => item };

        Draw(surface, tree);
        tree.Handle(new(ConsoleKey.DownArrow));

        roots.Clear();

        Draw(surface, tree);
        tree.Handle(new(ConsoleKey.DownArrow));
        Draw(surface, tree);

        Assert.NotNull(FrameText.Lines(terminal.Written));
    }

    [Fact]
    public void AListWhoseItemsVanishWhileItIsBeingDrawn()
    {
        var terminal = new FakeTerminal(30, 8);
        var surface = new Surface(terminal);
        var items = new List<string> { "alpha", "beta", "gamma" };
        var list = new ListBox<string>(Keymap)
        {
            Render = item =>
            {
                items.Clear();
                return item;
            },
            Items = items,
        };

        Draw(surface, list);

        Assert.Contains("alpha", FrameText.WithoutStyles(terminal.Written), StringComparison.Ordinal);
    }

    [Fact]
    public void ATableWhoseRowsVanishWhileItIsBeingDrawn()
    {
        var terminal = new FakeTerminal(40, 8);
        var surface = new Surface(terminal);
        var rows = new List<string> { "alpha", "beta", "gamma" };
        var table = new Table<string>(Keymap)
        {
            Rows = rows,
            Columns =
            [
                new()
                {
                    Header = static () => "Name",
                    Cell = item =>
                    {
                        rows.Clear();
                        return item;
                    },
                },
            ],
        };

        Draw(surface, table);

        Assert.NotNull(FrameText.Lines(terminal.Written));
    }

    [Fact]
    public void TheCutShortFrameIsReportedRatherThanSwallowed()
    {
        using var app = new TestApplication(60, 12, static builder => builder.AddView<VanishingView>("Vanishing"));

        app.Navigator.Apply(new("Vanishing"));
        app.Frame();

        var logged = app.Services.GetService(typeof(LogBuffer)) as LogBuffer;
        Assert.NotNull(logged);

        Assert.Contains(logged.Snapshot(),
            entry => entry.Message.Contains("shrank while it was being drawn", StringComparison.Ordinal));
    }

    public sealed class VanishingView : IArlecchinoView
    {
        private readonly Surface _surface;
        private readonly List<string> _items = ["alpha", "beta", "gamma"];
        private readonly ListBox<string> _list;

        public VanishingView(Surface surface, ArlecchinoOptions options)
        {
            _surface = surface;
            _list = new(options.Keymap)
            {
                Items = _items,
                Render = item =>
                {
                    _items.Clear();
                    return item;
                },
            };
        }

        public void Draw() => _list.Draw(_surface.Content);

        public ViewRoute Handle(KeyPress key) => ViewRoute.None;
    }

    private static void Draw(Surface surface, IArlecchinoWidget widget)
    {
        surface.StartFrame();
        widget.Draw(surface.Frame);
        surface.Build();
    }
}
