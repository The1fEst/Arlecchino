using System;
using System.Collections.Generic;

namespace Arlecchino.Input;

/// <summary>
/// Reads a paste out of what the Windows console reports. A terminal elsewhere wraps pasted text in
/// <c>ESC[200~</c> and <c>ESC[201~</c>, so an application is told where the block begins and ends and
/// the newline inside it is text rather than a key. The console reports the same paste as the key
/// presses that would have typed it, which is why the last newline of a pasted command ran it.
///
/// What the console does give away is timing: a paste arrives as a run of characters already waiting
/// in the buffer, where a hand types one key per read. A run of two or more is wrapped in the markers
/// the reader already understands, so a paste on Windows reaches the application as a paste.
///
/// A run of the same character is left alone: that is a held key repeating, not a paste.
/// </summary>
internal static class WindowsPaste
{
    private const string Start = "\e[200~";

    private const string End = "\e[201~";

    /// <summary>Whether a key press carries text a paste could be made of.</summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when it types a character rather than commanding something.</returns>
    public static bool Types(KeyPress key)
    {
        if ((key.Modifiers & ~KeyModifiers.Shift) != 0 || key.Character == '\0')
        {
            return false;
        }

        return !char.IsControl(key.Character) || key.Character is '\r' or '\n' or '\t';
    }

    /// <summary>
    /// Whether a run of key presses reads as a paste rather than as typing. A newline in the run
    /// settles it: nobody types one in the middle of a word, and it is the newline at the end of a
    /// pasted command that ran the command. Without one, it takes four characters, which is more than
    /// two keys that happened to land in the same read.
    /// </summary>
    /// <param name="run">The presses that were waiting together.</param>
    /// <returns><c>true</c> when the run should be handed on as pasted text.</returns>
    public static bool Reads(IReadOnlyList<KeyPress> run)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (run.Count < 2)
        {
            return false;
        }

        var varied = false;
        var broken = false;

        foreach (var key in run)
        {
            if (!Types(key))
            {
                return false;
            }

            varied |= key.Character != run[0].Character;
            broken |= key.Character is '\r' or '\n';
        }

        return varied && (broken || run.Count >= 4);
    }

    /// <summary>
    /// Wraps a run in the markers a terminal would have sent, as the key presses those markers are made
    /// of, so the reader takes the same path it takes everywhere else.
    /// </summary>
    /// <param name="run">The presses to wrap.</param>
    /// <returns>The presses to hand on, markers and all.</returns>
    public static IEnumerable<KeyPress> Wrapped(IReadOnlyList<KeyPress> run)
    {
        ArgumentNullException.ThrowIfNull(run);

        foreach (var key in Marker(Start))
        {
            yield return key;
        }

        foreach (var key in run)
        {
            yield return key;
        }

        foreach (var key in Marker(End))
        {
            yield return key;
        }
    }

    private static IEnumerable<KeyPress> Marker(string marker)
    {
        foreach (var character in marker)
        {
            yield return character == '\e'
                ? new(ConsoleKey.Escape, KeyModifiers.None, '\e')
                : new(default, KeyModifiers.None, character);
        }
    }
}
