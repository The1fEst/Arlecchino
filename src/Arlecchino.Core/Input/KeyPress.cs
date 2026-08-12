using System;

namespace Arlecchino.Input;

/// <summary>
/// One key press, as the framework hands it to a view. It is <see cref="ConsoleKeyInfo"/> with room for
/// Command, which that type has nowhere to put.
/// </summary>
/// <param name="Key">The key itself, or <c>default</c> when the terminal named no key and only sent a character.</param>
/// <param name="Modifiers">What was held with it.</param>
/// <param name="Character">The character it typed, or <c>'\0'</c> for keys that type nothing.</param>
public readonly record struct KeyPress(
    ConsoleKey Key,
    KeyModifiers Modifiers = default,
    char Character = '\0')
{
    /// <summary>Creates a press the terminal reported as a character with no key behind it.</summary>
    /// <param name="character">The character that arrived.</param>
    public KeyPress(char character)
        : this(default, default, character) { }

    /// <summary>
    /// Takes over what a console reported. Nothing is lost: the console cannot report Command in the
    /// first place, which is the whole reason this type exists.
    /// </summary>
    /// <param name="key">What the console handed over.</param>
    /// <returns>The same press.</returns>
    public static KeyPress From(ConsoleKeyInfo key) => new(key.Key, (KeyModifiers)key.Modifiers, key.KeyChar);

    /// <summary>
    /// Whether this is the press a terminal hands back when there was nothing to read — no key, no
    /// character, nothing held.
    /// </summary>
    public bool IsNothing => this == default;
}
