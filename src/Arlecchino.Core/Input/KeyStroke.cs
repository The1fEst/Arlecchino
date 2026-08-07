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
    /// </summary>
    /// <param name="pressed">The key that was pressed.</param>
    /// <returns><c>true</c> when the press is this combination.</returns>
    public bool Matches(KeyPress pressed)
    {
        if (pressed.Modifiers != Modifiers)
        {
            return false;
        }

        return pressed.Key == Key || (pressed.Key == default && MatchesCharacter(pressed.Character));
    }

    /// <summary>The same stroke with one modifier put in place of another.</summary>
    /// <param name="from">The modifier to take out.</param>
    /// <param name="to">The modifier to put in its place.</param>
    /// <returns>The rewritten stroke, or this one when the modifier is not held.</returns>
    public KeyStroke Replacing(KeyModifiers from, KeyModifiers to) =>
        from != KeyModifiers.None && (Modifiers & from) == from
            ? this with { Modifiers = (Modifiers & ~from) | to }
            : this;

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

    private static string NameOf(ConsoleKey key) => key switch
    {
        ConsoleKey.UpArrow => "↑",
        ConsoleKey.DownArrow => "↓",
        ConsoleKey.LeftArrow => "←",
        ConsoleKey.RightArrow => "→",
        ConsoleKey.Spacebar => "Space",
        ConsoleKey.Escape => "Esc",
        ConsoleKey.PageUp => "PgUp",
        ConsoleKey.PageDown => "PgDn",
        _ => key.ToString(),
    };
}
