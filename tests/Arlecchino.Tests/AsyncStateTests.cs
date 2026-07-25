using System;
using System.Threading.Tasks;
using Arlecchino.State;
using Xunit;

namespace Arlecchino.Tests;

public sealed class AsyncStateTests
{
    [Fact]
    public async Task LoadedValueArrivesOnTheFrameLoop()
    {
        using var app = new TestApplication();
        var rows = new AsyncState<string>(app.Dispatcher);
        var release = new TaskCompletionSource();

        rows.Load(async _ =>
        {
            await release.Task;
            return "loaded";
        });

        Assert.Equal(LoadStatus.Loading, rows.Status.Value);
        Assert.True(rows.IsLoading);

        release.SetResult();
        await WaitForPending(app);

        app.Frame();

        Assert.Equal("loaded", rows.Value);
        Assert.Equal(LoadStatus.Loaded, rows.Status.Value);
    }

    [Fact]
    public async Task FailureIsKeptInsteadOfThrowing()
    {
        using var app = new TestApplication();
        var rows = new AsyncState<string>(app.Dispatcher);

        rows.Load(_ => Task.FromException<string>(new InvalidOperationException("no network")));
        await WaitForPending(app);

        app.Frame();

        Assert.Equal(LoadStatus.Failed, rows.Status.Value);
        Assert.Equal("no network", rows.Error.Value?.Message);
    }

    [Fact]
    public async Task ReloadingCancelsTheLoadInFlight()
    {
        using var app = new TestApplication();
        var rows = new AsyncState<string>(app.Dispatcher);
        var first = new TaskCompletionSource();

        rows.Load(async _ =>
        {
            await first.Task;
            return "stale";
        });

        rows.Load(_ => Task.FromResult("fresh"));
        await WaitForPending(app);
        app.Frame();

        first.SetResult();
        await Task.Delay(20);
        app.Frame();

        Assert.Equal("fresh", rows.Value);
    }

    [Fact]
    public async Task LoadedValueRequestsARepaint()
    {
        using var app = new TestApplication();
        var rows = new AsyncState<int>(app.Dispatcher);

        rows.Load(_ => Task.FromResult(7));
        await WaitForPending(app);

        app.Repaint.TakeRequested();
        app.Frame();

        Assert.Equal(7, rows.Value);
        Assert.True(app.Repaint.IsRequested);
    }

    private static async Task WaitForPending(TestApplication app)
    {
        for (var attempt = 0; attempt < 100 && !app.Dispatcher.HasPending; attempt++)
        {
            await Task.Delay(10);
        }
    }
}
