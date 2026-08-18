namespace Arlecchino.Editing;

/// <summary>
/// The half-typed word something is being asked to finish, and the line it stands in. The line goes with
/// it because what a word could turn into depends on what stands in front of it.
/// </summary>
/// <param name="Line">The line as it stands.</param>
/// <param name="Start">Where the word begins in it.</param>
/// <param name="Length">How long the word is. It ends where the caret is.</param>
public readonly record struct CompletionAsk(string Line, int Start, int Length)
{
    /// <summary>The word itself, which is empty where the caret stands after a space.</summary>
    public string Word => Line.Substring(Start, Length);

    /// <summary>Whatever stands in front of the word.</summary>
    public string Prefix => Line[..Start];

    /// <summary>Whatever follows the caret, which finishing the word leaves where it is.</summary>
    public string Suffix => Line[(Start + Length)..];
}
