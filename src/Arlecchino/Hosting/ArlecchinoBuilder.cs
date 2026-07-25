using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Arlecchino.Commands;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;

namespace Arlecchino.Hosting;

/// <summary>
/// Configures an application while its services are being registered. Every method returns the
/// builder, so a whole application is described in one chain at startup.
/// </summary>
public sealed class ArlecchinoBuilder
{
    private readonly ViewRegistrations _registrations;
    private readonly ArlecchinoOptions _options;

    internal ArlecchinoBuilder(IServiceCollection services, ViewRegistrations registrations, ArlecchinoOptions options)
    {
        Services = services;
        _registrations = registrations;
        _options = options;
    }

    /// <summary>The service collection being built, for registering whatever the views depend on.</summary>
    public IServiceCollection Services { get; }

    /// <summary>The settings gathered so far, for anything the builder has no method for.</summary>
    public ArlecchinoOptions Options => _options;

    /// <summary>
    /// Registers a view at a route, built from the container so it can take whatever it needs in its
    /// constructor. Views are created on demand rather than at startup.
    /// </summary>
    /// <typeparam name="T">The view type.</typeparam>
    /// <param name="route">The route it answers to.</param>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder AddView<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(string route)
        where T : class, IView
    {
        _registrations.Add(route, static provider => ActivatorUtilities.CreateInstance<T>(provider));
        return this;
    }

    /// <summary>
    /// Registers a view built by hand, for the cases the container cannot cover on its own, such as a
    /// view that needs a value known only at startup.
    /// </summary>
    /// <param name="route">The route it answers to.</param>
    /// <param name="factory">Builds the view.</param>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder AddView(string route, Func<IServiceProvider, IView> factory)
    {
        _registrations.Add(route, factory);
        return this;
    }

    /// <summary>
    /// Registers a source of views that decides at run time which routes it serves. This is what the
    /// generated factory is registered through, and how a plugin adds views the host never listed.
    /// </summary>
    /// <typeparam name="TFactory">The factory type.</typeparam>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder AddViewFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TFactory>()
        where TFactory : class, IViewFactory
    {
        Services.AddSingleton<IViewFactory, TFactory>();
        return this;
    }

    /// <summary>
    /// Registers a command available everywhere. Its key has to carry a modifier, since plain letters
    /// belong to whatever is being typed.
    /// </summary>
    /// <typeparam name="TCommand">The command type.</typeparam>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>()
        where TCommand : class, IArlecchinoCommand
    {
        Services.AddSingleton<IArlecchinoCommand, TCommand>();
        return this;
    }

    /// <summary>
    /// Registers work to run once the container is ready but before the first frame, for loading what
    /// the opening view expects to find.
    /// </summary>
    /// <typeparam name="TStartup">The startup type.</typeparam>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder AddStartup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TStartup>()
        where TStartup : class, IArlecchinoStartup
    {
        Services.AddSingleton<IArlecchinoStartup, TStartup>();
        return this;
    }

    /// <summary>Sets the view the application opens on.</summary>
    /// <param name="route">The opening route.</param>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder StartAt(ViewRoute route)
    {
        _options.StartRoute = route;
        return this;
    }

    /// <summary>Sets the view the application opens on, by name.</summary>
    /// <param name="route">The opening route.</param>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder StartAt(string route) => StartAt(new ViewRoute(route));

    /// <summary>
    /// Chooses how typed characters are read. This is a trade-off rather than a preference: reading the
    /// terminal's own characters accepts any language but can misread keys on some terminals.
    /// </summary>
    /// <param name="mode">The mode to use.</param>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder UseTextInput(TextInputMode mode)
    {
        _options.TextInput = mode;
        return this;
    }

    /// <summary>Accepts only Latin letters and digits, in exchange for keys that always read correctly.</summary>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder UseLatinOnlyInput() => UseTextInput(TextInputMode.LatinOnly);

    /// <summary>Accepts whatever the terminal reports, so any language can be typed.</summary>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder UseNativeInput() => UseTextInput(TextInputMode.Native);

    /// <summary>Replaces the key bindings, which every widget then follows.</summary>
    /// <param name="keymap">The bindings to use.</param>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder UseKeymap(ArlecchinoKeymap keymap)
    {
        _options.Keymap = keymap;
        return this;
    }

    /// <summary>
    /// Turns the mouse on. It stays off by default because the terminal then stops handling selection
    /// itself, and copying text with the mouse no longer works the way the user expects. Windows reads
    /// the console's event queue for this, which also means quick-edit selection is off while it runs.
    /// </summary>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder UseMouse()
    {
        _options.MouseInput = true;
        return this;
    }

    /// <summary>Replaces the colours. What actually reaches the screen still depends on what the terminal supports.</summary>
    /// <param name="palette">The colours to use.</param>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder UseTheme(ThemePalette palette)
    {
        _options.Theme = palette;
        return this;
    }

    /// <summary>
    /// Replaces the wording the framework itself shows. This is the only way it is localised: nothing
    /// is looked up from resources.
    /// </summary>
    /// <param name="strings">The wording to use.</param>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder UseStrings(ArlecchinoStrings strings)
    {
        _options.Strings = strings;
        return this;
    }

    /// <summary>
    /// Draws to something other than the console, replacing whatever terminal was registered. This is
    /// how tests capture frames instead of writing them.
    /// </summary>
    /// <typeparam name="TTerminal">The terminal type.</typeparam>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder UseTerminal<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTerminal>()
        where TTerminal : class, ITerminal
    {
        Services.RemoveAll<ITerminal>();
        Services.AddSingleton<ITerminal, TTerminal>();
        return this;
    }

    /// <summary>
    /// Stops the application from taking over the terminal when the host starts. Everything stays
    /// registered, so a test can drive the loop itself frame by frame.
    /// </summary>
    /// <returns>The builder.</returns>
    public ArlecchinoBuilder WithoutHostedService()
    {
        Services.RemoveAll<ArlecchinoHostedService>();
        for (var i = Services.Count - 1; i >= 0; i--)
        {
            if (Services[i].ImplementationType == typeof(ArlecchinoHostedService))
            {
                Services.RemoveAt(i);
            }
        }

        return this;
    }
}
