using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Arlecchino.Atoms.History;

/// <summary>
/// The writing half of a <see cref="HistoryFile"/>: how a kind of value goes into JSON, and how one step
/// is appended to the end of the file as a line of its own.
/// </summary>
internal sealed class StepWriter : IValueWriter
{
    private const byte LineFeed = (byte)'\n';

    /// <summary>
    /// One <c>Action&lt;Utf8JsonWriter, T&gt;</c> per kind of value, held untyped because no one type
    /// covers them all. It is cast back under the key it was filed under, so the cast cannot be wrong.
    /// </summary>
    private readonly Dictionary<Type, object> _writers = [];

    /// <summary>The edits of the step being written, gathered by <see cref="CanTake"/>.</summary>
    private readonly List<IWritableAtomEdit> _step = [];

    private readonly List<string> _names = [];

    private Utf8JsonWriter? _json;
    private bool _probing;

    /// <summary>What the step last written down was named by, one name per edit of it.</summary>
    public IReadOnlyList<string> Names => _names;

    /// <summary>
    /// Says how a kind of value is written, once for the kind rather than once for every atom holding it.
    /// Saying it twice replaces what was said the first time.
    /// </summary>
    /// <typeparam name="T">What the atoms hold.</typeparam>
    /// <param name="write">Writes one value as a single JSON value.</param>
    public void Handles<T>(Action<Utf8JsonWriter, T> write) => _writers[typeof(T)] = write;

    /// <summary>
    /// Appends one step as a line, opening the file for it and closing it again. A run that recorded
    /// nothing leaves none, and one that is recording still lets it be read or removed.
    /// </summary>
    /// <param name="path">Where the file is.</param>
    /// <param name="edits">The edits of the step, in the order they were made.</param>
    /// <returns>Where the line starts and how long it is, or <c>null</c> when the step was skipped.</returns>
    public (long Offset, int Length)? Append(string path, IReadOnlyList<IAtomEdit> edits)
    {
        if (!CanTake(edits))
        {
            return null;
        }

        using var stream = new FileStream(
            path,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite | FileShare.Delete);

        using var json = new Utf8JsonWriter(stream);

        _json = json;

        try
        {
            var offset = stream.Position;

            Compose();

            json.Flush();
            stream.WriteByte(LineFeed);
            stream.Flush();

            return (offset, (int)(stream.Position - offset - 1));
        }
        finally
        {
            _json = null;
        }
    }

    /// <summary>Takes the two values of one edit, which the edit hands over typed.</summary>
    /// <typeparam name="T">What the atom holds, supplied by the edit.</typeparam>
    /// <param name="undone">The value undoing puts back.</param>
    /// <param name="redone">The value redoing puts back.</param>
    /// <returns><c>false</c> when this kind of value has no writer.</returns>
    bool IValueWriter.Write<T>(T undone, T redone)
    {
        if (!_writers.TryGetValue(typeof(T), out var stored))
        {
            return false;
        }

        if (_probing)
        {
            return true;
        }

        var write = (Action<Utf8JsonWriter, T>)stored;
        var json = _json!;

        json.WritePropertyName("from");
        write(json, undone);

        json.WritePropertyName("to");
        write(json, redone);

        return true;
    }

    /// <summary>Takes the contents a collection edit stands between, which the edit hands over typed.</summary>
    /// <typeparam name="T">What the collection holds, supplied by the edit.</typeparam>
    /// <param name="undone">The contents undoing lands on.</param>
    /// <param name="redone">The contents redoing lands on.</param>
    /// <returns><c>false</c> when this kind of item has no writer.</returns>
    bool IValueWriter.WriteItems<T>(T[] undone, T[] redone)
    {
        if (!_writers.TryGetValue(typeof(T), out var stored))
        {
            return false;
        }

        if (_probing)
        {
            return true;
        }

        var write = (Action<Utf8JsonWriter, T>)stored;
        var json = _json!;

        WriteAll(json, "from", write, undone);
        WriteAll(json, "to", write, redone);

        return true;
    }

    /// <summary>Takes the entries a map edit stands between, which the edit hands over typed.</summary>
    /// <typeparam name="TKey">What the entries are looked up by, supplied by the edit.</typeparam>
    /// <typeparam name="TValue">What is kept against each key, supplied by the edit.</typeparam>
    /// <param name="undone">The entries undoing lands on.</param>
    /// <param name="redone">The entries redoing lands on.</param>
    /// <returns><c>false</c> when either half of an entry has no writer.</returns>
    bool IValueWriter.WriteEntries<TKey, TValue>(
        KeyValuePair<TKey, TValue>[] undone,
        KeyValuePair<TKey, TValue>[] redone)
    {
        if (!_writers.TryGetValue(typeof(TKey), out var storedKey) ||
            !_writers.TryGetValue(typeof(TValue), out var storedValue))
        {
            return false;
        }

        if (_probing)
        {
            return true;
        }

        var writeKey = (Action<Utf8JsonWriter, TKey>)storedKey;
        var writeValue = (Action<Utf8JsonWriter, TValue>)storedValue;
        var json = _json!;

        WriteAll(json, "from", writeKey, writeValue, undone);
        WriteAll(json, "to", writeKey, writeValue, redone);

        return true;
    }

    private static void WriteAll<T>(Utf8JsonWriter json, string name, Action<Utf8JsonWriter, T> write, T[] values)
    {
        json.WriteStartArray(name);

        foreach (var value in values)
        {
            write(json, value);
        }

        json.WriteEndArray();
    }

    private static void WriteAll<TKey, TValue>(
        Utf8JsonWriter json,
        string name,
        Action<Utf8JsonWriter, TKey> writeKey,
        Action<Utf8JsonWriter, TValue> writeValue,
        KeyValuePair<TKey, TValue>[] entries)
    {
        json.WriteStartArray(name);

        foreach (var entry in entries)
        {
            json.WriteStartObject();

            json.WritePropertyName("key");
            writeKey(json, entry.Key);

            json.WritePropertyName("value");
            writeValue(json, entry.Value);

            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    /// <summary>
    /// Asks every edit whether it could be written, and gathers the ones that answered. A step half
    /// written would move some atoms and not others when it is read back, so a step is taken whole.
    /// </summary>
    /// <param name="edits">The edits of the step.</param>
    /// <returns><c>true</c> when every edit can be written.</returns>
    private bool CanTake(IReadOnlyList<IAtomEdit> edits)
    {
        _step.Clear();
        _names.Clear();
        _probing = true;

        try
        {
            foreach (var edit in edits)
            {
                if (edit is not IWritableAtomEdit writable || !writable.WriteTo(this))
                {
                    return false;
                }

                _step.Add(writable);
                _names.Add(writable.Key);
            }

            return true;
        }
        finally
        {
            _probing = false;
        }
    }

    /// <summary>
    /// Writes the step gathered by <see cref="CanTake"/> into the writer's own buffer, where it waits
    /// until it is flushed.
    /// </summary>
    private void Compose()
    {
        var json = _json!;

        json.WriteStartArray();

        for (var at = 0; at < _step.Count; at++)
        {
            json.WriteStartObject();
            json.WriteString("key", _names[at]);

            _step[at].WriteTo(this);

            json.WriteEndObject();
        }

        json.WriteEndArray();
    }
}
