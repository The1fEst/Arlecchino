using System;

namespace Arlecchino.Input;

/// <summary>
/// Turns a key press into the character it types. Ask this instead of reading <c>KeyPress.Character</c>
/// yourself — that is what keeps a field typing on a terminal that names the key and sends no character
/// with it.
/// </summary>
public static class KeyText
{
    /// <summary>
    /// The character a key press should type, or <c>null</c> for keys that type nothing — function
    /// keys, arrows, and unmapped combinations. Whatever the layout made of the key is taken as it is,
    /// so any language is typed without the application asking for it.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>The character to insert, or <c>null</c>.</returns>
    public static char? Resolve(KeyPress key) =>
        key.Character != '\0' && !char.IsControl(key.Character) ? key.Character : ByPosition(key);

    /// <summary>
    /// The character a press stands for as a shortcut rather than as text: where its key sits when the
    /// application reads keys by position, and what the layout typed otherwise.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>The character the shortcut is looked up under, or <c>null</c>.</returns>
    public static char? Shortcut(KeyPress key) => key.ByPosition != '\0' ? key.ByPosition : Resolve(key);

    /// <summary>
    /// The character the key would type where it sits, whatever the layout makes of it. A key that sits
    /// nowhere on a US keyboard stands for nothing.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>The character its position stands for, or <c>null</c>.</returns>
    public static char? ByPosition(KeyPress key)
    {
        var shift = key.Modifiers.HasFlag(KeyModifiers.Shift);

        if (key.Key is >= ConsoleKey.A and <= ConsoleKey.Z)
        {
            var letter = (char)('a' + (key.Key - ConsoleKey.A));
            return shift ? char.ToUpperInvariant(letter) : letter;
        }

        if (key.Key is >= ConsoleKey.D0 and <= ConsoleKey.D9)
        {
            return shift ? ")!@#$%^&*("[key.Key - ConsoleKey.D0] : (char)('0' + (key.Key - ConsoleKey.D0));
        }

        if (key.Key is >= ConsoleKey.NumPad0 and <= ConsoleKey.NumPad9)
        {
            return (char)('0' + (key.Key - ConsoleKey.NumPad0));
        }

        return key.Key switch
        {
            ConsoleKey.Spacebar => ' ',
            ConsoleKey.OemComma => shift ? '<' : ',',
            ConsoleKey.OemPeriod => shift ? '>' : '.',
            ConsoleKey.OemMinus => shift ? '_' : '-',
            ConsoleKey.OemPlus => shift ? '+' : '=',
            ConsoleKey.Oem1 => shift ? ':' : ';',
            ConsoleKey.Oem2 => shift ? '?' : '/',
            ConsoleKey.Oem3 => shift ? '~' : '`',
            ConsoleKey.Oem4 => shift ? '{' : '[',
            ConsoleKey.Oem5 => shift ? '|' : '\\',
            ConsoleKey.Oem6 => shift ? '}' : ']',
            ConsoleKey.Oem7 => shift ? '"' : '\'',
            _ => null,
        };
    }
}
