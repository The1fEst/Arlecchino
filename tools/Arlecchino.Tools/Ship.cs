using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Arlecchino.Tools;

/// <summary>
/// Prepares a release: sets the version, names the changelog section after it and moves the recorded public
/// API from unshipped to shipped. Run it, read the diff, commit, tag.
/// </summary>
internal static class Ship
{
    private const string Header = "#nullable enable";
    private const string Removal = "*REMOVED*";
    private const string Calendar = @"^(\d{4}\.(?:[1-9]|1[0-2]))\.([1-9][0-9]*)$";
    private const string UnreleasedHeading = "## Unreleased";
    private const string VersionElement = "<Version>([^<]+)</Version>";

    private static readonly string[] Packages =
        ["Arlecchino", "Arlecchino.Atoms.History", "Arlecchino.Core", "Arlecchino.Pictures", "Arlecchino.Testing"];

    /// <summary>Writes the release into the repository.</summary>
    /// <param name="args">The version to release, or nothing to take the next build of this month.</param>
    /// <returns>Zero when the repository was left ready to commit.</returns>
    internal static async Task<int> Run(string[] args)
    {
        if (args.Length > 1 || (args.Length == 1 && !Regex.IsMatch(args[0], Calendar)))
        {
            return Usage();
        }

        var props = Path.Combine(Program.Root(), "Directory.Build.props");
        var text = await File.ReadAllTextAsync(props);
        var current = Regex.Match(text, VersionElement).Groups[1].Value;

        if (current.Length == 0)
        {
            Console.WriteLine($"no <Version> in {props}");

            return 1;
        }

        var version = args.Length == 1 ? args[0] : Next(current);
        var changelog = Path.Combine(Program.Root(), "CHANGELOG.md");
        var changelogText = await File.ReadAllTextAsync(changelog);
        var unreleasedPattern = new Regex($"^{UnreleasedHeading}(?=\r?$)", RegexOptions.Multiline);

        if (unreleasedPattern.IsMatch(changelogText))
        {
            await File.WriteAllTextAsync(changelog, unreleasedPattern.Replace(changelogText, $"## {version}", 1));
            Console.WriteLine($"changelog: {UnreleasedHeading} -> ## {version}");
        }
        else if (Regex.IsMatch(changelogText, $"^## {Regex.Escape(version)}\r?$", RegexOptions.Multiline))
        {
            Console.WriteLine($"changelog already has ## {version}");
        }
        else
        {
            Console.WriteLine($"{changelog} has no {UnreleasedHeading} section to release");

            return 1;
        }

        if (current == version)
        {
            Console.WriteLine($"version stays {version}");
        }
        else
        {
            text = Regex.Replace(text, VersionElement, $"<Version>{version}</Version>");
            Console.WriteLine($"version {current} -> {version}");
        }

        await File.WriteAllTextAsync(props, text);

        foreach (var package in Packages)
        {
            Record(package);
        }

        return 0;
    }

    /// <summary>The version the repository is set to.</summary>
    /// <returns>What <c>Directory.Build.props</c> says, or an empty string when it says nothing.</returns>
    internal static string GetVersion() =>
        Regex.Match(File.ReadAllText(Path.Combine(Program.Root(), "Directory.Build.props")), VersionElement)
            .Groups[1].Value;

    private static int Usage()
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- ship [version]");
        Console.WriteLine();
        Console.WriteLine("Prepares a release: sets the version, renames ## Unreleased in CHANGELOG.md to it");
        Console.WriteLine("and moves the recorded public API from Unshipped to Shipped. Run it, read the diff,");
        Console.WriteLine("commit, tag.");
        Console.WriteLine();
        Console.WriteLine("A version is year.month.build: 2026.8.1 is the first release of August 2026.");
        Console.WriteLine("Given none, the version in Directory.Build.props says which build this is.");

        return 1;
    }

    /// <summary>
    /// The version a release cut today is due to carry: this year and month, and the build after the one
    /// the repository holds. A version from another month starts the count again at one.
    /// </summary>
    /// <param name="current">The version written in the repository.</param>
    /// <returns>The version to release.</returns>
    private static string Next(string current)
    {
        var today = DateTime.Now;
        var month = FormattableString.Invariant($"{today.Year}.{today.Month}");
        var match = Regex.Match(current, Calendar);

        var build = match.Success && match.Groups[1].Value == month
            ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) + 1
            : 1;

        return FormattableString.Invariant($"{month}.{build}");
    }

    private static void Record(string package)
    {
        var folder = Path.Combine(Program.Root(), "src", package);
        var shippedPath = Path.Combine(folder, "PublicAPI.Shipped.txt");
        var unshippedPath = Path.Combine(folder, "PublicAPI.Unshipped.txt");
        var entries = Entries(unshippedPath);

        if (entries.Count == 0)
        {
            Console.WriteLine($"{package}: nothing to ship");

            return;
        }

        var removals = entries
            .Where(static entry => entry.StartsWith(Removal, StringComparison.Ordinal))
            .Select(static entry => entry[Removal.Length..])
            .ToHashSet(StringComparer.Ordinal);

        var additions = entries.Where(static entry => !entry.StartsWith(Removal, StringComparison.Ordinal)).ToList();

        var total = Entries(shippedPath)
            .Where(entry => !removals.Contains(entry))
            .Concat(additions)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static entry => entry, StringComparer.Ordinal);

        File.WriteAllLines(shippedPath, new[] { Header }.Concat(total));
        File.WriteAllLines(unshippedPath, [Header]);

        Console.WriteLine($"{package}: shipped {additions.Count} added, {removals.Count} removed");
    }

    private static List<string> Entries(string path) =>
        [.. File.ReadAllLines(path).Skip(1).Where(static line => line.Trim().Length > 0)];
}
