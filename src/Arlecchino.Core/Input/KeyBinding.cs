using System;
using System.Collections.Generic;

namespace Arlecchino.Input;

/// <summary>
/// A key plus the exact modifiers that must be held with it, so <c>Ctrl+S</c> never fires on a bare <c>S</c>.
/// It is added to with <see cref="AddAlternative"/> and turned into a chord with <see cref="ThenKey"/>.
/// </summary>
/// <param name="Key">The key itself.</param>
/// <param name="Modifiers">Modifiers that must be held, exactly.</param>
/// <example>
/// <code>
/// new KeyBinding(ConsoleKey.Insert, KeyModifiers.Control)
///     .AddAlternative(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);
///
/// new KeyBinding(ConsoleKey.X, KeyModifiers.Control).ThenKey(ConsoleKey.T);
/// </code>
/// </example>
public readonly record struct KeyBinding(ConsoleKey Key, KeyModifiers Modifiers = default)
{
    private readonly KeyStroke? _second;
    private readonly KeyStroke[]? _alternatives;

    private KeyBinding(KeyStroke first, KeyStroke? second, KeyStroke[]? alternatives) : this(first.Key, first.Modifiers)
    {
        Typed = first.Typed;
        _second = second;
        _alternatives = alternatives;
    }

    /// <summary>
    /// A binding on a character rather than on a key, which is the only dependable way to name punctuation.
    /// It answers wherever that character can be typed, and the key screen writes the character itself.
    /// </summary>
    /// <param name="typed">The character to answer to.</param>
    public KeyBinding(char typed) : this(default(ConsoleKey)) => Typed = typed;

    /// <summary>The character this binding answers to, or <c>'\0'</c> when it is a binding on a key.</summary>
    public char Typed { get; }

    /// <summary>The combination the binding is named after, and the one it is written from.</summary>
    public KeyStroke First => Typed == '\0' ? new(Key, Modifiers) : new(Typed);

    /// <summary>
    /// The other combinations that trigger the same thing, in the order they were added. They are matched but
    /// never written, and each is a single press even where the binding itself is a chord.
    /// </summary>
    public IReadOnlyList<KeyStroke> Alternatives => _alternatives ?? [];

    /// <summary>The keystroke that finishes a chord, or <c>null</c> when the binding is one press.</summary>
    public KeyStroke? Second => _second;

    /// <summary>Whether this binding is unset and therefore matches nothing.</summary>
    public bool IsNone => Key == default && Typed == '\0';

    /// <summary>
    /// Whether this takes two keystrokes rather than one. A chord spends one combination on a leader and
    /// hands back the whole alphabet behind it, which is how an application reaches past what a terminal
    /// gives it.
    /// </summary>
    public bool IsChord => _second is not null;

    /// <summary>
    /// The same binding, with one more combination that triggers it, for the habits platforms disagree
    /// about. Call it as often as there are habits.
    /// </summary>
    /// <param name="key">The key of the added combination.</param>
    /// <param name="modifiers">Modifiers that must be held with it, exactly.</param>
    /// <returns>The binding, with the combination added after the ones already there.</returns>
    public KeyBinding AddAlternative(ConsoleKey key, KeyModifiers modifiers = default) =>
        new(First, _second, [.. Alternatives, new(key, modifiers)]);

    /// <summary>
    /// The same binding, finished by a second keystroke pressed after the first one is let go. A binding gets
    /// one finishing key, so calling this twice replaces it rather than growing a third keystroke.
    /// </summary>
    /// <param name="key">The key that finishes the chord.</param>
    /// <param name="modifiers">Modifiers held with it, which is usually none.</param>
    /// <returns>The chord.</returns>
    public KeyBinding ThenKey(ConsoleKey key, KeyModifiers modifiers = default) =>
        new(First, new(key, modifiers), _alternatives);

    /// <summary>
    /// The same binding with one modifier put in place of another, wherever it appears. It is how an
    /// application moves off a modifier its users cannot press.
    /// </summary>
    /// <param name="from">The modifier to take out.</param>
    /// <param name="to">The modifier to put in its place.</param>
    /// <returns>The rewritten binding, or this one when the modifier is not in it.</returns>
    public KeyBinding Replacing(KeyModifiers from, KeyModifiers to) =>
        new(First.Replacing(from, to), _second?.Replacing(from, to), MovedAlternatives(from, to));

    private KeyStroke[]? MovedAlternatives(KeyModifiers from, KeyModifiers to)
    {
        if (_alternatives is not { } present)
        {
            return null;
        }

        var moved = new KeyStroke[present.Length];

        for (var i = 0; i < present.Length; i++)
        {
            moved[i] = present[i].Replacing(from, to);
        }

        return moved;
    }

    /// <summary>
    /// Whether one key press is this whole binding. The combination it is named after counts only when the
    /// binding is one keystroke, since a chord is opened rather than matched; an alternative counts either
    /// way.
    /// </summary>
    /// <param name="pressed">The key that was pressed.</param>
    /// <returns><c>true</c> when the press should trigger this binding on its own.</returns>
    public bool Matches(KeyPress pressed) => (!IsChord && First.Matches(pressed)) || MatchesAlternative(pressed);

    /// <summary>
    /// Whether a key press is the first half of this chord. A binding of one keystroke opens nothing:
    /// it either matches or it does not.
    /// </summary>
    /// <param name="pressed">The key that was pressed.</param>
    /// <returns><c>true</c> when the chord has been started and the next key will finish it.</returns>
    public bool Opens(KeyPress pressed) => IsChord && First.Matches(pressed);

    /// <summary>Whether a key press is the second half of this chord.</summary>
    /// <param name="pressed">The key that was pressed.</param>
    /// <returns><c>true</c> when the chord is complete.</returns>
    public bool Closes(KeyPress pressed) => _second is { } second && second.Matches(pressed);

    private bool MatchesAlternative(KeyPress pressed)
    {
        foreach (var alternative in Alternatives)
        {
            if (alternative.Matches(pressed))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether two bindings stand for the same keys. The alternatives count and so does their order,
    /// since that is the order they are matched in.
    /// </summary>
    /// <param name="other">The binding to compare with.</param>
    /// <returns><c>true</c> when both are made of the same keystrokes.</returns>
    public bool Equals(KeyBinding other)
    {
        if (Key != other.Key || Modifiers != other.Modifiers || Typed != other.Typed || _second != other._second)
        {
            return false;
        }

        var mine = Alternatives;
        var theirs = other.Alternatives;

        if (mine.Count != theirs.Count)
        {
            return false;
        }

        for (var i = 0; i < mine.Count; i++)
        {
            if (mine[i] != theirs[i])
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>A hash over the same keystrokes equality compares.</summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();

        hash.Add(Key);
        hash.Add(Modifiers);
        hash.Add(Typed);
        hash.Add(_second);

        foreach (var alternative in Alternatives)
        {
            hash.Add(alternative);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// How the binding is shown to the user — <c>Ctrl+S</c>, <c>Alt+←</c>, <c>Esc</c>. A chord is
    /// written as its two keystrokes with a space between them, <c>Ctrl+X T</c>.
    /// </summary>
    /// <returns>The readable form, or an empty string when the binding is unset.</returns>
    public override string ToString()
    {
        if (IsNone)
        {
            return "";
        }

        return _second is { } second ? $"{First} {second}" : First.ToString();
    }
}
