using System;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Tests.Views;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Hosting;

public sealed class LeakTests
{
    private const int Visits = 100;

    [Fact]
    public void SubscriptionsDoNotPileUpAsScreensComeAndGo()
    {
        WatchingView.Notified = 0;

        using var app = new TestApplication(60,
            20,
            static builder => builder
                .AddStore<CounterStore>()
                .AddView<WatchingView>("Watching"));

        var counter = app.Services.GetService(typeof(CounterStore)) as CounterStore;
        Assert.NotNull(counter);

        for (var visit = 0; visit < Visits; visit++)
        {
            app.Navigator.Apply(new("Watching"));
            app.Navigator.Apply(ViewKind.Probe);
        }

        app.Navigator.Apply(new("Watching"));
        WatchingView.Notified = 0;

        counter.Count.Value++;

        Assert.Equal(1, WatchingView.Notified);
    }

    [Fact]
    public void AScopedStoreIsDisposedWithTheScreenThatAskedForIt()
    {
        DraftStore.Created = 0;
        DraftStore.Disposed = 0;

        using var app = new TestApplication(60,
            20,
            static builder => builder
                .AddStore<DraftStore>()
                .AddView<DraftingView>("Drafting"));

        for (var visit = 0; visit < Visits; visit++)
        {
            app.Navigator.Apply(new("Drafting"));
            app.Navigator.Apply(ViewKind.Probe);
        }

        Assert.Equal(Visits, DraftStore.Created);
        Assert.Equal(Visits, DraftStore.Disposed);
    }

    [Fact]
    public void WorkTiedToAScreenStopsWhenTheScreenDoes()
    {
        TickingView.Ticks = 0;

        using var app = new TestApplication(60,
            20,
            static builder => builder
                .AddView<TickingView>("Ticking"));

        app.Navigator.Apply(new("Ticking"));
        app.Advance(TimeSpan.FromSeconds(3));

        var whileOpen = TickingView.Ticks;
        Assert.True(whileOpen > 0);

        app.Navigator.Apply(ViewKind.Probe);
        app.Advance(TimeSpan.FromSeconds(10));

        Assert.Equal(whileOpen, TickingView.Ticks);
    }

    public sealed class CounterStore : IArlecchinoStore
    {
        public Atom<int> Count { get; } = new LocalAtom<int>(0);
    }

    public sealed class DraftStore : IArlecchinoScopedStore, IDisposable
    {
        public DraftStore() => Created++;

        public static int Created { get; set; }

        public static int Disposed { get; set; }

        public void Dispose() => Disposed++;
    }

    public sealed class WatchingView : IArlecchinoView
    {
        private readonly Surface _surface;

        public WatchingView(Surface surface, CounterStore counter, ViewLifetime lifetime)
        {
            _surface = surface;
            lifetime.Track(counter.Count.Subscribe(static () => Notified++));
        }

        public static int Notified { get; set; }

        public void Draw() => _surface.AppendLine("watching", Theme.Default);

        public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
    }

    public sealed class DraftingView : IArlecchinoView
    {
        private readonly Surface _surface;

        public DraftingView(Surface surface, DraftStore draft)
        {
            _surface = surface;
            _ = draft;
        }

        public void Draw() => _surface.AppendLine("drafting", Theme.Default);

        public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
    }

    public sealed class TickingView : IArlecchinoView
    {
        private readonly Surface _surface;

        public TickingView(Surface surface, Ticker ticker, ViewLifetime lifetime)
        {
            _surface = surface;
            lifetime.Track(ticker.Every(TimeSpan.FromSeconds(1), static () => Ticks++));
        }

        public static int Ticks { get; set; }

        public void Draw() => _surface.AppendLine("ticking", Theme.Default);

        public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
    }
}
