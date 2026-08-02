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

            Console.WriteLine($"          stream: {Program.Escaped(drawn.Output)}");
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
                Colours(width, height),
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
        var lines = Tmux("capture-pane", "-p", "-N").Split('\n');
        var grid = new string[height];

        for (var row = 0; row < height; row++)
        {
            grid[row] = Detabbed(row < lines.Length ? lines[row] : "");
        }

        return grid;
    }

    /// <summary>
    /// The colour every cell ended up in, as tmux hands it back: the text again, with the sequences it
    /// would send to redraw it left in. Reading them is a matter of walking the line and remembering
    /// what is in force, the same thing a terminal does — which is why both sides of the comparison are
    /// boiled down to <see cref="Paint"/> rather than compared as sequences. tmux writes the same colour
    /// differently from the way the frame asked for it, and neither spelling is wrong.
    ///
    /// A cell tmux never touched is not handed back at all, and reads as plain, which is what it is.
    /// </summary>
    /// <param name="width">Columns the pane is.</param>
    /// <param name="height">Rows the pane is.</param>
    /// <returns>The colour of every cell.</returns>
    private static Paint[][] Colours(int width, int height)
    {
        var lines = Tmux("capture-pane", "-p", "-N", "-e").Split('\n');
        var paints = new Paint[height][];
        var carried = Paint.Plain;

        for (var row = 0; row < height; row++)
        {
            paints[row] = Painted(row < lines.Length ? lines[row] : "", width, ref carried);
        }

        return paints;
    }

    /// <summary>
    /// What a cell's recorded style comes to. The screen keeps the sequence that was in force when the
    /// cell was written, which is how the frame spelled the colour; this is the colour itself.
    /// </summary>
    /// <param name="style">The sequence, empty where the style was reset.</param>
    /// <returns>The colour.</returns>
    private static Paint Painted(string style)
    {
        var paint = Paint.Plain;
        var index = 0;

        while (index < style.Length)
        {
            index = style[index] == '\e' ? Sequence(style, index, ref paint) : index + 1;
        }

        return paint;
    }

    /// <summary>
    /// The colour of every cell in one captured row. tmux writes the capture as the difference from
    /// what it last said, and carries that across the line break — a row drawn in the colour the row
    /// above ended in is handed back saying nothing about its colour at all. So the state is threaded
    /// from row to row rather than started afresh, which is the only reading under which the capture
    /// means what it looks like.
    ///
    /// A cell it never hands back is a different matter: it is blank and carries nothing, whatever was
    /// in force when the row before it ended.
    /// </summary>
    /// <param name="line">The captured row, sequences and all.</param>
    /// <param name="width">Columns the pane is.</param>
    /// <param name="paint">What is in force coming in, and what is left in force going out.</param>
    /// <returns>The colour of every cell in the row.</returns>
    private static Paint[] Painted(string line, int width, ref Paint paint)
    {
        var paints = new Paint[width];
        Array.Fill(paints, Paint.Plain);

        var column = 0;
        var index = 0;

        while (index < line.Length && column < width)
        {
            if (line[index] == '\e')
            {
                index = Sequence(line, index, ref paint);

                continue;
            }

            var length = TextWidth.NextClusterLength(line, index);
            var cells = Math.Max(1, TextWidth.OfCluster(line.AsSpan(index, length)));

            for (var cell = 0; cell < cells && column < width; cell++)
            {
                paints[column++] = paint;
            }

            index += length;
        }

        return paints;
    }

    private static int Sequence(string line, int index, ref Paint paint)
    {
        var at = index + 2;

        if (index + 1 >= line.Length || line[index + 1] != '[')
        {
            return index + 2;
        }

        while (at < line.Length && !char.IsBetween(line[at], '@', '~'))
        {
            at++;
        }

        if (at >= line.Length)
        {
            return line.Length;
        }

        if (line[at] == 'm')
        {
            paint = Repainted(paint, line[(index + 2)..at]);
        }

        return at + 1;
    }

    /// <summary>
    /// What a run of colour parameters leaves in force. Only what the framework can ask for is read —
    /// the eight colours and their bright halves, the palette, exact colour, and the four attributes a
    /// <see cref="TermColor"/> carries.
    /// </summary>
    /// <param name="paint">What was in force.</param>
    /// <param name="parameters">The parameters between the bracket and the <c>m</c>.</param>
    /// <returns>What is in force afterwards.</returns>
    private static Paint Repainted(Paint paint, string parameters)
    {
        var codes = parameters.Length == 0
            ? [0]
            : parameters.Split(';').Select(static code => int.TryParse(code, out var value) ? value : 0).ToArray();

        for (var at = 0; at < codes.Length; at++)
        {
            switch (codes[at])
            {
                case 0: paint = Paint.Plain; break;
                case 1: paint = paint with { Bold = true }; break;
                case 2: paint = paint with { Dim = true }; break;
                case 3: paint = paint with { Italic = true }; break;
                case 4: paint = paint with { Underline = true }; break;
                case 22: paint = paint with { Bold = false, Dim = false }; break;
                case 23: paint = paint with { Italic = false }; break;
                case 24: paint = paint with { Underline = false }; break;
                case 39: paint = paint with { Foreground = Paint.Default }; break;
                case 49: paint = paint with { Background = Paint.Default }; break;
                case >= 30 and <= 37: paint = paint with { Foreground = Indexed(codes[at] - 30) }; break;
                case >= 40 and <= 47: paint = paint with { Background = Indexed(codes[at] - 40) }; break;
                case >= 90 and <= 97: paint = paint with { Foreground = Indexed(codes[at] - 82) }; break;
                case >= 100 and <= 107: paint = paint with { Background = Indexed(codes[at] - 92) }; break;
                case 38: paint = paint with { Foreground = Extended(codes, ref at) }; break;
                case 48: paint = paint with { Background = Extended(codes, ref at) }; break;
            }
        }

        return paint;
    }

    private static string Indexed(int index) => index.ToString(CultureInfo.InvariantCulture);

    private static string Extended(int[] codes, ref int at)
    {
        if (at + 2 < codes.Length && codes[at + 1] == 5)
        {
            var index = codes[at + 2];
            at += 2;

            return Indexed(index);
        }

        if (at + 4 < codes.Length && codes[at + 1] == 2)
        {
            var colour = $"{codes[at + 2]},{codes[at + 3]},{codes[at + 4]}";
            at += 4;

            return colour;
        }

        at = codes.Length;

        return Paint.Default;
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

            differences.Add($"row {row}  grid: {Program.Escaped(ours)}");
            differences.Add($"          tmux: {Program.Escaped(theirs)}");
        }

        for (var row = 0; row < drawn.Paints.Length; row++)
        {
            for (var column = 0; column < drawn.Paints[row].Length; column++)
            {
                var ours = drawn.Paints[row][column];
                var theirs = played.Paints[row][column];

                if (ours == theirs)
                {
                    continue;
                }

                differences.Add($"paint at {row},{column}  grid: {ours}");
                differences.Add($"                  tmux: {theirs}");
            }
        }

        if (drawn.CursorRow != played.CursorRow || drawn.CursorColumn != played.CursorColumn)
        {
            differences.Add(
                $"cursor  grid: {drawn.CursorRow},{drawn.CursorColumn}  tmux: {played.CursorRow},{played.CursorColumn}");
        }

        return differences;
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

        yield return new Frames("styles-attributes", 20, 4, [
            static surface =>
            {
                surface.WriteAt(0, 0, "bold", new TermColor { Style = TextStyle.Bold, Foreground = TerminalColor.Red });
                surface.WriteAt(1, 0, "dim italic", new TermColor
                {
                    Style = TextStyle.Dim | TextStyle.Italic,
                    ExactForeground = new(10, 200, 30),
                });
                surface.WriteAt(2, 0, "underlined", new TermColor
                {
                    Style = TextStyle.Underline,
                    ExactForeground = new(255, 255, 255),
                    ExactBackground = new(0, 0, 128),
                });
                surface.WriteAt(3, 0, "all four", new TermColor
                {
                    Style = TextStyle.Bold | TextStyle.Dim | TextStyle.Italic | TextStyle.Underline,
                    Foreground = TerminalColor.BrightYellow,
                    Background = TerminalColor.Blue,
                });
            },
        ]);

        yield return new Frames("styles-palette", 20, 3, [
            static surface =>
            {
                surface.WriteAt(0, 0, "plain sixteen", new TermColor
                {
                    Foreground = TerminalColor.Cyan,
                    Background = TerminalColor.Black,
                });
                surface.WriteAt(1, 0, "bright", new TermColor
                {
                    Foreground = TerminalColor.BrightMagenta,
                    Background = TerminalColor.BrightBlack,
                });
                surface.WriteAt(2, 0, "exact falls back", new TermColor
                {
                    Foreground = TerminalColor.Green,
                    ExactForeground = new(1, 2, 3),
                });
            },
        ]) { Colour = ColorSupport.Palette };

        yield return new Frames("styles-none", 20, 3, [
            static surface =>
            {
                surface.WriteAt(0, 0, "no colour at all", Theme.Error);
                surface.WriteAt(1, 0, "none here either", Theme.Selected);
            },
        ]) { Colour = ColorSupport.None };

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

        yield return new Frames("passthrough", 20, 3, [
            static surface =>
            {
                surface.WriteAt(0, 0, "under the picture", Theme.Default);
                surface.Passthrough(1, 2, "\e_Gf=100,a=T;AAAABBBBCCCC\e\\");
            },
        ]);

        yield return new Raw("raw-passthrough-osc", 20, 3, "text\e]1337;File=inline=1:AAAA\amore");

        yield return new Raw("raw-alt-screen", 12, 3, "first\r\nsecond\e[?1049h\e[1;1Hover here");

        yield return new Raw("raw-alt-screen-back", 12, 3, "first\r\nsecond\e[?1049h\e[1;1Hover here\e[?1049l");
    }

    /// <summary>Something to write to a terminal, and the screen the emulator says it leaves.</summary>
    /// <param name="Name">What it is called on the command line.</param>
    /// <param name="Width">Columns the screen is.</param>
    /// <param name="Height">Rows the screen is.</param>
    private abstract record Scenario(string Name, int Width, int Height)
    {
        /// <summary>
        /// How much colour the terminal is told it can do. The sequences a style writes depend on it,
        /// and so does whether the emulator and tmux can be made to disagree about them.
        /// </summary>
        internal ColorSupport Colour { get; init; } = ColorSupport.TrueColor;

        internal abstract Drawn Draw();

        private protected static Drawn Read(string output, ScreenGrid screen)
        {
            var paints = new Paint[screen.Height][];

            for (var row = 0; row < screen.Height; row++)
            {
                paints[row] = new Paint[screen.Width];

                for (var column = 0; column < screen.Width; column++)
                {
                    paints[row][column] = Painted(screen.StyleAt(row, column));
                }
            }

            return new(output, screen.Lines(), paints, screen.CursorRow, screen.CursorColumn);
        }
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
            var previous = TerminalCapabilities.Color;
            TerminalCapabilities.Color = Colour;

            try
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
            finally
            {
                TerminalCapabilities.Color = previous;
            }
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
    /// <param name="Paints">The colour of every cell.</param>
    /// <param name="CursorRow">Where the cursor ended up.</param>
    /// <param name="CursorColumn">Where the cursor ended up.</param>
    private sealed record Drawn(string Output, string[] Lines, Paint[][] Paints, int CursorRow, int CursorColumn);

    /// <summary>The screen tmux ended up with.</summary>
    /// <param name="Lines">The screen, one string per row.</param>
    /// <param name="Paints">The colour of every cell.</param>
    /// <param name="CursorRow">Where the cursor ended up.</param>
    /// <param name="CursorColumn">Where the cursor ended up.</param>
    private sealed record Played(string[] Lines, Paint[][] Paints, int CursorRow, int CursorColumn);

    /// <summary>
    /// A colour boiled down to what it means rather than how it was spelled, so that the frame's way of
    /// asking for it and tmux's way of handing it back can be held against one another.
    /// </summary>
    /// <param name="Foreground">The colour of the symbol: <c>default</c>, a palette index, or <c>r,g,b</c>.</param>
    /// <param name="Background">The colour behind it, spelled the same way.</param>
    /// <param name="Bold">Whether it is bold.</param>
    /// <param name="Dim">Whether it is dim.</param>
    /// <param name="Italic">Whether it is italic.</param>
    /// <param name="Underline">Whether it is underlined.</param>
    private readonly record struct Paint(
        string Foreground,
        string Background,
        bool Bold,
        bool Dim,
        bool Italic,
        bool Underline)
    {
        internal const string Default = "default";

        internal static Paint Plain => new(Default, Default, false, false, false, false);

        public override string ToString()
        {
            var attributes = string.Concat(Bold ? " bold" : "", Dim ? " dim" : "", Italic ? " italic" : "",
                Underline ? " underline" : "");

            return $"on {Background} in {Foreground}{attributes}";
        }
    }
}
