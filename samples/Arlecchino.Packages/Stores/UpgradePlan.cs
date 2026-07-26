using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Atoms;
using Arlecchino.Packages.Model;
using Arlecchino.Packages.Scanning;

namespace Arlecchino.Packages.Stores;

public sealed class UpgradePlan : IArlecchinoStore
{
    private readonly UiDispatcher _dispatcher;
    private readonly List<string> _log = [];

    public UpgradePlan(UiDispatcher dispatcher) => _dispatcher = dispatcher;

    public Atom<string> Target { get; } = new TrackedAtom<string>("");

    public Atom<IReadOnlyList<string>> Projects { get; } = new TrackedAtom<IReadOnlyList<string>>([]);

    public Atom<bool> DryRun { get; } = new TrackedAtom<bool>(true);

    public Atom<bool> Running { get; } = new LocalAtom<bool>(false);

    public Atom<int> Written { get; } = new LocalAtom<int>(0);

    public IReadOnlyList<string> Log => _log;

    public void Restart(PackageRow package, Catalog catalog)
    {
        Target.Value = package.Latest ?? package.Highest();
        Projects.Value = Holders(package, catalog);
        Written.Value = 0;
        _log.Clear();
    }

    public static IReadOnlyList<string> Holders(PackageRow package, Catalog catalog)
    {
        var names = new List<string>();

        foreach (var use in package.Uses)
        {
            if (!use.Transitive && !names.Contains(use.Project))
            {
                names.Add(use.Project);
            }
        }

        if (names.Count > 0)
        {
            return names;
        }

        foreach (var project in catalog.Projects)
        {
            names.Add(project.Name);
        }

        return names;
    }

    public IReadOnlyList<IReadOnlyList<string>> Steps(PackageRow package, Catalog catalog)
    {
        var steps = new List<IReadOnlyList<string>>();
        var root = Root(catalog);

        foreach (var name in Projects.Value)
        {
            var path = PathOf(name, catalog);
            if (path.Length == 0)
            {
                continue;
            }

            var relative = root.Length == 0 ? path : Path.GetRelativePath(root, path).Replace('\\', '/');
            steps.Add(["add", relative, "package", package.Id, "--version", Target.Value]);
        }

        return steps;
    }

    public static string Root(Catalog catalog) => Path.GetDirectoryName(catalog.Solution) ?? "";

    public void Run(PackageRow package, Catalog catalog, CancellationToken token)
    {
        if (Running.Value)
        {
            return;
        }

        var steps = Steps(package, catalog);
        var root = Root(catalog);
        var dry = DryRun.Value;

        Running.Value = true;
        _log.Clear();

        Task.Run(async () =>
        {
            foreach (var step in steps)
            {
                Write(Dotnet.Describe(step));

                if (dry)
                {
                    continue;
                }

                try
                {
                    var result = await Dotnet.RunAsync(step, root, token).ConfigureAwait(false);
                    Write(result.Failed ? Tail(result.Error) : "  updated");
                }
                catch (Exception failure) when (failure is not OperationCanceledException)
                {
                    Write($"  {failure.Message}");
                }
            }

            _dispatcher.Post(() => Running.Value = false);
        }, token);
    }

    private void Write(string line) => _dispatcher.Post(() =>
    {
        _log.Add(line);
        Written.Value = _log.Count;
    });

    private static string PathOf(string name, Catalog catalog)
    {
        foreach (var project in catalog.Projects)
        {
            if (project.Name == name)
            {
                return project.Path;
            }
        }

        return "";
    }

    private static string Tail(string error)
    {
        var lines = error.Split('\n');

        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var trimmed = lines[i].Trim();
            if (trimmed.Length > 0)
            {
                return "  " + trimmed;
            }
        }

        return "  failed";
    }
}
