using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Tests.Views;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Navigation;

public sealed class ViewCommandTests
{
    [Fact]
    public void ViewCommandRunsOnItsKey()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();

        app.Press(ConsoleKey.B);

        Assert.Equal(["build"], CommandingView.Ran);
    }

    [Fact]
    public void ViewCommandCanNavigate()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Commanding);
        app.Press(ConsoleKey.G);

        Assert.Equal(ViewKind.Other, app.Navigator.CurrentRoute);
    }

    /// <summary>
    /// A command that is off takes nothing: the key carries on to the view, which is free to give it
    /// another meaning for exactly the times the command is unavailable. Swallowing it instead left a
    /// key that silently did nothing, and no way for the view to find out it had been pressed.
    /// </summary>
    [Fact]
    public void ADisabledViewCommandLetsItsKeyThrough()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();
        CommandingView.CanClean = false;

        app.Press(ConsoleKey.L);

        Assert.Equal(["handled L"], CommandingView.Ran);
        Assert.Equal(ViewKind.Commanding, app.Navigator.CurrentRoute);

        CommandingView.CanClean = true;
    }

    [Fact]
    public void AnEnabledViewCommandKeepsItsKeyFromTheView()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();

        app.Press(ConsoleKey.L);

        Assert.Equal(["clean"], CommandingView.Ran);
    }

    [Fact]
    public void KeysTheViewDoesNotBindStillReachHandle()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();

        app.Press(ConsoleKey.Z);

        Assert.Equal(["handled Z"], CommandingView.Ran);
    }

    [Fact]
    public void ViewCommandsShadowApplicationCommandsOnTheSameKey()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<ProbeCommand>());

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();

        app.Press(ConsoleKey.P);

        Assert.Equal(["print"], CommandingView.Ran);
        Assert.Equal("", app.State.Output);
    }

    [Fact]
    public void PaletteListsViewCommandsBeforeApplicationCommands()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<ProbeCommand>());

        app.Navigator.Apply(ViewKind.Commanding);
        app.Press(ConsoleKey.Oem1, KeyModifiers.Shift);

        var frame = app.Frame();

        Assert.Contains("build", frame, StringComparison.Ordinal);
        Assert.Contains("Probe command", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void PaletteRunsAViewCommandByItsKey()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<ProbeCommand>());

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();

        app.Press(ConsoleKey.Oem1, KeyModifiers.Shift);
        app.Press(ConsoleKey.B);

        Assert.Equal(["build"], CommandingView.Ran);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void HintsFallBackToTheCommandsOfTheView()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Commanding);

        var frame = app.Frame();

        Assert.Contains("B → build", frame, StringComparison.Ordinal);
        Assert.Contains("G → go", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewsWithoutCommandsKeepTheirOwnHints()
    {
        using var app = new TestApplication();

        Assert.Contains("o → other", app.Frame(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Holding Alt puts an escape in front of the key, so <c>Alt+Esc</c> is two of them. The runtime
    /// folds that prefix back for every other key but not for this one, which reached an application
    /// as two plain Escapes and left the binding impossible to press. The bytes here are the ones a
    /// real terminal sends, which is how the fault was found in the first place.
    /// </summary>
    [Fact]
    public void AltEscapeArrivesAsOneKeyRatherThanTwoPlainEscapes()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();

        app.ReadFromTerminal("\e\e");

        Assert.Equal(["stop"], CommandingView.Ran);
    }
}

public sealed class CommandingView : IArlecchinoView
{
    public static List<string> Ran { get; } = [];

    public static bool CanClean { get; set; } = true;

    private readonly Surface _surface;

    public CommandingView(Surface surface)
    {
        _surface = surface;
    }

    public void Draw() => _surface.AppendLine("commanding", Theme.Default);

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.For(ConsoleKey.B, static () => "build", static () => Ran.Add("build")),
        ViewCommand.For(ConsoleKey.P, static () => "print", static () => Ran.Add("print")),
        ViewCommand.Navigating(ConsoleKey.G, static () => "go", static () => ViewKind.Other),
        new()
        {
            Binding = new(ConsoleKey.L),
            Label = static () => "clean",
            IsEnabled = static () => CanClean,
            Run = static () =>
            {
                Ran.Add("clean");
                return ViewRoute.None;
            },
        },
        ViewCommand.For(new KeyBinding(ConsoleKey.Escape, KeyModifiers.Alt), static () => "stop",
            static () => Ran.Add("stop")),
    ];

    public ViewRoute Handle(KeyPress key)
    {
        Ran.Add($"handled {key.Key}");
        return ViewRoute.None;
    }
}
