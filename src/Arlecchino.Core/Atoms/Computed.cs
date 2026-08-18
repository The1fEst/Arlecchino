using System;
using System.Collections.Generic;

namespace Arlecchino.Atoms;

/// <summary>
/// A value derived from other atoms. It re-evaluates lazily and tracks whatever it read while doing
/// so, including other computed values and branches taken only sometimes — reading
/// <c>other.Value</c> inside the lambda is the subscription.
/// </summary>
/// <typeparam name="T">The kind of derived value.</typeparam>
public sealed class Computed<T> : IReadableAtom<T>
{
    private readonly Func<T> _compute;
    private readonly List<IDisposable> _dependencies = [];
    private readonly Action<Func<Action, IDisposable>> _track;

    private Action[] _listeners = [];
    private T _value = default!;
    private bool _isStale = true;

    /// <summary>Creates a derived value.</summary>
    /// <param name="compute">
    /// How to work it out. Reads of other atoms inside become the dependencies, so it may read
    /// different ones on different runs.
    /// </param>
    public Computed(Func<T> compute)
    {
        _compute = compute;

        var markStale = MarkStale;
        _track = subscribe => _dependencies.Add(subscribe(markStale));
    }

    /// <summary>The derived value, recomputed on the first read after any dependency changed.</summary>
    public T Value
    {
        get
        {
            if (_isStale)
            {
                Recompute();
            }

            if (AtomTracking.IsCapturing)
            {
                AtomTracking.NoteRead(Subscribe);
            }

            return _value;
        }
    }

    /// <summary>Calls back whenever the derived value goes stale.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener)
    {
        if (_isStale)
        {
            Recompute();
        }

        _listeners = [.. _listeners, listener];
        return new Subscription(this, listener);
    }

    private void Recompute()
    {
        foreach (var dependency in _dependencies)
        {
            dependency.Dispose();
        }

        _dependencies.Clear();
        _isStale = false;

        using var tracking = AtomTracking.Capture(_track);
        _value = _compute();
    }

    private void MarkStale()
    {
        if (_isStale)
        {
            return;
        }

        _isStale = true;

        foreach (var listener in _listeners)
        {
            listener();
        }
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

    private sealed class Subscription : IDisposable
    {
        private readonly Computed<T> _source;
        private Action? _listener;

        public Subscription(Computed<T> source, Action listener)
        {
            _source = source;
            _listener = listener;
        }

        public void Dispose()
        {
            if (_listener is null)
            {
                return;
            }

            _source.Unsubscribe(_listener);
            _listener = null;
        }
    }
}
