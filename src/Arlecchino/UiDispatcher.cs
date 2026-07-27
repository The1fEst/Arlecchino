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
    ///
    /// Only what was waiting when this was called is run. Work posted by that work waits for the next
    /// frame, which is what makes "do this on the next frame" — an action that posts itself — a loop
    /// the application can actually leave.
    /// </summary>
    /// <param name="onError">What to do with an action that threw.</param>
    public void RunPending(Action<Exception> onError)
    {
        for (var waiting = _pending.Count; waiting > 0 && _pending.TryDequeue(out var action); waiting--)
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
