using System;
using Arlecchino.Rendering.Colors;
using Arlecchino.Editing;
using Arlecchino.Input;
using Arlecchino.Modals.Asking;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

public sealed class TextAreaModalTests
{
    [Fact]
    public void EnterStartsANewLineInsteadOfConfirming()
    {
        using var app = new TestApplication();
        var result = "";

        app.State.RequestTextArea("Notes", "first", text => result = text);

        app.Press(ConsoleKey.Enter);
        app.Type("second");

        Assert.NotNull(app.State.Modal);
        Assert.Equal("", result);
        Assert.Equal("first\nsecond", ((TextAreaModal)app.State.Modal!).Text);
    }

    [Fact]
    public void SubmitConfirmsTheWholeText()
    {
        using var app = new TestApplication();
        var result = "";

        app.State.RequestTextArea("Notes", "one", text => result = text);

        app.Press(ConsoleKey.Enter);
        app.Type("two");
        app.Press(ConsoleKey.Enter, KeyModifiers.Control);

        Assert.Null(app.State.Modal);
        Assert.Equal("one\ntwo", result);
    }

    [Fact]
    public void EscapeThrowsTheTextAway()
    {
        using var app = new TestApplication();
        var result = "";

        app.State.RequestTextArea("Notes", "kept", text => result = text);

        app.Type("!");
        app.Press(ConsoleKey.Escape);

        Assert.Null(app.State.Modal);
        Assert.Equal("", result);
    }

    [Fact]
    public void BackspaceJoinsTheLineOntoTheOneAbove()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "one\ntwo", static _ => { });
        var modal = (TextAreaModal)app.State.Modal!;

        modal.MoveCaret(1, 0);
        app.Press(ConsoleKey.Backspace);

        Assert.Equal("onetwo", modal.Text);
        Assert.Equal(0, modal.Row);
        Assert.Equal(3, modal.Column);
    }

    [Fact]
    public void DeleteAtTheEndOfALinePullsTheNextOneUp()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "one\ntwo", static _ => { });
        var modal = (TextAreaModal)app.State.Modal!;

        modal.MoveCaret(0, 3);
        app.Press(ConsoleKey.Delete);

        Assert.Equal("onetwo", modal.Text);
    }

    [Fact]
    public void ArrowsWalkAcrossLineEnds()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "ab\ncd", static _ => { });
        var modal = (TextAreaModal)app.State.Modal!;

        modal.MoveCaret(0, 2);
        app.Press(ConsoleKey.RightArrow);

        Assert.Equal(1, modal.Row);
        Assert.Equal(0, modal.Column);

        app.Press(ConsoleKey.LeftArrow);

        Assert.Equal(0, modal.Row);
        Assert.Equal(2, modal.Column);
    }

    [Fact]
    public void ValidationKeepsTheDialogOpenAndSaysWhy()
    {
        using var app = new TestApplication();
        var result = "";

        app.State.RequestTextArea(
            "Notes",
            "short",
            text => result = text,
            static text => text.Length < 10 ? "at least ten characters" : null);

        app.Press(ConsoleKey.Enter, KeyModifiers.Control);

        Assert.NotNull(app.State.Modal);
        Assert.Equal("", result);
        Assert.Contains("at least ten characters", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryLineIsDrawnWithTheCaretOnTheCurrentOne()
    {
        using var app = new TestApplication(60, 20);

        app.State.RequestTextArea("Notes", "alpha\nbeta", static _ => { });
        var frame = app.Frame();

        Assert.Contains("alpha", frame, StringComparison.Ordinal);
        Assert.Contains("beta", frame, StringComparison.Ordinal);
        Assert.Equal(Theme.Caret.Ansi, app.StyleAfter("beta"));
    }

    [Fact]
    public void PastedTextKeepsItsLineBreaks()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "", static _ => { });
        app.ReadFromTerminal("\e[200~one\ntwo\e[201~");

        Assert.Equal("one\ntwo", ((TextAreaModal)app.State.Modal!).Text);
    }

    [Fact]
    public void TheWholeTextIsCopiedByEitherCopyCombination()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "one\ntwo", static _ => { });

        app.Press(ConsoleKey.Insert, KeyModifiers.Control);
        Assert.Contains("one\ntwo", app.Terminal.CopiedText, StringComparison.Ordinal);

        app.State.RequestTextArea("Notes", "third", static _ => { });
        app.Press(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);

        Assert.Contains("third", app.Terminal.CopiedText, StringComparison.Ordinal);
    }

    [Fact]
    public void CopyingLeavesTheDialogOpen()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "kept", static _ => { });
        app.Press(ConsoleKey.C, KeyModifiers.Control | KeyModifiers.Shift);

        Assert.NotNull(app.State.Modal);
        Assert.Equal("kept", ((TextAreaModal)app.State.Modal!).Text);
    }

    [Fact]
    public void ShiftAndAnArrowSelectsAcrossRows()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "one\ntwo", static _ => { });
        var modal = (TextAreaModal)app.State.Modal!;

        modal.MoveCaret(1, 3);
        app.Press(ConsoleKey.UpArrow, KeyModifiers.Shift);

        Assert.Equal("\ntwo", TextEditing.Selected(modal));
    }

    [Fact]
    public void TypingOverASelectionReplacesIt()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "one\ntwo", static _ => { });
        var modal = (TextAreaModal)app.State.Modal!;

        modal.MoveCaret(1, 3);
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Shift);
        app.Type("x");

        Assert.Equal("one\nx", modal.Text);
    }

    [Fact]
    public void AWordIsRubbedOutAtATime()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "one two", static _ => { });

        app.Press(ConsoleKey.Backspace, KeyModifiers.Alt);

        Assert.Equal("one ", ((TextAreaModal)app.State.Modal!).Text);
    }

    [Fact]
    public void TheSelectionIsWhatCuttingTakes()
    {
        using var app = new TestApplication();

        app.State.RequestTextArea("Notes", "one two", static _ => { });
        var modal = (TextAreaModal)app.State.Modal!;

        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Control | KeyModifiers.Shift);
        app.Press(ConsoleKey.Delete, KeyModifiers.Shift);

        Assert.Equal("two", app.Terminal.CopiedText);
        Assert.Equal("one ", modal.Text);
    }

    [Fact]
    public void LongTextScrollsSoTheCaretStaysVisible()
    {
        using var app = new TestApplication(60, 20);

        app.State.RequestTextArea("Notes", string.Join("\n", "l0", "l1", "l2", "l3", "l4", "l5"), static _ => { }, visibleRows: 3);
        var modal = (TextAreaModal)app.State.Modal!;

        modal.MoveCaret(5, 0);
        var frame = app.Frame();

        Assert.Contains("l5", frame, StringComparison.Ordinal);
        Assert.DoesNotContain("l0", frame, StringComparison.Ordinal);
    }
}
