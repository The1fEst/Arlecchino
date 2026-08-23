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

            return true;
        }
    }

    /// <summary>
    /// Reads one step back, seeking to it rather than holding the file in memory. The atoms it names are
    /// the ones running now, found by their <see cref="HistoryAtom{T}.Key"/>.
    /// </summary>
    /// <param name="step">Which step, counting from the oldest the file holds.</param>
    /// <returns>The edits, or <c>null</c> when the step is unreadable or names an atom nothing answers to.</returns>
    public List<IAtomEdit>? Read(int step)
    {
        lock (_writing)
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
                    : Rebuild(line.AsSpan(0, length));
            }
            catch (IOException)
            {
                return null;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(line);
            }
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

    private static string FullPath(string name) =>
        System.IO.Path.IsPathRooted(name) || name.Contains(System.IO.Path.DirectorySeparatorChar)
            ? name
            : System.IO.Path.Combine(AppContext.BaseDirectory, name);

    /// <summary>
    /// Turns one line back into edits. A line naming an atom nothing answers to is dropped whole, since
    /// half of it would move some atoms and not others.
    /// </summary>
    /// <param name="line">The bytes of the line, without its line feed.</param>
    /// <returns>The edits, or <c>null</c> when the line could not be rebuilt.</returns>
    private List<IAtomEdit>? Rebuild(ReadOnlySpan<byte> line)
    {
        try
        {
            using var document = JsonDocument.Parse(line.ToArray());

            var edits = new List<IAtomEdit>();

            foreach (var written in document.RootElement.EnumerateArray())
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
