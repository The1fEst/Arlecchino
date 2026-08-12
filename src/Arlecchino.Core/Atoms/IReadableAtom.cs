using System;

namespace Arlecchino.Atoms;

/// <summary>
/// Something that holds a value and tells interested parties when it changes. Implemented by
/// <see cref="Atom{T}"/> and <see cref="Computed{T}"/>.
/// </summary>
/// <typeparam name="T">The kind of value held.</typeparam>
public interface IReadableAtom<out T>
{
    /// <summary>
    /// The current value. Reading it inside a <see cref="Computed{T}"/> registers the dependency, so derived
    /// values need no dependency list.
    /// </summary>
    T Value { get; }

    /// <summary>Calls back whenever the value changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening; a view that subscribes must do so when it goes away.</returns>
    IDisposable Subscribe(Action listener);
}
