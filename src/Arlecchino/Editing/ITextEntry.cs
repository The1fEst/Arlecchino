namespace Arlecchino.Editing;

/// <summary>
/// A line of text being typed into, which is all the editing needs to know about whatever holds it. A
/// dialog field is one; so is a line an application draws for itself.
/// </summary>
public interface ITextEntry
{
    /// <summary>
    /// Whatever has been typed so far. Assigning it puts the caret and the anchor at the end, since
    /// replacing the text wholesale means neither refers to anything anymore.
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Where the caret sits, counted in characters from the start of the text. Values outside the text
    /// are pulled back in, so a caret can never point past the end.
    /// </summary>
    int Caret { get; set; }

    /// <summary>
    /// Where the selection was started from, counted the same way. It stands on the caret while nothing is
    /// selected, and the text between the two is what is selected while they differ.
    /// </summary>
    int Anchor { get; set; }
}
