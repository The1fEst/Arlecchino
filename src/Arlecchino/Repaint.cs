using System;
using System.Threading;
using Arlecchino.Atoms;

namespace Arlecchino;

/// <summary>
/// The "this frame is stale" signal the render loop waits on. Input, navigation, state changes and
/// atom writes raise it for you; raise it yourself when something else changes what a view draws.
/// </summary>
public sealed class Repaint : IDisposable
{
    private int _requested = 1;

    /// <summary>Starts listening for atom writes, so any of them marks the frame stale.</summary>
    public Repaint()
    {
        AtomChanges.Written += Request;
    }

    /// <summary>Whether a frame is owed. Reading it does not consume the request.</summary>
    public bool IsRequested => Volatile.Read(ref _requested) == 1;

    /// <summary>Asks for a frame. Safe to call from any thread and cheap to call repeatedly.</summary>
    public void Request() => Interlocked.Exchange(ref _requested, 1);

    /// <summary>Consumes the request; the render loop calls this once per tick.</summary>
    /// <returns><c>true</c> when a frame was owed.</returns>
    public bool TakeRequested() => Interlocked.Exchange(ref _requested, 0) == 1;

    /// <summary>Stops listening for atom writes.</summary>
    public void Dispose() => AtomChanges.Written -= Request;
}
