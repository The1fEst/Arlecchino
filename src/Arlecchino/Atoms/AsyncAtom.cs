using System;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Atoms.Local;

namespace Arlecchino.Atoms;

/// <summary>Where a background load has got to.</summary>
public enum LoadStatus : byte
{
    /// <summary>Nothing has been loaded yet.</summary>
    Idle,

    /// <summary>A load is running.</summary>
    Loading,

    /// <summary>The last load finished, and its result is in place.</summary>
    Loaded,

    /// <summary>The last load threw; see the error.</summary>
    Failed,
}

/// <summary>
/// A value produced by background work, with its progress exposed as state for a view to draw. Results are
/// handed back on the drawing thread, and a new load cancels the one before it.
/// </summary>
/// <typeparam name="T">What is being loaded.</typeparam>
public sealed class AsyncAtom<T> : IReadableAtom<T?>
{
    private readonly LocalAtom<T?> _value;
    private readonly LocalAtom<LoadStatus> _status = new(LoadStatus.Idle);
    private readonly LocalAtom<Exception?> _error = new(null);

    private CancellationTokenSource? _running;

    /// <summary>Creates the state, without starting anything.</summary>
    /// <param name="initial">What to hold until the first load finishes.</param>
    public AsyncAtom(T? initial = default)
    {
        _value = new(initial);
    }

    /// <summary>The last loaded value. It stays put while a new load runs, so the view keeps its content.</summary>
    public T? Value => _value.Value;

    /// <summary>Progress of the last load, for showing a spinner or an error.</summary>
    public IReadableAtom<LoadStatus> Status => _status;

    /// <summary>What the last load threw, or <c>null</c> when it did not fail.</summary>
    public IReadableAtom<Exception?> Error => _error;

    /// <summary>Whether a load is running right now.</summary>
    public bool IsLoading => _status.Value == LoadStatus.Loading;

    /// <summary>Watches for a new value. Progress changes on their own do not notify.</summary>
    /// <param name="listener">Called after the value changes.</param>
    /// <returns>Dispose to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _value.Subscribe(listener);

    /// <summary>Watches progress, which is what a spinner or an error line needs.</summary>
    /// <param name="listener">Called after the status changes.</param>
    /// <returns>Dispose to stop listening.</returns>
    public IDisposable SubscribeToStatus(Action listener) => _status.Subscribe(listener);

    /// <summary>
    /// Starts work in the background, canceling whatever was already running. Returns at once; the
    /// result, or the failure, arrives later on the UI thread.
    /// </summary>
    /// <param name="load">The work to run, given a token that is canceled when a newer load starts.</param>
    public void Load(Func<CancellationToken, Task<T>> load)
    {
        Cancel();

        var running = new CancellationTokenSource();
        _running = running;

        _status.Value = LoadStatus.Loading;
        _error.Value = null;

        _ = RunAsync(load, running);
    }

    /// <summary>
    /// Abandons the running load, keeping whatever was loaded before it. The state stops reporting itself as
    /// loading, so a spinner bound to it stops.
    /// </summary>
    public void Cancel()
    {
        _running?.Cancel();
        _running?.Dispose();
        _running = null;

        if (_status.Value == LoadStatus.Loading)
        {
            _status.Value = LoadStatus.Idle;
        }
    }

    private async Task RunAsync(Func<CancellationToken, Task<T>> load, CancellationTokenSource running)
    {
        try
        {
            var loaded = await load(running.Token).ConfigureAwait(false);

            if (running.IsCancellationRequested)
            {
                return;
            }

            FrameThread.Post(() =>
            {
                _value.Value = loaded;
                _status.Value = LoadStatus.Loaded;
            });
        }
        catch (OperationCanceledException) { }
        catch (Exception exception)
        {
            if (running.IsCancellationRequested)
            {
                return;
            }

            FrameThread.Post(() =>
            {
                _error.Value = exception;
                _status.Value = LoadStatus.Failed;
            });
        }
    }
}
