namespace Arlecchino.State;

/// <summary>
/// A field that is typed into. Shared by the text field and the number field, which is why both
/// behave the same way when it comes to editing and error messages.
/// </summary>
public interface ITextEntryModal : IAffixedModal
{
    /// <summary>
    /// What has been typed so far. Assigning it puts the caret at the end, since replacing the text
    /// wholesale means the old caret no longer refers to anything.
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Where the caret sits, counted in characters from the start of the text. Values outside the text
    /// are pulled back in, so a caret can never point past the end.
    /// </summary>
    int Caret { get; set; }

    /// <summary>Validation message shown under the field, cleared by typing.</summary>
    string? Message { get; set; }

    /// <summary>Whether to draw dots instead of the text. The value itself stays as typed.</summary>
    bool Masked { get; }

    /// <summary>Whether a character may be typed here at all.</summary>
    /// <param name="character">The character resolved from the key press.</param>
    /// <returns><c>true</c> when it should be inserted.</returns>
    bool AcceptsCharacter(char character);
}
