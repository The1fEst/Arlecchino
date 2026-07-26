using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Widgets;
using Xunit;

namespace Arlecchino.Tests;

public sealed class WidgetTests
{
    private static readonly ArlecchinoKeymap Keymap = new();

    private static (Surface Surface, FakeTerminal Terminal) CreateSurface(int width = 30, int height = 8)
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
    public void ScrollWindowKeepsTheSelectionVisible()
    {
        Assert.Equal(new(0, 3), ScrollWindow.Around(0, 10, 3));
        Assert.Equal(new(4, 3), ScrollWindow.Around(5, 10, 3));
        Assert.Equal(new(7, 3), ScrollWindow.Around(9, 10, 3));
        Assert.Equal(new(0, 4), ScrollWindow.Around(2, 4, 10));
        Assert.Equal(new(0, 0), ScrollWindow.Around(0, 0, 5));
    }

    [Fact]
    public void ListBoxDrawsItemsAndScrollsAroundTheSelection()
    {
        var (surface, terminal) = CreateSurface(12, 3);
        var list = new ListBox<string>(Keymap) { Render = static item => item, Items = ["a", "b", "c", "d", "e"] };

        list.Selected = 4;
        list.Place(surface.Frame);

        var lines = Render(surface, terminal);
        Assert.StartsWith("c", lines[0].Trim(), StringComparison.Ordinal);
        Assert.StartsWith("e", lines[2].Trim(), StringComparison.Ordinal);
    }

    [Fact]
    public void AListThatFitsHasNoScrollBar()
    {
        var (surface, terminal) = CreateSurface(12, 3);
        var list = new ListBox<string>(Keymap) { Render = static item => item, Items = ["a", "b", "c"] };

        list.Place(surface.Frame);

        Assert.DoesNotContain('│', string.Join("", Render(surface, terminal)));
    }

    [Fact]
    public void TheScrollThumbFollowsTheSelectionDownTheList()
    {
        var atTop = DrawScrollingList(0);
        var atBottom = DrawScrollingList(8);

        Assert.Equal('█', atTop[0][^1]);
        Assert.Equal('│', atTop[2][^1]);
        Assert.Equal('│', atBottom[0][^1]);
        Assert.Equal('█', atBottom[2][^1]);
    }

    private static string[] DrawScrollingList(int selected)
    {
        var (surface, terminal) = CreateSurface(12, 3);
        var list = new ListBox<string>(Keymap)
        {
            Render = static item => item,
            Items = ["a", "b", "c", "d", "e", "f", "g", "h", "i"],
            Selected = selected,
        };

        list.Place(surface.Frame);
        return Render(surface, terminal);
    }

    [Fact]
    public void AScrollBarLeavesRoomForItselfInsteadOfCoveringText()
    {
        var (surface, terminal) = CreateSurface(6, 2);
        var list = new ListBox<string>(Keymap)
        {
            Render = static item => item,
            Items = ["aaaaaa", "bbbbbb", "cccccc"],
        };

        list.Place(surface.Frame);

        Assert.Equal("aaaaa█", Render(surface, terminal)[0]);
    }

    [Fact]
    public void ListBoxMovesWithTheKeymap()
    {
        var list = new ListBox<string>(Keymap) { Render = static item => item, Items = ["a", "b", "c"] };

        Assert.Equal(FocusResult.Handled, list.Handle(new('\0', ConsoleKey.DownArrow, false, false, false)));
        Assert.Equal(1, list.Selected);

        list.Handle(new('\0', ConsoleKey.End, false, false, false));
        Assert.Equal(2, list.Selected);

        list.Handle(new('\0', ConsoleKey.Home, false, false, false));
        Assert.Equal(0, list.Selected);

        Assert.Equal(FocusResult.Ignored, list.Handle(new('\0', ConsoleKey.F5, false, false, false)));
    }

    [Fact]
    public void ListBoxActivatesTheSelectedItem()
    {
        var picked = "";
        var list = new ListBox<string>(Keymap)
        {
            Render = static item => item,
            Items = ["a", "b"],
            OnActivate = item =>
            {
                picked = item;
                return new("Somewhere");
            },
        };

        list.Selected = 1;
        var result = list.Handle(new('\0', ConsoleKey.Enter, false, false, false));

        Assert.Equal("b", picked);
        Assert.Equal(new("Somewhere"), result.Route);
    }

    [Fact]
    public void ListBoxRespondsToWheelAndClicks()
    {
        var (surface, _) = CreateSurface(12, 4);
        var region = surface.Frame;
        var list = new ListBox<string>(Keymap) { Render = static item => item, Items = ["a", "b", "c", "d"] };

        list.Place(region);

        list.HandleMouse(new(MouseAction.ScrolledDown, MouseButton.None, 0, 0, default));
        Assert.Equal(1, list.Selected);

        list.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, 2, 0, default));
        Assert.Equal(2, list.Selected);

        Assert.Equal(FocusResult.Ignored,
            list.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, 40, 0, default)));
    }

    [Fact]
    public void TableDrawsHeadersAndColumns()
    {
        var (surface, terminal) = CreateSurface(30, 4);
        var table = new Table<(string Name, int Size)>(Keymap)
        {
            Columns =
            [
                new() { Header = static () => "Name", Cell = static row => row.Name },
                new() { Header = static () => "Size", Cell = static row => row.Size.ToString(), Width = 6, AlignRight = true },
            ],
            Rows = [("first", 10), ("second", 200)],
        };

        table.Place(surface.Frame);

        var lines = Render(surface, terminal);
        Assert.StartsWith("Name", lines[0]);
        Assert.EndsWith("Size", lines[0].TrimEnd());
        Assert.StartsWith("first", lines[1]);
        Assert.EndsWith("10", lines[1].TrimEnd());
        Assert.EndsWith("200", lines[2].TrimEnd());
    }

    [Fact]
    public void TableSortsAndFlipsDirection()
    {
        var table = new Table<int>(Keymap)
        {
            Columns =
            [
                new()
                {
                    Header = static () => "Value",
                    Cell = static row => row.ToString(),
                    Sort = static (first, second) => first.CompareTo(second),
                },
            ],
            Rows = [3, 1, 2],
        };

        table.SortBy(0);
        Assert.Equal(1, table.SelectedRow);
        Assert.False(table.SortsDescending);

        table.SortBy(0);
        Assert.True(table.SortsDescending);
        Assert.Equal(3, table.SelectedRow);
    }

    [Fact]
    public void TableIgnoresSortingOnColumnsWithoutAComparison()
    {
        var table = new Table<int>(Keymap)
        {
            Columns = [new() { Header = static () => "Value", Cell = static row => row.ToString() }],
            Rows = [3, 1, 2],
        };

        table.SortBy(0);

        Assert.Equal(-1, table.SortedBy);
        Assert.Equal(3, table.SelectedRow);
    }

    [Fact]
    public void ProgressBarFillsProportionally()
    {
        var (surface, terminal) = CreateSurface(10, 1);
        var progress = new ProgressBar { Value = 50 };

        progress.Place(surface.Frame);

        var line = Render(surface, terminal)[0];
        Assert.Equal(5, CountOf(line, '█'));
        Assert.Equal(5, CountOf(line, '░'));
    }

    [Fact]
    public void ProgressBarShowsItsCaption()
    {
        var (surface, terminal) = CreateSurface(20, 1);
        var progress = new ProgressBar { Value = 25, Caption = static value => $"{value:0}%" };

        progress.Place(surface.Frame);

        Assert.Contains("25%", Render(surface, terminal)[0], StringComparison.Ordinal);
    }

    [Fact]
    public void SpinnerCyclesThroughItsFrames()
    {
        var spinner = new Spinner { Frames = ["a", "b"] };

        Assert.Equal("a", spinner.Current);
        spinner.Advance();
        Assert.Equal("b", spinner.Current);
        spinner.Advance();
        Assert.Equal("a", spinner.Current);
    }

    [Fact]
    public void TabsMoveWithArrowsAndReportTheChange()
    {
        var selected = new List<int>();
        var tabs = new Tabs(Keymap)
        {
            Titles = [static () => "One", static () => "Two", static () => "Three"],
            OnSelected = selected.Add,
        };

        tabs.Handle(new('\0', ConsoleKey.RightArrow, false, false, false));
        tabs.Handle(new('\0', ConsoleKey.RightArrow, false, false, false));
        tabs.Handle(new('\0', ConsoleKey.LeftArrow, false, false, false));

        Assert.Equal([1, 2, 1], selected);
        Assert.Equal(1, tabs.Selected);
    }

    [Fact]
    public void ClickingATabSelectsIt()
    {
        var (surface, _) = CreateSurface(30, 1);
        var tabs = new Tabs(Keymap)
        {
            Titles = [static () => "One", static () => "Two", static () => "Three"],
        };

        tabs.Place(surface.Frame);
        tabs.HandleMouse(new(MouseAction.Pressed, MouseButton.Left, 0, 8, default));

        Assert.Equal(1, tabs.Selected);
    }

    [Fact]
    public void StatusBarPutsItemsOnBothSides()
    {
        var (surface, terminal) = CreateSurface(30, 1);
        var status = new StatusBar
        {
            Left = [static () => "12 items"],
            Right = [static () => "Esc back"],
        };

        status.Place(surface.Frame);

        var line = Render(surface, terminal)[0];
        Assert.StartsWith("12 items", line);
        Assert.EndsWith("Esc back", line.TrimEnd());
    }

    [Fact]
    public void StatusBarDropsTheRightSideWhenItDoesNotFit()
    {
        var (surface, terminal) = CreateSurface(12, 1);
        var status = new StatusBar
        {
            Left = [static () => "a rather long left side"],
            Right = [static () => "right"],
        };

        status.Place(surface.Frame);

        Assert.DoesNotContain("right", Render(surface, terminal)[0], StringComparison.Ordinal);
    }

    private static int CountOf(string text, char character)
    {
        var count = 0;
        foreach (var current in text)
        {
            if (current == character)
            {
                count++;
            }
        }

        return count;
    }
}
