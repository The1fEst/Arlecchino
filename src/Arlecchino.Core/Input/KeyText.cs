using System;

namespace Arlecchino.Input;

/// <summary>
/// Turns a key press into the character it should type. Take it as a constructor parameter instead
/// of reading <c>ConsoleKeyInfo.KeyChar</c> yourself — that is what keeps filters and shortcuts
/// working on a non-latin layout.
/// </summary>
public sealed class KeyText
{
    /// <summary>Shared resolver for <see cref="TextInputMode.ByPosition"/>.</summary>
    public static KeyText ByPosition { get; } = new(TextInputMode.ByPosition);

    /// <summary>Shared resolver for <see cref="TextInputMode.Native"/>.</summary>
    public static KeyText Native { get; } = new(TextInputMode.Native);

    private readonly TextInputMode _mode;

    /// <summary>Creates a resolver for one mode.</summary>
    /// <param name="mode">How characters should be resolved.</param>
    public KeyText(TextInputMode mode)
    {
        _mode = mode;
    }

    /// <summary>Returns the shared resolver for a mode.</summary>
    /// <param name="mode">The mode wanted.</param>
    /// <returns>The matching resolver.</returns>
    public static KeyText For(TextInputMode mode) => mode == TextInputMode.Native ? Native : ByPosition;

    /// <summary>The mode this resolver works in.</summary>
    public TextInputMode Mode => _mode;

    /// <summary>
    /// The character a key press should type, or <c>null</c> for keys that type nothing — function
    /// keys, arrows, and unmapped combinations.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>The character to insert, or <c>null</c>.</returns>
    public char? Resolve(ConsoleKeyInfo key)
    {
        if (_mode == TextInputMode.ByPosition)
        {
            return ResolveByPhysicalKey(key);
        }

        return key.KeyChar != '\0' && !char.IsControl(key.KeyChar) ? key.KeyChar : ResolveByPhysicalKey(key);
    }

    private static char? ResolveByPhysicalKey(ConsoleKeyInfo key)
    {
        var shift = key.Modifiers.HasFlag(ConsoleModifiers.Shift);

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
