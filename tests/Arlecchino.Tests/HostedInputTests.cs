using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Arlecchino.Hosting;
using Arlecchino.State;
using Arlecchino.Testing;
using Arlecchino.Tests.Views;
using Xunit;

namespace Arlecchino.Tests;

public sealed class HostedInputTests
{
    private static readonly List<string> Logged = [];

    private static (ServiceProvider Provider, FakeTerminal Terminal) CreateHost(bool mouse)
    {
        var terminal = new FakeTerminal(80, 24);
        var services = new ServiceCollection();

        Logged.Clear();
        services.AddLogging(builder => builder.AddProvider(new CapturingProvider()));
        services.AddSingleton<IArlecchinoTerminal>(terminal);
        services.AddSingleton<IHostApplicationLifetime, NullLifetime>();

        var builder = services.AddArlecchino(options =>
        {
            options.MinimumWidth = 1;
            options.MinimumHeight = 1;
            options.AskTerminal = false;
        }).AddGeneratedViews().StartAt(ViewKind.Probe);
        if (mouse)
        {
            builder.UseMouse();
        }

        return (services.BuildServiceProvider(), terminal);
    }

    private static async Task<T> WaitFor<T>(Func<T> read, Func<T, bool> until)
    {
        for (var attempt = 0; attempt < 200; attempt++)
        {
            var value = read();
            if (until(value))
            {
                return value;
            }

            await Task.Delay(10);
        }

        return read();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task KeysTypedIntoTheTerminalReachTheView(bool mouse)
    {
        var (provider, terminal) = CreateHost(mouse);
        await using var _ = provider;

        var service = provider.GetServices<IHostedService>().OfType<ArlecchinoHostedService>().Single();
        var navigator = provider.GetRequiredService<Navigation.Navigator>();

        using var stopping = new CancellationTokenSource();
        await service.StartAsync(stopping.Token);

        terminal.Enqueue(new('o', ConsoleKey.O, false, false, false));

        var route = await WaitFor(() => navigator.CurrentRoute, current => current == ViewKind.Other);

        await service.StopAsync(stopping.Token);

        Assert.Equal(ViewKind.Other, route);
    }

    [Fact]
    public async Task AFullRedrawPaintsTheScreenAgainWithoutAnythingChanging()
    {
        var (provider, terminal) = CreateHost(mouse: false);
        await using var _ = provider;

        var service = provider.GetServices<IHostedService>().OfType<ArlecchinoHostedService>().Single();
        var screen = provider.GetRequiredService<Screen>();

        using var stopping = new CancellationTokenSource();
        await service.StartAsync(stopping.Token);

        var first = await WaitFor(() => terminal.Written, text => text.Length > 0);
        terminal.Clear();

        screen.RedrawEverything();

        var again = await WaitFor(() => terminal.Written, text => text.Length >= first.Length);

        await service.StopAsync(stopping.Token);

        Assert.Equal(
            FrameText.WithoutStyles(first).TrimEnd(),
            FrameText.WithoutStyles(again).TrimEnd());
    }

    [Fact]
    public async Task TheTerminalIsHandedBackWhenTheServiceStops()
    {
        var (provider, terminal) = CreateHost(mouse: true);
        await using var _ = provider;

        var service = provider.GetServices<IHostedService>().OfType<ArlecchinoHostedService>().Single();

        using var stopping = new CancellationTokenSource();
        await service.StartAsync(stopping.Token);
        await WaitFor(() => terminal.Written.Length, written => written > 0);

        Assert.True(terminal.IsFullScreen);
        Assert.True(terminal.IsMouseEnabled);
        Assert.True(terminal.IsPasteEnabled);

        await service.StopAsync(stopping.Token);

        Assert.False(terminal.IsFullScreen);
        Assert.False(terminal.IsMouseEnabled);
        Assert.False(terminal.IsPasteEnabled);
    }

    [Fact]
    public async Task FramesKeepBeingDrawnAfterInput()
    {
        var (provider, terminal) = CreateHost(mouse: false);
        await using var _ = provider;

        var service = provider.GetServices<IHostedService>().OfType<ArlecchinoHostedService>().Single();
        var state = provider.GetRequiredService<ArlecchinoState>();

        using var stopping = new CancellationTokenSource();
        await service.StartAsync(stopping.Token);

        await WaitFor(() => terminal.Written.Length, written => written > 0);
        terminal.Clear();

        FrameThread.Post(() => state.Output = "changed");

        var written = await WaitFor(() => terminal.Written, text => text.Contains("changed", StringComparison.Ordinal));

        await service.StopAsync(stopping.Token);

        Assert.True(
            written.Contains("changed", StringComparison.Ordinal),
            $"repaint requested: {provider.GetRequiredService<Repaint>().IsRequested}, " +
            $"loop done: {service.ExecuteTask?.IsCompleted}, " +
            $"written: {written.Length}, log: {string.Join(" || ", Logged)}");
    }

    private sealed class CapturingProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new Capturing();

        public void Dispose()
        {
        }

        private sealed class Capturing : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter) =>
                Logged.Add($"{logLevel}: {formatter(state, exception)} {exception}");
        }
    }

    private sealed class NullLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted => CancellationToken.None;

        public CancellationToken ApplicationStopping => CancellationToken.None;

        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication()
        {
        }
    }
}
