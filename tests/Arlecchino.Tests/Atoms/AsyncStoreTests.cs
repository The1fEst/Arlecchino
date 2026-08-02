using System;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Atoms;

public sealed class AsyncStoreTests
{
    [Fact]
    public void AStoreIsIdleUntilItIsStarted()
    {
        var store = new Probe();

        Assert.Equal(LoadStatus.Idle, store.Status.Value);
        Assert.False(store.IsLoading);
        Assert.False(store.Ready.IsCompleted);
    }

    [Fact]
    public void ItIsLoadingUntilItIsDone()
    {
        using var app = new TestApplication();
        var store = new Probe();

        var running = store.RunAsync(null, CancellationToken.None);

        Settle(store, LoadStatus.Loading);

        Assert.True(store.IsLoading);
        Assert.Equal("", store.Server.Value);

        store.Finish();
        Settle(store, LoadStatus.Loaded);

        Assert.True(store.IsLoaded);
        Assert.True(running.IsCompleted);
    }

    [Fact]
    public async Task ReadyCompletesWhenTheStoreIsLoaded()
    {
        using var app = new TestApplication();
        var store = new Probe();

        _ = store.RunAsync(null, CancellationToken.None);

        store.Finish();

        await store.Ready.WaitAsync(TimeSpan.FromSeconds(5));

        FrameThread.RunPending(static _ => { });

        Assert.Equal("loaded", store.Server.Value);
    }

    [Fact]
    public async Task ReadyHandsOverWhatTheLoadThrew()
    {
        using var app = new TestApplication();
        var store = new Probe { Fails = true };
        Exception? reported = null;

        _ = store.RunAsync(exception => reported = exception, CancellationToken.None);

        store.Finish();

        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(() => store.Ready.WaitAsync(TimeSpan.FromSeconds(5)));

        Settle(store, LoadStatus.Failed);

        Assert.True(store.Failed);
        Assert.Same(thrown, store.Error.Value);
        Assert.Same(thrown, reported);
    }

    [Fact]
    public async Task AStoreThatWasCancelledIsNotAFailure()
    {
        using var app = new TestApplication();
        using var stopping = new CancellationTokenSource();
        var store = new Probe { Cancels = true };

        _ = store.RunAsync(null, stopping.Token);

        await stopping.CancelAsync();
        store.Finish();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => store.Ready.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken.None));

        Settle(store, LoadStatus.Idle);

        Assert.False(store.Failed);
        Assert.Null(store.Error.Value);
    }

    [Fact]
    public void WhatItLoadedReachesTheAtomsOnTheDrawingThread()
    {
        using var app = new TestApplication();
        var store = new Probe();

        _ = store.RunAsync(null, CancellationToken.None);

        store.Finish();
        store.Wait();

        Assert.Equal("", store.Server.Value);

        FrameThread.RunPending(static _ => { });

        Assert.Equal("loaded", store.Server.Value);
    }

    private static void Settle(Probe store, LoadStatus expected)
    {
        var waited = TimeSpan.Zero;

        while (waited < TimeSpan.FromSeconds(5))
        {
            FrameThread.RunPending(static _ => { });

            if (store.Status.Value == expected)
            {
                return;
            }

            Thread.Sleep(10);
            waited += TimeSpan.FromMilliseconds(10);
        }

        Assert.Fail($"the store never reached {expected}; it is {store.Status.Value}");
    }

    private sealed class Probe : ArlecchinoAsyncStore
    {
        private readonly SemaphoreSlim _released = new(0);
        private readonly SemaphoreSlim _done = new(0);

        public LocalAtom<string> Server { get; } = new("");

        public bool Fails { get; init; }

        public bool Cancels { get; init; }

        public void Finish() => _released.Release();

        public void Wait() => Assert.True(_done.Wait(TimeSpan.FromSeconds(5)));

        protected override async Task LoadAsync(CancellationToken token)
        {
            try
            {
                await _released.WaitAsync(CancellationToken.None).ConfigureAwait(false);

                if (Cancels)
                {
                    token.ThrowIfCancellationRequested();
                }

                if (Fails)
                {
                    throw new InvalidOperationException("settings.json is not json");
                }

                Server.Post("loaded");
            }
            finally
            {
                _done.Release();
            }
        }
    }
}
