using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Arlecchino.Commands;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Widgets;
using Consumer.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Consumer;

public sealed class SettingsStore : IArlecchinoStore
{
    public Atom<string> Profile { get; } = new TrackedAtom<string>("consumer");
}

public sealed class BadgeWidget : IArlecchinoWidget
{
    private readonly SettingsStore _settings;

    public BadgeWidget(SettingsStore settings) => _settings = settings;

    public void Draw(SurfaceRegion region) => region.WriteLine(0, _settings.Profile.Value, Theme.Default);
}

public sealed class DefaultView : IArlecchinoView
{
    private readonly Surface _surface;
    private readonly BadgeWidget _badge;

    public DefaultView(Surface surface, BadgeWidget badge)
    {
        _surface = surface;
        _badge = badge;
    }

    public void Draw() => _badge.Draw(_surface.Content);

    public ViewRoute Handle(ConsoleKeyInfo key) => ViewRoute.None;

    public IReadOnlyList<ViewCommand> Commands() => [];
}

internal static class Program
{
    private static void Main()
    {
        var services = new ServiceCollection();

        services
            .AddArlecchino()
            .AddGeneratedViews()
            .AddGeneratedStores()
            .AddGeneratedWidgets()
            .AddGeneratedCommands()
            .WithoutHostedService()
            .StartAt(ViewKind.Default);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<Surface>().SetFixedSize(40, 6);
        provider.GetRequiredService<Navigator>().Apply(ViewKind.Default);

        var profile = provider.GetRequiredService<SettingsStore>().Profile.Value;
        var ticker = provider.GetRequiredService<Ticker>();

        using var scheduled = ticker.Every(TimeSpan.FromSeconds(1), static () => { });

        Console.WriteLine($"resolved {profile}, route {ViewKind.Default.Name}, ticker {scheduled is not null}");
    }
}
