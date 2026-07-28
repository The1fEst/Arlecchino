using System;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Atoms;
using Arlecchino.Hosting;
using Xunit;

namespace Arlecchino.Tests;

public sealed class AsyncStoreTests
{
    [Fact]
    public void NothingToLoadStaysIdle()
    {
        var loading = new StoreLoading();

        loading.Start([], CancellationToken.None);

        Assert.Equal(LoadStatus.Idle, loading.Status.Value);
        Assert.False(loading.IsLoading);
    }

    [Fact]
    public void AStoreIsLoadingUntilItIsDone()
    {
        using var app = new TestApplication();
        var store = new Probe();
        var loading = new StoreLoading();

        loading.Start([store], CancellationToken.None);

        Assert.True(loading.IsLoading);
        Assert.Equal("", store.Server.Value);

        store.Finish();
        Settle(loading);

        Assert.True(loading.IsLoaded);
        Assert.Equal("loaded", store.Server.Value);
    }

    [Fact]
    public void WhatItLoadedIsThereOnTheDrawingThread()
    {
        using var app = new TestApplication();
        var store = new Probe();
        var loading = new StoreLoading();

        loading.Start([store], CancellationToken.None);
        store.Finish();
        store.Wait();

        Assert.Equal("", store.Server.Value);

        FrameThread.RunPending(static _ => { });

        Assert.Equal("loaded", store.Server.Value);
    }

    [Fact]
    public void AStoreThatThrowsFailsTheLoadAndKeepsWhatItThrew()
    {
        using var app = new TestApplication();
        var store = new Probe { Fails = true };
        var loading = new StoreLoading();
        Exception? reported = null;

        loading.Start([store], CancellationToken.None, exception => reported = exception);

        store.Finish();
        Settle(loading);

        Assert.True(loading.Failed);
        Assert.IsType<InvalidOperationException>(loading.Error.Value);
        Assert.Same(loading.Error.Value, reported);
    }

    [Fact]
    public void OneFailureAmongSeveralIsStillAFailure()
    {
        using var app = new TestApplication();
        var good = new Probe();
        var bad = new Probe { Fails = true };
        var loading = new StoreLoading();

        loading.Start([good, bad], CancellationToken.None);

        good.Finish();
        bad.Finish();
        Settle(loading);

        Assert.True(loading.Failed);
    }

    [Fact]
    public void AStoreThatWasCancelledIsNotAFailure()
    {
        using var app = new TestApplication();
        using var stopping = new CancellationTokenSource();
        var store = new Probe { Cancels = true };
        var loading = new StoreLoading();

        loading.Start([store], stopping.Token);

        stopping.Cancel();
        store.Finish();
        Settle(loading);

        Assert.Null(loading.Error.Value);
        Assert.False(loading.Failed);
    }

    private static void Settle(StoreLoading loading)
    {
        var waited = TimeSpan.Zero;

        while (waited < TimeSpan.FromSeconds(5))
        {
            FrameThread.RunPending(static _ => { });

            if (!loading.IsLoading)
            {
                return;
            }

            Thread.Sleep(10);
            waited += TimeSpan.FromMilliseconds(10);
        }

        Assert.Fail("the stores never finished loading");
    }

    private sealed class Probe : IArlecchinoAsyncStore
    {
        private readonly SemaphoreSlim _released = new(0);
        private readonly SemaphoreSlim _done = new(0);

        public LocalAtom<string> Server { get; } = new("");

        public bool Fails { get; init; }

        public bool Cancels { get; init; }

        public void Finish() => _released.Release();

        public void Wait() => Assert.True(_done.Wait(TimeSpan.FromSeconds(5)));

        public async Task LoadAsync(CancellationToken token)
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
