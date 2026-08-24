namespace Arlecchino.Atoms.History.Collections;

/// <summary>
/// What every recorded change of a collection has in common: who it belongs to, and the contents either
/// side of it that a <see cref="HistoryFile"/> writes down.
/// </summary>
/// <typeparam name="TOwner">The collection the change was made to.</typeparam>
/// <typeparam name="T">What that collection holds.</typeparam>
internal abstract class ContentsEdit<TOwner, T> : IWritableAtomEdit
    where TOwner : class, IHistoryCollection<T>
{
    private readonly T[]? _undone;
    private readonly T[]? _redone;

    /// <summary>Takes the collection and what it held either side of the change.</summary>
    /// <param name="owner">The collection.</param>
    /// <param name="undone">What it held before, or nothing when no history is writing steps down.</param>
    /// <param name="redone">What it holds after, or nothing for the same reason.</param>
    protected ContentsEdit(TOwner owner, T[]? undone, T[]? redone)
    {
        Collection = owner;
        _undone = undone;
        _redone = redone;
    }

    /// <inheritdoc />
    public object Owner => Collection;

    /// <inheritdoc />
    public string Key => Collection.Key;

    /// <summary>The collection the change was made to.</summary>
    protected TOwner Collection { get; }

    /// <inheritdoc />
    public bool WriteTo(IValueWriter writer) =>
        _undone is not null && _redone is not null && Write(writer, _undone, _redone);

    /// <inheritdoc />
    public abstract void Undo();

    /// <inheritdoc />
    public abstract void Redo();

    /// <summary>Hands the contents over as the kind of thing the collection holds.</summary>
    /// <param name="writer">Where they go.</param>
    /// <param name="undone">What the collection held before the change.</param>
    /// <param name="redone">What it holds after it.</param>
    /// <returns><c>false</c> when the writer could not take them.</returns>
    protected virtual bool Write(IValueWriter writer, T[] undone, T[] redone) =>
        writer.WriteItems(undone, redone);
}
