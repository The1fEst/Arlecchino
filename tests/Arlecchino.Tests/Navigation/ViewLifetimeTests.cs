using System;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Tests.Views;
using Xunit;
using Arlecchino.Atoms;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Navigation;

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

        Assert.False(view.Watcher.IsDisposed);

        app.Navigator.Apply(ViewKind.Probe);

        Assert.True(view.Watcher.IsDisposed);
    }

    [Fact]
    public async Task ALoadStartedByAScreenStopsWhenTheScreenGoesAway()
    {
        using var app = new TestApplication(configure: static builder => builder.AddView<LoadingView>("Loading"));

        app.Navigator.Apply(new("Loading"));
        var view = LoadingView.Last!;
        var started = new TaskCompletionSource();
        var token = CancellationToken.None;

        view.Rows.Load(async given =>
        {
            token = given;
            started.SetResult();
            await Task.Delay(Timeout.Infinite, token);
            return "never";
        });

        await started.Task;
        app.Navigator.Apply(ViewKind.Probe);

        Assert.True(token.IsCancellationRequested);
        Assert.False(view.Rows.IsLoading);
    }
}

public sealed class Watcher : IDisposable
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
        Watcher = lifetime.Track(new Watcher());
        Last = this;
    }

    public ViewLifetime Lifetime { get; }

    public AsyncAtom<string> Rows { get; }

    public Watcher Watcher { get; }

    public void Draw() => _surface.AppendLine("loading", Theme.Default);

    public ViewRoute Handle(KeyPress key) => ViewRoute.None;
}
