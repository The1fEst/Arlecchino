namespace Arlecchino.Rendering.Colors;

/// <summary>
/// Anything that can style a cell. The frame writer only ever asks for <see cref="Ansi"/> and
/// compares styles by reference, so hold on to instances instead of building one per cell.
/// </summary>
public interface IArlecchinoColor
{
    /// <summary>
    /// The escape sequence that switches the terminal to this style, or an empty string when color
    /// is turned off. Implementations are expected to build it once and cache it.
    /// </summary>
    string Ansi { get; }
}
