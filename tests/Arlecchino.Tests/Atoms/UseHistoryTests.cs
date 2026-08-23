using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;
using Arlecchino.Atoms.History;
using Arlecchino.Atoms.History.Hosting;

namespace Arlecchino.Tests.Atoms;

public sealed class UseHistoryTests
{
    [Fact]
    public void WithoutTheCallThereIsNoHistoryToResolve()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Null(provider.GetService<AtomHistory>());
    }

    [Fact]
    public void TheCallBringsTheStackAndTheFileWithIt()
    {
        var services = new ServiceCollection();

        services.UseHistory();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<AtomHistory>());
        Assert.NotNull(provider.GetService<HistoryFile>());
    }

    [Fact]
    public void TheSettingsReachTheStackAndTheFile()
    {
        var services = new ServiceCollection();

        services.UseHistory(options =>
        {
            options.FileName = "chosen.history";
            options.Keep = true;
            options.Capacity = 7;
        });

        using var provider = services.BuildServiceProvider();

        Assert.Equal(7, provider.GetRequiredService<AtomHistory>().Capacity);
        Assert.True(provider.GetRequiredService<HistoryFile>().Keep);
        Assert.EndsWith("chosen.history", provider.GetRequiredService<HistoryFile>().Path, StringComparison.Ordinal);
    }

    /// <summary>
    /// Readying the history has to happen before the frame loop, which is a hosted service registered
    /// before this call, so the one added here goes to the front rather than to the end.
    /// </summary>
    [Fact]
    public void ReadyingTheHistoryIsTheFirstHostedServiceToRun()
    {
        var services = new ServiceCollection();

        services.AddHostedService<Later>();
        services.UseHistory();

        using var provider = services.BuildServiceProvider();

        Assert.IsNotType<Later>(Assert.IsAssignableFrom<IHostedService>(provider.GetServices<IHostedService>().First()));
    }

    [Fact]
    public void AskingTwiceIsRefused()
    {
        var services = new ServiceCollection();

        services.UseHistory();

        Assert.Throws<InvalidOperationException>(() => services.UseHistory());
    }

    private sealed class Later : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
