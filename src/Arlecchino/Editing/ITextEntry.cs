namespace Arlecchino.Editing;

/// <summary>
/// A line of text being typed into, which is all the editing needs to know about whatever holds it. A
/// dialog field is one; so is a line an application draws for itself.
/// </summary>
public interface ITextEntry
{
    /// <summary>
    /// Whatever has been typed so far. Assigning it puts the caret at the end, since replacing the text
    /// wholesale means the old caret no longer refers to anything.
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Where the caret sits, counted in characters from the start of the text. Values outside the text
    /// are pulled back in, so a caret can never point past the end.
    /// </summary>
    int Caret { get; set; }
}
