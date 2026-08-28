using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arlecchino.Atoms.History.Hosting;

/// <summary>Registers the undo history with the host's container.</summary>
public static class HistoryServiceCollectionExtensions
{
    /// <summary>
    /// Adds undo and redo; without this call nothing records a step. Naming a file makes the history
    /// outlive the run, and without one it is kept in memory.
    ///
    /// <code>
    /// builder.Services.UseHistory(options => options.FileName = "session.undo");
    /// </code>
    /// </summary>
    /// <param name="services">The container being built.</param>
    /// <param name="configure">Adjusts the settings before anything reads them.</param>
    /// <returns>The container, for chaining.</returns>
    /// <exception cref="InvalidOperationException">Called twice; there is one history per application.</exception>
    public static IServiceCollection UseHistory(this IServiceCollection services, Action<HistoryOptions>? configure = null)
    {
        if (services.Any(static registered => registered.ServiceType == typeof(HistoryOptions)))
        {
            throw new InvalidOperationException(
                "UseHistory has already been called. Atoms announce their steps to the whole process, so a " +
                "second history would collect the same steps into a second stack.");
        }

        var options = new HistoryOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        if (!string.IsNullOrEmpty(options.FileName))
        {
            services.AddSingleton(static provider =>
                new HistoryFile(provider.GetRequiredService<HistoryOptions>().FileName));
        }

        services.AddSingleton(static provider =>
        {
            var settings = provider.GetRequiredService<HistoryOptions>();

            return new AtomHistory(provider.GetService<HistoryFile>())
            {
                Capacity = settings.Capacity,
            };
        });

        services.Insert(0, ServiceDescriptor.Singleton<IHostedService, HistoryStartup>());

        return services;
    }
}
