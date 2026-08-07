namespace Arlecchino.Input;

/// <summary>
/// How a key press becomes a character, which decides what happens on a non-latin layout.
/// </summary>
public enum TextInputMode : byte
{
    /// <summary>
    /// The character is taken from where the key sits on the keyboard rather than from what the layout makes
    /// of it. So a filter or a shortcut reads the same on a Cyrillic or a Greek layout as it does on a US one,
    /// at the cost of not being able to type that language at all.
    /// </summary>
    ByPosition,

    /// <summary>Any non-control character is taken as typed, so any language can be typed.</summary>
    Native,
}
