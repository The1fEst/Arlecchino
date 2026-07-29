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

namespace Arlecchino.Hosting;

/// <summary>
/// Runs the application for as long as the host does: it takes over the terminal, drives drawing and
/// input, and gives the terminal back afterwards. Restoring is done on every way out — a normal stop,
/// Ctrl+C, an unhandled error, even process exit — because a terminal left in full-screen mode with
/// the mouse captured is unusable once the process is gone.
/// </summary>
internal class ArlecchinoHostedService : BackgroundService
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

    private readonly List<PosixSignalRegistration> _signals = [];

    private int _terminalLeft;
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
        IEnumerable<ArlecchinoAsyncStore> stores)
    {
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
    /// <param name="stoppingToken">Cancelled when the host is shutting down.</param>
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

        EnterTerminalModes();
        HookProcessSignals();

        try
        {
            await Task.WhenAll(_screen.Run(stoppingToken), ReadInput(stoppingToken));
        }
        catch (OperationCanceledException)
        {
        }
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

    private void EnterTerminalModes()
    {
        Interlocked.Exchange(ref _terminalLeft, 0);

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

    private void LeaveTerminalModes()
    {
        if (Interlocked.Exchange(ref _terminalLeft, 1) == 1)
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
    /// The signals a terminal application has to answer itself. Being killed or suspended without
    /// giving the screen back leaves the user staring at an alternate screen with no cursor and no
    /// prompt, which no amount of process cleanup afterwards can fix.
    /// </summary>
    private void HookPosixSignals()
    {
        Register(PosixSignal.SIGTERM, _ => LeaveTerminalModes());

        if (OperatingSystem.IsWindows())
        {
            return;
        }

        Register(PosixSignal.SIGHUP, _ => LeaveTerminalModes());
        Register(PosixSignal.SIGTSTP, _ => LeaveTerminalModes());
        Register(PosixSignal.SIGCONT, _ =>
        {
            EnterTerminalModes();
            _screen.RedrawEverything();
        });
    }

    private void Register(PosixSignal signal, Action<PosixSignalContext> handler)
    {
        try
        {
            _signals.Add(PosixSignalRegistration.Create(signal, handler));
        }
        catch (PlatformNotSupportedException)
        {
        }
        catch (ArgumentOutOfRangeException)
        {
        }
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
        LeaveTerminalModes();
    }

    private async Task ReadInput(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_terminal.KeyAvailable || _terminal.MouseAvailable)
            {
                _input.ReadPending();
                continue;
            }

            await Task.Delay(_options.InputPollInterval, stoppingToken);
        }
    }
}
