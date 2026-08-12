using System;

namespace Arlecchino.Atoms;

/// <summary>
/// The subscription list an atom keeps, held in one place, so every kind of atom notifies the same way. The
/// array is replaced rather than mutated, so a listener may unsubscribe while being notified.
/// </summary>
internal sealed class Listeners
{
    private Action[] _listeners = [];

    public IDisposable Add(Action listener)
    {
        _listeners = [.. _listeners, listener];
        return new Subscription(this, listener);
    }

    public void Notify()
    {
        foreach (var listener in _listeners)
        {
            listener();
        }
    }

    private void Remove(Action listener)
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
        private readonly Listeners _listeners;
        private Action? _listener;

        public Subscription(Listeners listeners, Action listener)
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
