using System;

namespace Arlecchino.Atoms;

/// <summary>
/// Something that holds a value and tells interested parties when it changes. Implemented by
/// <see cref="Atom{T}"/> and <see cref="Computed{T}"/>.
/// </summary>
/// <typeparam name="T">The kind of value held.</typeparam>
public interface IReadableAtom<out T> : IReadableAtom
{
    /// <summary>
    /// The current value. Reading it inside a <see cref="Computed{T}"/> registers the dependency, so derived
    /// values need no dependency list.
    /// </summary>
    T Value { get; }

    /// <summary>The value written out, which is what a widget shows when it is given no wording of its own.</summary>
    /// <returns>The text.</returns>
    string ToString();
}

/// <summary>
/// What every atom has in common, whatever it holds: it can be listened to, and it can be written out.
/// It is what a widget takes when it shows a value whose type it has no use for.
/// </summary>
public interface IReadableAtom
{
    /// <summary>What an atom holding nothing writes out as.</summary>
    const string UndefinedValue = "UNDEFINED";

    /// <summary>Calls back whenever the value changes.</summary>
    /// <param name="listener">What to run on change.</param>
    /// <returns>Dispose it to stop listening; a view that subscribes must do so when it goes away.</returns>
    IDisposable Subscribe(Action listener);
}
