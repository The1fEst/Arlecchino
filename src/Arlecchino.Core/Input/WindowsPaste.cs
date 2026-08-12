using System;
using System.Collections.Generic;

namespace Arlecchino.Input;

/// <summary>
/// Reads a paste out of what the Windows console reports, which is the key presses that would have typed it.
/// A run of characters already waiting in the buffer is wrapped in the markers the reader understands.
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
    /// Whether a run of key presses reads as a paste rather than as typing. A newline in the run settles it,
    /// and without one it takes four characters.
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
