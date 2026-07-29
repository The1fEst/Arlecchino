using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Commander.Model;

namespace Arlecchino.Commander.Files;

/// <summary>How much there is to do, counted before the work starts so a bar has a denominator.</summary>
/// <param name="Files">How many files the sources hold, at every depth.</param>
/// <param name="Folders">How many folders, the sources themselves included.</param>
/// <param name="Bytes">How much the files add up to.</param>
public readonly record struct Tally(int Files, int Folders, long Bytes)
{
    /// <summary>Files and folders together, which is what a delete works through.</summary>
    public int Items => Files + Folders;
}

public static class FileTasks
{
    /// <summary>
    /// Walks the sources without touching them, so the work that follows knows how far along it is. A
    /// folder is counted along with everything under it, which is what makes a bar for a delete of a
    /// deep tree mean anything.
    /// </summary>
    /// <param name="source">Where the entries live.</param>
    /// <param name="entries">What is about to be worked on.</param>
    /// <param name="token">Stops the count when the operation is cancelled before it starts.</param>
    /// <returns>What was found; zeroes when the count was cut short.</returns>
    public static Tally Measure(IFileSource source, IReadOnlyList<FileEntry> entries, CancellationToken token)
    {
        var files = 0;
        var folders = 0;
        var bytes = 0L;

        Spread(source, entries, token, entry =>
        {
            if (!entry.IsFolder)
            {
                Interlocked.Increment(ref files);
                Interlocked.Add(ref bytes, entry.Size);

                return;
            }

            Interlocked.Increment(ref folders);

            var below = Measure(source, Children(source, entry), token);

            Interlocked.Add(ref files, below.Files);
            Interlocked.Add(ref folders, below.Folders);
            Interlocked.Add(ref bytes, below.Bytes);
        });

        return new(files, folders, bytes);
    }

    /// <summary>
    /// What is inside a folder, without the entry that leads back out of it. A folder that cannot be
    /// read counts as empty here — the work itself will report why when it gets there.
    /// </summary>
    private static IReadOnlyList<FileEntry> Children(IFileSource source, FileEntry folder)
    {
        IReadOnlyList<FileEntry> listed;

        try
        {
            listed = source.List(folder.Path, true);
        }
        catch (Exception error) when (IsExpected(error))
        {
            return [];
        }

        var children = new List<FileEntry>(listed.Count);

        foreach (var child in listed)
        {
            if (!child.IsParent)
            {
                children.Add(child);
            }
        }

        return children;
    }

    public static void Copy(
        IFileSource from,
        IReadOnlyList<FileEntry> sources,
        IFileSource to,
        string target,
        Outcome outcome,
        CancellationToken token)
    {
        foreach (var source in sources)
        {
            CopyOne(from, source, to, to.Combine(target, source.Name), outcome, token);
        }
    }

    public static void Move(
        IFileSource from,
        IReadOnlyList<FileEntry> sources,
        IFileSource to,
        string target,
        Outcome outcome,
        CancellationToken token)
    {
        foreach (var source in sources)
        {
            MoveOne(from, source, to, to.Combine(target, source.Name), outcome, token);
        }
    }

    public static void Rename(IFileSource source, FileEntry entry, string target, Outcome outcome) =>
        MoveOne(source, entry, source, target, outcome, CancellationToken.None);

    public static void Delete(
        IFileSource source,
        IReadOnlyList<FileEntry> entries,
        Outcome outcome,
        CancellationToken token)
    {
        foreach (var entry in entries)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            DeleteOne(source, entry, outcome, token);
        }
    }

    public static string? CreateFolder(IFileSource source, string parent, string name)
    {
        try
        {
            var path = source.Combine(parent, name);

            source.CreateFolder(path);

            return path;
        }
        catch (Exception error) when (IsExpected(error))
        {
            return null;
        }
    }

    /// <summary>
    /// Removes one entry. A folder goes in a single request when the source can do that — one
    /// <c>rm -rf</c> on a server instead of a round trip per file — and is walked otherwise, several
    /// children at a time, so the count and the stop key keep working.
    /// </summary>
    private static void DeleteOne(IFileSource source, FileEntry entry, Outcome outcome, CancellationToken token)
    {
        try
        {
            outcome.Reached(entry.Name);

            if (!entry.IsFolder)
            {
                source.Delete(entry);
                outcome.Counted(0);
                return;
            }

            if (source.TryDeleteTree(entry))
            {
                outcome.Swept();
                return;
            }

            var children = Children(source, entry);

            Spread(source, children, token, child => DeleteOne(source, child, outcome, token));

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (!outcome.Failed)
            {
                source.Delete(entry);
            }

            outcome.CountedFolder();
        }
        catch (Exception error) when (IsExpected(error))
        {
            outcome.Failing(entry.Name, error.Message);
        }
    }

    /// <summary>
    /// Runs the same work over every entry, as many at a time as the source is willing to answer. A
    /// local disk keeps to one, so nothing changes for it; a server gets its latency hidden behind
    /// requests that overlap.
    /// </summary>
    private static void Spread(
        IFileSource source,
        IReadOnlyList<FileEntry> entries,
        CancellationToken token,
        Action<FileEntry> work)
    {
        if (source.Concurrency <= 1 || entries.Count < 2)
        {
            foreach (var entry in entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                work(entry);
            }

            return;
        }

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = source.Concurrency,
            CancellationToken = CancellationToken.None,
        };

        Parallel.ForEach(entries, options, entry =>
        {
            if (!token.IsCancellationRequested)
            {
                work(entry);
            }
        });
    }

    private static void CopyOne(
        IFileSource from,
        FileEntry source,
        IFileSource to,
        string target,
        Outcome outcome,
        CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return;
        }

        try
        {
            outcome.Reached(source.Name);

            if (!source.IsFolder)
            {
                outcome.Counted(Transfer(from, source.Path, to, target));
                return;
            }

            to.CreateFolder(target);
            outcome.CountedFolder();

            Spread(
                from.Concurrency <= to.Concurrency ? to : from,
                Children(from, source),
                token,
                child => CopyOne(from, child, to, to.Combine(target, child.Name), outcome, token));
        }
        catch (Exception error) when (IsExpected(error))
        {
            outcome.Failing(source.Name, error.Message);
        }
    }

    private static void MoveOne(
        IFileSource from,
        FileEntry source,
        IFileSource to,
        string target,
        Outcome outcome,
        CancellationToken token)
    {
        if (!ReferenceEquals(from, to) || !SameVolume(from, source.Path, target))
        {
            var copied = new Outcome();

            CopyOne(from, source, to, target, copied, token);
            outcome.Absorb(copied);

            if (!copied.Failed && !token.IsCancellationRequested)
            {
                DeleteOne(from, source, new(), token);
            }

            return;
        }

        try
        {
            outcome.Reached(source.Name);
            from.Move(source.Path, target);

            if (source.IsFolder)
            {
                outcome.CountedFolder();
            }
            else
            {
                outcome.Counted(source.Size);
            }
        }
        catch (Exception error) when (IsExpected(error))
        {
            outcome.Failing(source.Name, error.Message);
        }
    }

    private static long Transfer(IFileSource from, string source, IFileSource to, string target)
    {
        using var reading = from.OpenRead(source);
        using var writing = to.Create(target);

        reading.CopyTo(writing);

        return reading.CanSeek ? reading.Length : 0;
    }

    private static bool SameVolume(IFileSource source, string from, string target)
    {
        if (source.IsRemote)
        {
            return true;
        }

        return string.Equals(
            Path.GetPathRoot(Path.GetFullPath(from)),
            Path.GetPathRoot(Path.GetFullPath(target)),
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsExpected(Exception error) =>
        error is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException
            or NotSupportedException;
}
