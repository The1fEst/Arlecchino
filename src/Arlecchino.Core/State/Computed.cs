using System;
using System.Collections.Generic;

namespace Arlecchino.State;

/// <summary>
/// A value derived from other atoms. It re-evaluates lazily and tracks whatever it read while doing
/// so, including other computed values and branches taken only sometimes — reading
/// <c>other.Value</c> inside the lambda is the subscription.
/// </summary>
/// <typeparam name="T">Type of the derived value.</typeparam>
public sealed class Computed<T> : IReadableState<T>
{
    private readonly Func<T> _compute;
    private readonly List<Action> _listeners = [];
    private readonly List<IDisposable> _dependencies = [];

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

            StateTracking.NoteRead(Subscribe);
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

        _listeners.Add(listener);
        return new Subscription(_listeners, listener);
    }

    private void Recompute()
    {
        foreach (var dependency in _dependencies)
        {
            dependency.Dispose();
        }

        _dependencies.Clear();
        _isStale = false;

        using var tracking = StateTracking.Capture(subscribe => _dependencies.Add(subscribe(MarkStale)));
        _value = _compute();
    }

    private void MarkStale()
    {
        if (_isStale)
        {
            return;
        }

        _isStale = true;

        foreach (var listener in _listeners.ToArray())
        {
            listener();
        }
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
