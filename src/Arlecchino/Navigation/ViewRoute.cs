using System;

namespace Arlecchino.Navigation;

/// <summary>
/// Where to navigate: a name in a struct, compared ordinally. Routes are strings rather than an enum
/// because the framework has to name a screen without seeing the application's types — the generated
/// <c>ViewKind</c> lives in your assembly, not in Arlecchino.
/// </summary>
/// <param name="Name">Name of the route.</param>
public readonly record struct ViewRoute(string Name)
{
    /// <summary>The empty route: returned from a handler to stay where you are.</summary>
    public static ViewRoute None => default;

    /// <summary>Whether this is the empty route.</summary>
    public bool IsNone => string.IsNullOrEmpty(Name);

    /// <summary>Compares routes by name, case-sensitively.</summary>
    /// <param name="other">The route to compare with.</param>
    /// <returns><c>true</c> when both name the same screen.</returns>
    public bool Equals(ViewRoute other) => string.Equals(Name, other.Name, StringComparison.Ordinal);

    /// <summary>Hash of the route name.</summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode() => Name is null ? 0 : StringComparer.Ordinal.GetHashCode(Name);

    /// <summary>The route name, or <c>None</c> for the empty route.</summary>
    /// <returns>Readable form of the route.</returns>
    public override string ToString() => IsNone ? "None" : Name;
}
