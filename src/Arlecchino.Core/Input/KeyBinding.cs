using System;
using System.Collections.Generic;

namespace Arlecchino.Input;

/// <summary>
/// A key plus the exact modifiers that must be held with it, so <c>Ctrl+S</c> never fires on a bare
/// <c>S</c>. Every key the framework reacts to is one of these, which is what makes them rebindable.
///
/// A binding starts as the one combination it is named after and is added to from there:
/// <see cref="AddAlternative"/> for the combinations a platform disagrees about, and
/// <see cref="ThenKey"/> for a second keystroke, which turns the binding into a chord.
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
    private readonly KeyStroke[]? _alternatives;
    private readonly KeyStroke? _second;

    private KeyBinding(KeyStroke first, KeyStroke[]? alternatives, KeyStroke? second)
        : this(first.Key, first.Modifiers)
    {
        _alternatives = alternatives;
        _second = second;
    }

    /// <summary>The combination the binding is named after, and the one it is written from.</summary>
    public KeyStroke First => new(Key, Modifiers);

    /// <summary>
    /// The other combinations that trigger the same thing, in the order they were added. They are
    /// matched but never written, since a binding is shown under one name.
    ///
    /// An alternative is one keystroke even where the binding is a chord, which is how a chord reaches
    /// a keyboard the other way round: <c>Ctrl+G U</c> for the machine that has to spell it out, and
    /// <c>Ctrl+PgUp</c> for the one with the key.
    /// </summary>
    public IReadOnlyList<KeyStroke> Alternatives => _alternatives ?? [];

    /// <summary>The keystroke that finishes a chord, or <c>null</c> when the binding is one press.</summary>
    public KeyStroke? Second => _second;

    /// <summary>Whether this binding is unset and therefore matches nothing.</summary>
    public bool IsNone => Key == default;

    /// <summary>
    /// Whether this takes two keystrokes rather than one. A chord is how an application reaches past the
    /// modifiers a terminal will give it. A Mac terminal keeps Option for typing and its Command belongs to
    /// the window, so what is left are the letters held with Control — and there are not thirty of those.
    /// A leader spends one of them and hands back the whole alphabet behind it.
    /// </summary>
    public bool IsChord => _second is not null;

    /// <summary>
    /// The same binding, with one more combination that triggers it. Platforms disagree about some of
    /// them — copying is <c>Ctrl+Insert</c> in one habit and <c>Ctrl+Shift+C</c> in another — and a
    /// binding that carries both is right on either machine. Call it as often as there are habits.
    /// </summary>
    /// <param name="key">The key of the added combination.</param>
    /// <param name="modifiers">Modifiers that must be held with it, exactly.</param>
    /// <returns>The binding, with the combination added after the ones already there.</returns>
    public KeyBinding AddAlternative(ConsoleKey key, KeyModifiers modifiers = default) =>
        new(First, [.. Alternatives, new(key, modifiers)], _second);

    /// <summary>
    /// The same binding, finished by a second keystroke pressed after the first one is let go. The
    /// leader is expected to say what the group is about — the operations behind one, the places behind
    /// another — since what a person remembers is the grouping and not the letters.
    ///
    /// A binding gets one finishing key: calling this twice replaces it rather than growing a third
    /// keystroke, because a chord longer than two is a sequence nobody recalls.
    /// </summary>
    /// <param name="key">The key that finishes the chord.</param>
    /// <param name="modifiers">Modifiers held with it, which is usually none.</param>
    /// <returns>The chord.</returns>
    public KeyBinding ThenKey(ConsoleKey key, KeyModifiers modifiers = default) =>
        new(First, _alternatives, new(key, modifiers));

    /// <summary>
    /// The same binding with one modifier put in place of another, wherever it appears. This is how an
    /// application moves off a modifier its users cannot press — a Mac terminal keeps Option for typing
    /// accented characters, so <c>Alt</c> never arrives and <c>Super</c> is what that keyboard has spare.
    /// </summary>
    /// <param name="from">The modifier to take out.</param>
    /// <param name="to">The modifier to put in its place.</param>
    /// <returns>The rewritten binding, or this one when the modifier is not in it.</returns>
    public KeyBinding Replacing(KeyModifiers from, KeyModifiers to) =>
        new(First.Replacing(from, to), MovedAlternatives(from, to), _second?.Replacing(from, to));

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
    /// Whether one key press is this whole binding. The combination it is named after counts only when
    /// the binding is one keystroke — a chord is opened rather than matched — but an alternative counts
    /// either way, since an alternative is always a single press.
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
        if (Key != other.Key || Modifiers != other.Modifiers || _second != other._second)
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
