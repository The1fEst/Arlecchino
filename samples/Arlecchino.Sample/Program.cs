using System;
using System.Threading;
using Arlecchino;
using Arlecchino.Hosting;
using Arlecchino.Modals;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Sample;
using Arlecchino.Sample.Views;
using Arlecchino.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

if (args is ["--frame", ..])
{
    Frame(args.Length >= 2 ? args[1] : "default", args.Length >= 3 ? args[2] : "120x34");
    return;
}

if (args is ["--keys"])
{
    ReportKeys();
    return;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Services
    .AddArlecchino(options =>
    {
        options.MinimumWidth = 60;
        options.MinimumHeight = 16;
    })
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddGeneratedCommands()
    .UseLatinOnlyInput()
    .UseMouse()
    .StartAt(ViewKind.Default);

await builder.Build().RunAsync();

static void ReportKeys()
{
    Console.WriteLine("Press keys to see what the terminal reports. Esc twice to leave.");

    var lastWasEscape = false;

    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        var character = key.KeyChar == '\0' ? "none" : $"'{key.KeyChar}' U+{(int)key.KeyChar:X4}";

        Console.WriteLine($"Key={key.Key} ({(int)key.Key})   Char={character}   Modifiers={key.Modifiers}");

        if (key.Key == ConsoleKey.Escape || key.KeyChar == '\e')
        {
            if (lastWasEscape)
            {
                return;
            }

            lastWasEscape = true;
            continue;
        }

        lastWasEscape = false;
    }
}

static void Frame(string view, string size)
{
    var services = new ServiceCollection();
    services
        .AddArlecchino(options =>
        {
            options.MinimumWidth = 60;
            options.MinimumHeight = 16;
        })
        .AddGeneratedViews()
        .AddGeneratedStores()
        .AddCommand<AboutCommand>()
        .AddCommand<QuitCommand>()
        .WithoutHostedService();

    services.AddSingleton<IHostApplicationLifetime, NullLifetime>();

    using var provider = services.BuildServiceProvider();

    var parts = size.Split('x');
    var width = parts.Length == 2 && int.TryParse(parts[0], out var w) ? w : 120;
    var height = parts.Length == 2 && int.TryParse(parts[1], out var h) ? h : 34;

    provider.GetRequiredService<Surface>().SetFixedSize(width, height);

    var state = provider.GetRequiredService<ArlecchinoState>();
    var navigator = provider.GetRequiredService<Navigator>();

    if (view.Equals("number", StringComparison.OrdinalIgnoreCase))
    {
        state.Modal = new NumberModal
        {
            Title = "Price",
            Text = "12.50",
            Minimum = 0,
            Maximum = 500,
            Step = 5,
            Decimals = 2,
            Prefix = "$ ",
            OnSubmit = static _ => { },
        };
        navigator.Apply(ViewKind.Default);
    }
    else if (view.Equals("slider", StringComparison.OrdinalIgnoreCase))
    {
        state.Modal = new SliderModal
        {
            Title = "Volume",
            Value = 60,
            Suffix = " %",
            OnSubmit = static _ => { },
        };
        navigator.Apply(ViewKind.Default);
    }
    else if (view.Equals("toggle", StringComparison.OrdinalIgnoreCase))
    {
        state.RequestToggle("Fullscreen", true, static _ => { });
        navigator.Apply(ViewKind.Default);
    }
    else if (view.Equals("multi", StringComparison.OrdinalIgnoreCase))
    {
        state.RequestMultiChoice(
            "Columns",
            ["Name", "Date Modified", "Size", "Kind"],
            ["Name", "Size"],
            static _ => { });
        navigator.Apply(ViewKind.Default);
    }
    else if (view.Equals("password", StringComparison.OrdinalIgnoreCase))
    {
        state.RequestPassword("Passphrase", static _ => { });
        ((TextModal)state.Modal!).Text = "hunter2";
        navigator.Apply(ViewKind.Default);
    }
    else if (view.Equals("date", StringComparison.OrdinalIgnoreCase))
    {
        state.RequestDate("Release date", new(2026, 7, 25), static _ => { });
        navigator.Apply(ViewKind.Default);
    }
    else if (view.Equals("time", StringComparison.OrdinalIgnoreCase))
    {
        state.RequestTime("Start at", new(9, 41), static _ => { });
        navigator.Apply(ViewKind.Default);
    }
    else if (view.Equals("color", StringComparison.OrdinalIgnoreCase))
    {
        state.RequestColor("Accent color", new(63, 169, 245), static _ => { });
        navigator.Apply(ViewKind.Default);
    }
    else if (view.Equals("widgets", StringComparison.OrdinalIgnoreCase))
    {
        navigator.Apply(ViewKind.Widgets);
    }
    else if (view.Equals("settings", StringComparison.OrdinalIgnoreCase))
    {
        navigator.Apply(ViewKind.Settings);
    }
    else if (view.Equals("panes", StringComparison.OrdinalIgnoreCase))
    {
        navigator.Apply(ViewKind.Panes);
    }
    else if (view.Equals("picker", StringComparison.OrdinalIgnoreCase))
    {
        state.FilePicker = new(
            "Pick a folder", PickFolder: true, Environment.CurrentDirectory, ViewKind.Default, static _ => { });
        navigator.Apply(Routes.FilePicker);
    }
    else if (view.Equals("about", StringComparison.OrdinalIgnoreCase))
    {
        navigator.Apply(ViewKind.About);
    }
    else
    {
        navigator.Apply(ViewKind.Default);
    }

    provider.GetRequiredService<Screen>().DrawOnce();
    Console.WriteLine();
}

namespace Arlecchino.Sample
{
    internal sealed class NullLifetime : IHostApplicationLifetime
    {
        public CancellationToken ApplicationStarted => CancellationToken.None;
        public CancellationToken ApplicationStopping => CancellationToken.None;
        public CancellationToken ApplicationStopped => CancellationToken.None;

        public void StopApplication()
        {
        }
    }
}
