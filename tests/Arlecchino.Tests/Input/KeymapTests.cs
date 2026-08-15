using System;
using Arlecchino.Commands;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.State;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Input;

public sealed class KeymapTests
{
    [Fact]
    public void BindingMatchesOnlyTheExactModifiers()
    {
        var binding = new KeyBinding(ConsoleKey.S, KeyModifiers.Control);

        Assert.True(binding.Matches(new(ConsoleKey.S, KeyModifiers.Control)));
        Assert.False(binding.Matches(new(ConsoleKey.S)));
        Assert.False(binding.Matches(new(ConsoleKey.S, KeyModifiers.Control | KeyModifiers.Shift)));
    }

    [Fact]
    public void BindingReadsAsTheKeyItIs()
    {
        Assert.Equal("Ctrl+S", new KeyBinding(ConsoleKey.S, KeyModifiers.Control).ToString());
        Assert.Equal("Alt+←", new KeyBinding(ConsoleKey.LeftArrow, KeyModifiers.Alt).ToString());
        Assert.Equal("Esc", new KeyBinding(ConsoleKey.Escape).ToString());
        Assert.Equal("Ctrl+Alt+Shift+F5",
            new KeyBinding(ConsoleKey.F5,
                KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift).ToString());
    }

    [Fact]
    public void MovingByWordIsBoundUnderBothHabits()
    {
        var keymap = new ArlecchinoKeymap();

        Assert.True(keymap.WordLeft.Matches(new(ConsoleKey.LeftArrow, KeyModifiers.Control)));
        Assert.True(keymap.WordLeft.Matches(new(ConsoleKey.LeftArrow, KeyModifiers.Alt)));
        Assert.True(keymap.WordRight.Matches(new(ConsoleKey.RightArrow, KeyModifiers.Control)));
        Assert.True(keymap.WordRight.Matches(new(ConsoleKey.RightArrow, KeyModifiers.Alt)));
        Assert.False(keymap.WordLeft.Matches(new(ConsoleKey.LeftArrow)));
    }

    [Fact]
    public void RubbingOutAWordIsBoundUnderBothHabitsToo()
    {
        var keymap = new ArlecchinoKeymap();

        Assert.True(keymap.EraseWord.Matches(new(ConsoleKey.Backspace, KeyModifiers.Control)));
        Assert.True(keymap.EraseWord.Matches(new(ConsoleKey.Backspace, KeyModifiers.Alt)));
        Assert.False(keymap.EraseWord.Matches(new(ConsoleKey.Backspace)));
    }

    [Fact]
    public void RemappedCancelIsUsedByModals()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.UseKeymap(new() { Cancel = new(ConsoleKey.Q, KeyModifiers.Control) }));

        app.State.RequestText("Name", "x", null, static _ => { });

        app.Press(ConsoleKey.Escape);
        Assert.NotNull(app.State.Modal);

        app.Press(ConsoleKey.Q, KeyModifiers.Control);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void RemappedHistoryKeysWalkTheHistory()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.UseKeymap(new()
            {
                Back = new(ConsoleKey.Backspace),
                Forward = new(ConsoleKey.Backspace, KeyModifiers.Shift),
            }));

        app.Press(ConsoleKey.O);
        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);

        app.Press(ConsoleKey.Backspace);
        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);

        app.Press(ConsoleKey.Backspace, KeyModifiers.Shift);
        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void RemappedMarkKeyMarksInAMultiChoice()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.UseKeymap(new() { Mark = new(ConsoleKey.Insert) }));

        string[] picked = [];
        app.State.RequestMultiChoice("Columns", ["a", "b"], [], value => picked = [.. value]);

        app.Press(ConsoleKey.Insert);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(["a"], picked);
    }

    [Fact]
    public void CommandsCanBindModifiers()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<SaveCommand>());

        app.Press(ConsoleKey.S);
        Assert.Equal("", app.State.Output);

        app.Press(ConsoleKey.S, KeyModifiers.Control);
        Assert.Equal("saved", app.State.Output);
    }

    [Fact]
    public void PaletteShowsTheBindingItWouldTake()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<SaveCommand>());

        app.Press(ConsoleKey.Oem1, KeyModifiers.Shift);

        Assert.Contains("Ctrl+S", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void FilePickerLegendShowsTheBoundKeys()
    {
        using var app = new TestApplication(150,
            30,
            static builder =>
                builder.UseKeymap(new() { NextField = new(ConsoleKey.F2) }));

        app.State.FilePicker = new("Pick", PickFolder: true, "", ViewRoute.None, static _ => { });
        app.Navigator.Apply(Routes.FilePicker);

        Assert.Contains("F2", app.Frame(), StringComparison.Ordinal);
    }
}

public sealed class SaveCommand : IArlecchinoCommand
{
    private readonly ArlecchinoState _state;

    public SaveCommand(ArlecchinoState state)
    {
        _state = state;
    }

    public KeyBinding Binding => new(ConsoleKey.S, KeyModifiers.Control);

    public string Icon => "▪";

    public string Label => "Save";

    public ViewRoute Execute()
    {
        _state.Output = "saved";
        return ViewRoute.None;
    }
}
