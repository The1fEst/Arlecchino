using System;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Atoms.Local;

namespace Arlecchino.Atoms;

/// <summary>
/// A store that has to fetch something before it holds the truth. Override <see cref="LoadAsync"/>, and the
/// load is started as the application starts and its bookkeeping kept.
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
/// </summary>
/// <seealso cref="Status"/>
/// <seealso cref="Ready"/>
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
    /// Completes when the store is loaded, faults with whatever <see cref="LoadAsync"/> threw, and cancels
    /// when the application stopped first. A view reads <see cref="Status"/> instead of awaiting it.
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
    /// Fetches what the store needs, off the drawing thread, so what it loads reaches the atoms through
    /// <c>Post</c>. Throwing is a normal outcome and turns the status to failed.
    /// </summary>
    /// <param name="token">Canceled when the application is shutting down.</param>
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
