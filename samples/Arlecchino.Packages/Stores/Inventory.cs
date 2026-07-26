using System;
using System.Collections.Generic;
using System.IO;
using Arlecchino.Atoms;
using Arlecchino.Packages.Model;
using Arlecchino.Packages.Scanning;

namespace Arlecchino.Packages.Stores;

public enum Lens
{
    All,
    Outdated,
    Vulnerable,
    Deprecated,
    Drift,
}

public sealed class Inventory : IArlecchinoStore
{
    private readonly AsyncAtom<Catalog> _catalog;
    private readonly UiDispatcher _dispatcher;

    public Inventory(UiDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
        _catalog = new(dispatcher, Catalog.Empty);
    }

    public AsyncAtom<Catalog> Scan => _catalog;

    public Atom<string> Solution { get; } = new LocalAtom<string>("");

    public Atom<string> Filter { get; } = new LocalAtom<string>("");

    public Atom<int> Tab { get; } = new LocalAtom<int>(0);

    public Atom<bool> ShowTransitive { get; } = new LocalAtom<bool>(false);

    public Atom<PackageRow?> Selected { get; } = new LocalAtom<PackageRow?>(null);

    public Atom<ScanStep> Step { get; } = new LocalAtom<ScanStep>(new(0, 4, ""));

    public void Rescan()
    {
        if (Solution.Value.Length == 0)
        {
            return;
        }

        var progress = new StepProgress(_dispatcher, Step);
        _catalog.Load(token => PackageScanner.ScanAsync(Solution.Value, progress, token));
    }

    public IReadOnlyList<PackageRow> Visible()
    {
        var rows = new List<PackageRow>();
        var lens = (Lens)Tab.Value;

        foreach (var package in _catalog.Value?.Packages ?? [])
        {
            if (!package.Matches(Filter.Value))
            {
                continue;
            }

            if (package.IsTransitive && !ShowTransitive.Value)
            {
                continue;
            }

            if (Keeps(lens, package))
            {
                rows.Add(package);
            }
        }

        return rows;
    }

    public int Listed()
    {
        var count = 0;
        foreach (var package in _catalog.Value?.Packages ?? [])
        {
            if (!package.IsTransitive || ShowTransitive.Value)
            {
                count++;
            }
        }

        return count;
    }

    public int Listed(PackageHealth health)
    {
        var count = 0;
        foreach (var package in _catalog.Value?.Packages ?? [])
        {
            if ((!package.IsTransitive || ShowTransitive.Value) && package.Health == health)
            {
                count++;
            }
        }

        return count;
    }

    public static string FindSolution()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);

        while (folder is not null)
        {
            var fixture = Path.Combine(folder.FullName, "fixture", "Storefront.slnx");
            if (File.Exists(fixture))
            {
                return fixture;
            }

            folder = folder.Parent;
        }

        string[] patterns = ["*.slnx", "*.sln", "*.csproj"];

        foreach (var pattern in patterns)
        {
            var found = Directory.GetFiles(Directory.GetCurrentDirectory(), pattern);
            if (found.Length > 0)
            {
                return found[0];
            }
        }

        return "";
    }

    private static bool Keeps(Lens lens, PackageRow package) => lens switch
    {
        Lens.Outdated => package.IsOutdated,
        Lens.Vulnerable => package.Advisories.Count > 0,
        Lens.Deprecated => package.DeprecationReasons.Count > 0,
        Lens.Drift => package.ResolvedVersions().Count > 1,
        _ => true,
    };

    private sealed class StepProgress : IProgress<ScanStep>
    {
        private readonly UiDispatcher _dispatcher;
        private readonly Atom<ScanStep> _step;

        public StepProgress(UiDispatcher dispatcher, Atom<ScanStep> step)
        {
            _dispatcher = dispatcher;
            _step = step;
        }

        public void Report(ScanStep value) => _dispatcher.Post(() => _step.Value = value);
    }
}
