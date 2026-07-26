using Arlecchino.Hosting;
using Arlecchino.Packages.Frames;
using Arlecchino.Packages.Stores;
using Arlecchino.Packages.Views;
using Arlecchino.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var solution = Option(args, "--solution") ?? Inventory.FindSolution();

if (args is ["--frame", ..])
{
    HeadlessFrame.Render(
        args.Length >= 2 ? args[1] : "inventory",
        args.Length >= 3 ? args[2] : "120x34",
        Option(args, "--keys") ?? "",
        solution);

    return;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddArlecchino(options =>
    {
        options.MinimumWidth = 84;
        options.MinimumHeight = 20;
        options.ShowOutputLine = false;
        options.ShowHints = false;
    })
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddGeneratedCommands()
    .UseMouse()
    .UseTheme(ThemePalette.Arlecchino)
    .StartAt(ViewKind.Inventory);

var host = builder.Build();

host.Services.GetRequiredService<Inventory>().Solution.Value = solution;

await host.RunAsync();

static string? Option(string[] arguments, string name)
{
    for (var i = 0; i < arguments.Length - 1; i++)
    {
        if (arguments[i] == name)
        {
            return arguments[i + 1];
        }
    }

    return null;
}
