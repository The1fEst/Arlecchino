namespace Arlecchino.Input;

/// <summary>
/// How a key press becomes a character, which decides what happens on a non-latin layout.
/// </summary>
public enum TextInputMode : byte
{
    /// <summary>
    /// ASCII is taken as typed; anything else falls back to the physical key, so filters and
    /// shortcuts keep working on a Cyrillic layout without switching it.
    /// </summary>
    LatinOnly,

    /// <summary>Any non-control character is taken as typed.</summary>
    Native,
}
