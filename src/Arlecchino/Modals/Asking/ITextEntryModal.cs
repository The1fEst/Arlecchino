using Arlecchino.Editing;

namespace Arlecchino.Modals.Asking;

/// <summary>
/// A field that is typed into, shared by the text field and the number field so both edit and complain
/// alike. What is typed and where the caret is come from <see cref="ITextEntry"/>.
/// </summary>
public interface ITextEntryModal : IAffixedModal, ITextEntry
{
    /// <summary>Validation message shown under the field, cleared by typing.</summary>
    string? Message { get; set; }

    /// <summary>Whether to draw dots instead of the text. The value itself stays as typed.</summary>
    bool Masked { get; }

    /// <summary>Whether a character may be typed here at all.</summary>
    /// <param name="character">The character resolved from the key press.</param>
    /// <returns><c>true</c> when it should be inserted.</returns>
    bool AcceptsCharacter(char character);
}
