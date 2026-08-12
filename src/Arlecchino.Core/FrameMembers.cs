using System;
using System.Collections.Generic;
using Arlecchino.Atoms;

namespace Arlecchino;

/// <summary>
/// The names <see cref="FrameThread.Verify"/> puts in the message a thread gets when it writes what a frame
/// draws. Every one is built from <c>typeof</c> and <c>nameof</c>, so a rename leaves no stale wording.
/// </summary>
internal static class FrameMembers
{
    /// <summary>Names a member that has to be touched on the drawing thread.</summary>
    /// <typeparam name="T">The type declaring it.</typeparam>
    /// <param name="member">Its name, from <c>nameof</c>.</param>
    /// <returns>The name to check under.</returns>
    public static string Of<T>(string member) => $"{typeof(T).Name}.{member}";

    /// <summary>
    /// Names a member of a static class, which cannot go through <see cref="Of{T}"/> because a static
    /// type is not allowed as a type argument.
    /// </summary>
    /// <param name="declaring">The type declaring it, from <c>typeof</c>.</param>
    /// <param name="member">Its name, from <c>nameof</c>.</param>
    /// <returns>The name to check under.</returns>
    public static string Of(Type declaring, string member) => $"{declaring.Name}.{member}";

    /// <summary>Names an atom being given a new value.</summary>
    /// <typeparam name="T">The kind of value it holds, inferred.</typeparam>
    /// <param name="atom">The atom itself, named by the type it turns out to be.</param>
    /// <returns>The name to check under.</returns>
    public static string Writing<T>(IReadableAtom<T> atom) => $"Writing {atom.GetType().Name}";

    /// <summary>
    /// Names a collection atom being changed. Takes <see cref="IReadOnlyCollection{T}"/> rather than
    /// <see cref="IReadOnlyList{T}"/> because that is what all five families have in common — a map
    /// holds pairs and a set holds no order — and covariance carries each of them to it.
    /// </summary>
    /// <typeparam name="T">What the collection holds, inferred.</typeparam>
    /// <param name="atoms">The collection itself, named by the type it turns out to be.</param>
    /// <returns>The name to check under.</returns>
    public static string Changing<T>(IReadableAtom<IReadOnlyCollection<T>> atoms) => $"Changing {atoms.GetType().Name}";
}
