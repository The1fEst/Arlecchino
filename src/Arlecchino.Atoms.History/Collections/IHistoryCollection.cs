namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// What the shared machinery asks of a history collection. The five have different bases, so what they
/// have in common is taken through this rather than inherited.
/// </summary>
/// <typeparam name="T">What the collection holds, an entry at a time for a map.</typeparam>
internal interface IHistoryCollection<in T>
{
    /// <summary>What it is called in a <see cref="HistoryFile"/>.</summary>
    string Key { get; }

    /// <summary>Makes it hold what a step says it held, by the smallest change that gets there.</summary>
    /// <param name="target">The contents to land on.</param>
    void Land(T[] target);
}
