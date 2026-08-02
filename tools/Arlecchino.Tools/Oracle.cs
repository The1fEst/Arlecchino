using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Arlecchino.Rendering;
using Arlecchino.Testing;

namespace Arlecchino.Tools;

/// <summary>
/// Holds the screen <see cref="ScreenGrid"/> reads frames back from against the screen a real terminal
/// ends up with. The emulator and the code that writes the frames were written by the same head, so a
/// wrong idea about the edge of a row or the width of a symbol would be held by both and cancel out,
/// leaving every test green and the picture wrong. tmux is the second opinion.
/// </summary>
internal static class Oracle
{
    private const string Socket = "arlecchino-oracle";
    private const int TabStop = 8;

    /// <summary>Runs the scenarios and reports where the two screens differ.</summary>
    /// <param name="args">A name, or part of one, to run a single scenario; <c>--help</c> to explain itself.</param>
    /// <returns>Zero when the two screens agree everywhere.</returns>
    internal static int Run(string[] args)
    {
        if (args.Contains("--help"))
        {
            return Explain();
        }

        if (Which("tmux") is "")
        {
            Console.WriteLine("tmux is not on PATH; nothing to compare against");

            return 2;
        }

        TerminalCapabilities.Color = ColorSupport.TrueColor;

        var wanted = args.FirstOrDefault(static argument => !argument.StartsWith('-')) ?? "";
        var scenarios = Scenarios()
            .Where(scenario => wanted.Length == 0 || scenario.Name.Contains(wanted, StringComparison.Ordinal))
            .ToList();

        if (scenarios.Count == 0)
        {
            Console.WriteLine($"no scenario matches '{wanted}'");

            return 1;
        }

        var mismatched = 0;

        foreach (var scenario in scenarios)
        {
            var drawn = scenario.Draw();
            var played = Play(drawn.Output, scenario.Width, scenario.Height);
            var differences = Differences(drawn, played);

            if (differences.Count == 0)
            {
                Console.WriteLine($"  ok    {scenario.Name} ({scenario.Width}x{scenario.Height})");

                continue;
            }

            mismatched++;
            Console.WriteLine($"  DIFF  {scenario.Name} ({scenario.Width}x{scenario.Height})");

            foreach (var difference in differences)
            {
                Console.WriteLine($"          {difference}");
            }

            Console.WriteLine($"          stream: {Escaped(drawn.Output)}");
        }

        Tmux("kill-server");

        Console.WriteLine();
        Console.WriteLine(mismatched == 0
            ? $"{scenarios.Count} scenarios, the screen tmux draws is the screen ScreenGrid draws"
            : $"{scenarios.Count} scenarios, {mismatched} where they disagree");

        return mismatched == 0 ? 0 : 1;
    }

    private static int Explain()
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- oracle [name]");
        Console.WriteLine();
        Console.WriteLine("Draws frames through the real Surface, plays what it wrote into a tmux pane of");
        Console.WriteLine("the same size, and compares the screen tmux ended up with against the one");
        Console.WriteLine("ScreenGrid did, cursor included. Both halves of the test suite were written");
        Console.WriteLine("here, so this is the only thing that catches a wrong model they happen to share.");
        Console.WriteLine();
        Console.WriteLine("Pass a name, or part of one, to run a single scenario.");
        Console.WriteLine();
        Console.WriteLine("One thing tmux does that a screen does not: it keeps a tab in the cell it");
        Console.WriteLine("advanced from and hands it back on capture, so the capture is spread out to the");
        Console.WriteLine("stops it stood for before anything is compared.");

        return 0;
    }

    /// <summary>
    /// Plays what was written into a pane of the same size and reads the screen back. The pane is told
    /// to touch a file once it has swallowed the lot, which is what says the screen is worth reading —
    /// a pane that has exited says so by printing into itself, and that would scroll away the very
    /// picture being measured.
    /// </summary>
    /// <param name="output">The bytes the frames wrote.</param>
    /// <param name="width">Columns the pane is opened at.</param>
    /// <param name="height">Rows the pane is opened at.</param>
    /// <returns>The screen tmux ended up with.</returns>
    private static Played Play(string output, int width, int height)
    {
        var folder = Directory.CreateTempSubdirectory("arlecchino-oracle");

        try
        {
            var stream = Path.Combine(folder.FullName, "stream");
            var marker = Path.Combine(folder.FullName, "played");

            File.WriteAllText(stream, output, new UTF8Encoding(false));

            Tmux("kill-session", "-t", Socket);
            Tmux(
                "-f", "/dev/null",
                "new-session", "-d",
                "-s", Socket,
                "-x", width.ToString(CultureInfo.InvariantCulture),
                "-y", height.ToString(CultureInfo.InvariantCulture),
                "--",
                "sh", "-c", $"cat '{stream}'; : > '{marker}'; sleep 300");

            Await(() => File.Exists(marker), "tmux never finished playing the frames back");
            Settle(height);

            var size = Tmux("display-message", "-p", "#{pane_width}x#{pane_height}").Trim();
            var expected = $"{width}x{height}";

            if (size != expected)
            {
                throw new InvalidOperationException($"asked tmux for a {expected} pane and got {size}");
            }

            var cursor = Tmux("display-message", "-p", "#{cursor_y} #{cursor_x}").Trim().Split(' ');

            return new(
                Grid(height),
                int.Parse(cursor[0], CultureInfo.InvariantCulture),
                int.Parse(cursor[1], CultureInfo.InvariantCulture));
        }
        finally
        {
            folder.Delete(true);
        }
    }

    private static void Settle(int height)
    {
        var last = "";

        for (var attempt = 0; attempt < 25; attempt++)
        {
            var now = string.Join('\n', Grid(height));

            if (attempt > 0 && now == last)
            {
                return;
            }

            last = now;
            Thread.Sleep(20);
        }
    }

    private static string[] Grid(int height)
    {
        var lines = Tmux("capture-pane", "-p").Split('\n');
        var grid = new string[height];

        for (var row = 0; row < height; row++)
        {
            grid[row] = Detabbed(row < lines.Length ? lines[row] : "");
        }

        return grid;
    }

    private static string Detabbed(string line)
    {
        if (!line.Contains('\t', StringComparison.Ordinal))
        {
            return line;
        }

        var spread = new StringBuilder();

        foreach (var character in line)
        {
            if (character != '\t')
            {
                spread.Append(character);

                continue;
            }

            spread.Append(' ', TabStop - spread.Length % TabStop);
        }

        return spread.ToString();
    }

    private static List<string> Differences(Drawn drawn, Played played)
    {
        var differences = new List<string>();

        for (var row = 0; row < drawn.Lines.Length; row++)
        {
            var ours = drawn.Lines[row].TrimEnd();
            var theirs = played.Lines[row].TrimEnd();

            if (ours == theirs)
            {
                continue;
            }

            differences.Add($"row {row}  grid: {Escaped(ours)}");
            differences.Add($"          tmux: {Escaped(theirs)}");
        }

        if (drawn.CursorRow != played.CursorRow || drawn.CursorColumn != played.CursorColumn)
        {
            differences.Add(
                $"cursor  grid: {drawn.CursorRow},{drawn.CursorColumn}  tmux: {played.CursorRow},{played.CursorColumn}");
        }

        return differences;
    }

    private static string Escaped(string text)
    {
        var escaped = new StringBuilder("\"");

        foreach (var character in text)
        {
            escaped.Append(character switch
            {
                '\e' => "\\e",
                '\r' => "\\r",
                '\n' => "\\n",
                '"' => "\\\"",
                _ when char.IsControl(character) => $"\\u{(int)character:x4}",
                _ => character.ToString(),
            });
        }

        return escaped.Append('"').ToString();
    }

    private static void Await(Func<bool> done, string complaint)
    {
        for (var attempt = 0; attempt < 300; attempt++)
        {
            if (done())
            {
                return;
            }

            Thread.Sleep(20);
        }

        throw new TimeoutException(complaint);
    }

    private static string Tmux(params string[] arguments)
    {
        var start = new ProcessStartInfo("tmux") { RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("-L");
        start.ArgumentList.Add(Socket);

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("tmux would not start");

        var output = process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();

        return output;
    }

    private static string Which(string command)
    {
        var start = new ProcessStartInfo("/usr/bin/env") { RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("which");
        start.ArgumentList.Add(command);

        using var process = Process.Start(start);

        if (process is null)
        {
            return "";
        }

        var found = process.StandardOutput.ReadToEnd().Trim();
        process.StandardError.ReadToEnd();
        process.WaitForExit();

        return process.ExitCode == 0 ? found : "";
    }

    private static IEnumerable<Scenario> Scenarios()
    {
        yield return new Frames("plain", 20, 4, [
            static surface =>
            {
                surface.AppendLine("hello", Theme.Default);
                surface.AppendLine("world", Theme.Accent);
            },
        ]);

        yield return new Frames("every-cell", 20, 4, [
            static surface =>
            {
                for (var row = 0; row < 4; row++)
                {
                    surface.WriteAt(row, 0, new('#', 20), Theme.Default);
                }
            },
        ]);

        yield return new Frames("clipped", 20, 3, [
            static surface => surface.AppendLine(new('x', 60), Theme.Default),
        ]);

        yield return new Frames("wide", 20, 3, [
            static surface =>
            {
                surface.WriteAt(0, 0, "日本語テキスト", Theme.Default);
                surface.WriteAt(1, 1, "日本語", Theme.Accent);
            },
        ]);

        yield return new Frames("wide-at-edge", 20, 3, [
            static surface =>
            {
                surface.WriteAt(0, 18, "日", Theme.Default);
                surface.WriteAt(1, 19, "日", Theme.Default);
            },
        ]);

        yield return new Frames("emoji", 20, 3, [
            static surface =>
            {
                surface.WriteAt(0, 0, "🙂 ok", Theme.Default);
                surface.WriteAt(1, 0, "👍🏽 done", Theme.Default);
                surface.WriteAt(2, 0, "👨‍👩‍👧‍👦 family", Theme.Default);
            },
        ]);

        yield return new Frames("combining", 20, 3, [
            static surface =>
            {
                surface.WriteAt(0, 0, "éclair", Theme.Default);
                surface.WriteAt(1, 0, "éclair", Theme.Default);
                surface.WriteAt(2, 0, "àb̈c", Theme.Default);
            },
        ]);

        yield return new Frames("box-drawing", 20, 4, [
            static surface =>
            {
                surface.WriteAt(0, 0, "┌──────────┐", Theme.Default);
                surface.WriteAt(1, 0, "│ boxed    │", Theme.Default);
                surface.WriteAt(2, 0, "╞══════════╡", Theme.Default);
                surface.WriteAt(3, 0, "└──────────┘", Theme.Default);
            },
        ]);

        yield return new Frames("styles", 20, 3, [
            static surface =>
            {
                surface.WriteAt(0, 0, "red", Theme.Error);
                surface.WriteAt(0, 4, "muted", Theme.Muted);
                surface.WriteAt(1, 0, "selected row", Theme.Selected);
            },
        ]);

        yield return new Frames("diff-one-cell", 20, 4, [
            static surface => surface.WriteAt(0, 0, "hello", Theme.Default),
            static surface => surface.WriteAt(0, 0, "hellp", Theme.Default),
        ]);

        yield return new Frames("diff-scattered", 20, 4, [
            static surface =>
            {
                surface.WriteAt(0, 0, "aaaaaaaaaaaaaaaaaaaa", Theme.Default);
                surface.WriteAt(3, 0, "bbbbbbbbbbbbbbbbbbbb", Theme.Default);
            },
            static surface =>
            {
                surface.WriteAt(0, 0, "xaaaaaaaaaaaaaaaaaax", Theme.Default);
                surface.WriteAt(3, 0, "ybbbbbbbbbbbbbbbbbby", Theme.Default);
            },
        ]);

        yield return new Frames("diff-to-blank", 20, 4, [
            static surface =>
            {
                surface.WriteAt(0, 0, "something here", Theme.Default);
                surface.WriteAt(2, 3, "and here", Theme.Accent);
            },
            static _ => { },
        ]);

        yield return new Frames("diff-wide-to-narrow", 20, 3, [
            static surface => surface.WriteAt(0, 2, "日本語", Theme.Default),
            static surface => surface.WriteAt(0, 2, "ab", Theme.Default),
        ]);

        yield return new Frames("diff-narrow-to-wide", 20, 3, [
            static surface => surface.WriteAt(0, 2, "abcdef", Theme.Default),
            static surface => surface.WriteAt(0, 3, "日本", Theme.Default),
        ]);

        yield return new Frames("diff-shifted", 20, 3, [
            static surface => surface.WriteAt(1, 0, "the quick brown fox", Theme.Default),
            static surface => surface.WriteAt(1, 1, "the quick brown fox", Theme.Default),
        ]);

        yield return new Frames("diff-last-cell", 20, 4, [
            static surface => surface.WriteAt(3, 19, "a", Theme.Default),
            static surface => surface.WriteAt(3, 19, "b", Theme.Default),
            static surface => surface.WriteAt(3, 19, "日", Theme.Default),
        ]);

        yield return new Frames("diff-styles-only", 20, 3, [
            static surface => surface.WriteAt(0, 0, "recoloured", Theme.Default),
            static surface => surface.WriteAt(0, 0, "recoloured", Theme.Error),
        ]);

        yield return new Frames("two-columns", 2, 3, [
            static surface =>
            {
                surface.WriteAt(0, 0, "abc", Theme.Default);
                surface.WriteAt(1, 0, "日", Theme.Default);
                surface.WriteAt(2, 1, "日", Theme.Default);
            },
        ]);

        yield return new Frames("one-row", 20, 1, [
            static surface => surface.WriteAt(0, 0, "the whole screen ---", Theme.Default),
            static surface => surface.WriteAt(0, 0, "the whole screen +++", Theme.Default),
        ]);

        yield return new Raw("raw-wrap", 10, 4, "aaaaaaaaaabbbbbbbbbbccc");

        yield return new Raw("raw-wrap-off", 10, 4, "\e[?7laaaaaaaaaabbbbbbbbbbccc");

        yield return new Raw("raw-wrap-wide", 10, 4, "\e[1;10H日本");

        yield return new Raw("raw-wrap-off-wide", 10, 4, "\e[?7l\e[1;10H日本");

        yield return new Raw("raw-scroll", 10, 3, "one\r\ntwo\r\nthree\r\nfour\r\nfive");

        yield return new Raw("raw-erase-line", 12, 3, "\e[1;1Habcdefghijkl\e[1;5H\e[K\e[2;1Habcdefghijkl\e[2;5H\e[1K");

        yield return new Raw("raw-erase-screen", 12, 3, "\e[1;1Haaaa\e[2;1Hbbbb\e[3;1Hcccc\e[2;3H\e[J");

        yield return new Raw("raw-erase-above", 12, 3, "\e[1;1Haaaa\e[2;1Hbbbb\e[3;1Hcccc\e[2;3H\e[1J");

        yield return new Raw("raw-relative-moves", 12, 4, "\e[3;6Hx\e[Ay\e[2Dz\e[2Bq\e[3Cw");

        yield return new Raw("raw-backspace", 12, 2, "abc\bX\e[2;1Hd\b\bY");

        yield return new Raw("raw-tab", 32, 2, "a\tb\tc\e[2;1H\tz");

        yield return new Raw("raw-backspace-at-edge", 6, 2, "abcdef\bX");

        yield return new Raw("raw-wrap-off-onto-wide-tail", 6, 2, "\e[?7l日本語x");

        yield return new Raw("raw-over-wide-head", 12, 2, "日本語\e[1;3Hx");

        yield return new Raw("raw-over-wide-tail", 12, 2, "日本語\e[1;2Hx");

        yield return new Raw("raw-off-screen-jump", 12, 3, "\e[9;99Hx");
    }

    /// <summary>Something to write to a terminal, and the screen the emulator says it leaves.</summary>
    /// <param name="Name">What it is called on the command line.</param>
    /// <param name="Width">Columns the screen is.</param>
    /// <param name="Height">Rows the screen is.</param>
    private abstract record Scenario(string Name, int Width, int Height)
    {
        internal abstract Drawn Draw();

        private protected static Drawn Read(string output, ScreenGrid screen) =>
            new(output, screen.Lines(), screen.CursorRow, screen.CursorColumn);
    }

    /// <summary>
    /// Frames composed by the real <see cref="Surface"/>, which is the case worth checking: every frame
    /// but the first is written as the difference from the one before, and that is the writing whose
    /// meaning is being argued over.
    /// </summary>
    /// <param name="Name">What it is called on the command line.</param>
    /// <param name="Width">Columns the screen is.</param>
    /// <param name="Height">Rows the screen is.</param>
    /// <param name="Draws">One delegate per frame, drawing into the surface between start and build.</param>
    private sealed record Frames(string Name, int Width, int Height, Action<Surface>[] Draws)
        : Scenario(Name, Width, Height)
    {
        internal override Drawn Draw()
        {
            var terminal = new FakeTerminal(Width, Height);
            var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

            foreach (var draw in Draws)
            {
                surface.StartFrame();
                draw(surface);
                surface.Build();
            }

            return Read(terminal.Written, terminal.Screen);
        }
    }

    /// <summary>
    /// A stream written by hand, for the corners no frame reaches — a tab, a backspace, an erase, a
    /// symbol shoved past the right edge. The emulator answers for those too, and nothing else here
    /// would ask it to.
    /// </summary>
    /// <param name="Name">What it is called on the command line.</param>
    /// <param name="Width">Columns the screen is.</param>
    /// <param name="Height">Rows the screen is.</param>
    /// <param name="Output">The bytes, escapes and all.</param>
    private sealed record Raw(string Name, int Width, int Height, string Output) : Scenario(Name, Width, Height)
    {
        internal override Drawn Draw()
        {
            var screen = new ScreenGrid(Width, Height);
            screen.Apply(Output);

            return Read(Output, screen);
        }
    }

    /// <summary>What was written, and the screen the emulator says it leaves.</summary>
    /// <param name="Output">The bytes written.</param>
    /// <param name="Lines">The screen, one string per row.</param>
    /// <param name="CursorRow">Where the cursor ended up.</param>
    /// <param name="CursorColumn">Where the cursor ended up.</param>
    private sealed record Drawn(string Output, string[] Lines, int CursorRow, int CursorColumn);

    /// <summary>The screen tmux ended up with.</summary>
    /// <param name="Lines">The screen, one string per row.</param>
    /// <param name="CursorRow">Where the cursor ended up.</param>
    /// <param name="CursorColumn">Where the cursor ended up.</param>
    private sealed record Played(string[] Lines, int CursorRow, int CursorColumn);
}
