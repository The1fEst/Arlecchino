using System;
using System.Threading;
using System.Threading.Tasks;

using Arlecchino.Atoms.Local;

namespace Arlecchino.Atoms;

/// <summary>
/// A store that has to fetch something before it holds the truth — settings read from disk, a session
/// restored from a server, a catalogue that lives in a file. Derive from it, override
/// <see cref="LoadAsync"/>, and the framework starts the load as the application starts and keeps the
/// bookkeeping: no worker of its own, and no <c>TaskCompletionSource</c> written by hand.
///
/// <code>
/// public sealed class SettingsStore : ArlecchinoAsyncStore
/// {
///     public TrackedAtom&lt;string&gt; Server { get; } = new("127.0.0.1");
///
///     protected override async Task LoadAsync(CancellationToken token)
///     {
///         await using var fs = new FileStream(SettingsPath, FileMode.Open, FileAccess.Read);
///         var saved = await JsonSerializer.DeserializeAsync&lt;Saved&gt;(fs, cancellationToken: token);
///
///         Server.Post(saved.Server);
///     }
/// }
/// </code>
///
/// Reading the file is the application's own code — the framework has nothing to do with disks,
/// formats or paths.
///
/// The first frame is drawn without waiting: a terminal that hangs black on a slow disk is worse than
/// a screen that says it is loading. A view draws from <see cref="Status"/>, which is an atom and so
/// redraws by itself; code that is not a view — a worker, a command that must not run early — awaits
/// <see cref="Ready"/>.
/// </summary>
public abstract class ArlecchinoAsyncStore : IArlecchinoStore
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly LocalAtom<LoadStatus> _status = new(LoadStatus.Idle);
    private readonly LocalAtom<Exception?> _error = new(null);

    /// <summary>Creates the store.</summary>
    protected ArlecchinoAsyncStore()
    {
        _ready.Task.ContinueWith(
            static task => _ = task.Exception,
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    /// <summary>
    /// Completes when the store is loaded, faults with whatever <see cref="LoadAsync"/> threw, and is
    /// cancelled when the application stopped before the load finished. This is the one to await
    /// outside a view; a view reads <see cref="Status"/> instead, because it draws every frame rather
    /// than waiting.
    /// </summary>
    public Task Ready => _ready.Task;

    /// <summary>How the load is going, as an atom, so a view that reads it redraws when it changes.</summary>
    public IReadableAtom<LoadStatus> Status => _status;

    /// <summary>What the load threw, or <c>null</c> while it has not failed.</summary>
    public IReadableAtom<Exception?> Error => _error;

    /// <summary>Whether the load is still running.</summary>
    public bool IsLoading => _status.Value == LoadStatus.Loading;

    /// <summary>Whether the load finished and the atoms hold what it fetched.</summary>
    public bool IsLoaded => _status.Value == LoadStatus.Loaded;

    /// <summary>Whether the load threw. What it threw is in <see cref="Error"/>.</summary>
    public bool Failed => _status.Value == LoadStatus.Failed;

    /// <summary>
    /// Fetches what the store needs. It runs off the drawing thread, so what it loads reaches the
    /// atoms through <c>Post</c> — writing <c>Value</c> from here throws, and says so.
    ///
    /// Throwing is a normal outcome: the status turns to failed, the exception is kept for a view to
    /// draw and for <see cref="Ready"/> to hand to whoever awaits it, and the application carries on
    /// with whatever the atoms already hold.
    /// </summary>
    /// <param name="token">Cancelled when the application is shutting down.</param>
    /// <returns>A task that completes when the store is ready.</returns>
    protected abstract Task LoadAsync(CancellationToken token);

    internal async Task RunAsync(Action<Exception>? onError, CancellationToken token)
    {
        _status.Post(LoadStatus.Loading);

        try
        {
            await LoadAsync(token).ConfigureAwait(false);

            _status.Post(LoadStatus.Loaded);
            _ready.TrySetResult();
        }
        catch (OperationCanceledException)
        {
            _status.Post(LoadStatus.Idle);
            _ready.TrySetCanceled(token);
        }
        catch (Exception exception)
        {
            onError?.Invoke(exception);

            _error.Post(exception);
            _status.Post(LoadStatus.Failed);
            _ready.TrySetException(exception);
        }
    }
}
