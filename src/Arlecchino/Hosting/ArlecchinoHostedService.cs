using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Arlecchino.Diagnostics;
using Arlecchino.Navigation;
using Arlecchino.Input;
using Arlecchino.Atoms;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Rendering.Terminals;

namespace Arlecchino.Hosting;

/// <summary>
/// Runs the application for as long as the host does: it takes the terminal over, drives drawing and input,
/// and gives the terminal back on every way out, process exit included.
/// </summary>
internal sealed class ArlecchinoHostedService : BackgroundService
{
    private readonly IArlecchinoTerminal _terminal;
    private readonly Screen _screen;
    private readonly TerminalInputReader _input;
    private readonly Navigator _navigator;
    private readonly ArlecchinoOptions _options;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly AtomHistory _history;
    private readonly ILogger<ArlecchinoHostedService> _logger;
    private readonly IArlecchinoStartup[] _startups;
    private readonly ArlecchinoAsyncStore[] _stores;
    private readonly TerminalModes _modes;
    private readonly Handover _handover;

    private readonly List<PosixSignalRegistration> _signals = [];

    private int _restoring;

    private ConsoleCancelEventHandler? _cancelKeyHandler;
    private EventHandler? _processExitHandler;

    /// <summary>Creates the service.</summary>
    /// <param name="terminal">What is drawn to and read from.</param>
    /// <param name="screen">Draws the frames.</param>
    /// <param name="input">Turns what the terminal reports into keys and mouse events.</param>
    /// <param name="navigator">Holds the current view.</param>
    /// <param name="options">Settings gathered at startup.</param>
    /// <param name="lifetime">Used to stop the host on Ctrl+C or an unhandled error.</param>
    /// <param name="history">Cleared before the first frame, so startup edits are not undoable.</param>
    /// <param name="logger">Where failures are reported, since the screen is not usable for that.</param>
    /// <param name="startups">Work to run before the first frame.</param>
    /// <param name="stores">Stores that load themselves; started as the application starts.</param>
    /// <param name="modes">The terminal modes this application asked for, entered and left through one place.</param>
    /// <param name="handover">Asked whether the terminal is ours to read, since another program may have it.</param>
    public ArlecchinoHostedService(
        IArlecchinoTerminal terminal,
        Screen screen,
        TerminalInputReader input,
        Navigator navigator,
        ArlecchinoOptions options,
        IHostApplicationLifetime lifetime,
        AtomHistory history,
        ILogger<ArlecchinoHostedService> logger,
        IEnumerable<IArlecchinoStartup> startups,
        IEnumerable<ArlecchinoAsyncStore> stores,
        TerminalModes modes,
        Handover handover)
    {
        _modes = modes;
        _handover = handover;
        _stores = [.. stores];
        _history = history;
        _terminal = terminal;
        _screen = screen;
        _input = input;
        _navigator = navigator;
        _options = options;
        _lifetime = lifetime;
        _logger = logger;
        _startups = startups.ToArray();
    }

    /// <summary>
    /// Runs startup, takes over the terminal, and then draws and reads until the host stops. An error
    /// that escapes stops the host rather than leaving a half-drawn screen behind.
    /// </summary>
    /// <param name="stoppingToken">Canceled when the host is shutting down.</param>
    /// <returns>A task that completes once the terminal has been restored.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _history.Clear();

        foreach (var store in _stores)
        {
            _ = store.RunAsync(StoreFailed, stoppingToken);
        }

        foreach (var startup in _startups)
        {
            _navigator.Apply(startup.Start());
        }

        TakeTerminal();
        HookProcessSignals();

        try
        {
            await Task.WhenAll(_screen.Run(stoppingToken), ReadInput(stoppingToken));
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            Log.HostStopped(_logger, exception);
            _lifetime.StopApplication();
        }
        finally
        {
            RestoreTerminal();
        }
    }

    private void StoreFailed(Exception exception) =>
        Log.StoreFailed(_logger, exception);

    /// <summary>
    /// Takes the terminal over and asks it what it can do. The question is put once, as the application
    /// starts, since what a terminal is capable of does not change while it runs.
    /// </summary>
    private void TakeTerminal()
    {
        _modes.Enter();

        if (!_options.AskTerminal)
        {
            return;
        }

        var reply = TerminalProbe.Ask(_terminal, _options.TerminalAnswer);

        if (_options.PaletteForBackground is { } derive && TerminalCapabilities.Background is { } behind)
        {
            Theme.Palette = derive(behind);
        }

        Log.TerminalAnswered(
            _logger,
            reply,
            TerminalCapabilities.Sixel,
            TerminalCapabilities.Kitty,
            Glyphs.CellWidth,
            Glyphs.CellHeight);
    }

    private void HookProcessSignals()
    {
        _cancelKeyHandler = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            _lifetime.StopApplication();
        };

        _processExitHandler = (_, _) => RestoreTerminal();

        Console.CancelKeyPress += _cancelKeyHandler;
        AppDomain.CurrentDomain.ProcessExit += _processExitHandler;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        HookPosixSignals();
    }

    /// <summary>
    /// The signals a terminal application has to answer itself. Being killed or suspended without giving the
    /// screen back leaves an alternate screen with no cursor and no prompt.
    /// </summary>
    private void HookPosixSignals()
    {
        Register(PosixSignal.SIGTERM, _ => _modes.Leave());

        if (OperatingSystem.IsWindows())
        {
            return;
        }

        Register(PosixSignal.SIGHUP, _ => _modes.Leave());
        Register(PosixSignal.SIGTSTP, _ => _modes.Leave());
        Register(PosixSignal.SIGCONT,
            _ =>
            {
                _modes.Enter();
                _screen.RedrawEverything();
            });
    }

    private void Register(PosixSignal signal, Action<PosixSignalContext> handler)
    {
        try
        {
            _signals.Add(PosixSignalRegistration.Create(signal, handler));
        }
        catch (PlatformNotSupportedException) { }
        catch (ArgumentOutOfRangeException) { }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs eventArgs)
    {
        RestoreTerminal();

        if (eventArgs.ExceptionObject is Exception exception)
        {
            Log.TerminalRestored(_logger, exception);
        }
    }

    /// <summary>
    /// Unhooks everything and gives the terminal back. Three different threads can reach this — the
    /// loop finishing, process exit, an unhandled error — so it runs once and the rest walk past.
    /// </summary>
    private void RestoreTerminal()
    {
        if (Interlocked.Exchange(ref _restoring, 1) == 1)
        {
            return;
        }

        if (_cancelKeyHandler is not null)
        {
            Console.CancelKeyPress -= _cancelKeyHandler;
            _cancelKeyHandler = null;
        }

        if (_processExitHandler is not null)
        {
            AppDomain.CurrentDomain.ProcessExit -= _processExitHandler;
            _processExitHandler = null;
        }

        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

        foreach (var registration in _signals)
        {
            registration.Dispose();
        }

        _signals.Clear();
        _modes.Leave();
    }

    /// <summary>
    /// Reads the terminal for as long as the application runs, and waits while another program has it. The
    /// reader asks before every read rather than racing that program for the keys.
    /// </summary>
    /// <param name="stoppingToken">Canceled when the host is shutting down.</param>
    /// <returns>A task that completes once reading has stopped.</returns>
    private async Task ReadInput(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_handover.MayRead() && (_terminal.KeyAvailable || _terminal.MouseAvailable))
            {
                _input.ReadPending();
                continue;
            }

            await Task.Delay(_options.InputPollInterval, stoppingToken);
        }
    }
}
