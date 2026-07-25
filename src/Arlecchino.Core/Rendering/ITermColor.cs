namespace Arlecchino.Rendering;

/// <summary>
/// Anything that can style a cell. The frame writer only ever asks for <see cref="Ansi"/> and
/// compares styles by reference, so hold on to instances instead of building one per cell.
/// </summary>
public interface ITermColor
{
    /// <summary>
    /// The escape sequence that switches the terminal to this style, or an empty string when colour
    /// is turned off. Implementations are expected to build it once and cache it.
    /// </summary>
    string Ansi { get; }
}
