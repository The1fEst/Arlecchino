using System;
using Arlecchino.Navigation;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class HelpViewTests
{
    [Fact]
    public void TheHelpKeyOpensTheScreen()
    {
        using var app = new TestApplication();

        app.Press(ConsoleKey.F1);

        Assert.Equal(Routes.Help, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void EveryFrameworkKeyIsListedWithWhatItDoes()
    {
        using var app = new TestApplication(120, 40);

        app.Press(ConsoleKey.F1);
        var frame = app.Frame();

        Assert.Contains(app.Options.Strings.HelpFrameworkSection(), frame, StringComparison.Ordinal);
        Assert.Contains(app.Options.Keymap.Confirm.ToString(), frame, StringComparison.Ordinal);
        Assert.Contains("confirm, open, activate", frame, StringComparison.Ordinal);
        Assert.Contains(app.Options.Keymap.Copy.ToString(), frame, StringComparison.Ordinal);
    }

    [Fact]
    public void RegisteredCommandsAreListedToo()
    {
        using var app = new TestApplication(120, 40, static builder => builder.AddCommand<ProbeCommand>());

        app.Press(ConsoleKey.F1);
        var frame = app.Frame();

        Assert.Contains(app.Options.Strings.HelpCommandsSection(), frame, StringComparison.Ordinal);
        Assert.Contains("Probe", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void WithoutCommandsTheSectionSaysSo()
    {
        using var app = new TestApplication(120, 40);

        app.Press(ConsoleKey.F1);

        Assert.Contains(app.Options.Strings.HelpNoCommands(), app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommandsOfTheScreenItWasOpenedFromAreListed()
    {
        using var app = new TestApplication(120, 40, static builder =>
            builder.AddView<CommandingView>("Commanding"));

        app.Navigator.Apply(new("Commanding"));
        app.Press(ConsoleKey.F1);
        var frame = app.Frame();

        Assert.Contains(app.Options.Strings.HelpScreenSection("Commanding"), frame, StringComparison.Ordinal);
        Assert.Contains("build", frame, StringComparison.Ordinal);
        Assert.Contains("print", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void AScreenWithoutCommandsGetsNoSectionOfItsOwn()
    {
        using var app = new TestApplication(120, 40);

        app.Press(ConsoleKey.F1);

        Assert.DoesNotContain(app.Options.Strings.HelpScreenSection(ViewKind.Probe.Name), app.Frame(),
            StringComparison.Ordinal);
    }

    [Fact]
    public void TheKeysOfTheScreenStandBesideTheOnesThatWorkEverywhere()
    {
        using var app = new TestApplication(120, 24, static builder =>
            builder.AddView<CommandingView>("Commanding"));

        app.Navigator.Apply(new("Commanding"));
        app.Press(ConsoleKey.F1);

        Assert.True(SideBySide(app));
    }

    [Fact]
    public void TooNarrowForTwoColumnsTheyStackInstead()
    {
        using var app = new TestApplication(60, 24, static builder =>
            builder.AddView<CommandingView>("Commanding"));

        app.Navigator.Apply(new("Commanding"));
        app.Press(ConsoleKey.F1);

        Assert.False(SideBySide(app));
    }

    private static bool SideBySide(TestApplication app)
    {
        var everywhere = app.Options.Strings.HelpFrameworkSection();
        var screen = app.Options.Strings.HelpScreenSection("Commanding");

        foreach (var line in app.Frame().Split('\n'))
        {
            if (line.Contains(everywhere, StringComparison.Ordinal) &&
                line.Contains(screen, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    [Fact]
    public void CancelGoesBackToWhereItWasOpenedFrom()
    {
        using var app = new TestApplication();
        var before = app.Navigator.CurrentRoute;

        app.Press(ConsoleKey.F1);
        app.Press(ConsoleKey.Escape);

        Assert.Equal(before, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void TheKeyCanBeRebound()
    {
        using var app = new TestApplication(configure: static builder =>
            builder.UseKeymap(new() { Help = new(ConsoleKey.F2) }));

        app.Press(ConsoleKey.F1);
        Assert.NotEqual(Routes.Help, app.Navigator.CurrentRoute);

        app.Press(ConsoleKey.F2);
        Assert.Equal(Routes.Help, app.Navigator.CurrentRoute);
    }
}
