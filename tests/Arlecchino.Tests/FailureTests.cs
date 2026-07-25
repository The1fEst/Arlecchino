using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Testing;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class FailureTests
{
    [Fact]
    public void ViewThatThrowsWhileDrawingDoesNotBreakTheFrame()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Breaking);
        var frame = app.Frame();

        Assert.Contains(app.Options.Strings.ViewFailed(BreakingView.DrawFailure), frame, StringComparison.Ordinal);
    }

    [Fact]
    public void ViewThatThrowsWhileHandlingKeepsTheApplicationAlive()
    {
        using var app = new TestApplication();

        app.Navigator.Apply(ViewKind.Breaking);
        app.Press(ConsoleKey.X);

        Assert.Contains(BreakingView.HandleFailure, app.State.Output, StringComparison.Ordinal);
        Assert.Equal(ViewKind.Breaking, app.Navigator.CurrentRoute);
    }

    [Fact]
    public void ModalCallbackThatThrowsIsReportedInsteadOfCrashing()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "x", null, static _ => throw new InvalidOperationException("callback failed"));
        app.Press(ConsoleKey.Enter);

        Assert.Contains("callback failed", app.State.Output, StringComparison.Ordinal);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public async Task StoppingTheHostLeavesFullScreen()
    {
        var terminal = new FakeTerminal(80, 24);
        var services = new ServiceCollection();

        services.AddSingleton<ITerminal>(terminal);
        services.AddSingleton<IHostApplicationLifetime, TestLifetime>();
        services.AddArlecchino().AddGeneratedViews().StartAt(ViewKind.Probe);

        await using var provider = services.BuildServiceProvider();
        var service = Assert.Single(provider.GetServices<IHostedService>().OfType<ArlecchinoHostedService>());

        using var stopping = new CancellationTokenSource();
        await service.StartAsync(stopping.Token);
        await WaitUntil(() => terminal.IsFullScreen);

        Assert.True(terminal.IsFullScreen);

        await service.StopAsync(stopping.Token);

        Assert.False(terminal.IsFullScreen);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 100 && !condition(); attempt++)
        {
            await Task.Delay(10);
        }
    }

    private sealed class TestLifetime : IHostApplicationLifetime
    {
        private readonly CancellationTokenSource _stopping = new();

        public CancellationToken ApplicationStarted => CancellationToken.None;

        public CancellationToken ApplicationStopping => _stopping.Token;

        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication() => _stopping.Cancel();
    }
}

public sealed class BreakingView : IView
{
    public const string DrawFailure = "draw failed";
    public const string HandleFailure = "handle failed";

    private readonly Surface _surface;

    public BreakingView(Surface surface)
    {
        _surface = surface;
    }

    public void Draw()
    {
        _surface.AppendLine("about to fail", Theme.Default);
        throw new InvalidOperationException(DrawFailure);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => throw new InvalidOperationException(HandleFailure);
}
