using Arlecchino.Hosting;
using Arlecchino.Sample.Frames;
using Arlecchino.Sample.Probes;
using Arlecchino.Sample.Views;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

if (args is ["--frame", ..])
{
    Frame.Draw(args.Length >= 2 ? args[1] : "default", args.Length >= 3 ? args[2] : "120x34");
    return;
}

if (args is ["--keys"])
{
    KeyReport.Run();
    return;
}

if (args is ["--ask", ..])
{
    TerminalProbe.Ask(args.Length >= 2 && int.TryParse(args[1], out var wait) ? wait : 2000);
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
    .UseMouse()
    .StartAt(ViewKind.Default);

await builder.Build().RunAsync();
