using System;
using System.Collections.Generic;

namespace Arlecchino.Packages.Model;

public sealed record ProjectSummary(string Name, string Path, IReadOnlyList<string> Frameworks, IReadOnlyList<PackageUse> Uses);

public sealed class Catalog
{
    public static readonly Catalog Empty = new()
    {
        Solution = "",
        Packages = [],
        Projects = [],
        Notes = [],
    };

    public required string Solution { get; init; }

    public required IReadOnlyList<PackageRow> Packages { get; init; }

    public required IReadOnlyList<ProjectSummary> Projects { get; init; }

    public required IReadOnlyList<string> Notes { get; init; }

    public DateTimeOffset ScannedAt { get; init; }

    public int Count(PackageHealth health)
    {
        var found = 0;
        foreach (var package in Packages)
        {
            if (package.Health == health)
            {
                found++;
            }
        }

        return found;
    }

    public PackageRow? Find(string id)
    {
        foreach (var package in Packages)
        {
            if (string.Equals(package.Id, id, StringComparison.OrdinalIgnoreCase))
            {
                return package;
            }
        }

        return null;
    }
}
