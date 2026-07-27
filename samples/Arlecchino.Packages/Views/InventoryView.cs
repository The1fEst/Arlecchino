using System;
using System.Collections.Generic;
using System.IO;
using Arlecchino.Commands;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Packages.Model;
using Arlecchino.Packages.Stores;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Widgets;

namespace Arlecchino.Packages.Views;

public sealed class InventoryView : IArlecchinoView
{
    private const int HeaderRows = 2;

    private readonly Surface _surface;
    private readonly Inventory _inventory;
    private readonly UpgradePlan _plan;
    private readonly ArlecchinoState _state;
    private readonly FocusRing _focus;
    private readonly Tabs _tabs;
    private readonly Table<PackageRow> _table;
    private readonly StatusBar _status;
    private readonly ProgressBar _progress;
    private readonly Spinner _spinner = new();

    public InventoryView(
        Surface surface,
        Inventory inventory,
        UpgradePlan plan,
        ArlecchinoState state,
        ArlecchinoOptions options,
        ViewLifetime lifetime)
    {
        _surface = surface;
        _inventory = inventory;
        _plan = plan;
        _state = state;

        _table = new(options.Keymap)
        {
            Columns =
            [
                new()
                {
                    Header = static () => "Package",
                    Cell = static row => row.IsTransitive ? $"{row.Id} (transitive)" : row.Id,
                    Sort = static (first, second) => string.CompareOrdinal(first.Id, second.Id),
                },
                new()
                {
                    Header = static () => "Resolved",
                    Cell = static row => row.Resolved(),
                    Width = 24,
                    Sort = static (first, second) => VersionOrder.Compare(first.Highest(), second.Highest()),
                },
                new()
                {
                    Header = static () => "Latest",
                    Cell = static row => row.Latest ?? "—",
                    Width = 10,
                },
                new()
                {
                    Header = static () => "Used by",
                    Cell = static row => row.Uses.Count.ToString(),
                    Width = 8,
                    AlignRight = true,
                    Sort = static (first, second) => first.Uses.Count.CompareTo(second.Uses.Count),
                },
                new()
                {
                    Header = static () => "State",
                    Cell = Describe,
                    Width = 26,
                    Sort = static (first, second) => second.Health.CompareTo(first.Health),
                },
            ],
            ItemStyle = Paint,
            OnActivate = Open,
        };

        _tabs = new(options.Keymap)
        {
            Titles =
            [
                () => $"All {Total()}",
                () => $"Outdated {Count(PackageHealth.Outdated)}",
                () => $"Vulnerable {Count(PackageHealth.Vulnerable)}",
                () => $"Deprecated {Count(PackageHealth.Deprecated)}",
                () => $"Drift {Count(PackageHealth.Drift)}",
            ],
            OnSelected = index =>
            {
                _inventory.Tab.Value = index;
                _table.Selected = 0;
            },
        };

        _progress = new()
        {
            Maximum = 4,
            Caption = _ => Caption(),
            Style = Theme.Accent,
        };

        _status = new()
        {
            Left = [Shown, Scanned],
            Right = [Transitive, static () => "Enter details", static () => ": commands"],
        };

        _focus = new(options.Keymap);
        _focus.Add(_tabs);
        _focus.Add(_table);
        _focus.Focus(_table);

        _tabs.Select(_inventory.Tab.Value);

        lifetime.Track(_inventory.Scan.SubscribeToStatus(() => _state.Invalidate()));
        lifetime.Track(_inventory.Step.Subscribe(() => _state.Invalidate()));

        if (_inventory.Scan.Value is { Packages.Count: 0 } && !_inventory.Scan.IsLoading)
        {
            _inventory.Rescan();
        }
    }

    public void Draw()
    {
        var content = _surface.Content;
        var (header, rest) = content.SplitTop(HeaderRows);

        header.WriteLine(0, Title(), Theme.Header);
        header.WriteLine(1, Headline(), Failed() ? Theme.Error : Theme.Muted);

        if (_inventory.Scan.IsLoading)
        {
            _spinner.Advance();
            _spinner.Draw(header.SplitLeft(header.Width - 1).Right);
        }

        var body = _tabs.Draw(rest);
        var (room, status) = body.SplitTop(body.Height - 1);

        _table.Rows = _inventory.Visible();

        if (_inventory.Scan.IsLoading)
        {
            var (rows, bar) = room.SplitTop(room.Height - 2);

            _table.Draw(rows);
            _progress.Value = _inventory.Step.Value.Done;
            _progress.Draw(bar.Rows(1, 1));
        }
        else
        {
            _table.Draw(room);
        }

        _status.Draw(status);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key);

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.Navigating(ConsoleKey.Enter, static () => "details", () => Open(_table.SelectedRow)),
        ViewCommand.For(ConsoleKey.R, static () => "rescan", _inventory.Rescan),
        ViewCommand.For(ConsoleKey.F, static () => "filter", Filter),
        ViewCommand.For(ConsoleKey.T, static () => "transitive", Transitives),
        ViewCommand.Navigating(ConsoleKey.P, static () => "projects", static () => ViewKind.Projects),
        new()
        {
            Binding = new(ConsoleKey.U),
            Label = static () => "upgrade",
            IsEnabled = () => _table.SelectedRow is not null,
            Run = Upgrade,
        },
    ];

    private ViewRoute Upgrade()
    {
        if (_table.SelectedRow is not { } row || _inventory.Scan.Value is not { } catalog)
        {
            return ViewRoute.None;
        }

        _inventory.Selected.Value = row;
        _plan.Restart(row, catalog);

        return ViewKind.Upgrade;
    }

    private ViewRoute Open(PackageRow? row)
    {
        if (row is null)
        {
            return ViewRoute.None;
        }

        _inventory.Selected.Value = row;
        return ViewKind.Package;
    }

    private void Filter() =>
        _state.RequestText("Filter packages", _inventory.Filter.Value, null, typed =>
        {
            _inventory.Filter.Value = typed;
            _table.Selected = 0;
        });

    private void Transitives()
    {
        _inventory.ShowTransitive.Value = !_inventory.ShowTransitive.Value;
        _table.Selected = 0;
    }

    private string Title()
    {
        var solution = _inventory.Solution.Value;
        return solution.Length == 0 ? "No solution" : Path.GetFileName(solution);
    }

    private string Headline()
    {
        if (_inventory.Scan.Error.Value is { } failure)
        {
            return failure.Message;
        }

        if (_inventory.Scan.Value is not { Packages.Count: > 0 } catalog)
        {
            return _inventory.Solution.Value;
        }

        var notes = catalog.Notes.Count == 0 ? "" : $" · {catalog.Notes.Count} report(s) unavailable";
        return $"{catalog.Projects.Count} projects · {catalog.Packages.Count} packages{notes}";
    }

    private bool Failed() => _inventory.Scan.Error.Value is not null;

    private string Caption()
    {
        var step = _inventory.Step.Value;
        return $" {step.Done}/{step.Total} {step.Title}";
    }

    private string Shown() => $"{_table.Rows.Count} shown";

    private string Scanned() => _inventory.Scan.Value is { ScannedAt.Year: > 1 } catalog
        ? $"scanned at {catalog.ScannedAt:HH:mm:ss}"
        : "not scanned yet";

    private string Transitive() => _inventory.ShowTransitive.Value ? "t direct only" : "t transitive";

    private int Total() => _inventory.Listed();

    private int Count(PackageHealth health) => _inventory.Listed(health);

    private static string Describe(PackageRow row) => row.Health switch
    {
        PackageHealth.Vulnerable => $"{row.WorstAdvisory()?.Severity.ToLowerInvariant()} advisory",
        PackageHealth.Deprecated => "deprecated · " + string.Join(", ", row.DeprecationReasons).ToLowerInvariant(),
        PackageHealth.Drift => $"{row.ResolvedVersions().Count} versions in use",
        PackageHealth.Outdated => $"{row.Highest()} → {row.Latest}",
        _ => "up to date",
    };

    private static IArlecchinoColor Paint(PackageRow row) => row.Health switch
    {
        PackageHealth.Vulnerable => Theme.Error,
        PackageHealth.Deprecated => Theme.Warning,
        PackageHealth.Drift => Theme.Accent,
        PackageHealth.Outdated => Theme.Default,
        _ => Theme.Muted,
    };
}
