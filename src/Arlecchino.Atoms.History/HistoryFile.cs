using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace Arlecchino.Atoms.History;

/// <summary>
/// Where <see cref="AtomHistory"/> writes its steps, as one JSON array of edits per line in a file beside
/// the program. A run that is killed leaves every complete line readable.
/// </summary>
public sealed class HistoryFile : IDisposable, IValueWriter
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

    private readonly List<(IWritableAtomEdit Edit, string Key)> _step = [];

    private Utf8JsonWriter? _json;
    private FileStream? _stream;
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

        Writes<string>(static (writer, value) => writer.WriteStringValue(value));
        Writes<bool>(static (writer, value) => writer.WriteBooleanValue(value));
        Writes<int>(static (writer, value) => writer.WriteNumberValue(value));
        Writes<long>(static (writer, value) => writer.WriteNumberValue(value));
        Writes<double>(static (writer, value) => writer.WriteNumberValue(value));
        Writes<decimal>(static (writer, value) => writer.WriteNumberValue(value));
        Writes<Guid>(static (writer, value) => writer.WriteStringValue(value));
        Writes<DateOnly>(static (writer, value) => writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture)));
        Writes<TimeOnly>(static (writer, value) => writer.WriteStringValue(value.ToString("O", CultureInfo.InvariantCulture)));
        Writes<DateTimeOffset>(static (writer, value) => writer.WriteStringValue(value));
    }

    /// <summary>Where the file is, whether it exists yet or not.</summary>
    public string Path { get; }

    /// <summary>Whether the file outlives the run rather than being removed on the way out.</summary>
    public bool Keep { get; }

    /// <summary>Whether anything has been written, which is also whether the file is there.</summary>
    public bool Exists => File.Exists(Path);

    /// <summary>
    /// Says how a kind of value is written, once for the kind rather than once for every atom holding it.
    /// Saying it twice replaces what was said the first time.
    /// </summary>
    /// <typeparam name="T">What the atoms hold.</typeparam>
    /// <param name="write">Writes one value as a single JSON value.</param>
    public void Writes<T>(Action<Utf8JsonWriter, T> write)
    {
        ArgumentNullException.ThrowIfNull(write);

        lock (_writing)
        {
            _writers[typeof(T)] = write;
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

    /// <summary>
    /// Makes the file ready for a run, throwing away the scratch file a killed run left behind. A file the
    /// application asked to keep is left alone.
    /// </summary>
    public void Prepare()
    {
        if (!Keep)
        {
            Discard();
        }
    }

    /// <summary>Closes and removes the file, whether the application asked to keep it or not.</summary>
    public void Discard()
    {
        lock (_writing)
        {
            Close();
            TryDelete();
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
        ArgumentNullException.ThrowIfNull(edits);

        lock (_writing)
        {
            if (_disposed || edits.Count == 0 || !CanTake(edits))
            {
                return false;
            }

            Open();
            Compose();

            _json!.Flush();
            _stream!.WriteByte(LineFeed);
            _stream.Flush();

            _json.Reset();

            return true;
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
                if (edit is not IWritableAtomEdit writable ||
                    writable.Key is not { Length: > 0 } key ||
                    !writable.WriteTo(this))
                {
                    return false;
                }

                _step.Add((writable, key));
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
        _stream ??= new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.Read);
        _json ??= new Utf8JsonWriter(_stream);
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
