using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Arlecchino.Hosting;

namespace Arlecchino.Input;

/// <summary>
/// Turns what the terminal reports into keys and mouse events. Terminals send arrows, function keys
/// and mouse reports as escape sequences, so an escape has to be read together with what follows it.
/// Anything that turns out not to be a sequence is replayed key by key, which is what makes a plain
/// Escape work even though it starts the same way.
///
/// The rest of a sequence does not always arrive with its escape — over ssh or a busy terminal it can
/// land a few milliseconds later — so the reader waits a short while for it. That wait is also what a
/// lone Escape costs, which is the trade every terminal editor makes.
/// </summary>
public sealed class TerminalInputReader
{
    private const int LongestSequence = 32;
    private const string PasteStart = "200~";
    private const string PasteEnd = "\e[201~";
    private const int PollInterval = 1;

    private readonly IArlecchinoTerminal _terminal;
    private readonly InputRouter _router;
    private readonly ArlecchinoOptions _options;
    private readonly PendingInput? _pending;
    private readonly StringBuilder _sequence = new();

    /// <summary>
    /// Creates the reader. Everything it reads is routed as it is read, which is what a caller driving
    /// the reader itself wants — inside the framework it is built with a queue instead, so that the
    /// thread reading the terminal never touches what the frame loop is drawing.
    /// </summary>
    /// <param name="terminal">Where key presses come from.</param>
    /// <param name="router">Where the result is sent.</param>
    /// <param name="options">Supplies how long to wait for the rest of a sequence.</param>
    public TerminalInputReader(IArlecchinoTerminal terminal, InputRouter router, ArlecchinoOptions options)
        : this(terminal, router, options, null) { }

    internal TerminalInputReader(
        IArlecchinoTerminal terminal,
        InputRouter router,
        ArlecchinoOptions options,
        PendingInput? pending)
    {
        _terminal = terminal;
        _router = router;
        _options = options;
        _pending = pending;
    }

    private void Send(ConsoleKeyInfo key)
    {
        if (_pending is not null)
        {
            _pending.Add(key);
            return;
        }

        _router.ProcessKey(key);
    }

    private void Send(MouseEvent mouse)
    {
        if (_pending is not null)
        {
            _pending.Add(mouse);
            return;
        }

        _router.ProcessMouse(mouse);
    }

    private void SendPaste(string text)
    {
        if (_pending is not null)
        {
            _pending.AddPaste(text);
            return;
        }

        _router.ProcessPaste(text);
    }

    /// <summary>
    /// Reads everything waiting and returns, without blocking for more. Mouse events are drained too,
    /// since a terminal that reports them outside the key stream would otherwise pile them up.
    /// </summary>
    public void ReadPending()
    {
        while (_terminal.KeyAvailable || _terminal.MouseAvailable)
        {
            while (_terminal.MouseAvailable)
            {
                Send(_terminal.ReadMouse());
            }

            if (_terminal.KeyAvailable)
            {
                Read(_terminal.ReadKey());
            }
        }
    }

    /// <summary>
    /// Handles one key press, reading further keys itself when it looks like the start of a sequence.
    ///
    /// An escape followed by another escape is <c>Alt+Escape</c>: holding Alt puts an escape in front
    /// of the key, and the key here is itself an escape. The runtime folds that prefix back together
    /// for every other key — <c>\ea</c> arrives as <c>Alt+A</c> — but not for this one, which reached
    /// an application as two plain Escapes and left <c>Alt+Esc</c> impossible to bind.
    /// </summary>
    /// <param name="key">The key that was read.</param>
    public void Read(ConsoleKeyInfo key)
    {
        if (Nothing(key))
        {
            return;
        }

        if (key.Key != ConsoleKey.Escape || !WaitForKey())
        {
            Send(key);
            return;
        }

        var introducer = _terminal.ReadKey();

        if (introducer.Key == ConsoleKey.Escape)
        {
            Send(new ConsoleKeyInfo('\e', ConsoleKey.Escape, false, true, false));
            return;
        }

        if (introducer.KeyChar is not ('[' or 'O'))
        {
            Send(key);
            Send(introducer);
            return;
        }

        ReadSequenceBody(key, introducer);
    }

    private void ReadSequenceBody(ConsoleKeyInfo escape, ConsoleKeyInfo introducer)
    {
        _sequence.Clear();

        while (_sequence.Length < LongestSequence)
        {
            if (!WaitForKey())
            {
                Replay(escape, introducer);
                return;
            }

            var next = _terminal.ReadKey();
            _sequence.Append(next.KeyChar);

            if (!IsFinalByte(next.KeyChar))
            {
                continue;
            }

            Dispatch(_sequence.ToString(), escape, introducer);
            return;
        }

        Replay(escape, introducer);
    }

    private void Dispatch(string sequence, ConsoleKeyInfo escape, ConsoleKeyInfo introducer)
    {
        if (sequence == PasteStart)
        {
            ReadPaste();
            return;
        }

        if (EscapeSequenceParser.TryParseMouse(sequence, out var mouse))
        {
            Send(mouse);
            return;
        }

        if (EscapeSequenceParser.TryParseKey(sequence, out var key))
        {
            Send(key);
            return;
        }

        Replay(escape, introducer);
    }

    private void Replay(ConsoleKeyInfo escape, ConsoleKeyInfo introducer)
    {
        Send(escape);
        Send(introducer);

        foreach (var character in _sequence.ToString())
        {
            Send(new ConsoleKeyInfo(character, default, false, false, false));
        }

        _sequence.Clear();
    }

    /// <summary>
    /// Whether the key is the one a terminal hands back when there was nothing to read. A console read
    /// that fails answers <c>default</c>, which is indistinguishable from a key press of NUL with no
    /// modifiers — and a view that is handed that has no way to tell either. Nobody presses it, so it is
    /// dropped here rather than routed as input.
    /// </summary>
    /// <param name="key">The key that was read.</param>
    /// <returns><c>true</c> when nothing was actually pressed.</returns>
    private static bool Nothing(ConsoleKeyInfo key) =>
        key.Key == ConsoleKey.None && key.KeyChar == '\0' && key.Modifiers == 0;

    /// <summary>
    /// Waits for the next key of a sequence. A key already waiting returns at once; otherwise the
    /// terminal is given the configured grace period before the sequence is given up on.
    /// </summary>
    /// <returns><c>true</c> when a key arrived before the grace period ran out.</returns>
    private bool WaitForKey()
    {
        if (_terminal.KeyAvailable)
        {
            return true;
        }

        var deadline = Stopwatch.GetTimestamp() + (long)(_options.EscapeTimeout.TotalSeconds * Stopwatch.Frequency);

        while (Stopwatch.GetTimestamp() < deadline)
        {
            Thread.Sleep(PollInterval);

            if (_terminal.KeyAvailable)
            {
                return true;
            }
        }

        return false;
    }

    private void ReadPaste()
    {
        var pasted = new StringBuilder();

        while (WaitForKey())
        {
            pasted.Append(_terminal.ReadKey().KeyChar);

            if (!EndsPaste(pasted))
            {
                continue;
            }

            pasted.Length -= PasteEnd.Length;
            SendPaste(pasted.ToString());
            return;
        }

        SendPaste(pasted.ToString());
    }

    private static bool EndsPaste(StringBuilder pasted)
    {
        if (pasted.Length < PasteEnd.Length)
        {
            return false;
        }

        for (var i = 0; i < PasteEnd.Length; i++)
        {
            if (pasted[pasted.Length - PasteEnd.Length + i] != PasteEnd[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsFinalByte(char character) => character is >= '@' and <= '~' && character != '<';
}
