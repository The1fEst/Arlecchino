using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Arlecchino;

/// <summary>
/// Which thread draws, claimed by the frame loop as it starts. Views, widgets, atoms and the surface are
/// written without locks, and this is what turns that convention into something the framework checks.
/// </summary>
public static class FrameThread
{
    private static readonly ConcurrentQueue<Action> Pending = new();
    private static readonly FrameContext Context = new();

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
    /// Claims the calling thread as the one that draws. An application running the frame loop itself calls
    /// this too, so the checks know which thread is meant.
    /// </summary>
    /// <param name="wake">
    /// Asks for a frame, called whenever something is posted. The frame loop passes its repaint
    /// signal, so posted work is drawn without the caller having to ask.
    /// </param>
    /// <returns>
    /// A scope that gives the claim up again. Giving up the last claim drops what is still posted, since no
    /// frame is left for it to run before.
    /// </returns>
    public static IDisposable Claim(Action? wake = null)
    {
        var previousWake = Interlocked.Exchange(ref _wake, wake);
        var previous = Interlocked.Exchange(ref _drawing, Environment.CurrentManagedThreadId);

        return new Claimed(previous, previousWake);
    }

    /// <summary>
    /// Hands work to the drawing thread, to run just before the next frame in the order it was posted. With
    /// no thread drawing it waits for <see cref="RunPending"/>.
    /// </summary>
    /// <param name="action">What to run where it is safe to change what a frame draws.</param>
    public static void Post(Action action)
    {
        Pending.Enqueue(action);
        Volatile.Read(ref _wake)?.Invoke();
    }

    /// <summary>
    /// Hands asynchronous work to the drawing thread. It starts there, and every <c>await</c> inside it
    /// that was not told otherwise comes back there, so what it reads and writes is what a frame draws.
    /// </summary>
    /// <param name="work">
    /// What to run. Whatever it throws, before an <c>await</c> or after one, reaches the frame loop the
    /// way a posted action's failure does; being canceled is not a failure.
    /// </param>
    public static void Post(Func<Task> work) => Post(() => Watch(work()));

    /// <summary>
    /// Follows work that has not finished by the time it hands control back, and posts whatever it
    /// throws so that the frame loop reports it.
    /// </summary>
    /// <param name="running">The work, as it stands after its first synchronous stretch.</param>
    private static void Watch(Task running)
    {
        if (running.IsCompleted)
        {
            running.GetAwaiter().GetResult();

            return;
        }

        running.ContinueWith(
            static finished => Post(() => finished.GetAwaiter().GetResult()),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    /// <summary>
    /// Runs what was posted before this call. Called by the frame loop; work posted by that work
    /// waits for the next frame, so an action that posts itself is a loop you can leave.
    /// </summary>
    /// <param name="onError">What to do with an action that threw.</param>
    public static void RunPending(Action<Exception> onError)
    {
        var previousContext = SynchronizationContext.Current;

        SynchronizationContext.SetSynchronizationContext(Context);

        try
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
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previousContext);
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

    /// <summary>
    /// What an <c>await</c> inside posted work comes back through, which is the queue everything else is
    /// posted to. It is in force only while that work runs, so a wait anywhere else is left alone.
    /// </summary>
    private sealed class FrameContext : SynchronizationContext
    {
        /// <inheritdoc/>
        public override void Post(SendOrPostCallback callback, object? state) =>
            FrameThread.Post(() => callback(state));

        /// <summary>
        /// Runs the callback where the caller is already drawing, and refuses anywhere else: waiting for
        /// the drawing thread from another one deadlocks whenever that thread is what is being waited on.
        /// </summary>
        /// <param name="callback">What to run.</param>
        /// <param name="state">What to run it on.</param>
        /// <exception cref="InvalidOperationException">The caller is on another thread.</exception>
        public override void Send(SendOrPostCallback callback, object? state)
        {
            if (!IsCurrent)
            {
                throw new InvalidOperationException(
                    "Waiting on the drawing thread from another thread deadlocks. Hand the work over with " +
                    "FrameThread.Post instead, which runs it just before the next frame.");
            }

            callback(state);
        }

        /// <inheritdoc/>
        public override SynchronizationContext CreateCopy() => new FrameContext();
    }
}
