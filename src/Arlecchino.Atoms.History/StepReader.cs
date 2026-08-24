using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Arlecchino.Atoms.History;

/// <summary>
/// The reading half of a <see cref="HistoryFile"/>: how a kind of value comes back out of JSON, and how
/// one line of it becomes the edits of a step again.
/// </summary>
internal sealed class StepReader : IValueReader
{
    /// <summary>One <c>Func&lt;JsonElement, T&gt;</c> per kind of value, held untyped for the same reason
    /// the writers are.</summary>
    private readonly Dictionary<Type, object> _readers = [];

    private JsonElement _undone;
    private JsonElement _redone;

    /// <summary>
    /// Says how a kind of value is read, once for the kind rather than once for every atom holding it.
    /// Saying it twice replaces what was said the first time.
    /// </summary>
    /// <typeparam name="T">What the atoms hold.</typeparam>
    /// <param name="read">Reads one value back from what was written.</param>
    public void Handles<T>(Func<JsonElement, T> read) => _readers[typeof(T)] = read;

    /// <summary>
    /// Turns one line back into edits. A line naming an atom nothing answers to is dropped whole, since
    /// half of it would move some atoms and not others.
    /// </summary>
    /// <param name="line">The line as JSON.</param>
    /// <returns>The edits, or <c>null</c> when the line could not be rebuilt.</returns>
    public List<IAtomEdit>? Rebuild(JsonElement line)
    {
        if (line.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        try
        {
            var edits = new List<IAtomEdit>();

            foreach (var written in line.EnumerateArray())
            {
                if (!written.TryGetProperty("key", out var key) ||
                    key.GetString() is not { Length: > 0 } name ||
                    !HistoryChanges.TryFind(name, out var atom) ||
                    Restore(written, atom) is not { } edit)
                {
                    return null;
                }

                edits.Add(edit);
            }

            return edits.Count > 0 ? edits : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Builds the edit one written step stands for, for the atom answering to its name.</summary>
    /// <param name="writtenStep">The step as it was written down.</param>
    /// <param name="atom">The atom it names.</param>
    /// <returns>The edit, or <c>null</c> when the step is missing a half or does not read.</returns>
    public IAtomEdit? Restore(JsonElement writtenStep, IRestorableAtom atom) =>
        writtenStep.TryGetProperty("from", out _undone) && writtenStep.TryGetProperty("to", out _redone)
            ? atom.Restore(this)
            : null;

    /// <summary>Hands the two values of the edit being restored back to the atom that asked.</summary>
    /// <typeparam name="T">What the atom holds, supplied by the atom.</typeparam>
    /// <param name="undone">The value undoing puts back.</param>
    /// <param name="redone">The value redoing puts back.</param>
    /// <returns><c>false</c> when this kind of value has no reader, or the values do not parse.</returns>
    bool IValueReader.Read<T>(out T undone, out T redone)
    {
        undone = default!;
        redone = default!;

        if (!_readers.TryGetValue(typeof(T), out var stored))
        {
            return false;
        }

        var readValue = (Func<JsonElement, T>)stored;

        try
        {
            undone = readValue(_undone);
            redone = readValue(_redone);

            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Hands the contents of the collection step being restored back to the collection that asked.</summary>
    /// <typeparam name="T">What the collection holds, supplied by the collection.</typeparam>
    /// <param name="undone">The contents undoing lands on.</param>
    /// <param name="redone">The contents redoing lands on.</param>
    /// <returns><c>false</c> when this kind of item has no reader, or the contents do not parse.</returns>
    bool IValueReader.ReadItems<T>(out T[] undone, out T[] redone)
    {
        undone = [];
        redone = [];

        if (!_readers.TryGetValue(typeof(T), out var stored))
        {
            return false;
        }

        var readItem = (Func<JsonElement, T>)stored;

        try
        {
            return ReadAll(_undone, readItem, out undone) && ReadAll(_redone, readItem, out redone);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Hands the entries of the map step being restored back to the map that asked.</summary>
    /// <typeparam name="TKey">What the entries are looked up by, supplied by the map.</typeparam>
    /// <typeparam name="TValue">What is kept against each key, supplied by the map.</typeparam>
    /// <param name="undone">The entries undoing lands on.</param>
    /// <param name="redone">The entries redoing lands on.</param>
    /// <returns><c>false</c> when either half of an entry has no reader, or the entries do not parse.</returns>
    bool IValueReader.ReadEntries<TKey, TValue>(
        out KeyValuePair<TKey, TValue>[] undone,
        out KeyValuePair<TKey, TValue>[] redone)
    {
        undone = [];
        redone = [];

        if (!_readers.TryGetValue(typeof(TKey), out var storedKey) ||
            !_readers.TryGetValue(typeof(TValue), out var storedValue))
        {
            return false;
        }

        var readKey = (Func<JsonElement, TKey>)storedKey;
        var readValue = (Func<JsonElement, TValue>)storedValue;

        try
        {
            return ReadAll(_undone, readKey, readValue, out undone) &&
                   ReadAll(_redone, readKey, readValue, out redone);
        }
        catch (FormatException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool ReadAll<T>(JsonElement writtenItems, Func<JsonElement, T> read, out T[] values)
    {
        values = [];

        if (writtenItems.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        values = new T[writtenItems.GetArrayLength()];

        var at = 0;

        foreach (var item in writtenItems.EnumerateArray())
        {
            values[at++] = read(item);
        }

        return true;
    }

    private static bool ReadAll<TKey, TValue>(
        JsonElement writtenEntries,
        Func<JsonElement, TKey> readKey,
        Func<JsonElement, TValue> readValue,
        out KeyValuePair<TKey, TValue>[] entries)
    {
        entries = [];

        if (writtenEntries.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        entries = new KeyValuePair<TKey, TValue>[writtenEntries.GetArrayLength()];

        var at = 0;

        foreach (var item in writtenEntries.EnumerateArray())
        {
            if (!item.TryGetProperty("key", out var key) || !item.TryGetProperty("value", out var value))
            {
                return false;
            }

            entries[at++] = new(readKey(key), readValue(value));
        }

        return true;
    }
}
