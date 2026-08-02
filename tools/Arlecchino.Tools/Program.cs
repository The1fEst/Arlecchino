using System;
using System.IO;
using System.Threading.Tasks;

namespace Arlecchino.Tools;

/// <summary>
/// The way in. One tool to a file, named by its first argument, so that everything the repository is
/// maintained with builds and is inspected with the rest of it rather than sitting beside it as a
/// script nothing checks.
/// </summary>
internal static class Program
{
    private static Task<int> Main(string[] args) => args switch
    {
        ["oracle", .. var rest] => Task.FromResult(Oracle.Run(rest)),
        ["pack", .. var rest] => Task.FromResult(Pack.Run(rest)),
        ["ship", .. var rest] => Ship.Run(rest),
        _ => Task.FromResult(Usage()),
    };

    private static int Usage()
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- <tool> [arguments]");
        Console.WriteLine();
        Console.WriteLine("  oracle [name]     compare the screen the frames leave against a real terminal");
        Console.WriteLine("  pack              build the three packages into the local feed");
        Console.WriteLine("  ship <version>    prepare a release: version, public API, validation baseline");
        Console.WriteLine();
        Console.WriteLine("Ask a tool for --help to hear what it does at length.");

        return 1;
    }

    /// <summary>
    /// The repository this was built out of, found by walking up to the solution rather than by asking
    /// where the source file was, so that moving a tool cannot quietly point it at the wrong tree.
    /// </summary>
    /// <returns>The full path of the repository root.</returns>
    /// <exception cref="InvalidOperationException">Nothing above holds the solution.</exception>
    internal static string Root()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);

        while (folder is not null)
        {
            if (File.Exists(Path.Combine(folder.FullName, "Arlecchino.slnx")))
            {
                return folder.FullName;
            }

            folder = folder.Parent;
        }

        throw new InvalidOperationException($"no Arlecchino.slnx above {AppContext.BaseDirectory}");
    }
}
