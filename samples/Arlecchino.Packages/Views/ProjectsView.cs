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

public sealed record Branch(string Label, PackageRow? Package);

public sealed class ProjectsView : IArlecchinoView
{
    private const int SidebarWidth = 52;
    private const int HeaderRows = 2;

    private readonly Surface _surface;
    private readonly Inventory _inventory;
    private readonly FocusRing _focus;
    private readonly Tree<Branch> _tree;
    private readonly Table<ProjectSummary> _projects;
    private readonly StatusBar _status;

    public ProjectsView(Surface surface, Inventory inventory, ArlecchinoOptions options)
    {
        _surface = surface;
        _inventory = inventory;

        _tree = new(options.Keymap)
        {
            Render = static branch => branch.Label,
            ItemStyle = Paint,
            OnActivate = Open,
            Roots = Build(inventory.Scan.Value ?? Catalog.Empty),
        };

        _projects = new(options.Keymap)
        {
            Columns =
            [
                new() { Header = static () => "Project", Cell = static project => project.Name },
                new()
                {
                    Header = static () => "Direct",
                    Cell = static project => Direct(project).ToString(),
                    Width = 8,
                    AlignRight = true,
                    Sort = static (first, second) => Direct(first).CompareTo(Direct(second)),
                },
                new()
                {
                    Header = static () => "All",
                    Cell = static project => project.Uses.Count.ToString(),
                    Width = 6,
                    AlignRight = true,
                },
                new()
                {
                    Header = static () => "Frameworks",
                    Cell = static project => string.Join(", ", project.Frameworks),
                    Width = 12,
                },
            ],
            Rows = inventory.Scan.Value?.Projects ?? [],
        };

        _status = new()
        {
            Left = [Counted],
            Right = [static () => "Tab panes", static () => "Esc back"],
        };

        _focus = new(options.Keymap);
        _focus.Add(_tree);
        _focus.Add(_projects);
    }

    public void Draw()
    {
        var content = _surface.Content;
        var (header, rest) = content.SplitTop(HeaderRows);

        header.WriteLine(0, "Projects", Theme.Header);
        header.WriteLine(1, "→ opens a project, Enter on a package opens it", Theme.Muted);

        var (panes, status) = rest.SplitTop(rest.Height - 1);
        var (tree, side) = panes.SplitLeft(panes.Width - SidebarWidth);

        _tree.Draw(tree.Border(Theme.Info, "Dependency tree"));
        _projects.Draw(side.Border(Theme.Info, "Per project"));
        _status.Draw(status);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key);

    public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);

    public IReadOnlyList<ViewCommand> Commands() =>
    [
        ViewCommand.Navigating(ConsoleKey.Escape, static () => "back", static () => ViewKind.Inventory),
        ViewCommand.For(ConsoleKey.E, static () => "expand all", () => _tree.ExpandAll()),
        ViewCommand.For(ConsoleKey.C, static () => "collapse all", () => _tree.CollapseAll()),
    ];

    private ViewRoute Open(TreeNode<Branch> node)
    {
        if (node.Value.Package is not { } package)
        {
            return ViewRoute.None;
        }

        _inventory.Selected.Value = package;
        return ViewKind.Package;
    }

    private static IReadOnlyList<TreeNode<Branch>> Build(Catalog catalog)
    {
        var roots = new List<TreeNode<Branch>>();

        foreach (var project in catalog.Projects)
        {
            var direct = new List<TreeNode<Branch>>();
            var transitive = new List<TreeNode<Branch>>();

            foreach (var use in project.Uses)
            {
                var package = catalog.Find(use.Id);
                var label = $"{use.Id} {use.Resolved}";
                var node = new TreeNode<Branch> { Value = new(label, package) };

                (use.Transitive ? transitive : direct).Add(node);
            }

            var children = new List<TreeNode<Branch>>(direct);

            if (transitive.Count > 0)
            {
                children.Add(new()
                {
                    Value = new($"transitive ({transitive.Count})", null),
                    Children = transitive,
                });
            }

            roots.Add(new()
            {
                Value = new($"{project.Name} · {string.Join(", ", project.Frameworks)}", null),
                Children = children,
                IsExpanded = true,
            });
        }

        return roots;
    }

    private string Counted() => _inventory.Scan.Value is { } catalog
        ? $"{catalog.Projects.Count} projects · {catalog.Packages.Count} packages"
        : "";

    private static int Direct(ProjectSummary project)
    {
        var count = 0;
        foreach (var use in project.Uses)
        {
            if (!use.Transitive)
            {
                count++;
            }
        }

        return count;
    }

    private static IArlecchinoColor Paint(Branch branch) => branch.Package?.Health switch
    {
        PackageHealth.Vulnerable => Theme.Error,
        PackageHealth.Deprecated => Theme.Warning,
        PackageHealth.Drift => Theme.Accent,
        PackageHealth.Outdated => Theme.Default,
        PackageHealth.Ok => Theme.Muted,
        _ => Theme.Header,
    };
}
