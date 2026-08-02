using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets.Lists;
using Xunit;

namespace Arlecchino.Tests.Widgets;

public sealed class TreeTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    private static Tree<string> CreateTree()
    {
        var tree = new Tree<string>(Keymap) { Render = static value => value };

        tree.Roots =
        [
            new()
            {
                Value = "cars",
                Children =
                [
                    new() { Value = "bmw" },
                    new()
                    {
                        Value = "nissan",
                        Children = [new() { Value = "skyline" }],
                    },
                ],
            },
            new() { Value = "tracks" },
        ];

        return tree;
    }

    private static string[] Render(Tree<string> tree, int width = 24, int height = 8)
    {
        var terminal = new FakeTerminal(width, height);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        tree.Draw(surface.Frame);
        surface.Build();

        return FrameText.Lines(terminal.Written);
    }

    [Fact]
    public void CollapsedRootsShowOnlyTheirOwnRows()
    {
        var lines = Render(CreateTree());

        Assert.Equal("▸ cars", lines[0].TrimEnd());
        Assert.Equal("  tracks", lines[1].TrimEnd());
    }

    [Fact]
    public void ExpandingIndentsChildren()
    {
        var tree = CreateTree();
        tree.Handle(new('\0', ConsoleKey.RightArrow, false, false, false));

        var lines = Render(tree);

        Assert.Equal("▾ cars", lines[0].TrimEnd());
        Assert.Equal("    bmw", lines[1].TrimEnd());
        Assert.Equal("  ▸ nissan", lines[2].TrimEnd());
        Assert.Equal("  tracks", lines[3].TrimEnd());
    }

    [Fact]
    public void RightArrowOnAnExpandedNodeMovesIntoIt()
    {
        var tree = CreateTree();

        tree.Handle(new('\0', ConsoleKey.RightArrow, false, false, false));
        tree.Handle(new('\0', ConsoleKey.RightArrow, false, false, false));

        Assert.Equal("bmw", tree.SelectedNode?.Value);
    }

    [Fact]
    public void LeftArrowCollapsesThenWalksToTheParent()
    {
        var tree = CreateTree();
        tree.ExpandAll();

        tree.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        tree.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        Assert.Equal("nissan", tree.SelectedNode?.Value);

        tree.Handle(new('\0', ConsoleKey.LeftArrow, false, false, false));
        Assert.Equal("nissan", tree.SelectedNode?.Value);
        Assert.Equal(4, Render(tree).Length - CountBlank(Render(tree)));

        tree.Handle(new('\0', ConsoleKey.LeftArrow, false, false, false));
        Assert.Equal("cars", tree.SelectedNode?.Value);
    }

    [Fact]
    public void ExpandAllAndCollapseAllReachEveryLevel()
    {
        var tree = CreateTree();

        tree.ExpandAll();
        Assert.Equal(5, CountRows(tree));

        tree.CollapseAll();
        Assert.Equal(2, CountRows(tree));
    }

    [Fact]
    public void ConfirmTogglesABranchAndActivatesALeaf()
    {
        var activated = "";
        var tree = CreateTree();
        tree = new(Keymap)
        {
            Render = static value => value,
            OnActivate = node =>
            {
                activated = node.Value;
                return new("Leaf");
            },
            Roots = tree.Roots,
        };

        tree.Handle(new('\0', ConsoleKey.Enter, false, false, false));
        Assert.Equal(4, CountRows(tree));
        Assert.Equal("", activated);

        tree.Handle(new('\0', ConsoleKey.DownArrow, false, false, false));
        var result = tree.Handle(new('\0', ConsoleKey.Enter, false, false, false));

        Assert.Equal("bmw", activated);
        Assert.Equal(new("Leaf"), result.Route);
    }

    [Fact]
    public void ClickingTheMarkerTogglesWithoutActivating()
    {
        var activated = false;
        var tree = new Tree<string>(Keymap)
        {
            Render = static value => value,
            OnActivate = _ =>
            {
                activated = true;
                return ViewRoute.None;
            },
            Roots = CreateTree().Roots,
        };

        Render(tree);
        tree.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, 0, 0, default));

        Assert.Equal(4, CountRows(tree));
        Assert.False(activated);
    }

    [Fact]
    public void ClickingARowSelectsItAndTheSecondClickActivates()
    {
        var tree = CreateTree();
        tree.ExpandAll();
        Render(tree);

        tree.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, 1, 6, default));
        Assert.Equal("bmw", tree.SelectedNode?.Value);
    }

    [Fact]
    public void ExpandingReportsSoLazyChildrenCanBeLoaded()
    {
        var loaded = "";
        var tree = new Tree<string>(Keymap)
        {
            Render = static value => value,
            OnExpanding = node => loaded = node.Value,
            Roots = CreateTree().Roots,
        };

        tree.Handle(new('\0', ConsoleKey.RightArrow, false, false, false));

        Assert.Equal("cars", loaded);
    }

    private static int CountRows(Tree<string> tree)
    {
        var lines = Render(tree);
        return lines.Length - CountBlank(lines);
    }

    private static int CountBlank(string[] lines)
    {
        var blank = 0;
        foreach (var line in lines)
        {
            if (line.Trim().Length == 0)
            {
                blank++;
            }
        }

        return blank;
    }
}
