using System.IO;
using Arlecchino.Commander;
using Arlecchino.Commander.Frames;
using Arlecchino.Commander.Stores;
using Arlecchino.Commander.Views;
using Arlecchino.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var left = args.Length >= 1 && Directory.Exists(args[0]) ? args[0] : Directory.GetCurrentDirectory();
var right = args.Length >= 2 && Directory.Exists(args[1]) ? args[1] : left;

if (args is ["--frame", ..])
{
    HeadlessFrame.Render(
        args.Length >= 2 ? args[1] : "120x34",
        Option(args, "--keys") ?? "",
        Option(args, "--left") ?? Directory.GetCurrentDirectory(),
        Option(args, "--right") ?? Directory.GetCurrentDirectory(),
        Option(args, "--connect") ?? "",
        int.TryParse(Option(args, "--wait"), out var wait) ? wait : 0);

    return;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddArlecchino(CommanderOptions.Apply)
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddGeneratedCommands()
    .UseMouse()
    .StartAt(ViewKind.Commander);

var host = builder.Build();

host.Services.GetRequiredService<Panels>().Start(left, right);

await host.RunAsync();

static string? Option(string[] arguments, string name)
{
    for (var index = 0; index < arguments.Length - 1; index++)
    {
        if (arguments[index] == name)
        {
            return arguments[index + 1];
        }
    }

    return null;
}
