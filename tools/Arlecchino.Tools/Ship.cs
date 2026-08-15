using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Arlecchino.Tools;

/// <summary>
/// Prepares a release: sets the version and moves the recorded public API from unshipped to shipped. Run
/// it, read the diff, commit, tag.
/// </summary>
internal static class Ship
{
    private const string Header = "#nullable enable";
    private const string Gone = "*REMOVED*";
    private const string Calendar = @"^(\d{4}\.(?:[1-9]|1[0-2]))\.([1-9][0-9]*)$";

    private static readonly string[] Packages =
        ["Arlecchino", "Arlecchino.Core", "Arlecchino.Pictures", "Arlecchino.Testing"];

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
        var current = Regex.Match(text, "<Version>([^<]+)</Version>").Groups[1].Value;

        if (current.Length == 0)
        {
            Console.WriteLine($"no <Version> in {props}");

            return 1;
        }

        var version = args.Length == 1 ? args[0] : Next(current);

        if (current == version)
        {
            Console.WriteLine($"version stays {version}");
        }
        else
        {
            text = Regex.Replace(text, "<Version>[^<]+</Version>", $"<Version>{version}</Version>");
            Console.WriteLine($"version {current} -> {version}");
        }

        await File.WriteAllTextAsync(props, text);

        foreach (var package in Packages)
        {
            Record(package);
        }

        return 0;
    }

    private static int Usage()
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- ship [version]");
        Console.WriteLine();
        Console.WriteLine("Prepares a release: sets the version and moves the recorded public API from");
        Console.WriteLine("Unshipped to Shipped. Run it, read the diff, commit, tag.");
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
        var written = Regex.Match(current, Calendar);

        var build = written.Success && written.Groups[1].Value == month
            ? int.Parse(written.Groups[2].Value, CultureInfo.InvariantCulture) + 1
            : 1;

        return FormattableString.Invariant($"{month}.{build}");
    }

    private static void Record(string package)
    {
        var folder = Path.Combine(Program.Root(), "src", package);
        var shippedPath = Path.Combine(folder, "PublicAPI.Shipped.txt");
        var unshippedPath = Path.Combine(folder, "PublicAPI.Unshipped.txt");
        var unshipped = Entries(unshippedPath);

        if (unshipped.Count == 0)
        {
            Console.WriteLine($"{package}: nothing to ship");

            return;
        }

        var removed = unshipped
            .Where(static entry => entry.StartsWith(Gone, StringComparison.Ordinal))
            .Select(static entry => entry[Gone.Length..])
            .ToHashSet(StringComparer.Ordinal);

        var added = unshipped.Where(static entry => !entry.StartsWith(Gone, StringComparison.Ordinal)).ToList();

        var shipped = Entries(shippedPath)
            .Where(entry => !removed.Contains(entry))
            .Concat(added)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static entry => entry, StringComparer.Ordinal);

        File.WriteAllLines(shippedPath, new[] { Header }.Concat(shipped));
        File.WriteAllLines(unshippedPath, [Header]);

        Console.WriteLine($"{package}: shipped {added.Count} added, {removed.Count} removed");
    }

    private static List<string> Entries(string path) =>
        [.. File.ReadAllLines(path).Skip(1).Where(static line => line.Trim().Length > 0)];
}
