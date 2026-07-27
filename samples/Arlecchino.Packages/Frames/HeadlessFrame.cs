using System;
using System.Threading;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Packages.Model;
using Arlecchino.Packages.Stores;
using Arlecchino.Packages.Views;
using Arlecchino.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arlecchino.Packages.Frames;

public static class HeadlessFrame
{
    private const int ScanTimeoutSeconds = 120;
    private const int ScanningFrameDelay = 1600;
    private const int PollInterval = 100;
    private const int MinimumWidth = 84;
    private const int MinimumHeight = 20;

    public static void Render(string view, string size, string script, string solution)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IHostApplicationLifetime, NullLifetime>();
        services
            .AddArlecchino(options =>
            {
                options.MinimumWidth = MinimumWidth;
                options.MinimumHeight = MinimumHeight;
                options.ShowOutputLine = false;
                options.ShowHints = false;
            })
            .AddGeneratedViews()
            .AddGeneratedStores()
            .AddGeneratedCommands()
            .UseTheme(ThemePalette.Arlecchino)
            .WithoutHostedService();

        using var provider = services.BuildServiceProvider();

        var (width, height) = Size(size);
        provider.GetRequiredService<Surface>().SetFixedSize(width, height);


        var inventory = provider.GetRequiredService<Inventory>();

        inventory.Solution.Value = solution;
        inventory.Rescan();

        Settle(inventory, view is "scanning");

        if (view is "package" or "upgrade")
        {
            Select(provider, inventory);
        }

        provider.GetRequiredService<Navigator>().Apply(Route(view));

        Play(provider, script);

        provider.GetRequiredService<Screen>().DrawOnce();

        Console.WriteLine();
    }

    private static void Settle(Inventory inventory, bool midScan)
    {
        var deadline = midScan
            ? DateTime.UtcNow.AddMilliseconds(ScanningFrameDelay)
            : DateTime.UtcNow.AddSeconds(ScanTimeoutSeconds);

        while (inventory.Scan.IsLoading && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(PollInterval);
            FrameThread.RunPending(static _ => { });
        }

        FrameThread.RunPending(static _ => { });
    }

    private static void Play(IServiceProvider provider, string script)
    {
        if (script.Length == 0)
        {
            return;
        }

        var router = provider.GetRequiredService<InputRouter>();

        foreach (var key in KeyScript.Parse(script))
        {
            router.ProcessKey(key);
            FrameThread.RunPending(static _ => { });
        }
    }

    private static void Select(IServiceProvider provider, Inventory inventory)
    {
        var picked = Worst(inventory);
        inventory.Selected.Value = picked;

        if (picked is not null && inventory.Scan.Value is { } catalog)
        {
            provider.GetRequiredService<UpgradePlan>().Restart(picked, catalog);
        }
    }

    private static (int Width, int Height) Size(string size)
    {
        var parts = size.Split('x');

        return (parts.Length == 2 && int.TryParse(parts[0], out var columns) ? columns : 120,
            parts.Length == 2 && int.TryParse(parts[1], out var rows) ? rows : 34);
    }

    private static ViewRoute Route(string view) => view.ToLowerInvariant() switch
    {
        "package" => ViewKind.Package,
        "projects" => ViewKind.Projects,
        "upgrade" => ViewKind.Upgrade,
        _ => ViewKind.Inventory,
    };

    private static PackageRow? Worst(Inventory inventory)
    {
        PackageRow? worst = null;

        foreach (var package in inventory.Scan.Value?.Packages ?? [])
        {
            if (package.IsTransitive)
            {
                continue;
            }

            if (worst is null || Rank(package) > Rank(worst))
            {
                worst = package;
            }
        }

        return worst;
    }

    private static int Rank(PackageRow package) => ((int)package.Health * 10) + (package.WorstAdvisory()?.Rank ?? 0);

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
