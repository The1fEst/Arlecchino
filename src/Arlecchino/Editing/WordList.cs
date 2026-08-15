using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arlecchino.Editing;

/// <summary>
/// Words an application already holds: the names of its commands, the hosts it knows. They are read
/// through a delegate, so the list is whatever it is when the word is being finished.
/// </summary>
public sealed class WordList : ISuggestsWords
{
    private readonly Func<IReadOnlyList<string>> _words;

    /// <summary>Offers what a delegate lists.</summary>
    /// <param name="words">Everything that could be typed, whether it fits what has been or not.</param>
    public WordList(Func<IReadOnlyList<string>> words) => _words = words;

    /// <summary>
    /// The words that begin with what has been typed, in the order they were listed. Case is forgiven,
    /// since a name is looked for rather than checked.
    /// </summary>
    /// <param name="ask">The word and the line it stands in.</param>
    /// <param name="token">Not waited on: the list is already here.</param>
    /// <returns>What fits.</returns>
    public ValueTask<IReadOnlyList<string>> SuggestAsync(CompletionAsk ask, CancellationToken token)
    {
        var word = ask.Word;
        var fitting = new List<string>();

        foreach (var offered in _words())
        {
            if (offered.StartsWith(word, StringComparison.OrdinalIgnoreCase))
            {
                fitting.Add(offered);
            }
        }

        return ValueTask.FromResult<IReadOnlyList<string>>(fitting);
    }
}
