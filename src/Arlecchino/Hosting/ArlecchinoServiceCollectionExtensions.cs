using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Arlecchino.Commands;
using Arlecchino.Diagnostics;
using Arlecchino.Input;
using Arlecchino.Modals;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.State;
using Arlecchino.Views;
using Arlecchino.Atoms;

namespace Arlecchino.Hosting;

/// <summary>Registers Arlecchino with the host's container.</summary>
public static class ArlecchinoServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything an application needs and returns the builder that describes it. The look is
    /// installed here, before any thread has claimed the drawing, and a terminal already registered stands.
    /// </summary>
    /// <param name="services">The container being built.</param>
    /// <param name="configure">Adjusts the settings before anything reads them.</param>
    /// <returns>The builder, for describing views, commands and the rest.</returns>
    public static ArlecchinoBuilder AddArlecchino(this IServiceCollection services, Action<ArlecchinoOptions>? configure = null)
    {
        var options = new ArlecchinoOptions();
        configure?.Invoke(options);

        Theme.Palette = options.Theme;
        Glyphs.Graph = options.GraphSymbols;
        Glyphs.Picture = options.ImageProtocol;
        Glyphs.CellWidth = options.CellWidth;
        Glyphs.CellHeight = options.CellHeight;

        services.AddLogging();

        var registrations = new ViewRegistrations();

        services.AddSingleton(options);
        services.AddSingleton(registrations);
        services.TryAddSingleton<IArlecchinoTerminal, SystemTerminal>();
        services.AddSingleton(static provider =>
        {
            var options = provider.GetRequiredService<ArlecchinoOptions>();
            return new Surface(provider.GetRequiredService<IArlecchinoTerminal>())
            {
                HorizontalPadding = options.HorizontalPadding,
                VerticalPadding = options.VerticalPadding,
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
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<Ticker>();
        services.AddSingleton<Notifications>();
        services.AddSingleton<AtomHistory>();
        services.AddSingleton<ArlecchinoState>();
        services.AddSingleton<ArlecchinoReport>();
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
        services.AddSingleton(static provider => new CommandKeys(
            provider.GetRequiredService<Navigator>(),
            provider.GetRequiredService<CommandRegistry>()));
        services.AddSingleton(static provider => new ModalFrame(
            provider.GetRequiredService<Surface>(),
            provider.GetRequiredService<ArlecchinoState>(),
            provider.GetRequiredService<IArlecchinoTerminal>(),
            provider.GetRequiredService<ArlecchinoOptions>()));
        services.AddSingleton(static provider => new Screen(
            provider.GetRequiredService<ArlecchinoState>(),
            provider.GetRequiredService<Surface>(),
            provider.GetRequiredService<Navigator>(),
            provider.GetRequiredService<ArlecchinoOptions>(),
            provider.GetRequiredService<IArlecchinoTerminal>(),
            provider.GetRequiredService<Repaint>(),
            provider.GetRequiredService<Ticker>(),
            provider.GetRequiredService<LogOverlay>(),
            provider.GetRequiredService<PendingInput>(),
            provider.GetRequiredService<InputRouter>(),
            provider.GetRequiredService<CommandRegistry>(),
            provider.GetRequiredService<ModalFrame>(),
            provider.GetRequiredService<ILogger<Screen>>(),
            provider.GetRequiredService<CommandKeys>(),
            provider.GetService<IArlecchinoLayout>()));
        services.AddSingleton(static provider => new InputRouter(
            provider.GetRequiredService<ArlecchinoState>(),
            provider.GetRequiredService<Navigator>(),
            provider.GetRequiredService<IArlecchinoTerminal>(),
            provider.GetRequiredService<LogOverlay>(),
            provider.GetRequiredService<CommandRegistry>(),
            provider.GetRequiredService<ArlecchinoOptions>(),
            provider.GetRequiredService<KeyText>(),
            provider.GetRequiredService<Repaint>(),
            provider.GetRequiredService<ModalFrame>(),
            provider.GetRequiredService<ILogger<InputRouter>>(),
            provider.GetRequiredService<CommandKeys>(),
            provider.GetService<IArlecchinoLayout>(),
            provider.GetService<IHostApplicationLifetime>()));
        services.AddSingleton(static provider => new TerminalModes(
            provider.GetRequiredService<IArlecchinoTerminal>(),
            provider.GetRequiredService<ArlecchinoOptions>()));
        services.AddSingleton(static provider => new Handover(
            provider.GetRequiredService<IArlecchinoTerminal>(),
            provider.GetRequiredService<TerminalModes>(),
            provider.GetRequiredService<Screen>()));
        services.AddSingleton<PendingInput>();
        services.AddSingleton(static provider => new TerminalInputReader(
            provider.GetRequiredService<IArlecchinoTerminal>(),
            provider.GetRequiredService<InputRouter>(),
            provider.GetRequiredService<ArlecchinoOptions>(),
            provider.GetRequiredService<PendingInput>()));
        services.AddHostedService<ArlecchinoHostedService>();

        var builder = new ArlecchinoBuilder(services, registrations, options);
        builder.AddView<FilePickerView>(FilePickerView.Route);
        builder.AddView<NotificationsView>(NotificationsView.Route);
        builder.AddView<HelpView>(HelpView.Route);
        return builder;
    }
}
