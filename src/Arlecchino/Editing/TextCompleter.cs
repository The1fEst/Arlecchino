using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Hosting;
using Arlecchino.Input;

namespace Arlecchino.Editing;

/// <summary>
/// Finishing the word being typed, hung on any line of text. The first press fills in what every candidate
/// agrees on, later presses step through them, and typing on leaves the offer behind.
/// </summary>
/// <example>
/// <code>
/// var completer = new TextCompleter(entry, new WordList(() => Commands), new SpaceWords(), keymap);
///
/// if (completer.Handle(key))
/// {
///     return true;
/// }
/// </code>
/// </example>
public sealed class TextCompleter
{
    private readonly ITextEntry _entry;
    private readonly ISuggestsWords _words;
    private readonly ICutsWords _cuts;
    private readonly ArlecchinoKeymap _keymap;

    private CancellationTokenSource? _asking;
    private IReadOnlyList<string> _matches = [];
    private CompletionAsk _ask;
    private string _stem = "";
    private int _chosenIndex = -1;

    /// <summary>Hangs completion on a line.</summary>
    /// <param name="entry">The line being typed into.</param>
    /// <param name="words">Where the words come from.</param>
    /// <param name="cuts">Which part of the line is the word being finished.</param>
    /// <param name="keymap">The keys the application obeys, which <see cref="Handle"/> reads by.</param>
    public TextCompleter(ITextEntry entry, ISuggestsWords words, ICutsWords cuts, ArlecchinoKeymap keymap)
    {
        _entry = entry;
        _words = words;
        _cuts = cuts;
        _keymap = keymap;
    }

    /// <summary>
    /// What the last press found, for an application that draws them. It is empty while nothing has been
    /// offered and empties itself once the line has been typed into again.
    /// </summary>
    public IReadOnlyList<string> Words => IsOffering ? _matches : [];

    /// <summary>
    /// Which of <see cref="Words"/> is on the line now, or <c>-1</c> while the line holds only as much as
    /// they all agree on and none of them in particular.
    /// </summary>
    public int ChosenIndex => IsOffering ? _chosenIndex : -1;

    /// <summary>Whether the words offered are still the ones this line holds.</summary>
    private bool IsOffering => _matches.Count > 0 && _entry.Text == _stem;

    /// <summary>
    /// Finishes the word, or steps to the next of what was offered for it. Any other key is left alone and
    /// this can be asked before the editing keys are.
    /// </summary>
    /// <param name="key">The key that arrived.</param>
    /// <returns><c>true</c> when the key was one of these and has been dealt with.</returns>
    public bool Handle(KeyPress key)
    {
        if (_keymap.CompleteBack.Matches(key))
        {
            Complete(false);

            return true;
        }

        if (!_keymap.Complete.Matches(key))
        {
            return false;
        }

        Complete(true);

        return true;
    }

    /// <summary>
    /// Finishes the word being typed. Where the words are known already, this steps through them instead
    /// of asking for them again.
    /// </summary>
    /// <param name="forward"><c>true</c> to step to the next of them, <c>false</c> to the one before.</param>
    public void Complete(bool forward)
    {
        if (IsOffering)
        {
            Step(forward);

            return;
        }

        Ask();
    }

    /// <summary>
    /// Drops what was offered and gives up whatever is still being asked for. A line that is closed or
    /// wiped calls it; a line that is only typed into need not, since an offer outlives no edit.
    /// </summary>
    public void Forget()
    {
        _asking?.Cancel();
        _asking?.Dispose();
        _asking = null;
        _matches = [];
        _stem = "";
        _chosenIndex = -1;
    }

    /// <summary>
    /// Asks for the words the half-typed one could turn into. A source that knows them answers inside this
    /// call, and one that has to go and look answers on a later frame.
    /// </summary>
    private void Ask()
    {
        Forget();

        var ask = _cuts.Cut(_entry.Text, _entry.Caret);
        var asking = new CancellationTokenSource();
        var answer = _words.SuggestAsync(ask, asking.Token);

        _asking = asking;

        if (answer.IsCompletedSuccessfully)
        {
            Found(ask, answer.Result);

            return;
        }

        _ = Waited();

        async Task Waited()
        {
            try
            {
                var matches = await answer.ConfigureAwait(false);

                FrameThread.Post(() => Arrived(asking, ask, matches));
            }
            catch (OperationCanceledException) { }
        }
    }

    /// <summary>
    /// What a source that had to go and look came back with, on the drawing thread. It is put in only while
    /// it is still wanted: another press or a typed letter leaves it to be thrown away.
    /// </summary>
    /// <param name="asking">The press that asked, which is cancelled once anything else has happened.</param>
    /// <param name="ask">What was asked.</param>
    /// <param name="matches">What came back.</param>
    private void Arrived(CancellationTokenSource asking, CompletionAsk ask, IReadOnlyList<string> matches)
    {
        if (!ReferenceEquals(_asking, asking) || asking.IsCancellationRequested || _entry.Text != ask.Line)
        {
            return;
        }

        Found(ask, matches);
    }

    /// <summary>
    /// Puts what was found on the line: as much of it as every candidate agrees on, or the first of them
    /// where they agree on nothing more than was typed.
    /// </summary>
    /// <param name="ask">What was asked.</param>
    /// <param name="matches">What came back.</param>
    private void Found(CompletionAsk ask, IReadOnlyList<string> matches)
    {
        if (matches.Count == 0)
        {
            return;
        }

        var stem = Shared(matches);

        _ask = ask;
        _matches = matches;
        _chosenIndex = stem.Length > ask.Word.Length ? -1 : 0;

        Fill(_chosenIndex < 0 ? stem : matches[0]);
    }

    /// <summary>Steps through what was offered, coming round to the first again past the last.</summary>
    /// <param name="forward">Which way to step.</param>
    private void Step(bool forward)
    {
        _chosenIndex = forward
            ? (_chosenIndex + 1) % _matches.Count
            : _chosenIndex <= 0
                ? _matches.Count - 1
                : _chosenIndex - 1;

        Fill(_matches[_chosenIndex]);
    }

    /// <summary>
    /// Puts a word where the half-typed one was, with the caret after it and whatever followed the caret
    /// left where it was.
    /// </summary>
    /// <param name="word">What to put there.</param>
    private void Fill(string word)
    {
        _entry.Text = _ask.Prefix + word + _ask.Suffix;
        _entry.Caret = _ask.Start + word.Length;
        _entry.Anchor = _entry.Caret;
        _stem = _entry.Text;
    }

    /// <summary>
    /// The beginning every candidate shares, which is what a press fills in when there is more than one of
    /// them. Case is forgiven while they are compared, and the first of them lends its own.
    /// </summary>
    /// <param name="matches">The candidates.</param>
    /// <returns>The beginning they all have.</returns>
    private static string Shared(IReadOnlyList<string> matches)
    {
        var stemLength = matches[0].Length;

        for (var at = 1; at < matches.Count; at++)
        {
            var other = matches[at];
            var count = 0;

            while (count < stemLength &&
                   count < other.Length &&
                   char.ToLowerInvariant(matches[0][count]) == char.ToLowerInvariant(other[count]))
            {
                count++;
            }

            stemLength = count;
        }

        return matches[0][..stemLength];
    }
}
