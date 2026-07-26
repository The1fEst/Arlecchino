using System;

namespace Arlecchino.State;

/// <summary>
/// Something that holds a value and tells interested parties when it changes. Implemented by
/// <see cref="Atom{T}"/> and <see cref="Computed{T}"/>.
/// </summary>
/// <typeparam name="T">Type of the value held.</typeparam>
public interface IReadableState<out T>
{
    /// <summary>
    /// The current value. Reading it inside a <see cref="Computed{T}"/> also registers the
    /// dependency, which is why derived values need no dependency list.
    /// </summary>
    T Value { get; }

    /// <summary>Calls back whenever the value changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening; a view that subscribes must do so when it goes away.</returns>
    IDisposable Subscribe(Action listener);
}
