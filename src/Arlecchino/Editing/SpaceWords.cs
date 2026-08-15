using System;

namespace Arlecchino.Editing;

/// <summary>
/// Words told apart by the spaces between them, which is how a line of anything typed reads: the word being
/// finished is what stands between the last space and the caret.
/// </summary>
public sealed class SpaceWords : ICutsWords
{
    /// <inheritdoc/>
    public CompletionAsk Cut(string text, int caret)
    {
        var end = Math.Clamp(caret, 0, text.Length);
        var start = end;

        while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
        {
            start--;
        }

        return new(text, start, end - start);
    }
}
