using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Hosting;

/// <summary>Builds the host a terminal application runs on.</summary>
public static class ArlecchinoHost
{
    /// <summary>
    /// Builds the host, with settings from the environment and the command line and a log through the
    /// console. It watches no folder for changes, which costs seconds in a large one.
    /// </summary>
    /// <param name="args">What the application was started with, or <c>null</c> when it takes none.</param>
    /// <returns>The builder, for registering the application on with <c>AddArlecchino</c>.</returns>
    public static HostApplicationBuilder CreateBuilder(string[]? args = null)
    {
        var builder = new HostApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = AppContext.BaseDirectory,
            DisableDefaults = true,
        });

        builder.Configuration.AddEnvironmentVariables();

        if (args is { Length: > 0 })
        {
            builder.Configuration.AddCommandLine(args);
        }

        builder.Logging.AddConsole();

        return builder;
    }
}
