using System.Collections.Generic;

namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// What the history collections do with what they hold: copy it for a step, work out what it was before
/// one, and work out the smallest change from one contents to another.
/// </summary>
internal static class Contents
{
    /// <summary>What a collection holds now, copied, or nothing when no history is writing steps down.</summary>
    /// <typeparam name="T">What the collection holds.</typeparam>
    /// <param name="value">What it holds now.</param>
    /// <returns>The contents, or <c>null</c> when nothing would be written with them.</returns>
    public static T[]? Copied<T>(IEnumerable<T> value) => HistoryChanges.IsWriting ? [.. value] : null;

    /// <summary>The same contents without a span, which is what they were before it was put in.</summary>
    /// <typeparam name="T">What the collection holds.</typeparam>
    /// <param name="contents">What it holds now, or nothing when it was not copied.</param>
    /// <param name="index">Where the span starts.</param>
    /// <param name="count">How long it is.</param>
    /// <returns>The contents without it, or <c>null</c> when there were none to work from.</returns>
    public static T[]? Without<T>(T[]? contents, int index, int count) =>
        contents is null ? null : [.. contents[..index], .. contents[(index + count)..]];

    /// <summary>The same contents with a span put back where it was taken out.</summary>
    /// <typeparam name="T">What the collection holds.</typeparam>
    /// <param name="contents">What it holds now, or nothing when it was not copied.</param>
    /// <param name="index">Where the span goes.</param>
    /// <param name="items">What to put back.</param>
    /// <returns>The contents with it, or <c>null</c> when there were none to work from.</returns>
    public static T[]? With<T>(T[]? contents, int index, T[] items) =>
        contents is null ? null : [.. contents[..index], .. items, .. contents[index..]];

    /// <summary>The same contents with one item put back as it was.</summary>
    /// <typeparam name="T">What the collection holds.</typeparam>
    /// <param name="contents">What it holds now, or nothing when it was not copied.</param>
    /// <param name="index">Which item.</param>
    /// <param name="item">What it was before.</param>
    /// <returns>The contents with it, or <c>null</c> when there were none to work from.</returns>
    public static T[]? With<T>(T[]? contents, int index, T item)
    {
        if (contents is null)
        {
            return null;
        }

        var copy = (T[])contents.Clone();

        copy[index] = item;

        return copy;
    }

    /// <summary>
    /// The smallest splice that turns one contents into another: the ends they already agree on stay
    /// where they are, and only the middle is taken out and put back.
    /// </summary>
    /// <typeparam name="T">What the collection holds.</typeparam>
    /// <param name="contents">What it holds now.</param>
    /// <param name="target">What it should hold.</param>
    /// <param name="comparer">How the two are matched item by item.</param>
    /// <returns>Where the middle starts, how much of it goes, and what takes its place.</returns>
    public static (int Index, int Count, T[] Items) Difference<T>(
        IReadOnlyList<T> contents,
        T[] target,
        IEqualityComparer<T> comparer)
    {
        var head = 0;

        while (head < contents.Count && head < target.Length && comparer.Equals(contents[head], target[head]))
        {
            head++;
        }

        var tail = 0;

        while (head + tail < contents.Count &&
               head + tail < target.Length &&
               comparer.Equals(contents[contents.Count - 1 - tail], target[target.Length - 1 - tail]))
        {
            tail++;
        }

        return (head, contents.Count - head - tail, target[head..^tail]);
    }
}
