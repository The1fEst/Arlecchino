using System;
using System.Collections.Generic;

using Arlecchino.Atoms.Local;
using Arlecchino.Atoms.Tracked;

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
    private readonly Listeners _listeners = new();

    private T _value;
    private string? _member;

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

    /// <summary>
    /// Hands a value to the drawing thread from wherever you are: it is written just before the next
    /// frame, in the order it was posted, and everything a plain write does — notifying, asking for a
    /// repaint, recording an undo step — happens then. This is what background work calls instead of
    /// <see cref="Value"/>, which refuses a write from another thread.
    ///
    /// The write has not happened when this returns, so reading the atom back here still gives the old
    /// value. Several atoms that have to change together belong in one <c>FrameThread.Post</c> instead,
    /// so that no frame falls between them.
    /// </summary>
    /// <param name="value">The value to write on the drawing thread.</param>
    public void Post(T value) => FrameThread.Post(() => Value = value);

    /// <summary>Calls back whenever the value changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _listeners.Add(listener);

    private void Write(T value, bool recordHistory)
    {
        if (_comparer.Equals(_value, value))
        {
            return;
        }

        FrameThread.Verify(_member ??= FrameMembers.Writing(this));

        var previous = _value;
        _value = value;

        if (recordHistory && RecordsHistory && AtomChanges.IsRecording)
        {
            AtomChanges.NotifyRecorded(new Edit(this, previous, value));
        }

        _listeners.Notify();

        AtomChanges.NotifyWritten();
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
}
