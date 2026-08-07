using System;
using Arlecchino.Input;
using Xunit;
using Arlecchino.Modals.Asking;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Modals;

public sealed class TextModalTests
{
    [Fact]
    public void TypingBuildsTheTextAndEnterSubmitsIt()
    {
        using var app = new TestApplication();
        var submitted = "";

        app.State.RequestText("Name", "", null, value => submitted = value);
        app.Type("abc");
        app.Press(ConsoleKey.Enter);

        Assert.Equal("abc", submitted);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void BackspaceDeletesTheLastCharacter()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "ab", null, static _ => { });
        app.Press(ConsoleKey.Backspace);

        Assert.Equal("a", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void EscapeCancelsWithoutSubmitting()
    {
        using var app = new TestApplication();
        var submitted = false;

        app.State.RequestText("Name", "x", null, _ => submitted = true);
        app.Press(ConsoleKey.Escape);

        Assert.Null(app.State.Modal);
        Assert.False(submitted);
    }

    [Fact]
    public void FailedValidationKeepsTheModalOpenAndShowsTheMessage()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", static text => text.Length == 0 ? "required" : null, static _ => { });
        app.Press(ConsoleKey.Enter);

        Assert.NotNull(app.State.Modal);
        Assert.Contains("required", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TypingClearsTheValidationMessage()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", static text => text.Length == 0 ? "required" : null, static _ => { });
        app.Press(ConsoleKey.Enter);
        app.Type("a");

        Assert.Null(((TextModal)app.State.Modal!).Message);
    }

    [Fact]
    public void PasswordIsMaskedInTheFrameButNotInTheResult()
    {
        using var app = new TestApplication();
        var submitted = "";

        app.State.RequestPassword("Passphrase", value => submitted = value);
        app.Type("secret");

        var frame = app.Frame();
        Assert.Contains("••••••", frame, StringComparison.Ordinal);
        Assert.DoesNotContain("secret", frame, StringComparison.Ordinal);

        app.Press(ConsoleKey.Enter);
        Assert.Equal("secret", submitted);
    }

    [Fact]
    public void EmailIsRejectedUntilItLooksLikeAnAddress()
    {
        using var app = new TestApplication();
        var submitted = "";

        app.State.RequestEmail("Email", "", value => submitted = value);
        app.Type("nope");
        app.Press(ConsoleKey.Enter);

        Assert.NotNull(app.State.Modal);
        Assert.Contains(app.Options.Strings.NotAnEmail(), app.Frame(), StringComparison.Ordinal);

        app.Type("@example.com");
        app.Press(ConsoleKey.Enter);

        Assert.Equal("nope@example.com", submitted);
    }

    [Fact]
    public void LinkIsRejectedUntilItParsesAsHttp()
    {
        using var app = new TestApplication();
        var submitted = "";

        app.State.RequestUrl("Homepage", "", value => submitted = value);
        app.Type("ftp://example.com");
        app.Press(ConsoleKey.Enter);

        Assert.NotNull(app.State.Modal);

        app.State.RequestUrl("Homepage", "https://example.com", value => submitted = value);
        app.Press(ConsoleKey.Enter);

        Assert.Equal("https://example.com", submitted);
    }

    [Fact]
    public void FormatIsCheckedBeforeTheUserValidator()
    {
        using var app = new TestApplication();
        var validatorRan = false;

        app.State.Modal = new TextModal
        {
            Title = "Email",
            Text = "nope",
            Format = TextFormat.Email,
            Validate = _ =>
            {
                validatorRan = true;
                return null;
            },
            OnSubmit = static _ => { },
        };

        app.Press(ConsoleKey.Enter);

        Assert.False(validatorRan);
    }

    [Fact]
    public void AffixesAreDrawnAroundTheField()
    {
        using var app = new TestApplication();

        app.State.Modal = new TextModal
        {
            Title = "Handle",
            Text = "fest",
            Prefix = "@",
            Suffix = " on github",
            OnSubmit = static _ => { },
        };

        Assert.Contains("@fest▏ on github", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void TypingGoesInAtTheCaretAfterMovingLeft()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "ac", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow);
        app.Type("b");

        Assert.Equal("abc", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void BackspaceRemovesTheCharacterBeforeTheCaret()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow);
        app.Press(ConsoleKey.Backspace);

        Assert.Equal("ac", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void DeleteRemovesTheCharacterAfterTheCaret()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.Home);
        app.Press(ConsoleKey.Delete);

        var modal = (TextModal)app.State.Modal!;
        Assert.Equal("bc", modal.Text);
        Assert.Equal(0, modal.Caret);
    }

    [Fact]
    public void HomeAndEndMoveTheCaretToEitherEnd()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.Home);
        Assert.Equal(0, ((TextModal)app.State.Modal!).Caret);

        app.Press(ConsoleKey.End);
        Assert.Equal(3, ((TextModal)app.State.Modal!).Caret);
    }

    [Fact]
    public void TheCaretStopsAtBothEnds()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "ab", null, static _ => { });
        app.Press(ConsoleKey.RightArrow);
        Assert.Equal(2, ((TextModal)app.State.Modal!).Caret);

        for (var press = 0; press < 5; press++)
        {
            app.Press(ConsoleKey.LeftArrow);
        }

        Assert.Equal(0, ((TextModal)app.State.Modal!).Caret);
    }

    [Fact]
    public void ControlLeftMovesToTheStartOfTheWord()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "one two", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow, KeyModifiers.Control);

        Assert.Equal(4, ((TextModal)app.State.Modal!).Caret);
    }

    [Fact]
    public void ControlRightMovesPastTheNextWord()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "one two", null, static _ => { });
        app.Press(ConsoleKey.Home);
        app.Press(ConsoleKey.RightArrow, KeyModifiers.Control);

        Assert.Equal(3, ((TextModal)app.State.Modal!).Caret);
    }

    [Fact]
    public void ControlBackspaceErasesTheWordBeforeTheCaret()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "one two", null, static _ => { });
        app.Press(ConsoleKey.Backspace, KeyModifiers.Control);

        Assert.Equal("one ", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void ControlUErasesEverythingBeforeTheCaret()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "one two", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow);
        app.Press(ConsoleKey.U, KeyModifiers.Control);

        var modal = (TextModal)app.State.Modal!;
        Assert.Equal("o", modal.Text);
        Assert.Equal(0, modal.Caret);
    }

    [Fact]
    public void TheCaretIsDrawnWhereTheTextWillGo()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "abc", null, static _ => { });
        app.Press(ConsoleKey.Home);

        Assert.Contains("▏abc", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void ALongValueScrollsInsteadOfHidingTheCaret()
    {
        using var app = new TestApplication(40, 12);
        var value = new string('x', 200) + "END";

        app.State.RequestText("Path", value, null, static _ => { });

        var line = app.FrameLineContaining("END");
        Assert.Contains("END▏", line, StringComparison.Ordinal);
        Assert.Contains("…", line, StringComparison.Ordinal);
        Assert.True(line.Length <= 40);
    }

    [Fact]
    public void ScrollingBackToTheStartDropsTheLeadingMarker()
    {
        using var app = new TestApplication(40, 12);

        app.State.RequestText("Path", new('x', 200), null, static _ => { });
        app.Press(ConsoleKey.Home);

        var line = app.FrameLineContaining("▏");
        Assert.StartsWith("▏", line.TrimStart(' ', '│'), StringComparison.Ordinal);
        Assert.EndsWith("…", line.TrimEnd(' ', '│'), StringComparison.Ordinal);
    }

    [Fact]
    public void AMaskedFieldShowsOneDotPerSymbol()
    {
        using var app = new TestApplication();

        app.State.RequestPassword("Passphrase", static _ => { });
        app.State.Modal = new TextModal { Title = "Passphrase", Masked = true, Text = "👩‍👩‍👧ab", OnSubmit = static _ => { } };

        Assert.Contains("•••▏", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void BackspaceRemovesAWholeEmojiRatherThanHalfOfIt()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "ab👩‍👩‍👧c", null, static _ => { });
        app.Press(ConsoleKey.LeftArrow);
        app.Press(ConsoleKey.Backspace);

        Assert.Equal("abc", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void TheCaretStepsOverAnEmojiInOneMove()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "👩‍👩‍👧x", null, static _ => { });
        app.Press(ConsoleKey.Home);
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.Delete);

        Assert.Equal("👩‍👩‍👧", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void DeleteRemovesALetterWithItsCombiningMark()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "éx", null, static _ => { });
        app.Press(ConsoleKey.Home);
        app.Press(ConsoleKey.Delete);

        Assert.Equal("x", ((TextModal)app.State.Modal!).Text);
    }

    [Fact]
    public void AShownMessageStaysWhileTheInputIsStillWrong()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", static text => text.Length < 3 ? "too short" : null, static _ => { });
        app.Press(ConsoleKey.Enter);
        app.Type("ab");

        Assert.Equal("too short", ((TextModal)app.State.Modal!).Message);
    }

    [Fact]
    public void AShownMessageGoesAwayAsSoonAsTheInputBecomesValid()
    {
        using var app = new TestApplication();

        app.State.RequestText("Name", "", static text => text.Length < 3 ? "too short" : null, static _ => { });
        app.Press(ConsoleKey.Enter);
        app.Type("abc");

        Assert.Null(((TextModal)app.State.Modal!).Message);
    }

    [Fact]
    public void NothingIsReportedBeforeTheFirstAttemptToSubmit()
    {
        using var app = new TestApplication();

        app.State.RequestEmail("Email", "", static _ => { });
        app.Type("nope");

        Assert.Null(((TextModal)app.State.Modal!).Message);
    }

    [Fact]
    public void ANumberOutOfRangeReportsAgainAsItIsRetyped()
    {
        using var app = new TestApplication();

        app.State.RequestNumber("Count", 0m, 0m, 10m, static _ => { });
        app.Press(ConsoleKey.U, KeyModifiers.Control);
        app.Type("99");
        app.Press(ConsoleKey.Enter);

        var modal = (NumberModal)app.State.Modal!;
        Assert.NotNull(modal.Message);

        app.Press(ConsoleKey.Backspace);
        Assert.Null(modal.Message);
    }

    [Fact]
    public void SteppingANumberLeavesTheCaretAfterIt()
    {
        using var app = new TestApplication();

        app.State.RequestNumber("Count", 5m, 0m, 10m, static _ => { });
        app.Press(ConsoleKey.Home);
        app.Press(ConsoleKey.UpArrow);

        var modal = (NumberModal)app.State.Modal!;
        Assert.Equal("6", modal.Text);
        Assert.Equal(modal.Text.Length, modal.Caret);
    }
}
