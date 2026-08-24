using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Arlecchino.Atoms.History;

/// <summary>
/// Where the atoms that keep history announce their steps and their names, and where
/// <see cref="AtomHistory"/> hears them. It carries the whole process, so one history collects it all.
/// </summary>
internal static class HistoryChanges
{
    private static readonly ConcurrentDictionary<string, (Type ValueType, IRestorableAtom Atom)> Atoms =
        new(StringComparer.Ordinal);

    /// <summary>
    /// What puts a newly named atom back to what a file last saw it hold. An application has one history,
    /// but nothing here assumes it: each is asked in turn, and the first that knows the name answers.
    /// </summary>
    private static readonly List<Func<string, IRestorableAtom, bool>> Resuming = [];

    private static int _writing;

    /// <summary>Raised with a step, once per change of an atom that keeps history.</summary>
    public static event Action<IAtomEdit>? Recorded;

    /// <summary>
    /// Whether a history is listening. Atoms ask before describing a change, so a program that keeps no
    /// history pays nothing for the atoms that could have fed one.
    /// </summary>
    public static bool IsRecording => Recorded is not null;

    /// <summary>
    /// Whether a history is writing its steps down. A collection describes a change by copying what it
    /// holds either side of it, which is worth doing only for a step that will outlive the run.
    /// </summary>
    public static bool IsWriting => Volatile.Read(ref _writing) > 0;

    /// <summary>Announces a step.</summary>
    /// <param name="edit">The change that was made.</param>
    public static void NotifyRecorded(IAtomEdit edit) => Recorded?.Invoke(edit);

    /// <summary>Takes note of a history that writes its steps down.</summary>
    /// <returns>Dispose it to stop counting that history.</returns>
    public static IDisposable Writes()
    {
        Interlocked.Increment(ref _writing);

        return new Writer();
    }

    /// <summary>
    /// Takes what puts a newly named atom back to what a file last saw it hold. It is asked as each atom
    /// gives its name rather than once at startup, since a screen's store is built when the screen is.
    /// </summary>
    /// <param name="resume">Puts one named atom back, and says whether it had anything to go on.</param>
    /// <returns>Dispose it to stop putting atoms back.</returns>
    public static IDisposable Resumes(Func<string, IRestorableAtom, bool> resume)
    {
        lock (Resuming)
        {
            Resuming.Add(resume);
        }

        return new Resumer(resume);
    }

    /// <summary>
    /// Takes a named atom, so a step read from a file can find it again. The atom announced last under a
    /// name answers to it, which is what a screen's store rebuilt on the way back relies on.
    /// </summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="valueType">What the atom holds, which two atoms under one name have to agree on.</param>
    /// <param name="atom">The atom itself.</param>
    /// <exception cref="InvalidOperationException">The name is taken by an atom holding something else.</exception>
    public static void Name(string key, Type valueType, IRestorableAtom atom)
    {
        Atoms.AddOrUpdate(
            key,
            (valueType, atom),
            (_, entry) => entry.ValueType == valueType
                ? (valueType, atom)
                : throw new InvalidOperationException(
                    $"'{key}' already names an atom holding {entry.ValueType.Name}, and this one holds " +
                    $"{valueType.Name}. A name stands for one piece of state, so give one of them another."));

        Resume(key, atom);
    }

    /// <summary>Finds the atom a name stands for.</summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="atom">The atom, when one answered to the name.</param>
    /// <returns><c>true</c> when a named atom was found.</returns>
    public static bool TryFind(string key, [NotNullWhen(true)] out IRestorableAtom? atom)
    {
        if (Atoms.TryGetValue(key, out var named))
        {
            atom = named.Atom;
            return true;
        }

        atom = null;
        return false;
    }

    /// <summary>Puts one newly named atom back, asking each history until one knows the name.</summary>
    /// <param name="key">What the atom is called.</param>
    /// <param name="atom">The atom itself.</param>
    private static void Resume(string key, IRestorableAtom atom)
    {
        Func<string, IRestorableAtom, bool>[] resuming;

        lock (Resuming)
        {
            if (Resuming.Count == 0)
            {
                return;
            }

            resuming = [.. Resuming];
        }

        foreach (var resume in resuming)
        {
            if (resume(key, atom))
            {
                return;
            }
        }
    }

    /// <summary>One history putting atoms back, which stops when it is disposed.</summary>
    private sealed class Resumer : IDisposable
    {
        private readonly Func<string, IRestorableAtom, bool> _resume;

        public Resumer(Func<string, IRestorableAtom, bool> resume) => _resume = resume;

        public void Dispose()
        {
            lock (Resuming)
            {
                Resuming.Remove(_resume);
            }
        }
    }

    /// <summary>One counted history. Disposing it a second time counts down no further.</summary>
    private sealed class Writer : IDisposable
    {
        private bool _counted = true;

        public void Dispose()
        {
            if (!_counted)
            {
                return;
            }

            _counted = false;

            Interlocked.Decrement(ref _writing);
        }
    }
}
