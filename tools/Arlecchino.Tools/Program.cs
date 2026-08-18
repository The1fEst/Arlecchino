using System;
using System.IO;
using System.Text;
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
        ["keys", .. var rest] => Task.FromResult(Keys.Run(rest)),
        ["live", .. var rest] => Task.FromResult(Live.Run(rest)),
        ["oracle", .. var rest] => Task.FromResult(Oracle.Run(rest)),
        ["pack", .. var rest] => Task.FromResult(Pack.Run(rest)),
        ["ship", .. var rest] => Ship.Run(rest),
        ["terminal", .. var rest] => Task.FromResult(Terminal.Run(rest)),
        _ => Task.FromResult(Usage()),
    };

    private static int Usage()
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- <tool> [arguments]");
        Console.WriteLine();
        Console.WriteLine("  keys [name]       compare what a real terminal sends against what is read");
        Console.WriteLine("  live [options]    run an application in a real terminal and see it give it back");
        Console.WriteLine("  oracle [name]     compare the screen the frames leave against a real terminal");
        Console.WriteLine("  pack              build the four packages into the local feed");
        Console.WriteLine("  ship [version]    prepare a release: the version and the public API it carries");
        Console.WriteLine("  terminal [check]  ask, copy and paste against a real terminal");
        Console.WriteLine();
        Console.WriteLine("Ask a tool for --help to hear what it does at length.");

        return 1;
    }

    /// <summary>
    /// Bytes as they can be read back: escapes spelled out, everything else left alone. Both tools that
    /// argue with a terminal print sequences, and a sequence printed raw is a sequence that redraws the
    /// report it appears in.
    /// </summary>
    /// <param name="text">The bytes.</param>
    /// <returns>The bytes, quoted and legible.</returns>
    internal static string Escaped(string text)
    {
        var builder = new StringBuilder("\"");

        foreach (var character in text)
        {
            builder.Append(character switch
            {
                '\e' => "\\e",
                '\r' => "\\r",
                '\n' => "\\n",
                '"' => "\\\"",
                _ when char.IsControl(character) => $"\\u{(int)character:x4}",
                _ => character.ToString(),
            });
        }

        return builder.Append('"').ToString();
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
