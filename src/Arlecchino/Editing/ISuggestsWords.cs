using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Arlecchino.Editing;

/// <summary>
/// Where the words a half-typed one could turn into come from. It is asked rather than read, since the
/// answer can be a folder on the far side of a network.
/// </summary>
/// <seealso cref="WordList"/>
/// <seealso cref="TextCompleter"/>
public interface ISuggestsWords
{
    /// <summary>What the word being typed could still turn into.</summary>
    /// <param name="ask">The word and the line it stands in.</param>
    /// <param name="token">Gives up the wait, as it is given up when the line is typed into again.</param>
    /// <returns>The words, the likeliest first, or nothing when the word can go nowhere.</returns>
    ValueTask<IReadOnlyList<string>> SuggestAsync(CompletionAsk ask, CancellationToken token);
}
