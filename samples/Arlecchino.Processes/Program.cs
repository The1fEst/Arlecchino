using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Processes;
using Arlecchino.Processes.Views;
using Arlecchino.Rendering;

const int ReadTimeout = 1500;

if (args is ["--frame", ..])
{
    Frame(args.Length >= 2 ? args[1] : "processes", args.Length >= 3 ? args[2] : "120x34");
    return;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddArlecchino(options =>
    {
        options.MinimumWidth = 70;
        options.MinimumHeight = 16;
    })
    .AddGeneratedViews()
    .AddGeneratedStores()
    .UseMouse()
    .StartAt(ViewKind.Processes);

await builder.Build().RunAsync();

static void Frame(string view, string size)
{
    var services = new ServiceCollection();

    services.AddSingleton<IHostApplicationLifetime, NullLifetime>();
    services
        .AddArlecchino(options =>
        {
            options.MinimumWidth = 70;
            options.MinimumHeight = 16;
        })
        .AddGeneratedViews()
        .AddGeneratedStores()
        .WithoutHostedService();

    using var provider = services.BuildServiceProvider();

    var parts = size.Split('x');
    var width = parts.Length == 2 && int.TryParse(parts[0], out var w) ? w : 120;
    var height = parts.Length == 2 && int.TryParse(parts[1], out var h) ? h : 34;

    provider.GetRequiredService<Surface>().SetFixedSize(width, height);

    var navigator = provider.GetRequiredService<Navigator>();
    var screen = provider.GetRequiredService<Screen>();

    navigator.Apply(ViewKind.Processes);
    Thread.Sleep(ReadTimeout);
    screen.DrawOnce();

    if (view.Equals("details", StringComparison.OrdinalIgnoreCase))
    {
        var processes = provider.GetRequiredService<ProcessTable>();
        var rows = processes.Visible();

        if (rows.Count > 0)
        {
            processes.Selected.Value = rows[0];
        }

        navigator.Apply(ViewKind.Details);
        screen.DrawOnce();
    }

    Console.WriteLine();
}

internal sealed class NullLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted => CancellationToken.None;
    public CancellationToken ApplicationStopping => CancellationToken.None;
    public CancellationToken ApplicationStopped => CancellationToken.None;

    public void StopApplication()
    {
    }
}
