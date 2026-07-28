using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Atoms;

namespace Arlecchino.Hosting;

/// <summary>
/// Where the <see cref="IArlecchinoAsyncStore"/> loads have got to. The application starts drawing at
/// once and the loads run behind it, so this is what a view reads to say "loading…" instead of showing
/// an empty screen it cannot explain.
///
/// <code>
/// public void Draw()
/// {
///     if (_loading.IsLoading)
///     {
///         _surface.AppendLine("loading settings…", Theme.Muted, Align.Center);
///         return;
///     }
///
///     _form.Draw(_surface.Content);
/// }
/// </code>
///
/// The status is one answer for every async store there is: <c>Loading</c> until the last of them is
/// done, <c>Failed</c> if any of them threw, <c>Loaded</c> otherwise. An application with no async
/// stores stays <c>Idle</c>.
/// </summary>
public sealed class StoreLoading
{
    private readonly LocalAtom<LoadStatus> _status = new(LoadStatus.Idle);
    private readonly LocalAtom<Exception?> _error = new(null);

    private int _running;

    /// <summary>How the loading is going, as an atom, so a view that reads it redraws by itself.</summary>
    public IReadableAtom<LoadStatus> Status => _status;

    /// <summary>What the first failing store threw, or <c>null</c> while nothing has failed.</summary>
    public IReadableAtom<Exception?> Error => _error;

    /// <summary>Whether a store is still loading.</summary>
    public bool IsLoading => _status.Value == LoadStatus.Loading;

    /// <summary>Whether every store finished and none of them threw.</summary>
    public bool IsLoaded => _status.Value == LoadStatus.Loaded;

    /// <summary>Whether a store threw. What it threw is in <see cref="Error"/>.</summary>
    public bool Failed => _status.Value == LoadStatus.Failed;

    /// <summary>
    /// Starts every store loading and returns without waiting for them. Called by the hosted service
    /// as the application starts; a headless host calls it itself when it wants the same.
    /// </summary>
    /// <param name="stores">The stores to load.</param>
    /// <param name="token">Cancelled when the application is shutting down.</param>
    /// <param name="onError">What to do with a store that threw, besides remembering it.</param>
    public void Start(IEnumerable<IArlecchinoAsyncStore> stores, CancellationToken token, Action<Exception>? onError = null)
    {
        ArgumentNullException.ThrowIfNull(stores);

        var loading = new List<IArlecchinoAsyncStore>(stores);

        if (loading.Count == 0)
        {
            return;
        }

        _running = loading.Count;
        _status.Value = LoadStatus.Loading;

        foreach (var store in loading)
        {
            _ = Load(store, token, onError);
        }
    }

    private async Task Load(IArlecchinoAsyncStore store, CancellationToken token, Action<Exception>? onError)
    {
        try
        {
            await store.LoadAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            onError?.Invoke(exception);

            FrameThread.Post(() =>
            {
                _error.Value ??= exception;
                _status.Value = LoadStatus.Failed;
            });
        }

        FrameThread.Post(() =>
        {
            if (Interlocked.Decrement(ref _running) == 0 && _status.Value != LoadStatus.Failed)
            {
                _status.Value = LoadStatus.Loaded;
            }
        });
    }
}
