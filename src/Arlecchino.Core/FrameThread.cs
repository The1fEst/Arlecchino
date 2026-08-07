using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Arlecchino;

/// <summary>
/// Which thread draws. Views, widgets, atoms and the surface are written without locks because one thread
/// touches them, and this is what turns that from a convention into something the framework can check. The
/// frame loop claims the thread it runs on, and everything that must happen there asks before it changes
/// anything.
///
/// Nothing claims it outside a running application — a headless host, a test, a single
/// <c>DrawOnce</c> — so the checks stay quiet there and cost a null comparison.
/// </summary>
public static class FrameThread
{
    private static readonly ConcurrentQueue<Action> Pending = new();

    private static int _drawing;
    private static Action? _wake;

    /// <summary>Whether the calling thread is the one drawing, or nothing has claimed drawing yet.</summary>
    public static bool IsCurrent
    {
        get
        {
            var drawing = Volatile.Read(ref _drawing);
            return drawing == 0 || drawing == Environment.CurrentManagedThreadId;
        }
    }

    /// <summary>Whether anything posted is still waiting to run.</summary>
    public static bool HasPending => !Pending.IsEmpty;

    /// <summary>
    /// Claims the calling thread as the one that draws. Called by the frame loop as it starts; an
    /// application that runs the loop itself calls it too, so that the checks know where "here" is.
    /// </summary>
    /// <param name="wake">
    /// Asks for a frame, called whenever something is posted. The frame loop passes its repaint
    /// signal, so posted work is drawn without the caller having to ask.
    /// </param>
    /// <returns>
    /// A scope that gives the claim up again. Giving up the last claim drops what is still posted:
    /// with nobody drawing there is no frame left for it to run before.
    /// </returns>
    public static IDisposable Claim(Action? wake = null)
    {
        var previousWake = Interlocked.Exchange(ref _wake, wake);
        var previous = Interlocked.Exchange(ref _drawing, Environment.CurrentManagedThreadId);

        return new Claimed(previous, previousWake);
    }

    /// <summary>
    /// Hands work to the drawing thread, from wherever you are. It runs just before the next frame,
    /// in the order it was posted, and a frame is asked for by itself.
    ///
    /// With nobody drawing — a test, a headless render — it waits until something runs it, which is
    /// what <see cref="RunPending"/> is for.
    /// </summary>
    /// <param name="action">What to run where it is safe to change what a frame draws.</param>
    public static void Post(Action action)
    {
        Pending.Enqueue(action);
        Volatile.Read(ref _wake)?.Invoke();
    }

    /// <summary>
    /// Runs what was posted before this call. Called by the frame loop; work posted by that work
    /// waits for the next frame, so an action that posts itself is a loop you can leave.
    /// </summary>
    /// <param name="onError">What to do with an action that threw.</param>
    public static void RunPending(Action<Exception> onError)
    {
        for (var waiting = Pending.Count; waiting > 0 && Pending.TryDequeue(out var action); waiting--)
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

    /// <summary>
    /// Drops what was posted and never run. An application going away calls it, and so does a test
    /// host as it is disposed, so that work left over by one does not run inside the next.
    /// </summary>
    public static void DiscardPending() => Pending.Clear();

    /// <summary>
    /// Throws unless the caller is on the drawing thread. This is what a member that changes what a
    /// frame draws calls before changing anything.
    /// </summary>
    /// <param name="member">What was called, named in the message.</param>
    /// <exception cref="InvalidOperationException">The caller is on another thread.</exception>
    public static void Verify(string member)
    {
        var drawing = Volatile.Read(ref _drawing);

        if (drawing == 0 || drawing == Environment.CurrentManagedThreadId)
        {
            return;
        }

        throw new InvalidOperationException(
            $"{member} was called from thread {Environment.CurrentManagedThreadId}, but frames are drawn on " +
            $"thread {drawing}. Views, widgets and atoms are not thread-safe: hand the change over with " +
            "FrameThread.Post, which runs it just before the next frame.");
    }

    private sealed class Claimed : IDisposable
    {
        private readonly int _previous;
        private readonly Action? _previousWake;

        public Claimed(int previous, Action? previousWake)
        {
            _previous = previous;
            _previousWake = previousWake;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _drawing, _previous);
            Interlocked.Exchange(ref _wake, _previousWake);

            if (_previous == 0)
            {
                DiscardPending();
            }
        }
    }
}
