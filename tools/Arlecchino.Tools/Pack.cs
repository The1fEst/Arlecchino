using System;
using System.Diagnostics;
using System.IO;

namespace Arlecchino.Tools;

/// <summary>
/// Builds the three packages into the local feed under <c>artifacts/packages</c>. An application restores
/// from that feed, so a change left unpacked leaves it building against the released version.
/// </summary>
internal static class Pack
{
    private static readonly string[] Packages = ["Arlecchino.Core", "Arlecchino", "Arlecchino.Testing"];

    /// <summary>Packs the three packages, stopping at the first that will not build.</summary>
    /// <param name="args"><c>--help</c> to explain itself; nothing else is read.</param>
    /// <returns>Zero when all three were packed.</returns>
    internal static int Run(string[] args)
    {
        if (args.Contains("--help"))
        {
            Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- pack");
            Console.WriteLine();
            Console.WriteLine("Packs Arlecchino, Arlecchino.Core and Arlecchino.Testing in Release into");
            Console.WriteLine("artifacts/packages, the local feed an application is tried against before a");
            Console.WriteLine("change is released.");

            return 0;
        }

        var root = Program.Root();

        foreach (var package in Packages)
        {
            var project = Path.Combine(root, "src", package, $"{package}.csproj");
            var failure = Dotnet("pack", project, "--configuration", "Release");

            if (failure == 0)
            {
                continue;
            }

            Console.WriteLine($"{package} would not pack");

            return failure;
        }

        Console.WriteLine($"Packed into {Path.Combine(root, "artifacts", "packages")}");

        return 0;
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
