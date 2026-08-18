using System;
using System.Collections.Generic;
using Xunit;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Atoms;

public sealed class StateTests
{
    [Fact]
    public void WritingNotifiesSubscribers()
    {
        var name = new TrackedAtom<string>("");
        var sightings = new List<string>();

        using var subscription = name.Subscribe(() => sightings.Add(name.Value));

        name.Value = "first";
        name.Value = "second";

        Assert.Equal(["first", "second"], sightings);
    }

    [Fact]
    public void WritingTheSameValueChangesNothing()
    {
        var count = new TrackedAtom<int>(1);
        var notices = 0;

        using var subscription = count.Subscribe(() => notices++);

        count.Value = 1;

        Assert.Equal(0, notices);
    }

    [Fact]
    public void DisposingStopsNotifications()
    {
        var count = new TrackedAtom<int>(0);
        var notices = 0;

        var subscription = count.Subscribe(() => notices++);
        count.Value = 1;
        subscription.Dispose();
        count.Value = 2;

        Assert.Equal(1, notices);
    }

    [Fact]
    public void ASubscriberMayUnsubscribeWhileBeingNotified()
    {
        var count = new LocalAtom<int>(0);
        var notices = 0;
        var subscriptions = new List<IDisposable>();

        subscriptions.Add(count.Subscribe(() =>
        {
            notices++;
            subscriptions[0].Dispose();
        }));

        using var second = count.Subscribe(() => notices++);

        count.Value = 1;
        count.Value = 2;

        Assert.Equal(3, notices);
    }

    [Fact]
    public void ASubscriberAddedWhileNotifyingHearsTheNextWriteOnly()
    {
        var count = new LocalAtom<int>(0);
        var late = 0;
        IDisposable? link = null;

        using var first = count.Subscribe(() => link ??= count.Subscribe(() => late++));

        count.Value = 1;
        count.Value = 2;

        link?.Dispose();

        Assert.Equal(1, late);
    }

    [Fact]
    public void ComputedTracksWhatItReads()
    {
        var first = new TrackedAtom<string>("a");
        var second = new TrackedAtom<string>("b");
        var pair = new Computed<string>(() => first.Value + second.Value);

        Assert.Equal("ab", pair.Value);

        first.Value = "x";
        Assert.Equal("xb", pair.Value);

        second.Value = "y";
        Assert.Equal("xy", pair.Value);
    }

    [Fact]
    public void ComputedNotifiesItsOwnSubscribers()
    {
        var length = new TrackedAtom<int>(1);
        var doubling = new Computed<int>(() => length.Value * 2);
        var notices = 0;

        using var subscription = doubling.Subscribe(() => notices++);

        length.Value = 5;

        Assert.Equal(1, notices);
        Assert.Equal(10, doubling.Value);
    }

    [Fact]
    public void ComputedChainsThroughOtherComputed()
    {
        var price = new TrackedAtom<decimal>(100);
        var withTax = new Computed<decimal>(() => price.Value * 1.2m);
        var whole = new Computed<int>(() => (int)Math.Round(withTax.Value));

        Assert.Equal(120, whole.Value);

        price.Value = 200;
        Assert.Equal(240, whole.Value);
    }

    [Fact]
    public void WritingRequestsARepaint()
    {
        using var app = new TestApplication();
        var flag = new TrackedAtom<bool>(false);

        app.Repaint.TakeRequested();
        flag.Value = true;

        Assert.True(app.Repaint.IsRequested);
    }
}
