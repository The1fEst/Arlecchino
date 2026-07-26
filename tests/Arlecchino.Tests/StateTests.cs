using System;
using System.Collections.Generic;
using Xunit;
using Arlecchino.Atoms;

namespace Arlecchino.Tests;

public sealed class StateTests
{
    [Fact]
    public void WritingNotifiesSubscribers()
    {
        var name = new TrackedAtom<string>("");
        var seen = new List<string>();

        using var subscription = name.Subscribe(() => seen.Add(name.Value));

        name.Value = "first";
        name.Value = "second";

        Assert.Equal(["first", "second"], seen);
    }

    [Fact]
    public void WritingTheSameValueChangesNothing()
    {
        var count = new TrackedAtom<int>(1);
        var notified = 0;

        using var subscription = count.Subscribe(() => notified++);

        count.Value = 1;

        Assert.Equal(0, notified);
    }

    [Fact]
    public void DisposingStopsNotifications()
    {
        var count = new TrackedAtom<int>(0);
        var notified = 0;

        var subscription = count.Subscribe(() => notified++);
        count.Value = 1;
        subscription.Dispose();
        count.Value = 2;

        Assert.Equal(1, notified);
    }

    [Fact]
    public void ComputedTracksWhatItReads()
    {
        var first = new TrackedAtom<string>("a");
        var second = new TrackedAtom<string>("b");
        var joined = new Computed<string>(() => first.Value + second.Value);

        Assert.Equal("ab", joined.Value);

        first.Value = "x";
        Assert.Equal("xb", joined.Value);

        second.Value = "y";
        Assert.Equal("xy", joined.Value);
    }

    [Fact]
    public void ComputedNotifiesItsOwnSubscribers()
    {
        var length = new TrackedAtom<int>(1);
        var doubled = new Computed<int>(() => length.Value * 2);
        var notified = 0;

        using var subscription = doubled.Subscribe(() => notified++);

        length.Value = 5;

        Assert.Equal(1, notified);
        Assert.Equal(10, doubled.Value);
    }

    [Fact]
    public void ComputedChainsThroughOtherComputed()
    {
        var price = new TrackedAtom<decimal>(100);
        var withTax = new Computed<decimal>(() => price.Value * 1.2m);
        var rounded = new Computed<int>(() => (int)Math.Round(withTax.Value));

        Assert.Equal(120, rounded.Value);

        price.Value = 200;
        Assert.Equal(240, rounded.Value);
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
