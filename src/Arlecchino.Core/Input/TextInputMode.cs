namespace Arlecchino.Input;

/// <summary>
/// How a key press becomes a character, which decides what happens on a non-latin layout.
/// </summary>
public enum TextInputMode : byte
{
    /// <summary>
    /// The character is taken from where the key sits rather than from what the layout makes of it, so a
    /// shortcut reads the same on every layout and that language cannot be typed at all.
    /// </summary>
    ByPosition,

    /// <summary>Any non-control character is taken as typed, so any language can be typed.</summary>
    Native,
}
