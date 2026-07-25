using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Arlecchino.Commands;
using Arlecchino.Diagnostics;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Views;

namespace Arlecchino.Hosting;

/// <summary>Registers Arlecchino with the host's container.</summary>
public static class ArlecchinoServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything an application needs and returns the builder that describes it. The console
    /// terminal is only registered if nothing else claimed the role, so a terminal registered
    /// beforehand is left in place.
    /// </summary>
    /// <param name="services">The container being built.</param>
    /// <param name="configure">Adjusts the settings before anything reads them.</param>
    /// <returns>The builder, for describing views, commands and the rest.</returns>
    public static ArlecchinoBuilder AddArlecchino(this IServiceCollection services, Action<ArlecchinoOptions>? configure = null)
    {
        var options = new ArlecchinoOptions();
        configure?.Invoke(options);

        services.AddLogging();

        var registrations = new ViewRegistrations();

        services.AddSingleton(provider =>
        {
            _ = provider;
            Theme.Palette = options.Theme;
            return options;
        });

        services.AddSingleton(registrations);
        services.TryAddSingleton<ITerminal, SystemTerminal>();
        services.AddSingleton(static provider =>
        {
            var configured = provider.GetRequiredService<ArlecchinoOptions>();
            return new Surface(provider.GetRequiredService<ITerminal>())
            {
                HorizontalPadding = configured.HorizontalPadding,
                VerticalPadding = configured.VerticalPadding,
            };
        });
        services.AddSingleton(static provider =>
            KeyText.For(provider.GetRequiredService<ArlecchinoOptions>().TextInput));

        services.AddSingleton<Repaint>();
        services.AddSingleton<LogBuffer>();
        services.AddSingleton<LogOverlay>();
        services.AddSingleton<ILoggerProvider, ArlecchinoLoggerProvider>();
        services.AddSingleton<UiDispatcher>();
        services.AddSingleton<StateHistory>();
        services.AddSingleton<TuiState>();
        services.AddScoped<ViewLifetime>();
        services.AddSingleton<IViewFactory, RegisteredViewFactory>();
        services.AddSingleton<ViewResolver>();
        services.AddSingleton<Navigator>();
        services.AddSingleton<CommandRegistry>();
        services.AddSingleton<CommandConflicts>();
        services.AddSingleton<Screen>();
        services.AddSingleton<InputRouter>();
        services.AddSingleton<TerminalInputReader>();
        services.AddHostedService<ArlecchinoHostedService>();

        var builder = new ArlecchinoBuilder(services, registrations, options);
        builder.AddView<FilePickerView>(FilePickerView.Route);
        return builder;
    }
}
