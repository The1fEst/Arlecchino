using System;
using System.Collections.Concurrent;

namespace Arlecchino;

/// <summary>
/// The way back onto the frame loop from another thread. Views, state and the surface are not
/// thread-safe, so work that finishes in the background hands its result over through this queue.
/// </summary>
public sealed class UiDispatcher
{
    private readonly ConcurrentQueue<Action> _pending = new();
    private readonly Repaint _repaint;

    /// <summary>Creates the queue.</summary>
    /// <param name="repaint">Signal raised whenever something is posted.</param>
    public UiDispatcher(Repaint repaint)
    {
        _repaint = repaint;
    }

    /// <summary>Whether anything is waiting to run on the next frame.</summary>
    public bool HasPending => !_pending.IsEmpty;

    /// <summary>
    /// Queues work to run just before the next frame is composed. Safe from any thread, keeps the
    /// order it was posted in, and asks for a repaint by itself.
    /// </summary>
    /// <param name="action">What to run on the frame loop.</param>
    public void Post(Action action)
    {
        _pending.Enqueue(action);
        _repaint.Request();
    }

    /// <summary>
    /// Runs everything queued so far. Called by the frame loop; an action that throws is reported
    /// and the rest still run.
    /// </summary>
    /// <param name="onError">What to do with an action that threw.</param>
    public void RunPending(Action<Exception> onError)
    {
        while (_pending.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                onError(exception);
            }
        }
    }
}
