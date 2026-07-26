using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Packages.Model;
using Arlecchino.Packages.Stores;
using Arlecchino.Rendering;
using Arlecchino.Widgets;

namespace Arlecchino.Packages.Views;

public sealed class PackageView : IArlecchinoView
{
    private const int CardRows = 4;
    private const int AdvisoryRows = 6;

    private readonly Surface _surface;
    private readonly Inventory _inventory;
    private readonly UpgradePlan _plan;
    private readonly FocusRing _focus;
    private readonly ListBox<Advisory> _advisories;
    private readonly Table<PackageUse> _uses;
    private readonly StatusBar _status;

    public PackageView(Surface surface, Inventory inventory, UpgradePlan plan, ArlecchinoOptions options)
    {
        _surface = surface;
        _inventory = inventory;
        _plan = plan;

        _advisories = new(options.Keymap)
        {
            Render = static advisory => $" {advisory.Severity,-9} {advisory.Url}",
            ItemStyle = static advisory => advisory.Rank >= 3 ? Theme.Error : Theme.Warning,
        };

        _uses = new(options.Keymap)
        {
            Columns =
            [
                new() { Header = static () => "Project", Cell = static use => use.Project },
                new() { Header = static () => "Framework", Cell = static use => use.Framework, Width = 12 },
                new() { Header = static () => "Requested", Cell = static use => use.Requested, Width = 12 },
                new() { Header = static () => "Resolved", Cell = static use => use.Resolved, Width = 12 },
                new()
                {
                    Header = static () => "Kind",
                    Cell = static use => use.Transitive ? "transitive" : "direct",
                    Width = 12,
                },
            ],
            ItemStyle = static use => use.Transitive ? Theme.Muted : Theme.Default,
        };

        _status = new()
        {
            Left = [Versions],
            Right = [static () => "u upgrade", static () => "Esc back"],
        };

        _focus = new(options.Keymap);
        _focus.Add(_uses);
        _focus.Add(_advisories);
    }

    public void Draw()
    {
        var content = _surface.Content;

        if (_inventory.Selected.Value is not { } package)
        {
            content.WriteLine(0, "Nothing selected", Theme.Header);
            return;
        }

        var (card, rest) = content.SplitTop(CardRows);

        card.WriteLine(0, package.Id, Theme.Header);
        card.WriteLine(1, Summary(package), Theme.Muted);
        card.WriteLine(2, Advice(package), Advice(package).Length == 0 ? Theme.Muted : Theme.Warning);

        _advisories.Items = package.Advisories;

        var body = rest;
        if (package.Advisories.Count > 0)
        {
            var (advisories, below) = rest.SplitTop(Math.Min(AdvisoryRows, package.Advisories.Count + 2));
            _advisories.Place(advisories.Border(Theme.Info, "Advisories"));
            body = below;
        }

        var (uses, status) = body.SplitTop(body.Height - 1);

        _uses.Rows = package.Uses;
        _uses.Place(uses.Border(Theme.Info, "Used by"));
        _status.Place(status);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key);

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.Navigating(ConsoleKey.Escape, static () => "back", static () => ViewKind.Inventory),
        new()
        {
            Binding = new(ConsoleKey.U),
            Label = static () => "upgrade",
            IsEnabled = () => _inventory.Selected.Value is not null,
            Run = Upgrade,
        },
    ];

    private ViewRoute Upgrade()
    {
        if (_inventory.Selected.Value is not { } package || _inventory.Scan.Value is not { } catalog)
        {
            return ViewRoute.None;
        }

        _plan.Restart(package, catalog);
        return ViewKind.Upgrade;
    }

    private string Versions() => _inventory.Selected.Value is { } package
        ? $"{package.Uses.Count} uses · {package.ResolvedVersions().Count} versions"
        : "";

    private static string Summary(PackageRow package)
    {
        var latest = package.Latest is { Length: > 0 } known ? $"latest {known}" : "latest unknown";
        return $"resolved {package.Resolved()} · {latest}";
    }

    private static string Advice(PackageRow package)
    {
        if (package.DeprecationReasons.Count == 0)
        {
            return package.Health switch
            {
                PackageHealth.Vulnerable => "a known advisory covers the resolved version",
                PackageHealth.Drift => "the solution resolves more than one version of this package",
                PackageHealth.Outdated => $"{package.Highest()} is behind {package.Latest}",
                _ => "",
            };
        }

        var reasons = string.Join(", ", package.DeprecationReasons).ToLowerInvariant();

        return package.Alternative is { Length: > 0 } alternative
            ? $"deprecated ({reasons}) — the author points at {alternative}"
            : $"deprecated ({reasons})";
    }
}
