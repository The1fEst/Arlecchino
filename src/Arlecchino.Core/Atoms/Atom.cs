using System;
using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// One piece of application state that notifies what reads it and marks the frame stale by itself.
/// </summary>
/// <typeparam name="T">The kind of value held.</typeparam>
public class Atom<T> : IReadableAtom<T>
{
    private readonly IEqualityComparer<T> _comparer;
    private readonly Listeners _listeners = new();

    private T _value;
    private string? _member;

    /// <summary>Creates an atom holding a starting value.</summary>
    /// <param name="initial">The value to start with.</param>
    /// <param name="comparer">
    /// How to decide that writing to it changed nothing; the default comparer for <typeparamref name="T"/>
    /// is used when omitted.
    /// </param>
    public Atom(T initial, IEqualityComparer<T>? comparer = null)
    {
        _value = initial;
        _comparer = comparer ?? EqualityComparer<T>.Default;
    }

    /// <summary>
    /// The value. Writing an equal value changes nothing; any other write notifies subscribers and asks
    /// for a repaint.
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
        set => Write(value, announce: true);
    }

    /// <summary>
    /// Hands a value to the drawing thread, to be written just before the next frame in the order it was
    /// posted. Nothing is written when this returns, and atoms that must change together belong in one
    /// <c>FrameThread.Post</c>.
    /// </summary>
    /// <param name="value">The value to write on the drawing thread.</param>
    public void Post(T value) => FrameThread.Post(() => Value = value);

    /// <summary>Calls back whenever the value changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _listeners.Add(listener);

    /// <summary>
    /// Called once the value has changed and before subscribers hear of it. The atom does nothing with it;
    /// it is where a subclass keeps a record of its own, and <see cref="WriteSilently"/> does not reach it.
    /// </summary>
    /// <param name="previous">What the atom held.</param>
    /// <param name="current">What it holds now.</param>
    protected virtual void OnWritten(T previous, T current) { }

    /// <summary>
    /// Writes without reaching <see cref="OnWritten"/>. It is how a subclass puts a value back: replaying
    /// what it recorded must not be recorded again.
    /// </summary>
    /// <param name="value">The value to write.</param>
    protected void WriteSilently(T value) => Write(value, announce: false);

    private void Write(T value, bool announce)
    {
        if (_comparer.Equals(_value, value))
        {
            return;
        }

        FrameThread.Verify(_member ??= FrameMembers.Writing(this));

        var previous = _value;
        _value = value;

        if (announce)
        {
            OnWritten(previous, value);
        }

        _listeners.Notify();

        AtomChanges.NotifyWritten();
    }
}
