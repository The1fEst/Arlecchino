using System;

namespace Arlecchino.Editing;

/// <summary>
/// Everything up to the caret as one word, for a field that holds one thing: a path, a host, a name. Nothing
/// in it divides it, spaces included, since a name is allowed to have them.
/// </summary>
public sealed class WholeLine : ICutsWords
{
    /// <inheritdoc/>
    public CompletionAsk Cut(string text, int caret)
    {
        return new(text, 0, Math.Clamp(caret, 0, text.Length));
    }
}
