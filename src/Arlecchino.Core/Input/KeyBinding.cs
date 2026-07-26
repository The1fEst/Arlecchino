using System;
using System.Text;

namespace Arlecchino.Input;

/// <summary>
/// A key plus the exact modifiers that must be held with it, so <c>Ctrl+S</c> never fires on a bare
/// <c>S</c>. Every key the framework reacts to is one of these, which is what makes them rebindable.
/// </summary>
/// <param name="Key">The key itself.</param>
/// <param name="Modifiers">Modifiers that must be held, exactly.</param>
/// <param name="AlsoKey">
/// A second key that triggers the same thing, for actions the platforms disagree about — copying is
/// <c>Ctrl+Insert</c> in one habit and <c>Ctrl+Shift+C</c> in another.
/// </param>
/// <param name="AlsoModifiers">Modifiers for that second key.</param>
public readonly record struct KeyBinding(
    ConsoleKey Key,
    ConsoleModifiers Modifiers = default,
    ConsoleKey AlsoKey = default,
    ConsoleModifiers AlsoModifiers = default)
{
    /// <summary>Whether this binding is unset and therefore matches nothing.</summary>
    public bool IsNone => Key == default;

    /// <summary>
    /// Whether a key press is this binding, either of its combinations. Terminals that report no
    /// virtual key are still handled: letters, digits and the common control keys are then matched by
    /// the character typed.
    /// </summary>
    /// <param name="pressed">The key that was pressed.</param>
    /// <returns><c>true</c> when the press should trigger this binding.</returns>
    public bool Matches(ConsoleKeyInfo pressed) =>
        MatchesOne(pressed, Key, Modifiers) ||
        (AlsoKey != default && MatchesOne(pressed, AlsoKey, AlsoModifiers));

    private static bool MatchesOne(ConsoleKeyInfo pressed, ConsoleKey key, ConsoleModifiers modifiers)
    {
        if (pressed.Modifiers != modifiers)
        {
            return false;
        }

        return pressed.Key == key || (pressed.Key == default && MatchesCharacter(key, pressed.KeyChar));
    }

    private static bool MatchesCharacter(ConsoleKey key, char character) => key switch
    {
        >= ConsoleKey.A and <= ConsoleKey.Z =>
            char.ToUpperInvariant(character) == (char)('A' + (key - ConsoleKey.A)),
        >= ConsoleKey.D0 and <= ConsoleKey.D9 => character == (char)('0' + (key - ConsoleKey.D0)),
        ConsoleKey.Spacebar => character == ' ',
        ConsoleKey.Enter => character is '\r' or '\n',
        ConsoleKey.Escape => character == '\e',
        ConsoleKey.Tab => character == '\t',
        ConsoleKey.Backspace => character is '\b' or (char)127,
        _ => false,
    };

    /// <summary>
    /// How the binding is shown to the user — <c>Ctrl+S</c>, <c>Alt+←</c>, <c>Esc</c>. The palette
    /// and the hints box display this, so a rebound key relabels itself everywhere.
    /// </summary>
    /// <returns>The readable form, or an empty string when the binding is unset.</returns>
    public override string ToString()
    {
        if (IsNone)
        {
            return "";
        }

        var text = new StringBuilder();

        if (Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            text.Append("Ctrl+");
        }

        if (Modifiers.HasFlag(ConsoleModifiers.Alt))
        {
            text.Append("Alt+");
        }

        if (Modifiers.HasFlag(ConsoleModifiers.Shift))
        {
            text.Append("Shift+");
        }

        return text.Append(NameOf(Key)).ToString();
    }

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
