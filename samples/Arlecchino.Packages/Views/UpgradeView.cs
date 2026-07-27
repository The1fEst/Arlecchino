using System;
using System.Collections.Generic;
using Arlecchino.Commands;
using Arlecchino.Focus;
using Arlecchino.Forms;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Layout;
using Arlecchino.Navigation;
using Arlecchino.Packages.Scanning;
using Arlecchino.Packages.Stores;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Widgets;
using static Arlecchino.Layout.PaneSplit;
using static Arlecchino.Layout.PaneTree;

namespace Arlecchino.Packages.Views;

public sealed class UpgradeView : IArlecchinoView
{
    private const int HeaderRows = 3;
    private const int FormRows = 7;

    private readonly Surface _surface;
    private readonly Inventory _inventory;
    private readonly UpgradePlan _plan;
    private readonly ArlecchinoState _state;
    private readonly ViewLifetime _lifetime;
    private readonly FocusRing _focus;
    private readonly Spinner _spinner = new();
    private readonly PaneTree _layout;

    public UpgradeView(
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
        _lifetime = lifetime;

        var form = new Form(state, options)
        {
            Fields =
            [
                Field.Choice(static () => "Version", Versions(), plan.Target,
                    static () => "what every selected project will ask for"),
                Field.MultiChoice(static () => "Projects", Names(), plan.Projects,
                    static picked => picked.Count == 0 ? "none" : string.Join(", ", picked),
                    static () => "Space marks a project, Enter confirms"),
                Field.Toggle(static () => "Dry run", plan.DryRun, static value => value ? "yes" : "no",
                    static () => "yes prints the commands, no runs dotnet add package"),
                Field.Action(static () => "Apply", Apply, () => !plan.Running.Value && plan.Projects.Value.Count > 0),
            ],
        };

        var log = new ScrollPane(options.Keymap)
        {
            ContentHeight = () => Lines().Count,
            Content = region =>
            {
                var lines = Lines();
                for (var row = 0; row < lines.Count; row++)
                {
                    region.WriteLine(row, lines[row], Style(lines[row]));
                }
            },
        };

        var status = new StatusBar
        {
            Left = [Progress],
            Right = [static () => "Tab panes", static () => "Esc back"],
        };

        _layout = Branch(
            Rows,
            HeaderRows,
            Leaf(DrawHeader),
            Branch(
                Rows,
                FormRows,
                Leaf(form),
                Branch(
                    Rows,
                    PaneSize.CellsFromEnd(1),
                    Leaf(log, () => _plan.Log.Count == 0 ? "Planned commands" : "Output"),
                    Leaf(status))));

        _focus = _layout.AsFocusRing(options.Keymap);
    }

    public void Draw()
    {
        if (_inventory.Selected.Value is null)
        {
            _surface.Content.WriteLine(0, "Nothing selected", Theme.Header);
            return;
        }

        _layout.Draw(_surface.Content);
    }

    private void DrawHeader(SurfaceRegion header)
    {
        if (_inventory.Selected.Value is not { } package)
        {
            return;
        }

        header.WriteLine(0, $"Upgrade {package.Id}", Theme.Header);
        header.WriteLine(1, $"{package.Resolved()} → {_plan.Target.Value}", Theme.Muted);

        if (!_plan.Running.Value)
        {
            return;
        }

        _spinner.Advance();
        _spinner.Draw(header.SplitLeft(header.Width - 1).Right);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key);

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.Navigating(ConsoleKey.Escape, static () => "back", static () => ViewKind.Inventory),
    ];

    private ViewRoute Apply()
    {
        if (_inventory.Selected.Value is not { } package || _inventory.Scan.Value is not { } catalog)
        {
            return ViewRoute.None;
        }

        if (_plan.DryRun.Value)
        {
            _plan.Run(package, catalog, _lifetime.Closing);
            return ViewRoute.None;
        }

        _state.RequestConfirmation(
            $"Rewrite {_plan.Projects.Value.Count} project file(s)?",
            () => _plan.Run(package, catalog, _lifetime.Closing));

        return ViewRoute.None;
    }

    private IReadOnlyList<string> Lines()
    {
        if (_plan.Log.Count > 0)
        {
            return _plan.Log;
        }

        if (_inventory.Selected.Value is not { } package || _inventory.Scan.Value is not { } catalog)
        {
            return [];
        }

        var lines = new List<string>();
        foreach (var step in _plan.Steps(package, catalog))
        {
            lines.Add(Dotnet.Describe(step));
        }

        return lines;
    }

    private string Progress() => _plan.Running.Value
        ? $"{_spinner.Current} running"
        : _plan.Log.Count == 0 ? "nothing run yet" : $"{_plan.Log.Count} lines";

    private IReadOnlyList<string> Versions()
    {
        var versions = new List<string>();

        if (_inventory.Selected.Value is not { } package)
        {
            return versions;
        }

        if (package.Latest is { Length: > 0 } latest)
        {
            versions.Add(latest);
        }

        foreach (var resolved in package.ResolvedVersions())
        {
            if (!versions.Contains(resolved))
            {
                versions.Add(resolved);
            }
        }

        return versions;
    }

    private IReadOnlyList<string> Names()
    {
        var names = new List<string>();

        foreach (var project in _inventory.Scan.Value?.Projects ?? [])
        {
            names.Add(project.Name);
        }

        return names;
    }

    private static IArlecchinoColor Style(string line) =>
        line.StartsWith("dotnet", StringComparison.Ordinal) ? Theme.Accent : Theme.Muted;
}
