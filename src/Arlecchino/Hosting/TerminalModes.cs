using System.Threading;

namespace Arlecchino.Hosting;

/// <summary>
/// The modes an application puts a terminal into and has to take it back out of: the alternate screen,
/// the mouse, bracketed paste. Two things ask for them. One is the service that runs the application for
/// as long as the host lives, and the other is the handover that lends the terminal to another program
/// for a while. They are written once here, and both of them call the same pair.
///
/// Leaving runs once however many times it is asked for. A terminal is given back from the loop
/// finishing, from a signal, and from process exit, and each of them can be the first. Writing the
/// leaving sequences twice would put a stray escape on the screen the shell is about to draw its prompt
/// on.
/// </summary>
internal sealed class TerminalModes
{
    private readonly IArlecchinoTerminal _terminal;
    private readonly ArlecchinoOptions _options;

    private int _left = 1;

    /// <summary>Takes the modes one terminal is put into.</summary>
    /// <param name="terminal">The terminal itself.</param>
    /// <param name="options">Which of the modes this application asked for.</param>
    public TerminalModes(IArlecchinoTerminal terminal, ArlecchinoOptions options)
    {
        _terminal = terminal;
        _options = options;
    }

    /// <summary>Whether the terminal is in the modes this application wants it in.</summary>
    public bool AreOn => Volatile.Read(ref _left) == 0;

    /// <summary>Takes the terminal over, in the order a terminal expects to be taken over in.</summary>
    public void Enter()
    {
        Interlocked.Exchange(ref _left, 0);

        if (_options.UseAlternateScreen)
        {
            _terminal.EnterFullScreen();
        }

        if (_options.MouseInput)
        {
            _terminal.EnableMouse();
        }

        if (_options.BracketedPaste)
        {
            _terminal.EnablePaste();
        }
    }

    /// <summary>Gives the terminal back the way it was found, and does nothing when it already has been.</summary>
    public void Leave()
    {
        if (Interlocked.Exchange(ref _left, 1) == 1)
        {
            return;
        }

        if (_options.MouseInput)
        {
            _terminal.DisableMouse();
        }

        if (_options.BracketedPaste)
        {
            _terminal.DisablePaste();
        }

        _terminal.LeaveFullScreen();
    }
}
