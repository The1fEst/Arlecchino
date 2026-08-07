using System;
using System.Collections.Generic;
using Arlecchino.Atoms;

namespace Arlecchino;

/// <summary>
/// What <see cref="FrameThread.Verify"/> is told it is checking — the names that end up in the message
/// a thread gets when it writes what a frame draws. They are built here, so the wording stays one
/// wording, and so that no call site invents a string of its own.
///
/// Nothing in here is spelled out: the type comes from <c>typeof</c> and the member from
/// <c>nameof</c>, which is why a rename cannot leave a stale message behind. <see cref="Of{T}"/> takes
/// the type as a parameter rather than reading it here because the members that are checked live in the
/// <c>Arlecchino</c> package, which references this one and not the other way round.
///
/// The atom families take the atom rather than its type, and are named from the type it turns out to be. The
/// check sits in the abstract base, where <c>typeof</c> would say <c>Atom`1</c> for every atom in the
/// application instead of naming the one that was written to.
///
/// A name is built once and kept: the caller holds it in a <c>static readonly</c> where the member is
/// fixed, or in a field beside the value where the name depends on what the type turned out to be.
/// Nothing here caches on its own, because the argument to <see cref="FrameThread.Verify"/> is worked
/// out before the check runs — so a name rebuilt every time would be paid for on every frame that is
/// perfectly fine.
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
