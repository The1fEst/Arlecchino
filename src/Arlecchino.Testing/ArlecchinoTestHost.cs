using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;
using Arlecchino.Atoms;

namespace Arlecchino.Testing;

/// <summary>
/// A whole application wired up for a test: real services, a terminal in memory, and no loop running
/// in the background. Frames are drawn when asked for rather than on a timer, so a test presses keys
/// and then looks at what would be on screen, with nothing to wait for and nothing to race against.
/// </summary>
public sealed class ArlecchinoTestHost : IDisposable
{
    private readonly ServiceProvider _provider;

    /// <summary>
    /// Builds the application. The minimum size is dropped to one cell, so a test can work in a window
    /// far smaller than a real one without hitting the too-small notice. Colour is fixed at
    /// <see cref="ColorSupport.TrueColor"/> so that frames do not change with the environment the test
    /// runs in — assign <see cref="TerminalCapabilities.Color"/> afterwards to test another level.
    /// </summary>
    /// <param name="width">Columns of the fake terminal.</param>
    /// <param name="height">Rows of the fake terminal.</param>
    /// <param name="configure">Registers the views, commands and services under test.</param>
    public ArlecchinoTestHost(int width = 80, int height = 24, Action<ArlecchinoBuilder>? configure = null)
    {
        TerminalCapabilities.Color = ColorSupport.TrueColor;

        var services = new ServiceCollection();

        Terminal = new(width, height);
        Clock = new();
        services.AddSingleton<IArlecchinoTerminal>(Terminal);
        services.AddSingleton<TimeProvider>(Clock);

        var builder = services
            .AddArlecchino(options =>
            {
                options.MinimumWidth = 1;
                options.MinimumHeight = 1;
                options.AskTerminal = false;
            })
            .WithoutHostedService();

        configure?.Invoke(builder);

        _provider = services.BuildServiceProvider();
        _ = History;
    }

    /// <summary>The clock scheduled work runs on, moved by <see cref="Advance"/>.</summary>
    public TestClock Clock { get; }

    /// <summary>The terminal being drawn to, for asserting on raw output or resizing mid-test.</summary>
    public FakeTerminal Terminal { get; }

    /// <summary>
    /// What is on screen after every frame drawn so far. <see cref="FrameLines"/> reads the last frame
    /// as it was written, which is the whole picture only while frames are written whole; this is the
    /// picture itself, diffed frames and all.
    /// </summary>
    public ScreenGrid Screen => Terminal.Screen;

    /// <summary>The container, for reaching whatever the test registered.</summary>
    public IServiceProvider Services => _provider;

    /// <summary>The shared state, for opening dialogs or reading the output line.</summary>
    public ArlecchinoState State => _provider.GetRequiredService<ArlecchinoState>();

    /// <summary>Navigation, for checking or forcing which view is current.</summary>
    public Navigator Navigator => _provider.GetRequiredService<Navigator>();

    /// <summary>The cell grid, for tests that draw into it directly.</summary>
    public Surface Surface => _provider.GetRequiredService<Surface>();

    /// <summary>The settings, for changing them after the application is built.</summary>
    public ArlecchinoOptions Options => _provider.GetRequiredService<ArlecchinoOptions>();

    /// <summary>The repaint flag, for checking that something actually asked for a frame.</summary>
    public Repaint Repaint => _provider.GetRequiredService<Repaint>();

    /// <summary>Undo history. It is resolved as the host is built, so edits are recorded from the start.</summary>
    public AtomHistory History => _provider.GetRequiredService<AtomHistory>();

    /// <summary>Presses a key, routed exactly as a real one would be.</summary>
    /// <param name="key">The key.</param>
    /// <param name="shift">Whether Shift was held.</param>
    /// <param name="alt">Whether Alt was held.</param>
    /// <param name="control">Whether Ctrl was held.</param>
    public void Press(ConsoleKey key, bool shift = false, bool alt = false, bool control = false) =>
        _provider.GetRequiredService<InputRouter>().ProcessKey(new('\0', key, shift, alt, control));

    /// <summary>
    /// Types text one character at a time. The presses carry a character but no key, which is what a
    /// terminal reports for ordinary typing.
    /// </summary>
    /// <param name="text">What to type.</param>
    public void Type(string text)
    {
        var router = _provider.GetRequiredService<InputRouter>();
        foreach (var character in text)
        {
            router.ProcessKey(new(character, default, false, false, false));
        }
    }

    /// <summary>
    /// Routes a key exactly as the terminal reported it, character and all. <see cref="Press"/> and
    /// <see cref="Type"/> cover what a test writes by hand; this is for one played back from a
    /// <see cref="SessionTape"/>, where the character and the key both matter.
    /// </summary>
    /// <param name="key">The key as the terminal reported it.</param>
    public void Send(ConsoleKeyInfo key) => _provider.GetRequiredService<InputRouter>().ProcessKey(key);

    /// <summary>Routes a mouse event exactly as the terminal reported it.</summary>
    /// <param name="mouse">The event.</param>
    public void Send(MouseEvent mouse) => _provider.GetRequiredService<InputRouter>().ProcessMouse(mouse);

    /// <summary>Pastes a block of text, as bracketed paste delivers it.</summary>
    /// <param name="text">What was pasted.</param>
    public void SendPaste(string text) => _provider.GetRequiredService<InputRouter>().ProcessPaste(text);

    /// <summary>Clicks a cell, in the terminal's own coordinates.</summary>
    /// <param name="row">Row, counted from the top of the terminal.</param>
    /// <param name="column">Column, counted from its left edge.</param>
    /// <param name="button">Which button was pressed.</param>
    public void Click(int row, int column, MouseButton button = MouseButton.Left) =>
        _provider.GetRequiredService<InputRouter>()
            .ProcessMouse(new(MouseAction.Pressed, button, row, column, default));

    /// <summary>Turns the wheel over a cell.</summary>
    /// <param name="row">Row the pointer is over.</param>
    /// <param name="column">Column the pointer is over.</param>
    /// <param name="down">Whether the wheel turned down.</param>
    public void Scroll(int row, int column, bool down) =>
        _provider.GetRequiredService<InputRouter>()
            .ProcessMouse(
                new(down ? MouseAction.ScrolledDown : MouseAction.ScrolledUp, MouseButton.None, row, column, default));

    /// <summary>
    /// Feeds raw characters through the reader that recognises escape sequences. This is the way to
    /// test what a real terminal sends for arrows, function keys and mouse reports.
    /// </summary>
    /// <param name="sequence">The characters, escapes included.</param>
    public void ReadFromTerminal(string sequence)
    {
        Terminal.EnqueueText(sequence);
        _provider.GetRequiredService<TerminalInputReader>().ReadPending();
        DrainInput();
    }

    /// <summary>
    /// Routes whatever the reader has queued, which is what the frame loop does before it draws.
    /// <see cref="ReadFromTerminal"/> and <see cref="Frame"/> do it for you; call it yourself after
    /// driving <c>TerminalInputReader</c> directly, since the reader queues rather than routes.
    /// </summary>
    public void DrainInput() =>
        _provider.GetRequiredService<PendingInput>().Drain(_provider.GetRequiredService<InputRouter>());

    /// <summary>
    /// Moves the clock forward and runs whatever fell due, exactly as the frame loop would. The frame is
    /// not drawn by this — ask for one afterwards.
    /// </summary>
    /// <param name="amount">How far to move the clock.</param>
    public void Advance(TimeSpan amount)
    {
        Clock.Advance(amount);
        _provider.GetRequiredService<Ticker>().Run(static _ => { });
    }

    /// <summary>
    /// Draws a frame the way a running application does — as the difference from the last one — and
    /// returns what is on screen afterwards, styling and all stripped away.
    ///
    /// The frame written and the screen returned are not the same thing, and that is the point: an
    /// idle frame writes nothing at all, and a frame that changed one cell writes one cell. Reading
    /// the screen is what lets a test assert on the whole picture regardless.
    /// </summary>
    /// <returns>The screen.</returns>
    public string Frame()
    {
        DrainInput();
        Terminal.Clear();
        _provider.GetRequiredService<Screen>().DrawFrame();
        VerifyTheScreenIsTheFrame();

        return string.Join("\r\n", Terminal.Screen.Lines());
    }

    /// <summary>Draws a frame and returns the rows on screen afterwards.</summary>
    /// <returns>One string per row.</returns>
    public string[] FrameLines()
    {
        Frame();
        return Terminal.Screen.Lines();
    }

    /// <summary>
    /// Draws a frame whole and returns the colour sequences in it, in order. Whole rather than diffed
    /// on purpose: a diffed frame only restates the styles of the cells it rewrites, so the sequences
    /// in it are the ones that changed rather than the ones the frame is drawn in.
    /// </summary>
    /// <returns>The sequences as they appeared.</returns>
    public IReadOnlyList<string> Styles()
    {
        Terminal.Clear();
        _provider.GetRequiredService<Screen>().DrawOnce();
        return FrameText.StylesIn(Terminal.Written);
    }

    /// <summary>Whether a frame holds some text anywhere. Text split across rows will not be found.</summary>
    /// <param name="text">What to look for.</param>
    /// <returns><c>true</c> when it is there.</returns>
    public bool FrameContains(string text) => Frame().Contains(text, StringComparison.Ordinal);

    /// <summary>The first row holding some text, which is how a test reads what was drawn beside a label.</summary>
    /// <param name="text">What to look for.</param>
    /// <returns>The whole row, or an empty string when no row holds it.</returns>
    public string FrameLineContaining(string text)
    {
        foreach (var line in FrameLines())
        {
            if (line.Contains(text, StringComparison.Ordinal))
            {
                return line;
            }
        }

        return "";
    }

    /// <summary>
    /// Holds the screen the frames left against the frame that was last composed. Every frame but the
    /// first is written as the difference from the one before, so a cell the difference failed to send
    /// stays on screen as whatever used to be there — a stale symbol no assertion would think to look
    /// for, since the test only ever asks what is on screen. Comparing the two says outright that the
    /// difference added up to the picture.
    ///
    /// It runs on every frame a test draws rather than in a test of its own, because the frames worth
    /// checking are the ones real views produce, and those only exist while a test is running.
    /// </summary>
    /// <exception cref="InvalidOperationException">The screen and the frame disagree somewhere.</exception>
    private void VerifyTheScreenIsTheFrame()
    {
        var surface = Surface;
        var screen = Terminal.Screen;

        if (surface.FrameWidth != screen.Width || surface.FrameHeight != screen.Height)
        {
            throw new InvalidOperationException(
                $"the frame is {surface.FrameWidth}x{surface.FrameHeight} and the screen it was drawn to " +
                $"is {screen.Width}x{screen.Height}");
        }

        for (var row = 0; row < screen.Height; row++)
        {
            for (var column = 0; column < screen.Width; column++)
            {
                var (cell, style) = surface.Composed(row, column);

                if (string.Equals(cell, screen.CellAt(row, column), StringComparison.Ordinal) &&
                    string.Equals(style.Ansi, screen.StyleAt(row, column), StringComparison.Ordinal))
                {
                    continue;
                }

                throw new InvalidOperationException(
                    $"the diffed frames left row {row}, column {column} showing " +
                    $"{Quoted(screen.CellAt(row, column))} in {Quoted(screen.StyleAt(row, column))} " +
                    $"where the frame drew {Quoted(cell)} in {Quoted(style.Ansi)}." +
                    $"{Environment.NewLine}{Environment.NewLine}on screen:{Environment.NewLine}" +
                    $"{screen}{Environment.NewLine}{Environment.NewLine}drawn:{Environment.NewLine}{Drawn(surface)}");
            }
        }
    }

    private static string Drawn(Surface surface)
    {
        var lines = new string[surface.FrameHeight];

        for (var row = 0; row < surface.FrameHeight; row++)
        {
            var line = new StringBuilder(surface.FrameWidth);

            for (var column = 0; column < surface.FrameWidth; column++)
            {
                line.Append(surface.Composed(row, column).Cell);
            }

            lines[row] = line.ToString();
        }

        return string.Join('\n', lines);
    }

    private static string Quoted(string text) => $"\"{text.Replace("\e", "\\e", StringComparison.Ordinal)}\"";

    /// <summary>Disposes the container and everything in it, and drops work still posted to the frame.</summary>
    public void Dispose()
    {
        FrameThread.DiscardPending();
        _provider.Dispose();
    }
}
