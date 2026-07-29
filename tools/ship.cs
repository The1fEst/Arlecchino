#:property GenerateDocumentationFile=true

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

if (args.Length != 1 || !Regex.IsMatch(args[0], @"^\d+\.\d+\.\d+$"))
{
    Console.WriteLine("usage: dotnet run tools/ship.cs <version>");
    Console.WriteLine();
    Console.WriteLine("Prepares a release: sets the version, moves the recorded public API from");
    Console.WriteLine("Unshipped to Shipped, and points package validation at the release before it.");
    Console.WriteLine("Run it, read the diff, commit, tag.");

    return 1;
}

var version = args[0];
var props = Path.Combine(Root(), "Directory.Build.props");
var text = File.ReadAllText(props);
var current = Regex.Match(text, @"<Version>([^<]+)</Version>").Groups[1].Value;

if (current.Length == 0)
{
    Console.WriteLine($"no <Version> in {props}");

    return 1;
}

if (current == version)
{
    Console.WriteLine($"version stays {version}");
}
else
{
    text = Regex.Replace(text, @"<Version>[^<]+</Version>", $"<Version>{version}</Version>");
    Console.WriteLine($"version {current} -> {version}");
}

var baseline = Regex.Match(text, @"<PackageValidationBaselineVersion[^>]*>([^<]+)<").Groups[1].Value;

if (baseline == current)
{
    Console.WriteLine($"baseline stays {baseline}");
}
else if (await IsOnNuGet(current))
{
    text = Regex.Replace(
        text,
        @"(<PackageValidationBaselineVersion[^>]*>)[^<]+(</PackageValidationBaselineVersion>)",
        $"${{1}}{current}${{2}}");

    Console.WriteLine($"baseline {baseline} -> {current}");
}
else
{
    Console.WriteLine($"baseline stays {baseline}: {current} is not on nuget.org, so nothing can be compared against it");
}

File.WriteAllText(props, text);

foreach (var package in new[] { "Arlecchino", "Arlecchino.Core", "Arlecchino.Testing" })
{
    Ship(package);
}

return 0;

static void Ship(string package)
{
    const string Header = "#nullable enable";
    const string Gone = "*REMOVED*";

    var folder = Path.Combine(Root(), "src", package);
    var shippedPath = Path.Combine(folder, "PublicAPI.Shipped.txt");
    var unshippedPath = Path.Combine(folder, "PublicAPI.Unshipped.txt");
    var unshipped = Entries(unshippedPath);

    if (unshipped.Count == 0)
    {
        Console.WriteLine($"{package}: nothing to ship");

        return;
    }

    var removed = unshipped
        .Where(entry => entry.StartsWith(Gone, StringComparison.Ordinal))
        .Select(entry => entry[Gone.Length..])
        .ToHashSet(StringComparer.Ordinal);

    var added = unshipped.Where(entry => !entry.StartsWith(Gone, StringComparison.Ordinal)).ToList();

    var shipped = Entries(shippedPath)
        .Where(entry => !removed.Contains(entry))
        .Concat(added)
        .Distinct(StringComparer.Ordinal)
        .OrderBy(entry => entry, StringComparer.Ordinal);

    File.WriteAllLines(shippedPath, new[] { Header }.Concat(shipped));
    File.WriteAllLines(unshippedPath, [Header]);

    Console.WriteLine($"{package}: shipped {added.Count} added, {removed.Count} removed");
}

static List<string> Entries(string path) =>
    [.. File.ReadAllLines(path).Skip(1).Where(static line => line.Trim().Length > 0)];

static async Task<bool> IsOnNuGet(string version)
{
    try
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        var index = await http.GetStringAsync("https://api.nuget.org/v3-flatcontainer/arlecchino/index.json");

        return index.Contains($"\"{version}\"", StringComparison.Ordinal);
    }
    catch (Exception failure) when (failure is HttpRequestException or TaskCanceledException)
    {
        Console.WriteLine($"could not ask nuget.org: {failure.Message}");

        return false;
    }
}

static string Root([CallerFilePath] string here = "") =>
    Path.GetDirectoryName(Path.GetDirectoryName(here)!)!;
