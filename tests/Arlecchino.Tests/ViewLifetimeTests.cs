using System;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Atoms;

namespace Arlecchino.Tests;

public sealed class ViewLifetimeTests
{
    [Fact]
    public void LeavingAScreenCancelsItsToken()
    {
        using var app = new TestApplication(configure: static builder => builder.AddView<LoadingView>("Loading"));

        app.Navigator.Apply(new("Loading"));
        var view = LoadingView.Last!;

        Assert.False(view.Lifetime.Closing.IsCancellationRequested);

        app.Navigator.Apply(ViewKind.Probe);

        Assert.True(view.Lifetime.Closing.IsCancellationRequested);
    }

    [Fact]
    public void LeavingAScreenDisposesWhatItTracked()
    {
        using var app = new TestApplication(configure: static builder => builder.AddView<LoadingView>("Loading"));

        app.Navigator.Apply(new("Loading"));
        var view = LoadingView.Last!;

        Assert.False(view.Watched.IsDisposed);

        app.Navigator.Apply(ViewKind.Probe);

        Assert.True(view.Watched.IsDisposed);
    }

    [Fact]
    public async Task ALoadStartedByAScreenStopsWhenTheScreenGoesAway()
    {
        using var app = new TestApplication(configure: static builder => builder.AddView<LoadingView>("Loading"));

        app.Navigator.Apply(new("Loading"));
        var view = LoadingView.Last!;
        var started = new TaskCompletionSource();
        var observed = CancellationToken.None;

        view.Rows.Load(async token =>
        {
            observed = token;
            started.SetResult();
            await Task.Delay(Timeout.Infinite, token);
            return "never";
        });

        await started.Task;
        app.Navigator.Apply(ViewKind.Probe);

        Assert.True(observed.IsCancellationRequested);
        Assert.False(view.Rows.IsLoading);
    }
}

public sealed class Watched : IDisposable
{
    public bool IsDisposed { get; private set; }

    public void Dispose() => IsDisposed = true;
}

public sealed class LoadingView : IArlecchinoView
{
    public static LoadingView? Last { get; private set; }

    private readonly Surface _surface;

    public LoadingView(Surface surface, ViewLifetime lifetime)
    {
        _surface = surface;
        Lifetime = lifetime;
        Rows = lifetime.Loading<string>();
        Watched = lifetime.Track(new Watched());
        Last = this;
    }

    public ViewLifetime Lifetime { get; }

    public AsyncAtom<string> Rows { get; }

    public Watched Watched { get; }

    public void Draw() => _surface.AppendLine("loading", Theme.Default);

    public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;
}
