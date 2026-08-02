using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Arlecchino.Hosting;
using Arlecchino.Navigation;
using Arlecchino.Rendering.Terminals;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Arlecchino.Tools;

/// <summary>
/// The three things an application says to a terminal that nothing else here can check, because in a
/// test the terminal is a class that writes the answer down rather than one that answers.
///
/// Asking what the terminal can do rests on the reply to the last question arriving last; a fake that
/// replies on demand cannot get that wrong. Copying rests on a sequence a terminal has to accept, and
/// a fake accepts anything. Pasting rests on markers a terminal wraps the text in, and a fake wraps it
/// the way the test expects. Each is run here against tmux, which does none of those favours.
/// </summary>
internal static class Terminal
{
    private const string Socket = "arlecchino-terminal";
    private const string Copied = "what the application put on the clipboard";
    private const string Block = "two words";
    private const string Letters = "xyz";

    /// <summary>Runs the three checks and reports what the terminal made of each.</summary>
    /// <param name="args">A check to run on its own; <c>--help</c> to explain itself.</param>
    /// <returns>Zero when all three hold.</returns>
    internal static int Run(string[] args)
    {
        if (args.Contains("--help"))
        {
            return Explain();
        }

        if (args is ["--in-pane", var check, var into, ..])
        {
            return InPane(check, into);
        }

        if (Which("tmux") is "")
        {
            Console.WriteLine("tmux is not on PATH; nothing to talk to");

            return 2;
        }

        var wanted = args.FirstOrDefault(static argument => !argument.StartsWith('-')) ?? "";
        var checks = new (string Name, Func<string, string> Check)[]
        {
            ("probe", Probe),
            ("clipboard", Clipboard),
            ("paste", Paste),
        }.Where(one => wanted.Length == 0 || one.Name.Contains(wanted, StringComparison.Ordinal)).ToArray();

        if (checks.Length == 0)
        {
            Console.WriteLine($"no check matches '{wanted}'");

            return 1;
        }

        var folder = Directory.CreateTempSubdirectory("arlecchino-terminal");
        var wrong = 0;

        try
        {
            foreach (var (name, run) in checks)
            {
                var complaint = run(folder.FullName);

                Console.WriteLine(complaint is ""
                    ? $"  ok    {name}"
                    : $"  DIFF  {name}   {complaint}");

                wrong += complaint is "" ? 0 : 1;
            }
        }
        finally
        {
            Tmux("kill-server");
            folder.Delete(true);
        }

        Console.WriteLine();
        Console.WriteLine(wrong == 0
            ? $"{checks.Length} checks, the terminal and the application understand one another"
            : $"{checks.Length} checks, {wrong} they do not agree on");

        return wrong == 0 ? 0 : 1;
    }

    private static int Explain()
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- terminal [check]");
        Console.WriteLine();
        Console.WriteLine("Runs a real application in a tmux pane and checks the three things it says to");
        Console.WriteLine("a terminal that only a terminal can answer for:");
        Console.WriteLine();
        Console.WriteLine("  probe      it asks what the terminal can do, and does not swallow what was");
        Console.WriteLine("             typed while it waited for the answers");
        Console.WriteLine("  clipboard  what it copies reaches the terminal's own buffer intact");
        Console.WriteLine("  paste      text pasted in arrives as one block, markers stripped");

        return 0;
    }

    /// <summary>
    /// Asks what the terminal can do while somebody types. The answers matter less than the arrangement
    /// around them: the questions end with the one every terminal answers, and until that reply arrives
    /// nothing read can be assumed to be an answer rather than a keystroke. Type during the asking and
    /// the characters must still arrive.
    /// </summary>
    /// <param name="folder">Where the pane writes what it learned.</param>
    /// <returns>The complaint, or an empty string.</returns>
    private static string Probe(string folder)
    {
        var learned = Path.Combine(folder, "probe");

        Open("probe", learned);

        if (Wait($"{learned}.asking") is "")
        {
            return "the application never started asking";
        }

        Tmux("send-keys", Letters);

        var said = Wait(learned);

        if (said is "")
        {
            return "the application never finished asking";
        }

        if (!said.Contains("asked:yes", StringComparison.Ordinal))
        {
            return $"it did not get through the asking: {said}";
        }

        return said.Contains($"typed:{Letters}", StringComparison.Ordinal)
            ? ""
            : $"what was typed while it asked did not arrive whole: {said}";
    }

    /// <summary>
    /// Copies, and asks tmux what it received. A terminal only takes a copy it can parse, and tmux keeps
    /// what it took where it can be read back — which is the only way to know the sequence was framed
    /// and encoded properly rather than merely written.
    /// </summary>
    /// <param name="folder">Where the pane writes.</param>
    /// <returns>The complaint, or an empty string.</returns>
    private static string Clipboard(string folder)
    {
        var done = Path.Combine(folder, "clipboard");

        Open("clipboard", done, "set-clipboard on");

        if (Wait(done) is "")
        {
            return "the application never copied anything";
        }

        var buffer = Tmux("show-buffer").TrimEnd('\n');

        return buffer == Copied ? "" : $"the terminal's buffer holds {Program.Escaped(buffer)}";
    }

    /// <summary>
    /// Pastes through the terminal rather than by handing the application a string, and checks it
    /// arrives as one block with the markers taken off.
    /// </summary>
    /// <param name="folder">Where the pane writes.</param>
    /// <returns>The complaint, or an empty string.</returns>
    private static string Paste(string folder)
    {
        var heard = Path.Combine(folder, "paste");

        Open("paste", heard);

        if (Wait($"{heard}.ready") is "")
        {
            return "the application never started listening";
        }

        Tmux("set-buffer", Block);
        Tmux("paste-buffer", "-p");

        var said = Wait(heard);

        return said == $"pasted:{Block}"
            ? ""
            : $"the application heard {Program.Escaped(said)} instead of the block that was pasted";
    }

    /// <summary>
    /// The other half of each check, running inside the pane as a real application: it starts the
    /// framework the way an application does, writes down what happened, and leaves.
    /// </summary>
    /// <param name="check">Which of the three to run.</param>
    /// <param name="into">The file to write what happened to.</param>
    /// <returns>Zero.</returns>
    private static int InPane(string check, string into)
    {
        if (check == "clipboard")
        {
            new SystemTerminal().CopyToClipboard(Copied);
            File.WriteAllText(into, "copied");
            Thread.Sleep(TimeSpan.FromSeconds(20));

            return 0;
        }

        var asking = check == "probe";
        var recorder = new Recorder();
        var builder = Host.CreateApplicationBuilder();

        builder.Logging.ClearProviders();
        builder.Services
            .AddArlecchino(options =>
            {
                options.MinimumWidth = 1;
                options.MinimumHeight = 1;
                options.AskTerminal = asking;
                options.TerminalAnswer = TimeSpan.FromSeconds(2);
            })
            .AddView("probe", _ => recorder)
            .StartAt("probe");

        var host = builder.Build();

        File.WriteAllText($"{into}.asking", "asking");
        host.Start();
        File.WriteAllText($"{into}.ready", "ready");

        Until(() => asking ? recorder.Typed.Length >= Letters.Length : recorder.Pasted.Length > 0);

        File.WriteAllText(into, asking
            ? string.Create(
                CultureInfo.InvariantCulture,
                $"asked:yes sixel:{TerminalCapabilities.Sixel} kitty:{TerminalCapabilities.Kitty} " +
                $"cells:{TerminalCapabilities.CellSizeKnown} typed:{recorder.Typed}")
            : $"pasted:{recorder.Pasted}");

        host.StopAsync().GetAwaiter().GetResult();

        return 0;
    }

    private static void Open(string check, string into, string clipboard = "set-clipboard off")
    {
        Tmux("kill-session", "-t", Socket);
        Tmux(
            "-f", "/dev/null",
            "new-session", "-d",
            "-s", Socket,
            "-x", "80",
            "-y", "24",
            "-e", $"DOTNET_ROOT={Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? Runtime()}",
            "--",
            Self(), "terminal", "--in-pane", check, into);

        Tmux("set-option", "-s", clipboard.Split(' ')[0], clipboard.Split(' ')[1]);
    }

    private static void Until(Func<bool> done)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            if (done())
            {
                return;
            }

            Thread.Sleep(50);
        }
    }

    private static string Wait(string file)
    {
        for (var attempt = 0; attempt < 600; attempt++)
        {
            Thread.Sleep(50);

            if (File.Exists(file))
            {
                return File.ReadAllText(file).Trim();
            }
        }

        return "";
    }

    private static string Runtime() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet");

    private static string Self()
    {
        var path = Environment.ProcessPath ?? "";

        return Path.GetFileNameWithoutExtension(path) == "dotnet"
            ? Path.Combine(AppContext.BaseDirectory, "Arlecchino.Tools")
            : path;
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

    /// <summary>A screen that draws nothing and remembers what it was given.</summary>
    private sealed class Recorder : IArlecchinoView
    {
        private readonly List<char> _typed = [];

        internal string Typed => new([.. _typed]);

        internal string Pasted { get; private set; } = "";

        public void Draw()
        {
        }

        public ViewRoute Handle(ConsoleKeyInfo key)
        {
            if (!char.IsControl(key.KeyChar))
            {
                _typed.Add(key.KeyChar);
            }

            return ViewRoute.None;
        }

        public ViewRoute HandlePaste(string text)
        {
            Pasted = text;

            return ViewRoute.None;
        }
    }
}
