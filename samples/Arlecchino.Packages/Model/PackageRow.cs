using System;
using System.Collections.Generic;

namespace Arlecchino.Packages.Model;

public enum PackageHealth
{
    Ok,
    Outdated,
    Drift,
    Deprecated,
    Vulnerable,
}

public sealed record Advisory(string Severity, string Url)
{
    public int Rank => Severity.ToUpperInvariant() switch
    {
        "CRITICAL" => 4,
        "HIGH" => 3,
        "MODERATE" => 2,
        "LOW" => 1,
        _ => 0,
    };
}

public sealed record PackageUse(
    string Id,
    string Project,
    string Framework,
    string Requested,
    string Resolved,
    bool Transitive);

public sealed class PackageRow
{
    private readonly List<PackageUse> _uses = [];
    private readonly List<Advisory> _advisories = [];
    private readonly List<string> _deprecation = [];

    public PackageRow(string id) => Id = id;

    public string Id { get; }

    public IReadOnlyList<PackageUse> Uses => _uses;

    public IReadOnlyList<Advisory> Advisories => _advisories;

    public IReadOnlyList<string> DeprecationReasons => _deprecation;

    public string? Latest { get; set; }

    public string? Alternative { get; set; }

    public bool IsTransitive
    {
        get
        {
            foreach (var use in _uses)
            {
                if (!use.Transitive)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public PackageHealth Health
    {
        get
        {
            if (_advisories.Count > 0)
            {
                return PackageHealth.Vulnerable;
            }

            if (_deprecation.Count > 0)
            {
                return PackageHealth.Deprecated;
            }

            if (ResolvedVersions().Count > 1)
            {
                return PackageHealth.Drift;
            }

            return IsOutdated ? PackageHealth.Outdated : PackageHealth.Ok;
        }
    }

    public bool IsOutdated => Latest is { Length: > 0 } latest && VersionOrder.Compare(latest, Highest()) > 0;

    public Advisory? WorstAdvisory()
    {
        Advisory? worst = null;
        foreach (var advisory in _advisories)
        {
            if (worst is null || advisory.Rank > worst.Rank)
            {
                worst = advisory;
            }
        }

        return worst;
    }

    public IReadOnlyList<string> ResolvedVersions()
    {
        var versions = new List<string>();
        foreach (var use in _uses)
        {
            if (!versions.Contains(use.Resolved))
            {
                versions.Add(use.Resolved);
            }
        }

        versions.Sort(VersionOrder.Compare);
        return versions;
    }

    public string Resolved() => string.Join(", ", ResolvedVersions());

    public string Highest()
    {
        var versions = ResolvedVersions();
        return versions.Count == 0 ? "" : versions[^1];
    }

    public void Add(PackageUse use)
    {
        foreach (var known in _uses)
        {
            if (known == use)
            {
                return;
            }
        }

        _uses.Add(use);
    }

    public void Warn(Advisory advisory)
    {
        foreach (var known in _advisories)
        {
            if (known.Url == advisory.Url)
            {
                return;
            }
        }

        _advisories.Add(advisory);
    }

    public void Deprecate(IEnumerable<string> reasons)
    {
        foreach (var reason in reasons)
        {
            if (!_deprecation.Contains(reason))
            {
                _deprecation.Add(reason);
            }
        }
    }

    public bool Matches(string text) =>
        text.Length == 0 || Id.Contains(text, StringComparison.OrdinalIgnoreCase);
}
