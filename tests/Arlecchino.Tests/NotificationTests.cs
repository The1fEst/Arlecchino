using System;
using Arlecchino.Diagnostics;
using Arlecchino.Navigation;
using Xunit;

namespace Arlecchino.Tests;

public sealed class NotificationTests
{
    [Fact]
    public void TheOutputRowShowsTheNewestMessageAndThenGoesQuiet()
    {
        using var app = new TestApplication();

        app.State.Output = "saved";
        Assert.Contains("saved", app.Frame(), StringComparison.Ordinal);

        app.Advance(app.Options.NotificationTimeout + TimeSpan.FromSeconds(1));

        Assert.DoesNotContain("saved", app.Frame(), StringComparison.Ordinal);
        Assert.Equal("", app.State.Output);
    }

    [Fact]
    public void AMessageOutlivesTheRowItWasShownOn()
    {
        using var app = new TestApplication();

        app.State.Output = "saved";
        app.Advance(app.Options.NotificationTimeout + TimeSpan.FromSeconds(1));

        Assert.Single(app.State.Notifications.Entries);
        Assert.Equal("saved", app.State.Notifications.Entries[0].Text);
    }

    [Fact]
    public void AMessageLeavesTheListOnceItsLifetimeIsUp()
    {
        using var app = new TestApplication();

        app.State.Output = "saved";
        app.Advance(app.Options.NotificationLifetime + TimeSpan.FromMinutes(1));

        Assert.Empty(app.State.Notifications.Entries);
    }

    [Fact]
    public void TheScreenListsWhatWasSaidNewestFirst()
    {
        using var app = new TestApplication();

        app.State.Notifications.Notify("first");
        app.State.Notifications.Notify("second", NotificationLevel.Failure);

        app.Press(ConsoleKey.N, control: true);

        Assert.Equal(Routes.Notifications, app.Navigator.CurrentRoute);

        var lines = app.FrameLines();
        var second = Array.FindIndex(lines, line => line.Contains("second", StringComparison.Ordinal));
        var first = Array.FindIndex(lines, line => line.Contains("first", StringComparison.Ordinal));

        Assert.True(second >= 0 && first > second);
    }

    [Fact]
    public void ClickingTheOutputRowOpensTheScreen()
    {
        using var app = new TestApplication();

        app.State.Output = "saved";
        app.Frame();

        app.Click(app.Terminal.Height - 1, 3);

        Assert.Equal(Routes.Notifications, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void TheScreenClearsTheListAndGoesBack()
    {
        using var app = new TestApplication();

        app.State.Notifications.Notify("something");
        app.Press(ConsoleKey.N, control: true);
        app.Press(ConsoleKey.Backspace);

        Assert.Empty(app.State.Notifications.Entries);
        Assert.Contains(app.Options.Strings.NotificationsEmpty(), app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.Escape);

        Assert.NotEqual(Routes.Notifications, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void TheKeyThatOpensTheScreenCanBeChosen()
    {
        using var app = new TestApplication(configure: static builder => builder.UseNotifications(
            new('\0', ConsoleKey.F1, shift: false, alt: true, control: false),
            TimeSpan.FromSeconds(2)));

        app.State.Notifications.Notify("something");

        app.Press(ConsoleKey.F1);
        Assert.NotEqual(Routes.Notifications, app.Navigator.CurrentRoute);

        app.Press(ConsoleKey.F1, alt: true);

        Assert.Equal(Routes.Notifications, app.Navigator.CurrentRoute);
        Assert.Equal(TimeSpan.FromSeconds(2), app.Options.NotificationTimeout);
    }

    [Fact]
    public void AnEmptyOutputClearsTheRowAtOnce()
    {
        using var app = new TestApplication();

        app.State.Output = "saved";
        app.State.Output = "";

        Assert.Equal("", app.State.Output);
        Assert.Empty(app.State.Notifications.Entries);
    }
}
