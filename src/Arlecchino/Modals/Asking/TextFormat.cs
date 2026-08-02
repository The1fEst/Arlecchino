namespace Arlecchino.Modals.Asking;

/// <summary>
/// A built-in check a text field runs before your own validator, so common mistakes are caught with
/// a translated message instead of a hand-written regex.
/// </summary>
public enum TextFormat : byte
{
    /// <summary>Anything goes.</summary>
    Free,

    /// <summary>One <c>@</c>, no spaces, and a dotted domain.</summary>
    Email,

    /// <summary>An absolute <c>http</c> or <c>https</c> address.</summary>
    Url,
}
