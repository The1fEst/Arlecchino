using System;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Arlecchino.Tests;

public sealed class TickerTests
{
    [Fact]
    public void RepeatingWorkRunsOncePerInterval()
    {
        using var app = new TestApplication();
        var ticker = app.Services.GetRequiredService<Ticker>();
        var runs = 0;

        using var scheduled = ticker.Every(TimeSpan.FromSeconds(1), () => runs++);

        app.Advance(TimeSpan.FromMilliseconds(900));
        Assert.Equal(0, runs);

        app.Advance(TimeSpan.FromMilliseconds(200));
        Assert.Equal(1, runs);

        app.Advance(TimeSpan.FromSeconds(3));
        Assert.Equal(4, runs);
    }

    [Fact]
    public void DelayedWorkRunsOnceAndForgetsItself()
    {
        using var app = new TestApplication();
        var ticker = app.Services.GetRequiredService<Ticker>();
        var runs = 0;

        ticker.After(TimeSpan.FromSeconds(1), () => runs++);

        app.Advance(TimeSpan.FromSeconds(5));
        Assert.Equal(1, runs);

        app.Advance(TimeSpan.FromSeconds(5));
        Assert.Equal(1, runs);
        Assert.Null(ticker.NextDue);
    }

    [Fact]
    public void DisposingTheHandleStopsTheWork()
    {
        using var app = new TestApplication();
        var ticker = app.Services.GetRequiredService<Ticker>();
        var runs = 0;

        var scheduled = ticker.Every(TimeSpan.FromSeconds(1), () => runs++);

        app.Advance(TimeSpan.FromSeconds(1));
        scheduled.Dispose();
        app.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(1, runs);
    }

    [Fact]
    public void WorkThatRunsAsksForAFrame()
    {
        using var app = new TestApplication();
        var ticker = app.Services.GetRequiredService<Ticker>();

        using var scheduled = ticker.Every(TimeSpan.FromSeconds(1), static () => { });

        app.Frame();
        app.Repaint.TakeRequested();

        app.Advance(TimeSpan.FromMilliseconds(500));
        Assert.False(app.Repaint.TakeRequested());

        app.Advance(TimeSpan.FromSeconds(1));
        Assert.True(app.Repaint.TakeRequested());
    }

    [Fact]
    public void OneFailingActionDoesNotStopTheRest()
    {
        using var app = new TestApplication();
        var ticker = app.Services.GetRequiredService<Ticker>();
        var ran = false;

        using var failing = ticker.Every(TimeSpan.FromSeconds(1), static () => throw new InvalidOperationException("no"));
        using var working = ticker.Every(TimeSpan.FromSeconds(1), () => ran = true);

        app.Advance(TimeSpan.FromSeconds(1));

        Assert.True(ran);
    }

    [Fact]
    public void WorkTiedToAScreenStopsWithIt()
    {
        using var app = new TestApplication();
        var ticker = app.Services.GetRequiredService<Ticker>();
        var runs = 0;

        var scope = app.Services.CreateScope();
        var lifetime = scope.ServiceProvider.GetRequiredService<ViewLifetime>();

        lifetime.Track(ticker.Every(TimeSpan.FromSeconds(1), () => runs++));

        app.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(1, runs);

        scope.Dispose();
        app.Advance(TimeSpan.FromSeconds(5));

        Assert.Equal(1, runs);
    }
}
