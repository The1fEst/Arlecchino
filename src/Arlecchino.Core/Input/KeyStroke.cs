using System;
using System.Text;

namespace Arlecchino.Input;

/// <summary>
/// One key and the modifiers held with it, which is the smallest thing a binding can be made of. A
/// <see cref="KeyBinding"/> is one of these plus its alternatives and its finishing key.
/// </summary>
/// <param name="Key">The key itself.</param>
/// <param name="Modifiers">Modifiers that must be held, exactly.</param>
public readonly record struct KeyStroke(ConsoleKey Key, KeyModifiers Modifiers = default)
{
    /// <summary>
    /// A stroke on a character rather than on a key, which is the only way to name punctuation. It answers
    /// on every layout that can type that character, whatever the keyboard does to produce it.
    /// </summary>
    /// <param name="character">The character to answer to.</param>
    public KeyStroke(char character)
        : this(default(ConsoleKey)) => Character = character;

    /// <summary>The character this stroke answers to, or <c>'\0'</c> when it is a stroke on a key.</summary>
    public char Character { get; }

    /// <summary>Whether the stroke is unset and therefore stands for no key at all.</summary>
    public bool IsNone => Key == default && Character == '\0';

    /// <summary>
    /// Whether a key press is this stroke, by the key where one was reported and by the character otherwise.
    /// A key that answers to two names matches under either, and a stroke on a character forgives Shift.
    /// </summary>
    /// <param name="press">The key that was pressed.</param>
    /// <returns><c>true</c> when the press is this combination.</returns>
    public bool Matches(KeyPress press)
    {
        if (Character != '\0')
        {
            return (press.Modifiers & ~KeyModifiers.Shift) == KeyModifiers.None &&
                   (press.Character == Character ||
                    (press.Character == '\0' && new KeyStroke(press.Key).MatchesCharacter(Character)));
        }

        if (press.Modifiers != Modifiers)
        {
            return false;
        }

        return press.Key == Key ||
               Twinned(press.Key) == Twinned(Key) ||
               (press.Key == default && MatchesCharacter(press.Character));
    }

    /// <summary>
    /// The one name for a key that has two, where both type the same character. Keys that type different
    /// characters stay apart, so a binding on one never answers to the other.
    /// </summary>
    /// <param name="key">Either name.</param>
    /// <returns>The name both are read as.</returns>
    private static ConsoleKey Twinned(ConsoleKey key) => key switch
    {
        ConsoleKey.Divide => ConsoleKey.Oem2,
        ConsoleKey.Subtract => ConsoleKey.OemMinus,
        ConsoleKey.Decimal => ConsoleKey.OemPeriod,
        _ => key,
    };

    /// <summary>The same stroke with one modifier put in place of another.</summary>
    /// <param name="from">The modifier to take out.</param>
    /// <param name="to">The modifier to put in its place.</param>
    /// <returns>The rewritten stroke, or this one when the modifier is not held.</returns>
    public KeyStroke Replacing(KeyModifiers from, KeyModifiers to) =>
        from != KeyModifiers.None && (Modifiers & from) == from
            ? this with { Modifiers = (Modifiers & ~from) | to }
            : this;

    /// <summary>
    /// What a key would have typed, for the terminals that send the character and no virtual key. The
    /// punctuation follows the US layout the <c>Oem</c> names already assume.
    /// </summary>
    /// <param name="character">The character that arrived.</param>
    /// <returns><c>true</c> when this key is the one that types it.</returns>
    private bool MatchesCharacter(char character) => Key switch
    {
        >= ConsoleKey.A and <= ConsoleKey.Z =>
            char.ToUpperInvariant(character) == (char)('A' + (Key - ConsoleKey.A)),
        >= ConsoleKey.D0 and <= ConsoleKey.D9 => character == (char)('0' + (Key - ConsoleKey.D0)),
        ConsoleKey.Spacebar => character == ' ',
        ConsoleKey.Enter => character is '\r' or '\n',
        ConsoleKey.Escape => character == '\e',
        ConsoleKey.Tab => character == '\t',
        ConsoleKey.Backspace => character is '\b' or (char)127,
        ConsoleKey.Oem1 => character is ';' or ':',
        ConsoleKey.Oem2 => character is '/' or '?',
        ConsoleKey.Oem3 => character is '`' or '~',
        ConsoleKey.Oem4 => character is '[' or '{',
        ConsoleKey.Oem5 => character is '\\' or '|',
        ConsoleKey.Oem6 => character is ']' or '}',
        ConsoleKey.Oem7 => character is '\'' or '"',
        ConsoleKey.OemComma => character is ',' or '<',
        ConsoleKey.OemPeriod => character is '.' or '>',
        ConsoleKey.OemMinus => character is '-' or '_',
        ConsoleKey.OemPlus => character is '=' or '+',
        _ => false,
    };

    /// <summary>
    /// How the stroke is shown to the user — <c>Ctrl+S</c>, <c>Alt+←</c>, <c>Esc</c>. The palette and
    /// the hints box display this, so a rebound key relabels itself everywhere.
    /// </summary>
    /// <returns>The readable form, or an empty string when the stroke is unset.</returns>
    public override string ToString()
    {
        if (IsNone)
        {
            return "";
        }

        if (Character != '\0')
        {
            return Character.ToString();
        }

        var text = new StringBuilder();

        if (Modifiers.HasFlag(KeyModifiers.Control))
        {
            text.Append("Ctrl+");
        }

        if (Modifiers.HasFlag(KeyModifiers.Alt))
        {
            text.Append("Alt+");
        }

        if (Modifiers.HasFlag(KeyModifiers.Super))
        {
            text.Append(SuperName);
        }

        if (Modifiers.HasFlag(KeyModifiers.Shift))
        {
            text.Append("Shift+");
        }

        return text.Append(NameOf(Key)).ToString();
    }

    /// <summary>
    /// What to call the key next to the space bar. The same bit is a different key cap depending on
    /// where the application is running, and the hints box is read by whoever is looking at it.
    /// </summary>
    private static string SuperName => OperatingSystem.IsMacOS() ? "Cmd+" : "Win+";

    /// <summary>
    /// What to write on the key. Punctuation is named after the character it types rather than after the
    /// runtime's own name for it, which a key screen would be unreadable with.
    /// </summary>
    /// <param name="key">The key to name.</param>
    /// <returns>What to call it.</returns>
    private static string NameOf(ConsoleKey key) => Twinned(key) switch
    {
        ConsoleKey.UpArrow => "↑",
        ConsoleKey.DownArrow => "↓",
        ConsoleKey.LeftArrow => "←",
        ConsoleKey.RightArrow => "→",
        ConsoleKey.Spacebar => "Space",
        ConsoleKey.Escape => "Esc",
        ConsoleKey.PageUp => "PgUp",
        ConsoleKey.PageDown => "PgDn",
        ConsoleKey.Oem1 => ";",
        ConsoleKey.Oem2 => "/",
        ConsoleKey.Oem3 => "`",
        ConsoleKey.Oem4 => "[",
        ConsoleKey.Oem5 => "\\",
        ConsoleKey.Oem6 => "]",
        ConsoleKey.Oem7 => "'",
        ConsoleKey.OemComma => ",",
        ConsoleKey.OemPeriod => ".",
        ConsoleKey.OemMinus => "-",
        ConsoleKey.OemPlus => "=",
        ConsoleKey.Add => "+",
        ConsoleKey.Multiply => "*",
        _ => key.ToString(),
    };
}
