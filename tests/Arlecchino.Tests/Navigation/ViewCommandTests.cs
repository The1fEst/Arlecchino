using System;
using System.Collections.Generic;
using Arlecchino.Commands;
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

    [Fact]
    public void DisabledViewCommandDoesNothingAndDoesNotFallThrough()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Commanding);
        CommandingView.Ran.Clear();
        CommandingView.CanClean = false;

        app.Press(ConsoleKey.L);

        Assert.Empty(CommandingView.Ran);
        Assert.Equal(ViewKind.Commanding, app.Navigator.CurrentRoute);

        CommandingView.CanClean = true;
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
        app.Press(ConsoleKey.Oem1, shift: true);

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

        app.Press(ConsoleKey.Oem1, shift: true);
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
    ];

    public ViewRoute Handle(ConsoleKeyInfo key)
    {
        Ran.Add($"handled {key.Key}");
        return ViewRoute.None;
    }
}
