using System;

namespace Arlecchino.Atoms;

/// <summary>
/// The plumbing that lets a <see cref="Computed{T}"/> discover its dependencies: while it runs, the
/// atoms it reads register themselves here. Implementations of <see cref="IReadableAtom{T}"/> call
/// into it; nothing else needs to.
/// </summary>
internal static class AtomTracking
{
    [ThreadStatic]
    private static Action<Func<Action, IDisposable>>? _track;

    /// <summary>Called by an atom when it is read, so an enclosing computation can depend on it.</summary>
    /// <param name="subscribe">How to subscribe to that atom.</param>
    public static void NoteRead(Func<Action, IDisposable> subscribe) => _track?.Invoke(subscribe);

    /// <summary>Collects reads made on this thread until the returned scope is disposed.</summary>
    /// <param name="track">What to do with each read.</param>
    /// <returns>The scope; disposing it restores the previous collector.</returns>
    public static IDisposable Capture(Action<Func<Action, IDisposable>> track)
    {
        var previous = _track;
        _track = track;
        return new Scope(previous);
    }

    private sealed class Scope : IDisposable
    {
        private readonly Action<Func<Action, IDisposable>>? _previous;

        public Scope(Action<Func<Action, IDisposable>>? previous)
        {
            _previous = previous;
        }

        public void Dispose() => _track = _previous;
    }
}
