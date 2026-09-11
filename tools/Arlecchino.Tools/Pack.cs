using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace Arlecchino.Tools;

/// <summary>
/// Builds the packages into the local feed under <c>artifacts/packages</c>. An application restores from
/// that feed, so a change left unpacked leaves it building against the released version.
/// </summary>
internal static class Pack
{
    /// <summary>What a version may look like: three numbers, and a suffix for one that is not a release.</summary>
    private const string Number = @"^\d+\.\d+\.\d+(?:[.-][0-9A-Za-z.-]+)?$";

    private static readonly string[] Packages =
        ["Arlecchino.Core", "Arlecchino", "Arlecchino.Atoms.History", "Arlecchino.Pictures", "Arlecchino.Testing"];

    /// <summary>Packs every package, stopping at the first that will not build.</summary>
    /// <param name="args">The version to carry, <c>--to</c> and where to put them, or <c>--help</c>.</param>
    /// <returns>Zero when all of them were packed.</returns>
    internal static int Run(string[] args)
    {
        if (args.Contains("--help"))
        {
            return Usage(0);
        }

        if (!Read(args, out var version, out var folder))
        {
            return Usage(1);
        }

        var root = Program.Root();
        var feed = folder.Length > 0 ? folder : Path.Combine(root, "artifacts", "packages");

        foreach (var package in Packages)
        {
            var failure = Dotnet([.. Arguments(root, package, version, folder)]);

            if (failure == 0)
            {
                continue;
            }

            Console.WriteLine($"{package} would not pack");

            return failure;
        }

        Console.WriteLine($"Packed into {feed}");

        return 0;
    }

    private static int Usage(int code)
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- pack [version] [--to <folder>]");
        Console.WriteLine();
        Console.WriteLine("Packs Arlecchino, Arlecchino.Core, Arlecchino.Atoms.History, Arlecchino.Pictures");
        Console.WriteLine("and Arlecchino.Testing in Release into artifacts/packages, the local feed an");
        Console.WriteLine("application is tried against before a change is released.");
        Console.WriteLine();
        Console.WriteLine("A version given here is carried by all five, dependencies between them included,");
        Console.WriteLine("and overrides the one in Directory.Build.props. Give one that no release carries");
        Console.WriteLine("— 2026.8.7-local.1 — since a version already in the global packages folder is");
        Console.WriteLine("restored from there and never looked for in a feed.");
        Console.WriteLine();
        Console.WriteLine("--to puts them somewhere else, which is how a feed outside the repository is fed.");

        return code;
    }

    /// <summary>
    /// Reads the arguments. A version and a folder may each be given once, and anything else is a mistake
    /// worth stopping on rather than packing the wrong thing somewhere unasked for.
    /// </summary>
    /// <param name="args">What was passed.</param>
    /// <param name="version">The version to carry, or empty to keep the repository's.</param>
    /// <param name="folder">Where to put them, or empty for the feed in the repository.</param>
    /// <returns><c>false</c> when the arguments say nothing this tool can act on.</returns>
    private static bool Read(string[] args, out string version, out string folder)
    {
        version = "";
        folder = "";

        for (var at = 0; at < args.Length; at++)
        {
            if (args[at] == "--to")
            {
                if (at + 1 == args.Length || folder.Length > 0)
                {
                    return false;
                }

                folder = Path.GetFullPath(args[++at]);

                continue;
            }

            if (version.Length > 0 || !Regex.IsMatch(args[at], Number))
            {
                return false;
            }

            version = args[at];
        }

        return true;
    }

    private static List<string> Arguments(string root, string package, string version, string folder)
    {
        var arguments = new List<string>
        {
            "pack",
            Path.Combine(root, "src", package, $"{package}.csproj"),
            "--configuration",
            "Release",
        };

        if (version.Length > 0)
        {
            arguments.Add($"-p:Version={version}");
        }

        if (folder.Length > 0)
        {
            arguments.Add($"-p:PackageOutputPath={folder}");
        }

        return arguments;
    }

    private static int Dotnet(params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet");

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet would not start");

        process.WaitForExit();

        return process.ExitCode;
    }
}
