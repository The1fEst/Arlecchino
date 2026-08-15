using System;
using System.Diagnostics;
using System.Threading;

namespace Arlecchino.Hosting;

/// <summary>
/// Lends the terminal to a full-screen program of its own — an editor, a pager, a shell — and takes it back
/// afterward. It runs on the drawing thread and blocks it, so no frame lands on top of the other program.
/// </summary>
public sealed class Handover
{
    private static readonly string Giving = FrameMembers.Of<Handover>(nameof(Give));
    private static readonly TimeSpan Parking = TimeSpan.FromSeconds(2);

    private const int PollInterval = 1;

    private readonly IArlecchinoTerminal _terminal;
    private readonly TerminalModes _modes;
    private readonly Screen _screen;

    private int _away;
    private int _reading;

    /// <summary>Puts the handover over one terminal.</summary>
    /// <param name="terminal">The terminal being lent.</param>
    /// <param name="modes">What is given back before the other program runs, and taken again after.</param>
    /// <param name="screen">Asked for a whole frame once the terminal is ours again.</param>
    internal Handover(IArlecchinoTerminal terminal, TerminalModes modes, Screen screen)
    {
        _terminal = terminal;
        _modes = modes;
        _screen = screen;
    }

    /// <summary>Whether another program has the terminal at this moment.</summary>
    public bool IsAway => Volatile.Read(ref _away) == 1;

    /// <summary>
    /// Runs a program with the terminal to itself and waits for it to end. None of its three streams is
    /// redirected, so what it writes and what is typed into it go straight to the terminal.
    /// </summary>
    /// <param name="start">The program and its arguments.</param>
    /// <returns>What it exited with.</returns>
    /// <exception cref="InvalidOperationException">Nothing could be started from what was asked for.</exception>
    public int Run(ProcessStartInfo start)
    {
        start.UseShellExecute = false;
        start.RedirectStandardInput = false;
        start.RedirectStandardOutput = false;
        start.RedirectStandardError = false;

        var code = 0;

        Give(() =>
        {
            using var running = Process.Start(start) ??
                                throw new InvalidOperationException($"{start.FileName} did not start.");

            running.WaitForExit();

            code = running.ExitCode;
        });

        return code;
    }

    /// <summary>
    /// Hands the terminal over for the length of a call and takes it back however that call ends, error
    /// included. What is taken back is the terminal that was in force to begin with.
    /// </summary>
    /// <param name="work">What to do while the terminal belongs to the other program.</param>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public void Give(Action work)
    {
        FrameThread.Verify(Giving);

        var ours = _modes.AreOn;

        Volatile.Write(ref _away, 1);
        WaitForTheReader();
        _modes.Leave();

        try
        {
            work();
        }
        finally
        {
            Drop();

            if (ours)
            {
                _modes.Enter();
            }

            Volatile.Write(ref _away, 0);

            _screen.RedrawEverything();
        }
    }

    /// <summary>
    /// Whether the thread that reads the terminal may read it, asked before every read. Answering it is what
    /// tells <see cref="Give"/> that nothing else is competing for the keyboard.
    /// </summary>
    /// <returns><c>true</c> when the terminal is ours to read.</returns>
    internal bool MayRead()
    {
        Volatile.Write(ref _reading, IsAway ? 0 : 1);

        return !IsAway;
    }

    /// <summary>
    /// Waits until the reader has answered that it is not reading, and gives up after a deadline. An
    /// application whose keys go unread can still be quit; one stuck in here cannot.
    /// </summary>
    private void WaitForTheReader()
    {
        var started = Stopwatch.GetTimestamp();

        while (Volatile.Read(ref _reading) == 1 && Stopwatch.GetElapsedTime(started) < Parking)
        {
            Thread.Sleep(PollInterval);
        }
    }

    /// <summary>
    /// Throws away what the terminal has waiting. Type-ahead meant for the program that just ended, or half a
    /// reply it left behind, would otherwise arrive here as keys that were never pressed.
    /// </summary>
    private void Drop()
    {
        while (_terminal.KeyAvailable)
        {
            _terminal.ReadKey();
        }

        while (_terminal.MouseAvailable)
        {
            _terminal.ReadMouse();
        }
    }
}
