using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;
using Arlecchino.State;

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
    /// far smaller than a real one without hitting the too-small notice.
    /// </summary>
    /// <param name="width">Columns of the fake terminal.</param>
    /// <param name="height">Rows of the fake terminal.</param>
    /// <param name="configure">Registers the views, commands and services under test.</param>
    public ArlecchinoTestHost(int width = 80, int height = 24, Action<ArlecchinoBuilder>? configure = null)
    {
        var services = new ServiceCollection();

        Terminal = new(width, height);
        services.AddSingleton<ITerminal>(Terminal);

        var builder = services
            .AddArlecchino(options =>
            {
                options.MinimumWidth = 1;
                options.MinimumHeight = 1;
            })
            .WithoutHostedService();

        configure?.Invoke(builder);

        _provider = services.BuildServiceProvider();
        _ = History;
    }

    /// <summary>The terminal being drawn to, for asserting on raw output or resizing mid-test.</summary>
    public FakeTerminal Terminal { get; }

    /// <summary>The container, for reaching whatever the test registered.</summary>
    public IServiceProvider Services => _provider;

    /// <summary>The shared state, for opening dialogs or reading the output line.</summary>
    public TuiState State => _provider.GetRequiredService<TuiState>();

    /// <summary>Navigation, for checking or forcing which view is current.</summary>
    public Navigator Navigator => _provider.GetRequiredService<Navigator>();

    /// <summary>The cell grid, for tests that draw into it directly.</summary>
    public Surface Surface => _provider.GetRequiredService<Surface>();

    /// <summary>The settings, for changing them after the application is built.</summary>
    public ArlecchinoOptions Options => _provider.GetRequiredService<ArlecchinoOptions>();

    /// <summary>The repaint flag, for checking that something actually asked for a frame.</summary>
    public Repaint Repaint => _provider.GetRequiredService<Repaint>();

    /// <summary>The dispatcher, for running posted work the way a real frame would.</summary>
    public UiDispatcher Dispatcher => _provider.GetRequiredService<UiDispatcher>();

    /// <summary>Undo history. It is resolved as the host is built, so edits are recorded from the start.</summary>
    public StateHistory History => _provider.GetRequiredService<StateHistory>();

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
        _provider.GetRequiredService<InputRouter>().ProcessMouse(
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
    }

    /// <summary>Draws a frame and returns it as plain text, with the styling stripped.</summary>
    /// <returns>The frame.</returns>
    public string Frame()
    {
        Terminal.Clear();
        _provider.GetRequiredService<Screen>().DrawOnce();
        return FrameText.WithoutStyles(Terminal.Written);
    }

    /// <summary>Draws a frame and returns its rows.</summary>
    /// <returns>One string per row.</returns>
    public string[] FrameLines() => Frame().Split("\r\n");

    /// <summary>Draws a frame and returns the colour sequences in it, in order.</summary>
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

    /// <summary>Disposes the container and everything in it.</summary>
    public void Dispose() => _provider.Dispose();
}
