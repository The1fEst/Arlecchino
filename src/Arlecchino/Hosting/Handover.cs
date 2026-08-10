using System;
using System.Diagnostics;
using System.Threading;

namespace Arlecchino.Hosting;

/// <summary>
/// Lends the terminal to another program and takes it back afterward. An editor, a pager, a shell — a
/// full-screen application of its own cannot share a terminal with this one, so the only way to run it
/// is to stop being a terminal application for as long as it lasts.
///
/// Four things have to happen before the other program writes a byte, and they have to happen in this
/// order. The thread reading keys is parked, or it would eat what is typed into whatever now owns the
/// screen. The modes are given back, so the other program finds the terminal as its own shell would have
/// left it. Only then does it run. And when it ends, whatever it left on the screen is thrown away and
/// the next frame is drawn whole, because the surface only knows what it drew itself and would otherwise
/// patch a picture that is no longer there.
///
/// It is called on the drawing thread, and it blocks that thread — which is the point. Nothing is drawn
/// while somebody else has the screen, and a frame that slipped out in the middle would land on top of
/// the other program.
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
    /// Runs a program with the terminal to itself and waits for it to end. What it writes goes straight
    /// to the terminal and what is typed goes straight to it: none of the three streams is redirected,
    /// since a program that is being given the screen is being given the keyboard with it.
    /// </summary>
    /// <param name="start">The program and its arguments.</param>
    /// <returns>What it exited with.</returns>
    /// <exception cref="InvalidOperationException">Nothing could be started from what was asked for.</exception>
    public int Run(ProcessStartInfo start)
    {
        ArgumentNullException.ThrowIfNull(start);

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
    /// Hands the terminal over for the length of a call and takes it back however that call ends. An
    /// error thrown by the work reaches the caller with the screen already restored, so a program that
    /// could not be started is a message on the output row rather than a terminal nobody can type in.
    ///
    /// What is taken back is what was in force to begin with. An application drawing inline rather than
    /// on the alternate screen, or one running without the mouse, is handed back the terminal it had
    /// rather than the one this would have asked for.
    /// </summary>
    /// <param name="work">What to do while the terminal is somebody else's.</param>
    /// <exception cref="InvalidOperationException">Called from off the drawing thread.</exception>
    public void Give(Action work)
    {
        ArgumentNullException.ThrowIfNull(work);
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
    /// Whether the thread that reads the terminal may read it. Asked before every read: while the
    /// terminal is somebody else's the reader parks instead, and saying so is what lets
    /// <see cref="Give"/> know that nothing else is competing for the keyboard.
    /// </summary>
    /// <returns><c>true</c> when the terminal is ours to read.</returns>
    internal bool MayRead()
    {
        Volatile.Write(ref _reading, IsAway ? 0 : 1);

        return !IsAway;
    }

    /// <summary>
    /// Waits until the reader has answered that it is not reading. There is a deadline on it because a
    /// reader that never answers must not take the application with it: an application whose keys nobody
    /// reads can still be quit, and an application stuck in here cannot.
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
    /// Throws away what the terminal has waiting. Type-ahead meant for the program that just ended, and
    /// whatever it left behind of its own — the reply to a query it made, half a mouse report — would
    /// otherwise arrive here as keys nobody pressed.
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
