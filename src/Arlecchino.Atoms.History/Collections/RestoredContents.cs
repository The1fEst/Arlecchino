namespace Arlecchino.Atoms.History.Collections;

/// <summary>What a step read back from the file puts a collection on, either side of it.</summary>
/// <typeparam name="T">What the collection holds, an entry at a time for a map.</typeparam>
internal sealed class RestoredContents<T> : IAtomEdit
{
    private readonly IHistoryCollection<T> _collection;
    private readonly T[] _undone;
    private readonly T[] _redone;

    /// <summary>Takes the collection and the contents the step stands between.</summary>
    /// <param name="collection">The collection the step names.</param>
    /// <param name="undone">The contents undoing lands on.</param>
    /// <param name="redone">The contents redoing lands on.</param>
    public RestoredContents(IHistoryCollection<T> collection, T[] undone, T[] redone)
    {
        _collection = collection;
        _undone = undone;
        _redone = redone;
    }

    /// <inheritdoc />
    public object Owner => _collection;

    /// <inheritdoc />
    public void Undo() => _collection.Land(_undone);

    /// <inheritdoc />
    public void Redo() => _collection.Land(_redone);
}
