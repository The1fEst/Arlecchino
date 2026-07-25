namespace Arlecchino.State;

/// <summary>
/// A field that shows something around its value — a currency sign, a unit. Affixes are decoration
/// only: the callback still receives the bare value.
/// </summary>
public interface IAffixedModal
{
    /// <summary>Drawn before the value, such as <c>$ </c>.</summary>
    string Prefix { get; }

    /// <summary>Drawn after the value, such as <c> %</c>.</summary>
    string Suffix { get; }
}
