using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Arlecchino.Editing;
using Arlecchino.Hosting;
using Xunit;

namespace Arlecchino.Tests.Editing;

/// <summary>
/// Finishing the word being typed: what one press fills in, what the presses after it step through, and what
/// becomes of an answer that arrives after the line has moved on.
/// </summary>
public sealed class CompletionTests
{
    private const int Frames = 100;
    private const int Wait = 2;

    private static readonly ArlecchinoKeymap Keys = new();

    [Fact]
    public void ThePressFillsInAsMuchAsEveryWordAgreesOn()
    {
        var entry = new TextEntry { Text = "co" };
        var completer = Completing(entry, "commit", "commander");

        completer.Complete(true);

        Assert.Equal("comm", entry.Text);
        Assert.Equal(4, entry.Caret);
    }

    [Fact]
    public void OneWordIsPutInWhole()
    {
        var entry = new TextEntry { Text = "git st" };
        var completer = Completing(entry, "status");

        completer.Complete(true);

        Assert.Equal("git status", entry.Text);
        Assert.Equal(10, entry.Caret);
    }

    [Fact]
    public void OnlyTheWordUnderTheCaretIsReplaced()
    {
        var entry = new TextEntry { Text = "cp fi there", Caret = 5 };
        var completer = Completing(entry, "file.txt");

        completer.Complete(true);

        Assert.Equal("cp file.txt there", entry.Text);
        Assert.Equal(11, entry.Caret);
    }

    [Fact]
    public void APressAfterTheOneThatFilledInStepsThroughWhatWasOffered()
    {
        var entry = new TextEntry { Text = "co" };
        var completer = Completing(entry, "commit", "commander");

        completer.Complete(true);
        completer.Complete(true);

        Assert.Equal("commit", entry.Text);

        completer.Complete(true);

        Assert.Equal("commander", entry.Text);

        completer.Complete(true);

        Assert.Equal("commit", entry.Text);
    }

    [Fact]
    public void SteppingBackComesRoundToTheLastOfThem()
    {
        var entry = new TextEntry { Text = "co" };
        var completer = Completing(entry, "commit", "commander");

        completer.Complete(true);
        completer.Complete(false);

        Assert.Equal("commander", entry.Text);
    }

    [Fact]
    public void WordsThatAgreeOnNothingNewGoStraightToTheFirstOfThem()
    {
        var entry = new TextEntry { Text = "c" };
        var completer = Completing(entry, "cp", "cat");

        completer.Complete(true);

        Assert.Equal("cp", entry.Text);
    }

    [Fact]
    public void WhatWasOfferedIsLeftBehindOnceTheLineIsTypedInto()
    {
        var entry = new TextEntry { Text = "co" };
        var completer = Completing(entry, "commit", "commander");

        completer.Complete(true);

        Assert.Equal(2, completer.Words.Count);

        TextEditing.Insert(entry, 'i');

        Assert.Empty(completer.Words);
        Assert.Equal(-1, completer.ChosenIndex);
    }

    [Fact]
    public void WhichOfThemIsOnTheLineIsSaidOnlyOnceOneOfThemIs()
    {
        var entry = new TextEntry { Text = "co" };
        var completer = Completing(entry, "commit", "commander");

        completer.Complete(true);

        Assert.Equal(-1, completer.ChosenIndex);

        completer.Complete(true);

        Assert.Equal(0, completer.ChosenIndex);
    }

    [Fact]
    public void AWordThatNothingFitsLeavesTheLineAlone()
    {
        var entry = new TextEntry { Text = "zz" };
        var completer = Completing(entry, "commit");

        completer.Complete(true);

        Assert.Equal("zz", entry.Text);
        Assert.Empty(completer.Words);
    }

    [Fact]
    public void AFieldOfOnePathIsAllOneWord()
    {
        var entry = new TextEntry { Text = "My Doc" };
        var completer = new TextCompleter(
            entry,
            new WordList(static () => ["My Documents"]),
            new WholeLine(),
            Keys);

        completer.Complete(true);

        Assert.Equal("My Documents", entry.Text);
    }

    [Fact]
    public void TheKeyIsTakenAndEveryOtherKeyIsLeftAlone()
    {
        var entry = new TextEntry { Text = "co" };
        var completer = Completing(entry, "commit");

        Assert.False(completer.Handle(new(ConsoleKey.A, default, 'a')));
        Assert.True(completer.Handle(new(ConsoleKey.Tab)));
        Assert.Equal("commit", entry.Text);
    }

    [Fact]
    public void ForgettingDropsWhatWasOffered()
    {
        var entry = new TextEntry { Text = "co" };
        var completer = Completing(entry, "commit", "commander");

        completer.Complete(true);
        completer.Forget();

        Assert.Empty(completer.Words);
    }

    [Fact]
    public void AWordThatHasToBeLookedUpArrivesOnALaterFrame()
    {
        var entry = new TextEntry { Text = "co" };
        var asks = new TaskCompletionSource<IReadOnlyList<string>>();
        var completer = new TextCompleter(entry, new Later(asks.Task), new SpaceWords(), Keys);

        completer.Complete(true);

        Assert.Equal("co", entry.Text);

        asks.SetResult(["commit"]);
        Waited(() => entry.Text == "commit");

        Assert.Equal("commit", entry.Text);
    }

    [Fact]
    public void AnAnswerIsThrownAwayWhenTheLineHasMovedOn()
    {
        var entry = new TextEntry { Text = "co" };
        var asks = new TaskCompletionSource<IReadOnlyList<string>>();
        var completer = new TextCompleter(entry, new Later(asks.Task), new SpaceWords(), Keys);

        completer.Complete(true);
        TextEditing.Insert(entry, 'x');

        asks.SetResult(["commit"]);
        Waited(() => false);

        Assert.Equal("cox", entry.Text);
        Assert.Empty(completer.Words);
    }

    private static TextCompleter Completing(TextEntry entry, params string[] words) =>
        new(entry, new WordList(() => words), new SpaceWords(), Keys);

    /// <summary>
    /// Runs what was posted to the drawing thread, as a run of frames would, until something is so. A source
    /// that answers on another thread posts whenever it gets round to it, so this waits rather than looking
    /// once.
    /// </summary>
    /// <param name="until">What is being waited for.</param>
    private static void Waited(Func<bool> until)
    {
        for (var frame = 0; frame < Frames && !until(); frame++)
        {
            FrameThread.RunPending(static error => throw error);
            Thread.Sleep(Wait);
        }
    }

    /// <summary>A source that answers only once it is told to, as a folder on a server does.</summary>
    private sealed class Later : ISuggestsWords
    {
        private readonly Task<IReadOnlyList<string>> _answer;

        public Later(Task<IReadOnlyList<string>> answer) => _answer = answer;

        public ValueTask<IReadOnlyList<string>> SuggestAsync(CompletionAsk ask, CancellationToken token) =>
            new(_answer.WaitAsync(token));
    }
}
