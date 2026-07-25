using System;
using System.Collections.Generic;

namespace Arlecchino.State;

/// <summary>
/// An atom: one piece of application state that notifies what reads it and marks the frame stale by
/// itself, so a screen driven by atoms never needs a manual repaint request.
/// </summary>
/// <typeparam name="T">Type of the value held.</typeparam>
public sealed class State<T> : IReadableState<T>
{
    private readonly List<Action> _listeners = [];
    private readonly IEqualityComparer<T> _comparer;

    private T _value;

    /// <summary>Creates an atom holding a starting value.</summary>
    /// <param name="initial">The value to start with.</param>
    /// <param name="comparer">
    /// How to decide that a write changed nothing; the default comparer for <typeparamref name="T"/>
    /// is used when omitted.
    /// </param>
    public State(T initial, IEqualityComparer<T>? comparer = null)
    {
        _value = initial;
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    /// <summary>
    /// Whether edits of this atom enter the undo history. On by default; turn it off for state that
    /// should not be undoable, such as a cursor position or a load in progress.
    /// </summary>
    public bool RecordsHistory { get; init; } = true;

    /// <summary>
    /// The value. Writing an equal value changes nothing and notifies nobody; any other write
    /// notifies subscribers, records an undo step and asks for a repaint.
    /// </summary>
    public T Value
    {
        get
        {
            StateTracking.NoteRead(Subscribe);
            return _value;
        }
        set => Write(value, recordHistory: true);
    }

    /// <summary>Calls back whenever the value changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener)
    {
        _listeners.Add(listener);
        return new Subscription(_listeners, listener);
    }

    /// <summary>
    /// Writes without recording an undo step — for restoring a value or for changes the user did not
    /// make.
    /// </summary>
    /// <param name="value">The value to store.</param>
    public void SetWithoutHistory(T value) => Write(value, recordHistory: false);

    private void Write(T value, bool recordHistory)
    {
        if (_comparer.Equals(_value, value))
        {
            return;
        }

        var previous = _value;
        _value = value;

        if (recordHistory && RecordsHistory && StateChanges.IsRecording)
        {
            StateChanges.NotifyRecorded(new Edit(this, previous, value));
        }

        foreach (var listener in _listeners.ToArray())
        {
            listener();
        }

        StateChanges.NotifyWritten();
    }

    private sealed class Edit : IStateEdit
    {
        private readonly State<T> _state;
        private readonly T _before;
        private readonly T _after;

        public Edit(State<T> state, T before, T after)
        {
            _state = state;
            _before = before;
            _after = after;
        }

        public object Owner => _state;

        public void Undo() => _state.SetWithoutHistory(_before);

        public void Redo() => _state.SetWithoutHistory(_after);
    }

    private sealed class Subscription : IDisposable
    {
        private readonly List<Action> _listeners;
        private Action? _listener;

        public Subscription(List<Action> listeners, Action listener)
        {
            _listeners = listeners;
            _listener = listener;
        }

        public void Dispose()
        {
            if (_listener is null)
            {
                return;
            }

            _listeners.Remove(_listener);
            _listener = null;
        }
    }
}
