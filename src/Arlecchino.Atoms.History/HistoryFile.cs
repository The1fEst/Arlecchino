using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Arlecchino.Atoms.History;

/// <summary>
/// Where <see cref="AtomHistory"/> writes its steps, as one JSON array of edits per line in a file beside
/// the program. A run that is killed leaves every complete line readable.
/// </summary>
public sealed class HistoryFile : IDisposable, IValueWriter, IValueReader
{
    /// <summary>What the file is called when the application did not name one.</summary>
    public const string DefaultName = "arlecchino.history";

    private const byte LineFeed = (byte)'\n';

#if NET9_0_OR_GREATER
    private readonly System.Threading.Lock _writing = new();
#else
    private readonly object _writing = new();
#endif

    /// <summary>
    /// One <c>Action&lt;Utf8JsonWriter, T&gt;</c> per kind of value, held untyped because no one type
    /// covers them all. It is cast back under the key it was filed under, so the cast cannot be wrong.
    /// </summary>
    private readonly Dictionary<Type, object> _writers = [];

    /// <summary>One <c>Func&lt;JsonElement, T&gt;</c> per kind of value, held the same way.</summary>
    private readonly Dictionary<Type, object> _readers = [];

    /// <summary>Where every step written down starts and how long it is, in the order they were made.</summary>
    private readonly List<(long Offset, int Length)> _steps = [];

    /// <summary>
    /// Which step last named each atom, for <see cref="Resume"/>. It is worked out on the first ask,
    /// since a run that never resumes anything has no reason to read the file it appends to.
    /// </summary>
    private Dictionary<string, int>? _newest;

    private readonly List<(IWritableAtomEdit Edit, string Key)> _step = [];

    private Utf8JsonWriter? _json;
    private FileStream? _stream;
    private JsonElement _undone;
    private JsonElement _redone;
    private bool _probing;
    private bool _disposed;

    /// <summary>Names the file. Nothing is created or removed until <see cref="Prepare"/> or the first step.</summary>
    /// <param name="name">
    /// File name, placed beside the program. A name with a directory in it is used as it stands, for an
    /// application that keeps its history somewhere the program folder cannot be written to.
    /// </param>
    /// <param name="keep">
    /// Whether the file outlives the run. The default removes it on the way out, which makes it a scratch
    /// file that only matters while the program is up.
    /// </param>
    public HistoryFile(string? name = null, bool keep = false)
    {
        Path = FullPath(string.IsNullOrEmpty(name) ? DefaultName : name);
        Keep = keep;

        Handles<string>(static (writer, value) => writer.WriteStringValue(value), static element => element.GetString()!);
        Handles(static (writer, value) => writer.WriteBooleanValue(value), static element => element.GetBoolean());
        Handles(static (writer, value) => writer.WriteNumberValue(value), static element => element.GetInt32());
        Handles(static (writer, value) => writer.WriteNumberValue(value), static element => element.GetInt64());
        Handles(static (writer, value) => writer.WriteNumberValue(value), static element => element.GetDouble());
        Handles(static (writer, value) => writer.WriteNumberValue(value), static element => element.GetDecimal());
        Handles(static (writer, value) => writer.WriteStringValue(value), static element => element.GetGuid());
        Handles(static (writer, value) => writer.WriteStringValue(value), static element => element.GetDateTimeOffset());
        Handles(
            static (writer, value) => writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture)),
            static element => DateOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture));
        Handles(
            static (writer, value) => writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture)),
            static element => TimeOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture));
    }

    /// <summary>Where the file is, whether it exists yet or not.</summary>
    public string Path { get; }

    /// <summary>Whether the file outlives the run rather than being removed on the way out.</summary>
    public bool Keep { get; }

    /// <summary>Whether anything has been written, which is also whether the file is there.</summary>
    public bool Exists => File.Exists(Path);

    /// <summary>How many steps the file holds, counting the ones an earlier run left.</summary>
    public int Count
    {
        get
        {
            lock (_writing)
            {
                return _steps.Count;
            }
        }
    }

    /// <summary>
    /// Says how a kind of value is written and read, once for the kind rather than once for every atom
    /// holding it. Saying it twice replaces what was said the first time.
    /// </summary>
    /// <typeparam name="T">What the atoms hold.</typeparam>
    /// <param name="write">Writes one value as a single JSON value.</param>
    /// <param name="read">Reads one value back from what was written.</param>
    public void Handles<T>(Action<Utf8JsonWriter, T> write, Func<JsonElement, T> read)
    {
        lock (_writing)
        {
            _writers[typeof(T)] = write;
            _readers[typeof(T)] = read;
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

        var reader = (Func<JsonElement, T>)stored;

        try
        {
            undone = reader(_undone);
            redone = reader(_redone);

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

    /// <summary>
    /// Makes the file ready for a run. A scratch file is thrown away, since a killed run left it; a file
    /// the application asked to keep is indexed instead, so its steps can be read back.
    /// </summary>
    public void Prepare()
    {
        if (!Keep)
        {
            Discard();
            return;
        }

        lock (_writing)
        {
            Close();
            Index();
        }
    }

    /// <summary>Closes and removes the file, whether the application asked to keep it or not.</summary>
    public void Discard()
    {
        lock (_writing)
        {
            Close();
            TryDelete();
            _steps.Clear();
            _newest = null;
        }
    }

    /// <summary>
    /// Appends one step. The file is opened by the first step that can be written, so a run that recorded
    /// nothing leaves no file.
    /// </summary>
    /// <param name="edits">The edits of the step, in the order they were made.</param>
    /// <returns><c>true</c> when the step was written, <c>false</c> when it was skipped.</returns>
    public bool Write(IReadOnlyList<IAtomEdit> edits)
    {
        lock (_writing)
        {
            if (_disposed || edits.Count == 0 || !CanTake(edits))
            {
                return false;
            }

            Open();

            var offset = _stream!.Position;

            Compose();

            _json!.Flush();
            _stream.WriteByte(LineFeed);
            _stream.Flush();

            _json.Reset();
            _steps.Add((offset, (int)(_stream.Position - offset - 1)));

            Remember(_steps.Count - 1);

            return true;
        }
    }

    /// <summary>
    /// Reads one step back, seeking to it rather than holding the file in memory. The atoms it names are
    /// the ones running now, found by the names they were given.
    /// </summary>
    /// <param name="step">Which step, counting from the oldest the file holds.</param>
    /// <returns>The edits, or <c>null</c> when the step is unreadable or names an atom nothing answers to.</returns>
    public List<IAtomEdit>? Read(int step)
    {
        lock (_writing)
        {
            using var document = Parsed(step);

            return document is null ? null : Rebuild(document.RootElement);
        }
    }

    /// <summary>
    /// Puts one atom back to what the file last saw it hold. It does not become a step, since what is
    /// put back is what the file already says.
    /// </summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="atom">The atom answering to that name.</param>
    /// <returns><c>false</c> when no step names it, or the one that does cannot be read.</returns>
    public bool Resume(string key, IRestorableAtom atom)
    {
        lock (_writing)
        {
            _newest ??= Newest();

            if (!_newest.TryGetValue(key, out var step))
            {
                return false;
            }

            using var document = Parsed(step);

            if (document is null)
            {
                return false;
            }

            foreach (var written in document.RootElement.EnumerateArray())
            {
                if (!written.TryGetProperty("key", out var name) ||
                    name.GetString() != key ||
                    !written.TryGetProperty("from", out _undone) ||
                    !written.TryGetProperty("to", out _redone) ||
                    atom.Restore(this) is not { } edit)
                {
                    continue;
                }

                edit.Redo();

                return true;
            }

            return false;
        }
    }

    /// <summary>Closes the file, and removes it unless the application asked to keep it.</summary>
    public void Dispose()
    {
        lock (_writing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            Close();

            if (!Keep)
            {
                TryDelete();
            }
        }
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

    private static string FullPath(string name) =>
        System.IO.Path.IsPathRooted(name) || name.Contains(System.IO.Path.DirectorySeparatorChar)
            ? name
            : System.IO.Path.Combine(AppContext.BaseDirectory, name);

    /// <summary>
    /// Reads one step off the disk, seeking to it rather than holding the file in memory.
    /// </summary>
    /// <param name="step">Which step, counting from the oldest the file holds.</param>
    /// <returns>The line as JSON, or <c>null</c> when the file no longer has it, or it does not parse.</returns>
    private JsonDocument? Parsed(int step)
    {
        if (step < 0 || step >= _steps.Count)
        {
            return null;
        }

        var (offset, length) = _steps[step];
        var line = ArrayPool<byte>.Shared.Rent(length);

        try
        {
            using var source = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            source.Seek(offset, SeekOrigin.Begin);

            return source.ReadAtLeast(line.AsSpan(0, length), length, throwOnEndOfStream: false) < length
                ? null
                : JsonDocument.Parse(line.AsSpan(0, length).ToArray());
        }
        catch (IOException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(line);
        }
    }

    /// <summary>Notes which step the names just written sit at, for as long as they are being tracked.</summary>
    /// <param name="step">The step they were written by.</param>
    private void Remember(int step)
    {
        if (_newest is null)
        {
            return;
        }

        foreach (var (_, key) in _step)
        {
            _newest[key] = step;
        }
    }

    /// <summary>Which step last named each atom, worked out once and kept up as steps are written.</summary>
    /// <returns>The step every name was last written by.</returns>
    private Dictionary<string, int> Newest()
    {
        var newest = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var step = 0; step < _steps.Count; step++)
        {
            using var document = Parsed(step);

            if (document is not { RootElement.ValueKind: JsonValueKind.Array })
            {
                continue;
            }

            foreach (var written in document.RootElement.EnumerateArray())
            {
                if (written.TryGetProperty("key", out var key) && key.GetString() is { Length: > 0 } name)
                {
                    newest[name] = step;
                }
            }
        }

        return newest;
    }

    /// <summary>
    /// Turns one line back into edits. A line naming an atom nothing answers to is dropped whole, since
    /// half of it would move some atoms and not others.
    /// </summary>
    /// <param name="line">The line as JSON.</param>
    /// <returns>The edits, or <c>null</c> when the line could not be rebuilt.</returns>
    private List<IAtomEdit>? Rebuild(JsonElement line)
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
                    !written.TryGetProperty("from", out _undone) ||
                    !written.TryGetProperty("to", out _redone) ||
                    atom.Restore(this) is not { } edit)
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

    /// <summary>Walks the file the last run left, noting where each line starts and how long it is.</summary>
    private void Index()
    {
        _steps.Clear();
        _newest = null;

        if (!File.Exists(Path))
        {
            return;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(8192);

        try
        {
            using var source = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            long start = 0;
            long at = 0;
            int count;

            while ((count = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (var i = 0; i < count; i++, at++)
                {
                    if (buffer[i] != LineFeed)
                    {
                        continue;
                    }

                    _steps.Add((start, (int)(at - start)));
                    start = at + 1;
                }
            }
        }
        catch (IOException)
        {
            _steps.Clear();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
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
        _probing = true;

        try
        {
            foreach (var edit in edits)
            {
                if (edit is not IWritableAtomEdit writable || !writable.WriteTo(this))
                {
                    return false;
                }

                _step.Add((writable, writable.Key));
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

        foreach (var (edit, key) in _step)
        {
            json.WriteStartObject();
            json.WriteString("key", key);

            edit.WriteTo(this);

            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    private void Open()
    {
        _stream ??= new(Path, FileMode.Append, FileAccess.Write, FileShare.Read);
        _json ??= new(_stream);
    }

    private void Close()
    {
        _json?.Dispose();
        _json = null;

        _stream?.Dispose();
        _stream = null;
    }

    private void TryDelete()
    {
        try
        {
            File.Delete(Path);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
