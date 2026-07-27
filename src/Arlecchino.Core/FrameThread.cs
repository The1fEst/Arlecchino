using System;
using System.Threading;

namespace Arlecchino;

/// <summary>
/// Which thread draws. Views, widgets, atoms and the surface are written without locks because one
/// thread touches them, and this is what turns that from a convention into something the framework
/// can check: the frame loop claims the thread it runs on, and everything that must happen there
/// asks before it changes anything.
///
/// Nothing claims it outside a running application — a headless host, a test, a single
/// <c>DrawOnce</c> — so the checks stay quiet there and cost a null comparison.
/// </summary>
public static class FrameThread
{
    private static int _drawing;

    /// <summary>Whether the calling thread is the one drawing, or nothing has claimed drawing yet.</summary>
    public static bool IsCurrent
    {
        get
        {
            var drawing = Volatile.Read(ref _drawing);
            return drawing == 0 || drawing == Environment.CurrentManagedThreadId;
        }
    }

    /// <summary>
    /// Claims the calling thread as the one that draws. Called by the frame loop as it starts; an
    /// application that runs the loop itself calls it too, so that the checks know where "here" is.
    /// </summary>
    /// <returns>A scope that gives the claim up again.</returns>
    public static IDisposable Claim()
    {
        var previous = Interlocked.Exchange(ref _drawing, Environment.CurrentManagedThreadId);
        return new Claimed(previous);
    }

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
            "UiDispatcher.Post, which runs it just before the next frame.");
    }

    private sealed class Claimed : IDisposable
    {
        private readonly int _previous;

        public Claimed(int previous)
        {
            _previous = previous;
        }

        public void Dispose() => Interlocked.Exchange(ref _drawing, _previous);
    }
}
