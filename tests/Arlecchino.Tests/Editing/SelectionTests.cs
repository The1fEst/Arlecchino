using System;
using Arlecchino.Editing;
using Arlecchino.Input;
using Arlecchino.Modals.Asking;
using Arlecchino.Tests.Support;
using Xunit;

namespace Arlecchino.Tests.Editing;

/// <summary>
/// Selecting text with Shift held: what the keys select, and what an edit does to the selection rather
/// than to the character beside the caret.
/// </summary>
public sealed class SelectionTests
{
    [Fact]
    public void NothingIsSelectedUntilAKeyWithShiftIsPressed()
    {
        var entry = new TestEntry { Text = "abc" };

        Assert.Equal("", TextEditing.Selected(entry));

        TextEditing.MoveCaret(entry, -1);

        Assert.Equal("", TextEditing.Selected(entry));
    }

    [Fact]
    public void TakingTheCaretWithShiftSelectsWhatItPassed()
    {
        var entry = new TestEntry { Text = "abc" };

        TextEditing.SelectCaret(entry, -1);
        TextEditing.SelectCaret(entry, -1);

        Assert.Equal("bc", TextEditing.Selected(entry));
        Assert.Equal(1, entry.Caret);
    }

    [Fact]
    public void MovingWithoutShiftDropsTheSelection()
    {
        var entry = new TestEntry { Text = "abc" };

        TextEditing.SelectToStart(entry);
        TextEditing.MoveCaret(entry, 1);

        Assert.Equal("", TextEditing.Selected(entry));
    }

    [Fact]
    public void TypingReplacesWhatIsSelected()
    {
        var entry = new TestEntry { Text = "one two" };

        TextEditing.SelectWord(entry, -1);
        TextEditing.Insert(entry, 'x');

        Assert.Equal("one x", entry.Text);
        Assert.Equal(5, entry.Caret);
    }

    [Fact]
    public void RubbingOutTakesTheSelectionRatherThanASymbol()
    {
        var entry = new TestEntry { Text = "abcd" };

        TextEditing.SelectCaret(entry, -1);
        TextEditing.SelectCaret(entry, -1);
        TextEditing.Backspace(entry);

        Assert.Equal("ab", entry.Text);
        Assert.Equal("", TextEditing.Selected(entry));
    }

    [Fact]
    public void SelectingEverythingTakesTheWholeLine()
    {
        var entry = new TestEntry { Text = "git status" };

        TextEditing.MoveToStart(entry);
        TextEditing.SelectAll(entry);

        Assert.Equal("git status", TextEditing.Selected(entry));
    }

    [Fact]
    public void AWholeSymbolIsSelectedAtATime()
    {
        var entry = new TestEntry { Text = "a😀" };

        TextEditing.SelectCaret(entry, -1);

        Assert.Equal("😀", TextEditing.Selected(entry));
    }

    [Fact]
    public void ShiftAndAnArrowSelectsInAFieldOfADialog()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Type("x");

        Assert.Equal("abx", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void TheSelectionInAFieldIsCopiedRatherThanTheWholeValue()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Press(ConsoleKey.Insert, KeyModifiers.Control);

        Assert.Equal("c", app.Terminal.CopiedText);
    }

    [Fact]
    public void CuttingTakesTheSelectionOffTheField()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Press(ConsoleKey.Delete, KeyModifiers.Shift);

        Assert.Equal("c", app.Terminal.CopiedText);
        Assert.Equal("ab", ((TextModal)app.State.Modal!).Text);
    }
}
