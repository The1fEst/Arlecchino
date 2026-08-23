using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Arlecchino.Atoms.History.Hosting;

/// <summary>Registers the undo history with the host's container.</summary>
public static class HistoryServiceCollectionExtensions
{
    /// <summary>
    /// Adds undo and redo. Without this call an application has atoms and no history: nothing records
    /// steps, and the atoms that could feed one allocate nothing for it.
    ///
    /// <code>
    /// builder.Services.UseHistory(options => options.Keep = true);
    /// </code>
    /// </summary>
    /// <param name="services">The container being built.</param>
    /// <param name="configure">Adjusts the settings before anything reads them.</param>
    /// <returns>The container, for chaining.</returns>
    /// <exception cref="InvalidOperationException">Called twice; there is one history per application.</exception>
    public static IServiceCollection UseHistory(this IServiceCollection services, Action<HistoryOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (services.Any(static registered => registered.ServiceType == typeof(HistoryOptions)))
        {
            throw new InvalidOperationException(
                "UseHistory has already been called. Atoms announce their steps to the whole process, so a " +
                "second history would collect the same steps into a second stack.");
        }

        var options = new HistoryOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton(static provider =>
        {
            var settings = provider.GetRequiredService<HistoryOptions>();

            return new HistoryFile(settings.FileName, settings.Keep);
        });
        services.AddSingleton(static provider =>
        {
            var settings = provider.GetRequiredService<HistoryOptions>();

            return new AtomHistory(provider.GetRequiredService<HistoryFile>()) { Capacity = settings.Capacity };
        });

        services.Insert(0, ServiceDescriptor.Singleton<IHostedService, HistoryStartup>());

        return services;
    }
}
