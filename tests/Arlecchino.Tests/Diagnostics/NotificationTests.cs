using System;
using System.Collections.Generic;
using System.Threading;
using Arlecchino.Diagnostics;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Diagnostics;

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

        app.Press(ConsoleKey.N, KeyModifiers.Control);

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
        app.Press(ConsoleKey.N, KeyModifiers.Control);
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
            new(ConsoleKey.F1, KeyModifiers.Alt),
            TimeSpan.FromSeconds(2)));

        app.State.Notifications.Notify("something");

        app.Press(ConsoleKey.F1);
        Assert.NotEqual(Routes.Notifications, app.Navigator.CurrentRoute);

        app.Press(ConsoleKey.F1, KeyModifiers.Alt);

        Assert.Equal(Routes.Notifications, app.Navigator.CurrentRoute);
        Assert.Equal(TimeSpan.FromSeconds(2), app.Options.NotificationTimeout);
    }

    [Fact]
    public void TheListIsBoundedHoweverYoungTheMessagesAre()
    {
        using var app = new TestApplication();

        app.State.Notifications.Capacity = 10;

        for (var index = 0; index < 25; index++)
        {
            app.State.Notifications.Notify($"message {index}");
        }

        var entries = app.State.Notifications.Entries;

        Assert.Equal(10, entries.Count);
        Assert.Equal("message 24", entries[0].Text);
        Assert.Equal("message 15", entries[^1].Text);
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

    [Fact]
    public void WorkStillRunningKeepsTheRowAndCountsUp()
    {
        using var app = new TestApplication();
        var counter = new Counter();

        app.State.Notifications.Raise(Running(() => $"copied {counter.Files} files"));

        Assert.Contains("copied 0 files", app.Frame(), StringComparison.Ordinal);

        counter.Files = 7;
        app.Advance(app.Options.NotificationTimeout + TimeSpan.FromSeconds(1));

        Assert.Contains("copied 7 files", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void AnOpenedNotificationShowsItsDetailAndRunsItsAction()
    {
        using var app = new TestApplication();
        var stopped = false;

        app.State.Notifications.Raise(new(DateTimeOffset.UtcNow, NotificationLevel.Warning, "3 files failed")
        {
            Detail = static () => "one.txt: in use",
            Actions = [new(static () => "Retry", () => stopped = true)],
        });

        app.Navigator.Apply(Routes.Notifications);
        app.Frame();
        app.Press(ConsoleKey.Enter);

        Assert.Contains("one.txt: in use", app.Frame(), StringComparison.Ordinal);
        Assert.Contains("Retry", app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.Enter);

        Assert.True(stopped);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void AnOpenedNotificationWithoutActionsJustCloses()
    {
        using var app = new TestApplication();

        app.State.Output = "saved";
        app.Navigator.Apply(Routes.Notifications);
        app.Frame();
        app.Press(ConsoleKey.Enter);

        Assert.Contains("saved", app.Frame(), StringComparison.Ordinal);

        app.Press(ConsoleKey.Escape);

        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void WorkThatSaysHowFarAlongItIsGetsABar()
    {
        using var app = new TestApplication();
        var entry = app.State.Notifications.Raise(new(DateTimeOffset.UtcNow, NotificationLevel.Information, "copying")
        {
            Progress = static () => "copying",
            Share = static () => 0.5,
        });

        app.Navigator.Apply(Routes.Notifications);
        app.Frame();
        app.Press(ConsoleKey.Enter);

        Assert.Contains("50%", app.Frame(), StringComparison.Ordinal);
        Assert.Equal(0.5, entry.Filled());
    }

    [Fact]
    public void WorkThatEndsChangesTheLineSomeoneIsAlreadyReading()
    {
        using var app = new TestApplication();
        var entry = app.State.Notifications.Raise(Running(static () => "copying 3 of 9"));

        app.Navigator.Apply(Routes.Notifications);
        app.Frame();
        app.Press(ConsoleKey.Enter);

        Assert.Contains("copying 3 of 9", app.Frame(), StringComparison.Ordinal);

        app.State.Notifications.Settle(entry, "Copied 9 files", NotificationLevel.Warning);

        Assert.Contains("Copied 9 files", app.Frame(), StringComparison.Ordinal);
        Assert.False(entry.IsRunning);
        Assert.Equal(NotificationLevel.Warning, entry.Loudness);
        Assert.Empty(entry.Actions);
    }

    [Fact]
    public void WorkThatEndedAgesFromWhenItEnded()
    {
        using var app = new TestApplication();
        var entry = app.State.Notifications.Raise(Running(static () => "copying"));

        app.Advance(app.Options.NotificationLifetime + TimeSpan.FromMinutes(1));

        Assert.Single(app.State.Notifications.Entries);

        app.State.Notifications.Settle(entry, "Copied");

        Assert.Single(app.State.Notifications.Entries);

        app.Advance(app.Options.NotificationLifetime + TimeSpan.FromMinutes(1));

        Assert.Empty(app.State.Notifications.Entries);
    }

    [Fact]
    public void ClearingTheListLeavesWorkThatIsStillRunning()
    {
        using var app = new TestApplication();

        app.State.Notifications.Notify("saved");

        var entry = app.State.Notifications.Raise(Running(static () => "copying"));

        app.Press(ConsoleKey.N, KeyModifiers.Control);
        app.Press(ConsoleKey.Backspace);

        Assert.Single(app.State.Notifications.Entries);
        Assert.Same(entry, app.State.Notifications.Entries[0]);

        app.State.Notifications.Settle(entry, "Copied");
        app.State.Notifications.Clear();

        Assert.Empty(app.State.Notifications.Entries);
    }

    [Fact]
    public void WorkThatEndedIsWithdrawnFromTheList()
    {
        using var app = new TestApplication();
        var entry = app.State.Notifications.Raise(Running(static () => "copying"));

        app.State.Notifications.Withdraw(entry);

        Assert.Empty(app.State.Notifications.Entries);
    }

    [Fact]
    public void WhatIsWorthShowingIsEverythingRunningAndWhateverEndedLately()
    {
        using var app = new TestApplication();

        app.State.Notifications.Notify("saved");

        var copying = app.State.Notifications.Raise(Running(static () => "copying"));

        Assert.Equal(["copying", "saved"], Lines(app));

        app.Advance(app.Options.NotificationTimeout + TimeSpan.FromSeconds(1));

        Assert.Equal(["copying"], Lines(app));

        app.State.Notifications.Settle(copying, "Copied 9 files");

        Assert.Equal(["Copied 9 files"], Lines(app));

        app.Advance(app.Options.NotificationTimeout + TimeSpan.FromSeconds(1));

        Assert.Empty(app.State.Notifications.Recent);
    }

    private static string[] Lines(TestApplication app)
    {
        var lines = new List<string>();

        foreach (var entry in app.State.Notifications.Recent)
        {
            lines.Add(entry.Line);
        }

        return [.. lines];
    }

    private static Notification Running(Func<string> progress) =>
        new(DateTimeOffset.UtcNow, NotificationLevel.Information, "working") { Progress = progress };

    [Fact]
    public void RaisingFromAnotherThreadIsCaughtRatherThanTolerated()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        Exception? thrown = null;

        var background = new Thread(() =>
        {
            try
            {
                app.State.Notifications.Notify("from a worker");
            }
            catch (Exception failure)
            {
                thrown = failure;
            }
        });

        background.Start();
        background.Join();

        Assert.IsType<InvalidOperationException>(thrown);
        Assert.Empty(app.State.Notifications.Entries);
    }

    [Fact]
    public void PostingIsHowBackgroundWorkSaysSomething()
    {
        using var app = new TestApplication();
        using var drawing = FrameThread.Claim();

        var background = new Thread(() => FrameThread.Post(() => app.State.Notifications.Notify("done")));

        background.Start();
        background.Join();

        FrameThread.RunPending(static _ => { });

        Assert.Single(app.State.Notifications.Entries);
    }

    [Fact]
    public void AnEntryFallingOffAsksForAFrameByItself()
    {
        using var app = new TestApplication();

        app.State.Notifications.Notify("said");

        var repaint = (Repaint)app.Services.GetService(typeof(Repaint))!;

        repaint.TakeRequested();

        Assert.False(repaint.IsRequested);

        app.Advance(app.Options.NotificationLifetime + TimeSpan.FromSeconds(1));

        Assert.True(repaint.IsRequested);
        Assert.Empty(app.State.Notifications.Entries);
    }

    private sealed class Counter
    {
        public int Files { get; set; }
    }
}
