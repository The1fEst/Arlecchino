using System;
using System.IO;
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

    /// <summary>Naming a file is what asks for one, and the rest of the settings reach the stack.</summary>
    [Fact]
    public void TheSettingsReachTheStackAndTheFile()
    {
        var services = new ServiceCollection();

        services.UseHistory(options =>
        {
            options.FileName = "chosen.history";
            options.Capacity = 7;
        });

        using var provider = services.BuildServiceProvider();

        Assert.Equal(7, provider.GetRequiredService<AtomHistory>().Capacity);
        Assert.EndsWith("chosen.history", provider.GetRequiredService<HistoryFile>().Path, StringComparison.Ordinal);
    }

    /// <summary>Registers the stack and no file when the settings name none.</summary>
    [Fact]
    public void WithoutAFileNameTheHistoryIsOnlyInMemory()
    {
        var services = new ServiceCollection();

        services.UseHistory();

        using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<AtomHistory>());
        Assert.Null(provider.GetService<HistoryFile>());
    }

    /// <summary>
    /// The stores are built after the host has started, so asking for state to be resumed has to still
    /// be in force by then rather than having been a moment at startup.
    /// </summary>
    [Fact]
    public async Task AskingForStateToBeResumedReachesTheAtomsTheStoresBuild()
    {
        var folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        Directory.CreateDirectory(folder);

        try
        {
            var path = Path.Combine(folder, "resumed.history");

            using (var earlier = new HistoryFile(path))
            using (new AtomHistory(earlier))
            {
                new HistoryAtom<string>("start", "Hosted.Server").Value = "changed";
            }

            var services = new ServiceCollection();

            services.UseHistory(options => options.FileName = path);

            await using var provider = services.BuildServiceProvider();
            var started = provider.GetServices<IHostedService>().First();

            await started.StartAsync(CancellationToken.None);

            using var history = provider.GetRequiredService<AtomHistory>();

            Assert.Equal("changed", new HistoryAtom<string>("start", "Hosted.Server").Value);
        }
        finally
        {
            Directory.Delete(folder, true);
        }
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
