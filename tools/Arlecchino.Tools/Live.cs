using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

namespace Arlecchino.Tools;

/// <summary>
/// Runs a whole application in a real terminal and holds it to what a terminal application owes the
/// person who started it: take the screen, draw on it, and give it back exactly as it was found.
///
/// Nothing else here can answer that. The fake terminal records taking the screen as a flag being set,
/// which is true whether or not the sequence that does it was ever written, written correctly, or
/// written back on the way out. A pane knows: tmux says whether the alternate screen is in force,
/// whether the cursor is showing and whether the mouse was asked for, and the shell underneath is
/// still there to be compared against afterwards.
/// </summary>
internal static class Live
{
    private const string Socket = "arlecchino-live";
    private const string Marker = "before-the-application-started";

    /// <summary>Starts the application in a pane, drives it, and reports what the terminal was left like.</summary>
    /// <param name="args">
    /// <c>--app</c> to run something other than the sample, <c>--keys</c> for keys to press once it is
    /// up, <c>--size</c> for the pane, <c>--quit</c> for what closes it; <c>--help</c> to explain itself.
    /// </param>
    /// <returns>Zero when the application drew a screen and gave the terminal back.</returns>
    internal static int Run(string[] args)
    {
        if (args.Contains("--help"))
        {
            return Explain();
        }

        if (Which("tmux") is "")
        {
            Console.WriteLine("tmux is not on PATH; nothing to run the application in");

            return 2;
        }

        var size = (Option(args, "--size") ?? "100x30").Split('x');
        var quit = Option(args, "--quit") ?? "C-c";
        var keys = (Option(args, "--keys") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var app = Option(args, "--app") ?? Sample();

        if (!File.Exists(app))
        {
            Console.WriteLine($"nothing to run at {app}");

            return 1;
        }

        Console.WriteLine($"running {app}");
        Console.WriteLine();

        var complaints = new List<string>();

        Shell(size[0], size[1]);
        Say($"echo {Marker}");

        var before = Screen();

        if (!before.Contains(Marker, StringComparison.Ordinal))
        {
            Console.WriteLine("the shell never printed anything, so there is nothing to compare against");

            return 1;
        }

        Say(app);

        if (!Until(() => State().Alternate == 1))
        {
            Console.WriteLine(Framed("what the application drew", Screen()));
            Console.WriteLine("it never took a screen of its own");

            Tmux("kill-server");

            return 1;
        }

        Settle();

        var running = State();
        var drawn = Screen();

        Console.WriteLine(Framed("what the application drew", drawn));

        if (running.Alternate != 1)
        {
            complaints.Add("it never took a screen of its own, so quitting will leave its frames behind");
        }

        if (drawn.Trim().Length == 0)
        {
            complaints.Add("it took the screen and drew nothing on it");
        }

        if (drawn.Contains(Marker, StringComparison.Ordinal))
        {
            complaints.Add("the shell underneath is still showing through what it drew");
        }

        foreach (var key in keys)
        {
            Tmux("send-keys", key);
            Settle();

            Console.WriteLine(Framed($"after {key}", Screen()));
        }

        Tmux("send-keys", quit);
        Until(() => State().Alternate == 0);
        Settle();

        var after = State();
        var back = Screen();

        if (after.Alternate != 0)
        {
            complaints.Add("it kept the screen it took");
        }

        if (running.Cursor == 0 && after.Cursor != 1)
        {
            complaints.Add("it hid the cursor and left it hidden");
        }

        if (running.Mouse == 1 && after.Mouse != 0)
        {
            complaints.Add("it asked for the mouse and never gave it back");
        }

        if (!back.Contains(Marker, StringComparison.Ordinal))
        {
            complaints.Add("what was on screen before it started did not come back");
        }

        Tmux("kill-server");

        Console.WriteLine($"while running: {running}");
        Console.WriteLine($"after quitting: {after}");
        Console.WriteLine();

        if (complaints.Count == 0)
        {
            Console.WriteLine("it took the screen, drew on it, and gave it back as it was found");

            return 0;
        }

        foreach (var complaint in complaints)
        {
            Console.WriteLine($"  {complaint}");
        }

        return 1;
    }

    private static int Explain()
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- live [options]");
        Console.WriteLine();
        Console.WriteLine("Starts an application in a tmux pane from a live shell, presses whatever keys");
        Console.WriteLine("it was given, quits, and holds the terminal to what it was before: the screen");
        Console.WriteLine("given back, the cursor showing, the mouse released, the shell where it was.");
        Console.WriteLine();
        Console.WriteLine("  --app <path>     what to run; the sample when not said");
        Console.WriteLine("  --keys \"Down Tab\"  keys to press once it is up, tmux's names, space separated");
        Console.WriteLine("  --size 100x30    how big the pane is");
        Console.WriteLine("  --quit C-c       what closes it");

        return 0;
    }

    /// <summary>
    /// The sample, built if it has not been. It is run as the binary rather than through the build tool
    /// because a build printing into the pane is a build printing over the very screen being read.
    /// </summary>
    /// <returns>The path of the built sample.</returns>
    private static string Sample()
    {
        var project = Path.Combine(Program.Root(), "samples", "Arlecchino.Sample");
        var built = Path.Combine(project, "bin", "Release", "net10.0", "Arlecchino.Sample");

        if (File.Exists(built))
        {
            return built;
        }

        Console.WriteLine("building the sample");
        Dotnet("build", project, "--configuration", "Release");

        return built;
    }

    /// <summary>
    /// Opens a pane running a shell rather than the application itself. The point of the exercise is
    /// what the terminal looks like once the application has gone, and an application that owns the
    /// pane takes the pane with it when it leaves.
    /// </summary>
    /// <param name="width">Columns.</param>
    /// <param name="height">Rows.</param>
    private static void Shell(string width, string height)
    {
        Tmux("kill-session", "-t", Socket);
        Tmux(
            "-f",
            "/dev/null",
            "new-session",
            "-d",
            "-s",
            Socket,
            "-x",
            width,
            "-y",
            height,
            "-e",
            $"DOTNET_ROOT={Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? Runtime()}",
            "--",
            "sh",
            "-i");

        Settle();
    }

    private static string Runtime() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet");

    private static void Say(string line)
    {
        Tmux("send-keys", line, "Enter");
        Settle();
    }

    private static string Screen() => Tmux("capture-pane", "-p");

    private static Terminal State()
    {
        var told = Tmux(
                "display-message",
                "-p",
                "#{alternate_on} #{cursor_flag} #{mouse_any_flag}")
            .Trim()
            .Split(' ');

        return new(Number(told, 0), Number(told, 1), Number(told, 2));
    }

    private static int Number(string[] told, int at) =>
        at < told.Length && int.TryParse(told[at], CultureInfo.InvariantCulture, out var value) ? value : -1;

    /// <summary>
    /// Waits for the terminal to say something is so, or gives up. Everything here happens at the pace
    /// of a process starting and a frame being drawn, which on a busy machine is not the pace anything
    /// here can guess at.
    /// </summary>
    /// <param name="said">What is being waited for.</param>
    /// <returns><c>true</c> when it became so.</returns>
    private static bool Until(Func<bool> said)
    {
        for (var attempt = 0; attempt < 300; attempt++)
        {
            if (said())
            {
                return true;
            }

            Thread.Sleep(100);
        }

        return false;
    }

    /// <summary>
    /// Waits for the pane to stop changing. An application draws when it has something to draw and not
    /// on a clock, so there is nothing to wait a fixed time for — only for it to have finished.
    /// </summary>
    private static void Settle()
    {
        var last = "";

        for (var attempt = 0; attempt < 60; attempt++)
        {
            Thread.Sleep(50);

            var now = Screen();

            if (attempt > 0 && now == last)
            {
                return;
            }

            last = now;
        }
    }

    private static string Framed(string title, string screen)
    {
        var lines = screen.TrimEnd().Split('\n');
        var rule = new string('─', 100);

        return $"  {title}{Environment.NewLine}  {rule}{Environment.NewLine}" + string.Join(Environment.NewLine, lines.Select(static line => $"  {line}")) + $"{Environment.NewLine}  {rule}{Environment.NewLine}";
    }

    private static string? Option(string[] args, string name)
    {
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (args[index] == name)
            {
                return args[index + 1];
            }
        }

        return null;
    }

    private static void Dotnet(params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true };

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("dotnet would not start");

        process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();
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

    /// <summary>What the terminal says about itself.</summary>
    /// <param name="Alternate">Whether a screen of its own is in force.</param>
    /// <param name="Cursor">Whether the cursor is showing.</param>
    /// <param name="Mouse">Whether the mouse was asked for.</param>
    private readonly record struct Terminal(int Alternate, int Cursor, int Mouse)
    {
        public override string ToString() =>
            $"own screen {Yes(Alternate)}, cursor {Yes(Cursor)}, mouse {Yes(Mouse)}";

        private static string Yes(int flag) => flag switch
        {
            1 => "yes",
            0 => "no",
            _ => "unsaid",
        };
    }
}
