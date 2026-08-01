using System;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.State;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class KeymapTests
{
    [Fact]
    public void BindingMatchesOnlyTheExactModifiers()
    {
        var binding = new KeyBinding(ConsoleKey.S, ConsoleModifiers.Control);

        Assert.True(binding.Matches(new('\0', ConsoleKey.S, false, false, true)));
        Assert.False(binding.Matches(new('\0', ConsoleKey.S, false, false, false)));
        Assert.False(binding.Matches(new('\0', ConsoleKey.S, true, false, true)));
    }

    [Fact]
    public void BindingReadsAsTheKeyItIs()
    {
        Assert.Equal("Ctrl+S", new KeyBinding(ConsoleKey.S, ConsoleModifiers.Control).ToString());
        Assert.Equal("Alt+←", new KeyBinding(ConsoleKey.LeftArrow, ConsoleModifiers.Alt).ToString());
        Assert.Equal("Esc", new KeyBinding(ConsoleKey.Escape).ToString());
        Assert.Equal("Ctrl+Alt+Shift+F5",
            new KeyBinding(ConsoleKey.F5,
                ConsoleModifiers.Control | ConsoleModifiers.Alt | ConsoleModifiers.Shift).ToString());
    }

    [Fact]
    public void RemappedCancelIsUsedByModals()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.UseKeymap(new() { Cancel = new(ConsoleKey.Q, ConsoleModifiers.Control) }));

        app.State.RequestText("Name", "x", null, static _ => { });

        app.Press(ConsoleKey.Escape);
        Assert.NotNull(app.State.Modal);

        app.Press(ConsoleKey.Q, control: true);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void RemappedHistoryKeysWalkTheHistory()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.UseKeymap(new()
            {
                Back = new(ConsoleKey.Backspace),
                Forward = new(ConsoleKey.Backspace, ConsoleModifiers.Shift),
            }));

        app.Press(ConsoleKey.O);
        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);

        app.Press(ConsoleKey.Backspace);
        Assert.Equal(ViewKind.Probe, app.Navigator.CurrentRoute);

        app.Press(ConsoleKey.Backspace, shift: true);
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

        app.Press(ConsoleKey.S, control: true);
        Assert.Equal("saved", app.State.Output);
    }

    [Fact]
    public void PaletteShowsTheBindingItWouldTake()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<SaveCommand>());

        app.Press(ConsoleKey.Oem1, shift: true);

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

    public KeyBinding Binding => new(ConsoleKey.S, ConsoleModifiers.Control);

    public string Icon => "▪";

    public string Label => "Save";

    public ViewRoute Execute()
    {
        _state.Output = "saved";
        return ViewRoute.None;
    }
}
