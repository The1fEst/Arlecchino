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
using Arlecchino.Atoms;

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
        services.TryAddSingleton<IArlecchinoTerminal, SystemTerminal>();
        services.AddSingleton(static provider =>
        {
            var configured = provider.GetRequiredService<ArlecchinoOptions>();
            return new Surface(provider.GetRequiredService<IArlecchinoTerminal>())
            {
                HorizontalPadding = configured.HorizontalPadding,
                VerticalPadding = configured.VerticalPadding,
            };
        });
        services.AddSingleton(static provider =>
            KeyText.For(provider.GetRequiredService<ArlecchinoOptions>().TextInput));
        services.AddSingleton(static provider =>
            provider.GetRequiredService<ArlecchinoOptions>().Keymap);
        services.AddSingleton(static provider =>
            provider.GetRequiredService<ArlecchinoOptions>().Strings);

        services.AddSingleton<Repaint>();
        services.AddSingleton<LogBuffer>();
        services.AddSingleton<LogOverlay>();
        services.AddSingleton<ILoggerProvider, ArlecchinoLoggerProvider>();
        services.AddSingleton<UiDispatcher>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<Ticker>();
        services.AddSingleton<Notifications>();
        services.AddSingleton<AtomHistory>();
        services.AddSingleton<ArlecchinoState>();
        services.AddScoped<ViewLifetime>();
        services.AddSingleton<IArlecchinoViewFactory, RegisteredViewFactory>();
        services.AddSingleton<ViewResolver>();
        services.AddSingleton(static provider => new Navigator(
            provider.GetRequiredService<ViewResolver>(),
            provider.GetRequiredService<ArlecchinoOptions>(),
            provider.GetRequiredService<Repaint>(),
            provider.GetRequiredService<CommandConflicts>()));
        services.AddSingleton<CommandRegistry>();
        services.AddSingleton<CommandConflicts>();
        services.AddSingleton(static provider => new Screen(
            provider.GetRequiredService<ArlecchinoState>(),
            provider.GetRequiredService<Surface>(),
            provider.GetRequiredService<Navigator>(),
            provider.GetRequiredService<ArlecchinoOptions>(),
            provider.GetRequiredService<IArlecchinoTerminal>(),
            provider.GetRequiredService<Repaint>(),
            provider.GetRequiredService<UiDispatcher>(),
            provider.GetRequiredService<Ticker>(),
            provider.GetRequiredService<LogOverlay>(),
            provider.GetRequiredService<ILogger<Screen>>()));
        services.AddSingleton(static provider => new InputRouter(
            provider.GetRequiredService<ArlecchinoState>(),
            provider.GetRequiredService<Navigator>(),
            provider.GetRequiredService<IArlecchinoTerminal>(),
            provider.GetRequiredService<LogOverlay>(),
            provider.GetRequiredService<CommandRegistry>(),
            provider.GetRequiredService<ArlecchinoOptions>(),
            provider.GetRequiredService<KeyText>(),
            provider.GetRequiredService<Repaint>(),
            provider.GetRequiredService<ILogger<InputRouter>>()));
        services.AddSingleton<TerminalInputReader>();
        services.AddHostedService<ArlecchinoHostedService>();

        var builder = new ArlecchinoBuilder(services, registrations, options);
        builder.AddView<FilePickerView>(FilePickerView.Route);
        builder.AddView<NotificationsView>(NotificationsView.Route);
        return builder;
    }
}
