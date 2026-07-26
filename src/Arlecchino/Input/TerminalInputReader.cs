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
    private readonly StringBuilder _sequence = new();

    /// <summary>Creates the reader.</summary>
    /// <param name="terminal">Where key presses come from.</param>
    /// <param name="router">Where the result is sent.</param>
    /// <param name="options">Supplies how long to wait for the rest of a sequence.</param>
    public TerminalInputReader(IArlecchinoTerminal terminal, InputRouter router, ArlecchinoOptions options)
    {
        _terminal = terminal;
        _router = router;
        _options = options;
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
                _router.ProcessMouse(_terminal.ReadMouse());
            }

            if (_terminal.KeyAvailable)
            {
                Read(_terminal.ReadKey());
            }
        }
    }

    /// <summary>
    /// Handles one key press, reading further keys itself when it looks like the start of a sequence.
    /// </summary>
    /// <param name="key">The key that was read.</param>
    public void Read(ConsoleKeyInfo key)
    {
        if (key.Key != ConsoleKey.Escape || !WaitForKey())
        {
            _router.ProcessKey(key);
            return;
        }

        var introducer = _terminal.ReadKey();
        if (introducer.KeyChar is not ('[' or 'O'))
        {
            _router.ProcessKey(key);
            _router.ProcessKey(introducer);
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
            _router.ProcessMouse(mouse);
            return;
        }

        if (EscapeSequenceParser.TryParseKey(sequence, out var key))
        {
            _router.ProcessKey(key);
            return;
        }

        Replay(escape, introducer);
    }

    private void Replay(ConsoleKeyInfo escape, ConsoleKeyInfo introducer)
    {
        _router.ProcessKey(escape);
        _router.ProcessKey(introducer);

        foreach (var character in _sequence.ToString())
        {
            _router.ProcessKey(new(character, default, false, false, false));
        }

        _sequence.Clear();
    }

    /// <summary>
    /// Waits for the next key of a sequence. A key already waiting returns at once; otherwise the
    /// terminal is given the configured grace period before the sequence is given up on.
    /// </summary>
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
            _router.ProcessPaste(pasted.ToString());
            return;
        }

        _router.ProcessPaste(pasted.ToString());
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
