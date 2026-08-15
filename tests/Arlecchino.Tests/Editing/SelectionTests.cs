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
        var entry = new Line { Text = "abc" };

        Assert.Equal("", TextEditing.Selected(entry));

        TextEditing.MoveCaret(entry, -1);

        Assert.Equal("", TextEditing.Selected(entry));
    }

    [Fact]
    public void TakingTheCaretWithShiftSelectsWhatItPassed()
    {
        var entry = new Line { Text = "abc" };

        TextEditing.SelectCaret(entry, -1);
        TextEditing.SelectCaret(entry, -1);

        Assert.Equal("bc", TextEditing.Selected(entry));
        Assert.Equal(1, entry.Caret);
    }

    [Fact]
    public void MovingWithoutShiftDropsTheSelection()
    {
        var entry = new Line { Text = "abc" };

        TextEditing.SelectToStart(entry);
        TextEditing.MoveCaret(entry, 1);

        Assert.Equal("", TextEditing.Selected(entry));
    }

    [Fact]
    public void TypingReplacesWhatIsSelected()
    {
        var entry = new Line { Text = "one two" };

        TextEditing.SelectWord(entry, -1);
        TextEditing.Insert(entry, 'x');

        Assert.Equal("one x", entry.Text);
        Assert.Equal(5, entry.Caret);
    }

    [Fact]
    public void RubbingOutTakesTheSelectionRatherThanASymbol()
    {
        var entry = new Line { Text = "abcd" };

        TextEditing.SelectCaret(entry, -1);
        TextEditing.SelectCaret(entry, -1);
        TextEditing.Backspace(entry);

        Assert.Equal("ab", entry.Text);
        Assert.Equal("", TextEditing.Selected(entry));
    }

    [Fact]
    public void SelectingEverythingTakesTheWholeLine()
    {
        var entry = new Line { Text = "git status" };

        TextEditing.MoveToStart(entry);
        TextEditing.SelectAll(entry);

        Assert.Equal("git status", TextEditing.Selected(entry));
    }

    [Fact]
    public void AWholeSymbolIsSelectedAtATime()
    {
        var entry = new Line { Text = "a😀" };

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

        Assert.Equal("c", app.Terminal.Copied);
    }

    [Fact]
    public void CuttingTakesTheSelectionOffTheField()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Press(ConsoleKey.Delete, KeyModifiers.Shift);

        Assert.Equal("c", app.Terminal.Copied);
        Assert.Equal("ab", ((TextModal)app.State.Modal!).Text);
    }

    private sealed class Line : ITextEntry
    {
        private string _text = "";
        private int _caret;
        private int _anchor;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                _caret = value.Length;
                _anchor = value.Length;
            }
        }

        public int Caret
        {
            get => _caret;
            set => _caret = Math.Clamp(value, 0, _text.Length);
        }

        public int Anchor
        {
            get => _anchor;
            set => _anchor = Math.Clamp(value, 0, _text.Length);
        }
    }
}
