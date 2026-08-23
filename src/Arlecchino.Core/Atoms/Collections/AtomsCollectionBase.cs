using System;

namespace Arlecchino.Atoms.Collections;

/// <summary>
/// What the five collection atoms have in common: who is listening, the check that a change is being
/// made on the drawing thread, and the seam a subclass watches changes through.
/// </summary>
public abstract class AtomsCollectionBase
{
    private readonly Listeners _listeners = new();

    private string? _member;

    /// <summary>Calls back whenever the contents change.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening.</returns>
    public IDisposable Subscribe(Action listener) => _listeners.Add(listener);

    /// <summary>
    /// Whether a subclass is listening, which decides whether the collection describes what changed.
    /// Describing a change copies what was there, so an unwatched collection skips it.
    /// </summary>
    protected virtual bool IsObserved => false;

    /// <summary>Registers the read with whatever is working out a computed value.</summary>
    private protected void Track()
    {
        if (AtomTracking.IsCapturing)
        {
            AtomTracking.NoteRead(Subscribe);
        }
    }

    /// <summary>Refuses a change made from off the drawing thread.</summary>
    /// <exception cref="InvalidOperationException">Called from another thread.</exception>
    private protected void Verify() => FrameThread.Verify(_member ??= FrameMembers.Changing(this));

    /// <summary>Tells the listeners the contents changed, and asks for a frame.</summary>
    private protected void Notify()
    {
        _listeners.Notify();

        AtomChanges.NotifyWritten();
    }
}
