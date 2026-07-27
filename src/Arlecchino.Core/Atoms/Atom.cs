using System;
using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// An atom: one piece of application state that notifies what reads it and marks the frame stale by
/// itself, so a screen driven by atoms never needs a manual repaint request.
///
/// Whether an edit can be undone is decided by the type that is created — <see cref="TrackedAtom{T}"/>
/// or <see cref="LocalAtom{T}"/> — rather than by a flag set afterwards, so the declaration says
/// which kind of state it is. Everything that takes an atom takes this base type, so the two are
/// interchangeable at the call site.
/// </summary>
/// <typeparam name="T">Type of the value held.</typeparam>
public abstract class Atom<T> : IReadableAtom<T>
{
    private readonly IEqualityComparer<T> _comparer;

    private Action[] _listeners = [];
    private T _value;

    /// <summary>Creates an atom holding a starting value.</summary>
    /// <param name="initial">The value to start with.</param>
    /// <param name="comparer">
    /// How to decide that a write changed nothing; the default comparer for <typeparamref name="T"/>
    /// is used when omitted.
    /// </param>
    protected Atom(T initial, IEqualityComparer<T>? comparer = null)
    {
        _value = initial;
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    /// <summary>Whether edits of this atom enter the undo history.</summary>
    protected abstract bool RecordsHistory { get; }

    /// <summary>
    /// The value. Writing an equal value changes nothing and notifies nobody; any other write
    /// notifies subscribers, asks for a repaint, and records an undo step when the atom is undoable.
    /// </summary>
    public T Value
    {
        get
        {
            if (AtomTracking.IsCapturing)
            {
                AtomTracking.NoteRead(Subscribe);
            }

            return _value;
        }
        set => Write(value, recordHistory: true);
    }

    /// <summary>Calls back whenever the value changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener)
    {
        _listeners = [.. _listeners, listener];
        return new Subscription(this, listener);
    }

    private void Write(T value, bool recordHistory)
    {
        if (_comparer.Equals(_value, value))
        {
            return;
        }

        FrameThread.Verify($"Writing {GetType().Name}");

        var previous = _value;
        _value = value;

        if (recordHistory && RecordsHistory && AtomChanges.IsRecording)
        {
            AtomChanges.NotifyRecorded(new Edit(this, previous, value));
        }

        foreach (var listener in _listeners)
        {
            listener();
        }

        AtomChanges.NotifyWritten();
    }

    private void Unsubscribe(Action listener)
    {
        var index = Array.IndexOf(_listeners, listener);
        if (index < 0)
        {
            return;
        }

        var remaining = new Action[_listeners.Length - 1];
        Array.Copy(_listeners, remaining, index);
        Array.Copy(_listeners, index + 1, remaining, index, remaining.Length - index);
        _listeners = remaining;
    }

    private sealed class Edit : IAtomEdit
    {
        private readonly Atom<T> _state;
        private readonly T _before;
        private readonly T _after;

        public Edit(Atom<T> state, T before, T after)
        {
            _state = state;
            _before = before;
            _after = after;
        }

        public object Owner => _state;

        public void Undo() => _state.Write(_before, recordHistory: false);

        public void Redo() => _state.Write(_after, recordHistory: false);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly Atom<T> _atom;
        private Action? _listener;

        public Subscription(Atom<T> atom, Action listener)
        {
            _atom = atom;
            _listener = listener;
        }

        public void Dispose()
        {
            if (_listener is null)
            {
                return;
            }

            _atom.Unsubscribe(_listener);
            _listener = null;
        }
    }
}
