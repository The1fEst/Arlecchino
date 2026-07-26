using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Packages.Model;

namespace Arlecchino.Packages.Scanning;

public sealed record ScanStep(int Done, int Total, string Title);

public static class PackageScanner
{
    private static readonly (string Title, string Switch)[] Passes =
    [
        ("package graph", "--include-transitive"),
        ("newer versions", "--outdated"),
        ("advisories", "--vulnerable"),
        ("deprecations", "--deprecated"),
    ];

    public static async Task<Catalog> ScanAsync(string solution, IProgress<ScanStep> progress, CancellationToken token)
    {
        var packages = new Dictionary<string, PackageRow>(StringComparer.OrdinalIgnoreCase);
        var projects = new Dictionary<string, List<PackageUse>>(StringComparer.Ordinal);
        var frameworks = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var notes = new List<string>();

        for (var pass = 0; pass < Passes.Length; pass++)
        {
            var (title, option) = Passes[pass];
            progress.Report(new(pass, Passes.Length, title));

            var report = await ReadAsync(solution, option, token).ConfigureAwait(false);
            var problem = Problem(report);

            if (problem is not null)
            {
                if (pass == 0)
                {
                    throw new InvalidOperationException(problem);
                }

                notes.Add($"{option} — {problem}");
                continue;
            }

            Merge(report, pass == 0, packages, projects, frameworks);
        }

        progress.Report(new(Passes.Length, Passes.Length, "done"));

        var rows = new List<PackageRow>(packages.Values);
        rows.Sort(static (first, second) => string.CompareOrdinal(first.Id, second.Id));

        var summaries = new List<ProjectSummary>();
        foreach (var (path, uses) in projects)
        {
            summaries.Add(new(Path.GetFileNameWithoutExtension(path), path, frameworks[path], uses));
        }

        summaries.Sort(static (first, second) => string.CompareOrdinal(first.Name, second.Name));

        return new()
        {
            Solution = solution,
            Packages = rows,
            Projects = summaries,
            Notes = notes,
            ScannedAt = DateTimeOffset.Now,
        };
    }

    private static void Merge(
        ListReport report,
        bool takeUses,
        Dictionary<string, PackageRow> packages,
        Dictionary<string, List<PackageUse>> projects,
        Dictionary<string, List<string>> frameworks)
    {
        foreach (var project in report.Projects ?? [])
        {
            if (project.Path is not { Length: > 0 } path)
            {
                continue;
            }

            var name = Path.GetFileNameWithoutExtension(path);

            foreach (var framework in project.Frameworks ?? [])
            {
                var moniker = framework.Framework ?? "";

                if (takeUses)
                {
                    if (!projects.TryGetValue(path, out var uses))
                    {
                        uses = [];
                        projects[path] = uses;
                        frameworks[path] = [];
                    }

                    if (!frameworks[path].Contains(moniker))
                    {
                        frameworks[path].Add(moniker);
                    }
                }

                Take(framework.TopLevelPackages, false);
                Take(framework.TransitivePackages, true);

                void Take(List<ReportPackage>? listed, bool transitive)
                {
                    foreach (var listing in listed ?? [])
                    {
                        if (listing.Id is not { Length: > 0 } id)
                        {
                            continue;
                        }

                        if (!packages.TryGetValue(id, out var row))
                        {
                            row = new(id);
                            packages[id] = row;
                        }

                        var resolved = listing.ResolvedVersion ?? "";
                        var use = new PackageUse(id, name, moniker, listing.RequestedVersion ?? resolved, resolved, transitive);

                        row.Add(use);

                        if (takeUses)
                        {
                            projects[path].Add(use);
                        }

                        if (listing.LatestVersion is { Length: > 0 } latest)
                        {
                            row.Latest = latest;
                        }

                        foreach (var vulnerability in listing.Vulnerabilities ?? [])
                        {
                            row.Warn(new(vulnerability.Severity ?? "Unknown", vulnerability.AdvisoryUrl ?? ""));
                        }

                        if (listing.DeprecationReasons is { Count: > 0 } reasons)
                        {
                            row.Deprecate(reasons);
                        }

                        if (listing.AlternativePackage?.Id is { Length: > 0 } alternative)
                        {
                            row.Alternative = alternative;
                        }
                    }
                }
            }
        }
    }

    private static async Task<ListReport> ReadAsync(string solution, string option, CancellationToken token)
    {
        var result = await Dotnet
            .RunAsync(["list", solution, "package", "--format", "json", option], "", token)
            .ConfigureAwait(false);

        if (result.Output.Length == 0)
        {
            throw new InvalidOperationException(Firstline(result.Error) ?? $"dotnet list package exited with {result.ExitCode}");
        }

        try
        {
            return JsonSerializer.Deserialize(result.Output, ReportJson.Default.ListReport)
                   ?? throw new InvalidOperationException("the report was empty");
        }
        catch (JsonException failure)
        {
            throw new InvalidOperationException($"the report could not be read — {failure.Message}", failure);
        }
    }

    private static string? Problem(ListReport report)
    {
        foreach (var problem in report.Problems ?? [])
        {
            if (problem.Text is { Length: > 0 } text)
            {
                return text;
            }
        }

        return null;
    }

    private static string? Firstline(string text)
    {
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0)
            {
                return trimmed;
            }
        }

        return null;
    }
}
