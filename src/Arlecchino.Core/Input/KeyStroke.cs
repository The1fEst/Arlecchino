using System;
using System.Text;

namespace Arlecchino.Input;

/// <summary>
/// One key and the modifiers held with it — the smallest thing a binding can be made of. A
/// <see cref="KeyBinding"/> is one of these, plus the alternatives that mean the same, plus the key
/// that finishes it when the binding is a chord.
/// </summary>
/// <param name="Key">The key itself.</param>
/// <param name="Modifiers">Modifiers that must be held, exactly.</param>
public readonly record struct KeyStroke(ConsoleKey Key, KeyModifiers Modifiers = default)
{
    /// <summary>Whether the stroke is unset and therefore stands for no key at all.</summary>
    public bool IsNone => Key == default;

    /// <summary>
    /// Whether a key press is this stroke. Terminals that report no virtual key are still handled:
    /// letters, digits and the common control keys are then matched by the character typed.
    ///
    /// Some keys answer to two names, one where they sit on the keyboard and one on the keypad, and the
    /// runtime hands back whichever it likes: a slash arrives as <c>Divide</c> even when it was pressed
    /// next to the shift. A binding written on either name answers to both.
    /// </summary>
    /// <param name="pressed">The key that was pressed.</param>
    /// <returns><c>true</c> when the press is this combination.</returns>
    public bool Matches(KeyPress pressed)
    {
        if (pressed.Modifiers != Modifiers)
        {
            return false;
        }

        return pressed.Key == Key ||
            Twinned(pressed.Key) == Twinned(Key) ||
            (pressed.Key == default && MatchesCharacter(pressed.Character));
    }

    /// <summary>
    /// The one name for a key that has two. A slash typed next to the shift is handed back as the keypad's
    /// divide, and the two type the same character, so nothing here wants to tell them apart.
    ///
    /// Only where the character is the same. The keypad's plus and the key that carries the equals sign are
    /// two keys, not one — <c>+</c> and <c>=</c> are not the same thing to type — and folding them together
    /// would make a binding on one answer to the other.
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
    /// punctuation is the US layout the <c>Oem</c> names already assume — a keyboard that puts those
    /// characters elsewhere reports the key itself, which is matched before this is asked.
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
    /// What to write on the key. The punctuation is named after the character it types rather than after
    /// the name the runtime gives it: nobody reading a key screen knows what <c>Oem2</c> is, and the whole
    /// point of the screen is to be read.
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
