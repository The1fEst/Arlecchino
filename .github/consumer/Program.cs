using System;
using System.Collections.Generic;
using Arlecchino.Atoms;
using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;
using Arlecchino.Commands;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
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

    public SurfaceRegion Draw(SurfaceRegion region)
    {
        region.WriteLine(0, _settings.Profile.Value, Theme.Default);

        return region.Rows(1, region.Height - 1);
    }
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

    public ViewRoute Handle(KeyPress key) => ViewRoute.None;

    public IReadOnlyList<ViewCommand> Commands() =>
        [ViewCommand.For(ConsoleKey.R, static () => "reload", static () => { })];
}

public sealed class HistoryStore : IArlecchinoStore
{
    public Atom<int> Visits { get; } = new LocalAtom<int>(0);
}

public sealed class CountWidget : IArlecchinoWidget
{
    private readonly HistoryStore _history;

    public CountWidget(HistoryStore history) => _history = history;

    public SurfaceRegion Draw(SurfaceRegion region)
    {
        region.WriteLine(0, _history.Visits.Value.ToString(), Theme.Secondary);

        return region.Rows(1, region.Height - 1);
    }
}

public sealed class AboutCommand : IArlecchinoCommand
{
    public KeyBinding Binding => new(ConsoleKey.A);

    public string Icon => "?";

    public string Label => "About";

    public ViewRoute Execute() => ViewRoute.None;
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

        var navigator = provider.GetRequiredService<Navigator>();
        navigator.Apply(ViewKind.Default);

        var profile = provider.GetRequiredService<SettingsStore>().Profile.Value;
        var visits = provider.GetRequiredService<HistoryStore>().Visits.Value;
        var badge = provider.GetRequiredService<CountWidget>();
        var commands = provider.GetRequiredService<CommandRegistry>().Commands.Count;
        var ticker = provider.GetRequiredService<Ticker>();

        using var tick = ticker.Every(TimeSpan.FromSeconds(1), static () => { });

        Console.WriteLine($"resolved {profile}, route {ViewKind.Default.Name}, visits {visits}, " +
                          $"widget {badge is not null}, commands {commands}, " +
                          $"view commands {navigator.CurrentCommands.Count}, ticker {tick is not null}");
    }
}
