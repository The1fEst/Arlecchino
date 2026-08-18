using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Arlecchino.Hosting;

namespace Arlecchino.Input;

/// <summary>
/// Turns what the terminal reports into keys and mouse events, reading an escape together with what follows
/// it. Anything that turns out not to be a sequence is replayed key by key, after a short wait.
/// </summary>
public sealed class TerminalInputReader
{
    private const int LongestSequence = 32;
    private const int LongestReply = 128;
    private const string PasteStart = "200~";
    private const string PasteEnd = "\e[201~";
    private const int PollInterval = 1;

    private readonly IArlecchinoTerminal _terminal;
    private readonly InputRouter _router;
    private readonly ArlecchinoOptions _options;
    private readonly PendingInput? _pending;
    private readonly StringBuilder _sequence = new();

    /// <summary>
    /// Creates the reader, routing everything as it is read. Inside the framework it is built with a queue
    /// instead, so the reading thread never touches what the frame loop draws.
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

    private void Send(KeyPress key)
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
    /// Handles one key press, reading further keys itself where it looks like the start of a sequence. An
    /// escape followed by another escape is <c>Alt+Escape</c>, which the runtime does not fold back together.
    /// </summary>
    /// <param name="key">The key that was read.</param>
    public void Read(KeyPress key)
    {
        if (key.IsNothing)
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
            Send(new KeyPress(ConsoleKey.Escape, KeyModifiers.Alt, '\e'));
            return;
        }

        if (introducer.Character is ']' or '_' or 'P' or '^' or 'X')
        {
            ReadStringBody();
            return;
        }

        if (introducer.Character is not ('[' or 'O'))
        {
            Send(key);
            Send(introducer);
            return;
        }

        ReadSequenceBody(key, introducer);
    }

    /// <summary>
    /// Reads to the end of a string the terminal answered in and drops it. A terminal sends one only when
    /// asked, so none of it was typed.
    /// </summary>
    private void ReadStringBody()
    {
        var escaped = false;

        while (WaitForKey())
        {
            var next = _terminal.ReadKey();

            if (next.Character == '\a' || (escaped && next.Character == '\\'))
            {
                return;
            }

            escaped = next.Character == '\e';
        }
    }

    private void ReadSequenceBody(KeyPress escape, KeyPress introducer)
    {
        _sequence.Clear();

        while (_sequence.Length < Room())
        {
            if (!WaitForKey())
            {
                Replay(escape, introducer);
                return;
            }

            var next = _terminal.ReadKey();
            _sequence.Append(next.Character);

            if (!IsFinalByte(next.Character))
            {
                continue;
            }

            Dispatch(_sequence.ToString(), escape, introducer);
            return;
        }

        Replay(escape, introducer);
    }

    /// <summary>
    /// How long the sequence being read may run. A key takes a handful of characters; a terminal listing
    /// what it can do takes as many numbers as it has answers.
    /// </summary>
    /// <returns>The characters this sequence is allowed.</returns>
    private int Room() =>
        _sequence.Length > 0 && _sequence[0] is '?' or '>' or '=' ? LongestReply : LongestSequence;

    private void Dispatch(string sequence, KeyPress escape, KeyPress introducer)
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
            if (!key.IsNothing)
            {
                Send(key);
            }

            return;
        }

        if (IsReply(sequence))
        {
            return;
        }

        Replay(escape, introducer);
    }

    /// <summary>
    /// Whether the sequence is the terminal answering rather than the keyboard speaking. A private marker
    /// in front of the numbers says so, as does the letter a window report ends with.
    /// </summary>
    /// <param name="sequence">What followed the introducer, final byte included.</param>
    /// <returns><c>true</c> when the sequence is an answer rather than a key.</returns>
    private static bool IsReply(string sequence) =>
        sequence.Length > 0 && (sequence[0] is '?' or '>' or '=' || sequence[^1] == 't');

    private void Replay(KeyPress escape, KeyPress introducer)
    {
        Send(escape);
        Send(introducer);

        foreach (var character in _sequence.ToString())
        {
            Send(new KeyPress(character));
        }

        _sequence.Clear();
    }

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
        var pastedText = new StringBuilder();

        while (WaitForKey())
        {
            pastedText.Append(_terminal.ReadKey().Character);

            if (!EndsPaste(pastedText))
            {
                continue;
            }

            pastedText.Length -= PasteEnd.Length;
            SendPaste(pastedText.ToString());
            return;
        }

        SendPaste(pastedText.ToString());
    }

    private static bool EndsPaste(StringBuilder pastedText)
    {
        if (pastedText.Length < PasteEnd.Length)
        {
            return false;
        }

        for (var i = 0; i < PasteEnd.Length; i++)
        {
            if (pastedText[pastedText.Length - PasteEnd.Length + i] != PasteEnd[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsFinalByte(char character) => character is >= '@' and <= '~' && character != '<';
}
