using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Arlecchino.Tools;

/// <summary>
/// Holds what the input reader understands against what a real terminal actually sends. The escape
/// sequences the tests feed it were written by the same head that wrote the reader, so a key spelled
/// wrong in both places reads correctly in every test and does nothing at all in a terminal. Here the
/// spelling comes from tmux: a key is pressed in a pane, the bytes it produced are read off the pty,
/// and those bytes — nobody's guess — are what the reader is asked to make sense of.
///
/// What tmux sends is tmux's answer, not every terminal's. It is still an answer nobody here wrote.
/// </summary>
internal static class Keys
{
    private const string Socket = "arlecchino-keys";

    /// <summary>Presses each key in a real pane and reports what the reader made of it.</summary>
    /// <param name="args">A name, or part of one, to try a single key; <c>--help</c> to explain itself.</param>
    /// <returns>Zero when every key read back as the key that was pressed.</returns>
    internal static int Run(string[] args)
    {
        if (args.Contains("--help"))
        {
            return Explain();
        }

        if (args is ["--listen", var into, ..])
        {
            return Listening(into);
        }

        if (args is ["--decode", var captured, ..])
        {
            return Decode(captured);
        }

        if (Which("tmux") is "")
        {
            Console.WriteLine("tmux is not on PATH; nothing to press keys with");

            return 2;
        }

        var wanted = args.FirstOrDefault(static argument => !argument.StartsWith('-')) ?? "";
        var presses = Presses()
            .Where(press => wanted.Length == 0 || press.Send.Contains(wanted, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (presses.Count == 0)
        {
            Console.WriteLine($"no key matches '{wanted}'");

            return 1;
        }

        var folder = Directory.CreateTempSubdirectory("arlecchino-keys");

        try
        {
            var sent = Bytes(folder.FullName, presses);
            var handed = Handed(folder.FullName, presses);

            Tmux("kill-server");

            using var reader = new Reading();
            var wrong = 0;
            var doubles = 0;

            foreach (var press in presses)
            {
                var bytes = sent[press.Send];
                var console = handed[press.Send];
                var read = reader.Read(console);
                var complaint = press.Disagrees(read);
                var fake = reader.ReadText(bytes);

                Console.WriteLine(complaint is ""
                    ? $"  ok    {press.Send,-10}  {Program.Escaped(bytes),-16}  {read}"
                    : $"  DIFF  {press.Send,-10}  {Program.Escaped(bytes),-16}  {read}   {complaint}");

                wrong += complaint is "" ? 0 : 1;

                if (fake.ToString() == read.ToString())
                {
                    continue;
                }

                doubles += press.Folded ? 0 : 1;
                Console.WriteLine(press.Folded
                    ? $"        {"",-10}  {"",-16}  a terminal sends this in two, and the fake says so: {fake}"
                    : $"        {"",-10}  {"",-16}  a test feeding those bytes sees {fake} instead");
            }

            Console.WriteLine();
            Console.WriteLine(wrong == 0
                ? $"{presses.Count} keys, every one read back as the key that was pressed"
                : $"{presses.Count} keys, {wrong} the reader disagrees about");

            if (doubles > 0)
            {
                Console.WriteLine(
                    $"{doubles} the fake terminal hands over differently than a real console does");
            }

            return wrong == 0 && doubles == 0 ? 0 : 1;
        }
        finally
        {
            folder.Delete(true);
        }
    }

    /// <summary>
    /// Presses every key into a pane running nothing but <c>cat</c> and collects the bytes. The pty is
    /// the thing being asked, so the fewer programs between the key and the bytes the better.
    /// </summary>
    /// <param name="folder">Where the pane writes.</param>
    /// <param name="presses">The keys to press.</param>
    /// <returns>The bytes each key produced.</returns>
    private static Dictionary<string, string> Bytes(string folder, IEnumerable<Press> presses)
    {
        var typed = Path.Combine(folder, "typed");

        Open("sh", "-c", $"stty raw -echo; cat -u > '{typed}'");
        Await(() => File.Exists(typed), "the pane never started listening");

        var bytes = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var press in presses)
        {
            bytes[press.Send] = Grown(typed, press.Send, static (stream, count) =>
            {
                var read = new byte[count];
                stream.ReadExactly(read);

                return Encoding.Latin1.GetString(read);
            });
        }

        return bytes;
    }

    /// <summary>
    /// Presses every key into a pane reading them the way an application does, and collects what the
    /// console handed over. This is the shape the reader actually meets: some sequences the runtime
    /// understands itself, the rest arrive a character at a time.
    /// </summary>
    /// <param name="folder">Where the listener writes.</param>
    /// <param name="presses">The keys to press.</param>
    /// <returns>The presses the console reported for each key.</returns>
    private static Dictionary<string, ConsoleKeyInfo[]> Handed(string folder, IEnumerable<Press> presses)
    {
        var heard = Path.Combine(folder, "heard");

        Open(Self(), "keys", "--listen", heard);
        Await(() => File.Exists(heard), "the listener never started");

        var handed = new Dictionary<string, ConsoleKeyInfo[]>(StringComparer.Ordinal);

        foreach (var press in presses)
        {
            handed[press.Send] = Grown(heard, press.Send, static (stream, count) =>
            {
                var read = new byte[count];
                stream.ReadExactly(read);

                return Parsed(Encoding.UTF8.GetString(read));
            });
        }

        return handed;
    }

    private static ConsoleKeyInfo[] Parsed(string lines) =>
    [
        .. lines
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(static line => line.Split('\t'))
            .Where(static parts => parts.Length == 3)
            .Select(static parts => new ConsoleKeyInfo(
                (char)int.Parse(parts[1], CultureInfo.InvariantCulture),
                (ConsoleKey)int.Parse(parts[0], CultureInfo.InvariantCulture),
                ((ConsoleModifiers)int.Parse(parts[2], CultureInfo.InvariantCulture) & ConsoleModifiers.Shift) != 0,
                ((ConsoleModifiers)int.Parse(parts[2], CultureInfo.InvariantCulture) & ConsoleModifiers.Alt) != 0,
                ((ConsoleModifiers)int.Parse(parts[2], CultureInfo.InvariantCulture) & ConsoleModifiers.Control) != 0)),
    ];

    /// <summary>Presses one key and reads whatever the pane wrote down because of it.</summary>
    /// <typeparam name="T">What the caller makes of the bytes that arrived.</typeparam>
    /// <param name="file">The file the pane appends to.</param>
    /// <param name="key">The key, named the way tmux names it.</param>
    /// <param name="read">Turns the bytes that arrived into an answer.</param>
    /// <returns>The answer, or what an empty stretch of bytes comes to.</returns>
    private static T Grown<T>(string file, string key, Func<FileStream, int, T> read)
    {
        var before = new FileInfo(file).Length;

        Tmux("send-keys", "-t", Socket, key);

        var settled = -1L;

        for (var attempt = 0; attempt < 200; attempt++)
        {
            Thread.Sleep(10);

            var now = new FileInfo(file).Length;

            if (now > before && now == settled)
            {
                break;
            }

            settled = now;
        }

        var length = new FileInfo(file).Length;

        using var stream = File.OpenRead(file);
        stream.Seek(before, SeekOrigin.Begin);

        return read(stream, (int)Math.Max(0, length - before));
    }

    private static string Self()
    {
        var path = Environment.ProcessPath ?? "";

        return Path.GetFileNameWithoutExtension(path) == "dotnet"
            ? Path.Combine(AppContext.BaseDirectory, "Arlecchino.Tools")
            : path;
    }

    private static void Open(params string[] command)
    {
        Tmux("kill-session", "-t", Socket);
        Tmux([
            "-f", "/dev/null",
            "new-session", "-d",
            "-s", Socket,
            "-x", "80",
            "-y", "10",
            "--",
            .. command,
        ]);
    }

    /// <summary>
    /// The other half of the tool, running inside the pane: reads keys the way an application does and
    /// writes down what it was handed. <c>Console.ReadKey</c> understands some sequences itself and
    /// passes the rest through a character at a time, and which is which is the whole question — a fake
    /// terminal that hands over the wrong shape makes every test that feeds it agree with nothing.
    /// </summary>
    /// <param name="into">The file each press is written to.</param>
    /// <returns>Zero, when it is killed.</returns>
    private static int Listening(string into)
    {
        File.WriteAllText(into, "");

        while (!Console.IsInputRedirected)
        {
            var key = Console.ReadKey(true);

            File.AppendAllText(
                into,
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{(int)key.Key}\t{(int)key.KeyChar}\t{(int)key.Modifiers}\n"));
        }

        return 0;
    }

    /// <summary>
    /// Reads a stretch of bytes captured from a terminal and says what an application would have been
    /// told. Some of what a terminal sends cannot be asked for on demand — a mouse report is made by a
    /// hand on a mouse — so the way to check those is to catch them once and hold the reader to them.
    /// </summary>
    /// <param name="captured">A file of bytes as they came off the pty.</param>
    /// <returns>Zero when the bytes said something.</returns>
    private static int Decode(string captured)
    {
        if (!File.Exists(captured))
        {
            Console.WriteLine($"nothing captured at {captured}");

            return 1;
        }

        var bytes = Encoding.Latin1.GetString(File.ReadAllBytes(captured));

        using var reader = new Reading();
        var heard = reader.ReadText(bytes);

        Console.WriteLine($"{bytes.Length} bytes: {Program.Escaped(bytes[..Math.Min(60, bytes.Length)])}...");
        Console.WriteLine();
        Console.WriteLine(heard.ToString());

        return heard.ToString() is "nothing" ? 1 : 0;
    }

    private static int Explain()
    {
        Console.WriteLine("usage: dotnet run --project tools/Arlecchino.Tools -- keys [name]");
        Console.WriteLine();
        Console.WriteLine("Presses each key in a tmux pane, reads the bytes it really produced off the");
        Console.WriteLine("pty, and feeds those to the reader an application listens through. The table");
        Console.WriteLine("it prints is the one worth keeping: key, the bytes a terminal sends for it,");
        Console.WriteLine("and what the reader made of them.");
        Console.WriteLine();
        Console.WriteLine("Pass a name, or part of one, to try a single key.");

        return 0;
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

    private static void Tmux(params string[] arguments)
    {
        var start = new ProcessStartInfo("tmux") { RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("-L");
        start.ArgumentList.Add(Socket);

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new InvalidOperationException("tmux would not start");

        process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();
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

    private static IEnumerable<Press> Presses()
    {
        yield return new("Up", ConsoleKey.UpArrow);
        yield return new("Down", ConsoleKey.DownArrow);
        yield return new("Left", ConsoleKey.LeftArrow);
        yield return new("Right", ConsoleKey.RightArrow);
        yield return new("Home", ConsoleKey.Home);
        yield return new("End", ConsoleKey.End);
        yield return new("PPage", ConsoleKey.PageUp);
        yield return new("NPage", ConsoleKey.PageDown);
        yield return new("IC", ConsoleKey.Insert);
        yield return new("DC", ConsoleKey.Delete);
        yield return new("BSpace", ConsoleKey.Backspace);
        yield return new("Tab", ConsoleKey.Tab);
        yield return new("BTab", ConsoleKey.Tab, ConsoleModifiers.Shift);
        yield return new("Enter", ConsoleKey.Enter);
        yield return new("Escape", ConsoleKey.Escape);

        for (var number = 1; number <= 12; number++)
        {
            yield return new($"F{number}", ConsoleKey.F1 + number - 1);
        }

        yield return new("S-Up", ConsoleKey.UpArrow, ConsoleModifiers.Shift);
        yield return new("C-Up", ConsoleKey.UpArrow, ConsoleModifiers.Control);
        yield return new("M-Up", ConsoleKey.UpArrow, ConsoleModifiers.Alt);
        yield return new("C-Left", ConsoleKey.LeftArrow, ConsoleModifiers.Control);
        yield return new("C-Right", ConsoleKey.RightArrow, ConsoleModifiers.Control);
        yield return new("C-Home", ConsoleKey.Home, ConsoleModifiers.Control);
        yield return new("C-End", ConsoleKey.End, ConsoleModifiers.Control);
        yield return new("S-F1", ConsoleKey.F1, ConsoleModifiers.Shift);

        yield return new("a", ConsoleKey.A, default, 'a');
        yield return new("Z", ConsoleKey.Z, ConsoleModifiers.Shift, 'Z');
        yield return new("Space", ConsoleKey.Spacebar, default, ' ');
        yield return new("C-a", ConsoleKey.A, ConsoleModifiers.Control);
        yield return new("M-a", ConsoleKey.A, ConsoleModifiers.Alt, 'a', true);
    }

    /// <summary>A key to press, and the key an application should be told about when it is.</summary>
    /// <param name="Send">The key, named the way tmux names it.</param>
    /// <param name="Key">The key the reader should report, or nothing when only the character matters.</param>
    /// <param name="Modifiers">The modifiers it should report.</param>
    /// <param name="Character">The character it should report, where the key is a typed one.</param>
    /// <param name="Folded">
    /// Whether a console folds this into one press where a terminal sends two. The fake terminal models
    /// the terminal on purpose — two presses in quick succession is the shape the reader has to time out
    /// on — so the two disagreeing here is the arrangement working, not a fault.
    /// </param>
    private sealed record Press(
        string Send,
        ConsoleKey Key,
        ConsoleModifiers Modifiers = default,
        char Character = '\0',
        bool Folded = false)
    {
        /// <summary>What is wrong with what the reader made of the press, if anything.</summary>
        /// <param name="read">What the reader reported.</param>
        /// <returns>The complaint, or an empty string when the two agree.</returns>
        internal string Disagrees(Heard read)
        {
            if (read.Keys.Count != 1)
            {
                return $"expected one key, got {read.Keys.Count}";
            }

            var got = read.Keys[0];
            var wanted = Character == '\0'
                ? $"{Key}{Suffix(Modifiers)}"
                : $"'{Character}'{Suffix(Modifiers)}";

            if (Key != default && got.Key != Key)
            {
                return $"expected {wanted}";
            }

            if (Character != '\0' && got.KeyChar != Character)
            {
                return $"expected {wanted}";
            }

            return got.Modifiers == Modifiers ? "" : $"expected {wanted}";
        }

        private static string Suffix(ConsoleModifiers modifiers) => modifiers == default ? "" : $"+{modifiers}";
    }

    /// <summary>What the reader made of some presses.</summary>
    /// <param name="Keys">The key presses it reported, in order.</param>
    /// <param name="Pastes">The blocks of pasted text it reported.</param>
    /// <param name="Mice">The mouse events it reported, spelled out.</param>
    private sealed record Heard(
        IReadOnlyList<ConsoleKeyInfo> Keys,
        IReadOnlyList<string> Pastes,
        IReadOnlyList<string> Mice)
    {
        public override string ToString()
        {
            var parts = Keys
                .Select(static key => key.Modifiers == default
                    ? Spell(key)
                    : $"{Spell(key)}+{key.Modifiers}")
                .Concat(Pastes.Select(static paste => $"paste {Program.Escaped(paste)}"))
                .Concat(Mice);

            return string.Join(", ", parts) is var text && text.Length > 0 ? text : "nothing";
        }

        private static string Spell(ConsoleKeyInfo key) =>
            key.Key != default ? key.Key.ToString() : $"'{key.KeyChar}'";
    }

    /// <summary>
    /// An application listening for keys, with everything that would take one before the screen does
    /// pointed at a key no terminal sends. The reader is the thing under test, and a key swallowed by
    /// the log overlay or the command palette on its way past would read as a reader that lost it.
    /// </summary>
    private sealed class Reading : IDisposable
    {
        private readonly ArlecchinoTestHost _host;
        private readonly Recorder _recorder = new();

        internal Reading()
        {
            _host = new(80, 10, builder =>
            {
                builder.Options.CommandPaletteKey = '§';
                builder
                    .UseKeymap(new()
                    {
                        ToggleLog = new(ConsoleKey.F24),
                        Notifications = new(ConsoleKey.F23),
                        Help = new(ConsoleKey.F22),
                        Back = new(ConsoleKey.F21),
                        Forward = new(ConsoleKey.F20),
                    })
                    .AddView("keys", _ => _recorder)
                    .StartAt("keys");
            });
        }

        internal Heard Read(IEnumerable<ConsoleKeyInfo> handed)
        {
            _recorder.Forget();

            foreach (var key in handed)
            {
                _host.Terminal.Enqueue(key);
            }

            _host.Services.GetRequiredService<TerminalInputReader>().ReadPending();
            _host.DrainInput();

            return new([.. _recorder.Keys], [.. _recorder.Pastes], [.. _recorder.Mice]);
        }

        internal Heard ReadText(string bytes)
        {
            _recorder.Forget();
            _host.ReadFromTerminal(bytes);

            return new([.. _recorder.Keys], [.. _recorder.Pastes], [.. _recorder.Mice]);
        }

        public void Dispose() => _host.Dispose();
    }

    private sealed class Recorder : IArlecchinoView
    {
        internal List<ConsoleKeyInfo> Keys { get; } = [];

        internal List<string> Pastes { get; } = [];

        internal List<string> Mice { get; } = [];

        internal void Forget()
        {
            Keys.Clear();
            Pastes.Clear();
            Mice.Clear();
        }

        public void Draw()
        {
        }

        public ViewRoute Handle(ConsoleKeyInfo key)
        {
            Keys.Add(key);

            return ViewRoute.None;
        }

        public ViewRoute HandlePaste(string text)
        {
            Pastes.Add(text);

            return ViewRoute.None;
        }

        public ViewRoute HandleMouse(MouseEvent mouse)
        {
            Mice.Add($"{mouse.Action} {mouse.Button} at {mouse.Row},{mouse.Column}");

            return ViewRoute.None;
        }
    }
}
