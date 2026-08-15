namespace Arlecchino.Editing;

/// <summary>
/// Which part of a line is the word being finished. A line of shell reaches back to the last space, where
/// a field holding one path is all one word.
/// </summary>
/// <seealso cref="SpaceWords"/>
/// <seealso cref="WholeLine"/>
public interface ICutsWords
{
    /// <summary>Cuts the word being typed out of the line.</summary>
    /// <param name="text">The line as it stands.</param>
    /// <param name="caret">Where the caret is, which is where the word ends.</param>
    /// <returns>The word and the line it was cut from.</returns>
    CompletionAsk Cut(string text, int caret);
}
