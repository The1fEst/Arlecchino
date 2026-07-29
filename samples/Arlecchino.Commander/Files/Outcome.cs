using System;
using System.Collections.Generic;
using System.Threading;
using Arlecchino.Commander.Model;

namespace Arlecchino.Commander.Files;

public sealed class Outcome
{
    private readonly Lock _gate = new();
    private readonly List<string> _errors = [];

    private int _files;
    private int _folders;
    private long _bytes;
    private string _current = "";
    private Tally _planned;
    private bool _measured;
    private bool _swept;

    public int Files => Volatile.Read(ref _files);

    public int Folders => Volatile.Read(ref _folders);

    public long Bytes => Interlocked.Read(ref _bytes);

    public string Current => Volatile.Read(ref _current);

    public IReadOnlyList<string> Errors
    {
        get
        {
            lock (_gate)
            {
                return [.. _errors];
            }
        }
    }

    public bool Failed
    {
        get
        {
            lock (_gate)
            {
                return _errors.Count > 0;
            }
        }
    }

    /// <summary>Whether the work has been sized up, which is when a bar can be drawn for it.</summary>
    public bool IsMeasured => Volatile.Read(ref _measured);

    /// <summary>What there is to do in total, counted before the work started.</summary>
    public Tally Planned
    {
        get
        {
            lock (_gate)
            {
                return _planned;
            }
        }
    }

    /// <summary>
    /// How far along the work is, from <c>0</c> to <c>1</c>. Bytes carry the answer when they are
    /// being moved and counts carry it otherwise, so a delete — which shifts no bytes — still fills
    /// the bar as it works through the tree.
    /// </summary>
    public double Share
    {
        get
        {
            var total = Planned;

            if (total.Bytes > 0 && Bytes > 0)
            {
                return Math.Clamp((double)Bytes / total.Bytes, 0, 1);
            }

            return total.Items > 0 ? Math.Clamp((double)(Files + Folders) / total.Items, 0, 1) : 0;
        }
    }

    public void Planning(Tally total)
    {
        lock (_gate)
        {
            _planned = total;
        }

        Volatile.Write(ref _measured, true);
    }

    public void Reached(string name) => Volatile.Write(ref _current, name);

    public void Counted(long bytes)
    {
        Interlocked.Increment(ref _files);
        Interlocked.Add(ref _bytes, bytes);
    }

    public void CountedFolder() => Interlocked.Increment(ref _folders);

    /// <summary>
    /// Counts a folder the source removed whole. What was inside it was never listed, so the counts
    /// say so rather than pretending one folder was all there was to it.
    /// </summary>
    public void Swept()
    {
        CountedFolder();
        Volatile.Write(ref _swept, true);
    }

    public void Failing(string what, string why)
    {
        lock (_gate)
        {
            _errors.Add($"{what}: {why}");
        }
    }

    public void Absorb(Outcome other)
    {
        Interlocked.Add(ref _files, other.Files);
        Interlocked.Add(ref _folders, other.Folders);
        Interlocked.Add(ref _bytes, other.Bytes);

        lock (_gate)
        {
            _errors.AddRange(other.Errors);
        }
    }

    /// <summary>What has been done so far, for a status line that is redrawn while the work runs.</summary>
    /// <returns>The counts, and the item being worked on when there is one.</returns>
    public string Progress()
    {
        if (!IsMeasured)
        {
            return "counting…";
        }

        var total = Planned;
        var done = total.Items > 0 ? $"{Files + Folders} of {total.Items}" : Counts();

        return Current.Length == 0 ? done : $"{done} · {Current}";
    }

    public string Describe(string verb)
    {
        var counted = Counts();
        var failures = Errors.Count;

        if (counted.Length == 0)
        {
            return failures == 0 ? "Nothing to do" : $"{verb} nothing, {failures} failed";
        }

        var done = Volatile.Read(ref _swept) ? $"{verb} {counted} with everything under them" : $"{verb} {counted}";

        return failures == 0 ? done : $"{done}; {failures} failed";
    }

    private string Counts()
    {
        var parts = new List<string>(3);

        if (Files > 0)
        {
            parts.Add(Files == 1 ? "1 file" : $"{Files} files");
        }

        if (Folders > 0)
        {
            parts.Add(Folders == 1 ? "1 folder" : $"{Folders} folders");
        }

        if (parts.Count == 0)
        {
            return "";
        }

        if (Bytes > 0)
        {
            parts.Add($"{Sizes.Grouped(Bytes)} bytes");
        }

        return string.Join(", ", parts);
    }
}
