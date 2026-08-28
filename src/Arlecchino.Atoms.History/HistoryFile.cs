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
public sealed class HistoryFile : IDisposable
{
    /// <summary>What the file is called when the application did not name one.</summary>
    public const string DefaultName = "arlecchino.history";

    private const byte LineFeed = (byte)'\n';

#if NET9_0_OR_GREATER
    private readonly System.Threading.Lock _writing = new();
#else
    private readonly object _writing = new();
#endif

    private readonly StepWriter _writer = new();
    private readonly StepReader _reader = new();

    /// <summary>Where every step written down starts and how long it is, in the order they were made.</summary>
    private readonly List<(long Offset, int Length)> _steps = [];

    /// <summary>
    /// Which step last named each atom, for <see cref="Restore"/>. It is worked out on the first ask,
    /// since a run that restores nothing has no reason to read the file it appends to.
    /// </summary>
    private Dictionary<string, int>? _newest;

    private bool _disposed;

    /// <summary>Names the file. Nothing is created or removed until <see cref="Prepare"/> or the first step.</summary>
    /// <param name="name">
    /// File name, placed beside the program. A name with a directory in it is used as it stands, for an
    /// application that keeps its history somewhere the program folder cannot be written to.
    /// </param>
    public HistoryFile(string? name = null)
    {
        Path = FullPath(string.IsNullOrEmpty(name) ? DefaultName : name);

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
            _writer.Handles(write);
            _reader.Handles(read);
        }
    }

    /// <summary>
    /// Makes the file ready for a run: what an earlier one left is indexed rather than thrown away, so
    /// its steps can be read back and the atoms it names can come up holding what it last saw them hold.
    /// </summary>
    public void Prepare()
    {
        lock (_writing)
        {
            _writer.Close();
            Index();
        }
    }

    /// <summary>Closes and removes the file, losing every step an earlier run wrote down.</summary>
    public void Discard()
    {
        lock (_writing)
        {
            _writer.Close();
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
            if (_disposed || edits.Count == 0 || _writer.Append(Path, edits) is not { } line)
            {
                return false;
            }

            _steps.Add(line);

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

            return document is null ? null : _reader.Rebuild(document.RootElement);
        }
    }

    /// <summary>
    /// Puts one atom back to what the file last saw it hold. It does not become a step, since what is
    /// put back is what the file already says.
    /// </summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="atom">The atom answering to that name.</param>
    /// <returns><c>false</c> when no step names it, or the one that does cannot be read.</returns>
    public bool Restore(string key, IRestorableAtom atom)
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
                    _reader.Restore(written, atom) is not { } edit)
                {
                    continue;
                }

                edit.Redo();

                return true;
            }

            return false;
        }
    }

    /// <summary>Closes the file, leaving it where it is for the next run to read.</summary>
    public void Dispose()
    {
        lock (_writing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            _writer.Close();
        }
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
            using var source = new FileStream(
                Path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);

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

        foreach (var key in _writer.Names)
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
            using var source = new FileStream(
                Path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);

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
