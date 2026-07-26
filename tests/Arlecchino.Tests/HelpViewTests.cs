using System;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
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
            builder.UseKeymap(new ArlecchinoKeymap { Help = new KeyBinding(ConsoleKey.F2) }));

        app.Press(ConsoleKey.F1);
        Assert.NotEqual(Routes.Help, app.Navigator.CurrentRoute);

        app.Press(ConsoleKey.F2);
        Assert.Equal(Routes.Help, app.Navigator.CurrentRoute);
    }
}
