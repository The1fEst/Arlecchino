using System;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Rendering.Terminals;
using Arlecchino.Testing;
using Xunit;

namespace Arlecchino.Tests.Hosting;

public sealed class TerminalProbeTests
{
    [Fact]
    public void DeviceAttributesListingFourMeanSixel()
    {
        var answers = TerminalProbe.Read("\e[?62;4;6;22c", 80, 24);

        Assert.True(answers.Sixel);
        Assert.False(answers.Kitty);
    }

    [Fact]
    public void DeviceAttributesWithoutFourMeanNoSixel()
    {
        Assert.False(TerminalProbe.Read("\e[?62;22c", 80, 24).Sixel);
        Assert.False(TerminalProbe.Read("\e[?1;2c", 80, 24).Sixel);
    }

    [Fact]
    public void AnAttributeThatMerelyContainsAFourIsNotSixel()
    {
        Assert.False(TerminalProbe.Read("\e[?64;41c", 80, 24).Sixel);
    }

    [Fact]
    public void TheKittyQueryBeingAnsweredMeansKitty()
    {
        var answers = TerminalProbe.Read("\e_Gi=31;OK\e\\\e[?62;22c", 80, 24);

        Assert.True(answers.Kitty);
        Assert.False(answers.Sixel);
    }

    [Fact]
    public void ACellSizeReportIsTakenAsItIs()
    {
        var answers = TerminalProbe.Read("\e[6;38;19t\e[?62;22c", 80, 24);

        Assert.Equal(19, answers.CellWidth);
        Assert.Equal(38, answers.CellHeight);
    }

    [Fact]
    public void ATextAreaReportIsDividedByTheGrid()
    {
        var answers = TerminalProbe.Read("\e[4;480;800t\e[?62;22c", 80, 24);

        Assert.Equal(10, answers.CellWidth);
        Assert.Equal(20, answers.CellHeight);
    }

    [Fact]
    public void TheCellReportWinsOverTheTextArea()
    {
        var answers = TerminalProbe.Read("\e[4;480;800t\e[6;38;19t\e[?62;22c", 80, 24);

        Assert.Equal(19, answers.CellWidth);
        Assert.Equal(38, answers.CellHeight);
    }

    [Fact]
    public void TheBackgroundIsReadWhateverWidthTheChannelsCameIn()
    {
        Assert.Equal(
            new Rgb(0x12, 0x34, 0x56),
            TerminalProbe.Read("\e]11;rgb:1234/3456/5678\a\e[?62;22c", 80, 24).Background);

        Assert.Equal(
            new Rgb(0x12, 0x34, 0x56),
            TerminalProbe.Read("\e]11;rgb:12/34/56\a\e[?62;22c", 80, 24).Background);

        Assert.Equal(
            new Rgb(0x11, 0x22, 0x33),
            TerminalProbe.Read("\e]11;rgb:1/2/3\a\e[?62;22c", 80, 24).Background);
    }

    [Fact]
    public void ABackgroundThatWasNotSaidStaysUnknown()
    {
        Assert.Null(TerminalProbe.Read("\e[?62;22c", 80, 24).Background);
        Assert.Null(TerminalProbe.Read("\e]11;rgb:12/34\a", 80, 24).Background);
        Assert.Null(TerminalProbe.Read("\e]11;rgb:12-34-56\a", 80, 24).Background);
        Assert.Null(TerminalProbe.Read("\e]11;#123456\a", 80, 24).Background);
    }

    [Fact]
    public void ATerminalThatSaidNothingSettlesEverything()
    {
        var answers = TerminalProbe.Read("", 80, 24);

        Assert.False(answers.Sixel);
        Assert.False(answers.Kitty);
        Assert.Equal(0, answers.CellWidth);
        Assert.Equal(0, answers.CellHeight);
    }

    [Fact]
    public void HalfAnAnswerIsNoAnswer()
    {
        Assert.Equal(0, TerminalProbe.Read("\e[6;38", 80, 24).CellWidth);
        Assert.Equal(0, TerminalProbe.Read("\e[6;38;19", 80, 24).CellWidth);
        Assert.Equal(0, TerminalProbe.Read("\e[6;;t", 80, 24).CellWidth);
        Assert.False(TerminalProbe.Read("\e[?62;4;6;22", 80, 24).Sixel);
    }

    [Fact]
    public void KeysTypedDuringTheWaitDoNotBecomeCapabilities()
    {
        var answers = TerminalProbe.Read("hello\e[?62;22cworld", 80, 24);

        Assert.False(answers.Sixel);
        Assert.False(answers.Kitty);
        Assert.Equal(0, answers.CellWidth);
    }

    [Fact]
    public void AskingATerminalThatSaysNothingLeavesEverythingAlone()
    {
        var was = (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty, Glyphs.CellWidth, Glyphs.CellHeight);

        try
        {
            var terminal = new FakeTerminal(80, 24);

            Assert.False(TerminalProbe.Ask(terminal, TimeSpan.FromMilliseconds(5)));
            Assert.Equal(was.CellWidth, Glyphs.CellWidth);
            Assert.Contains("\e[c", terminal.Written, StringComparison.Ordinal);
            Assert.Contains("a=q", terminal.Written, StringComparison.Ordinal);
            Assert.Contains("\e[16t", terminal.Written, StringComparison.Ordinal);
        }
        finally
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty, Glyphs.CellWidth, Glyphs.CellHeight) = was;
        }
    }

    [Fact]
    public void ASizeThatWasReportedIsToldApartFromTheStandingGuess()
    {
        var was = (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty,
            TerminalCapabilities.CellSizeKnown, Glyphs.CellWidth, Glyphs.CellHeight);

        try
        {
            var silent = new FakeTerminal(80, 24);

            TerminalProbe.Ask(silent, TimeSpan.FromMilliseconds(5));

            Assert.False(TerminalCapabilities.CellSizeKnown);
            Assert.Equal(was.CellWidth, Glyphs.CellWidth);

            var talking = new FakeTerminal(80, 24);

            talking.EnqueueText("\e[6;20;10t\e[?62;22c");
            TerminalProbe.Ask(talking, TimeSpan.FromSeconds(5));

            Assert.True(TerminalCapabilities.CellSizeKnown);
            Assert.Equal(10, Glyphs.CellWidth);
            Assert.Equal(20, Glyphs.CellHeight);
        }
        finally
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty,
                TerminalCapabilities.CellSizeKnown, Glyphs.CellWidth, Glyphs.CellHeight) = was;
        }
    }

    [Fact]
    public void AKeyPressedDuringTheWaitIsHandedBackRatherThanEaten()
    {
        var was = (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty, Glyphs.CellWidth);

        try
        {
            var terminal = new FakeTerminal(80, 24);

            terminal.EnqueueText("qw");

            TerminalProbe.Ask(terminal, TimeSpan.FromSeconds(5));

            Assert.True(terminal.KeyAvailable);
            Assert.Equal('q', terminal.ReadKey().KeyChar);
            Assert.Equal('w', terminal.ReadKey().KeyChar);
        }
        finally
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty, Glyphs.CellWidth) = was;
        }
    }

    [Fact]
    public void KeysTypedInBetweenTheAnswersAreHandedBackToo()
    {
        var was = (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty,
            TerminalCapabilities.CellSizeKnown, Glyphs.CellWidth, Glyphs.CellHeight);

        try
        {
            var terminal = new FakeTerminal(80, 24);

            terminal.EnqueueText("\e[6;25;11txy\e[?65;4;22cz");

            Assert.True(TerminalProbe.Ask(terminal, TimeSpan.FromSeconds(5)));
            Assert.True(TerminalCapabilities.Sixel);
            Assert.Equal(11, Glyphs.CellWidth);

            Assert.Equal('x', terminal.ReadKey().KeyChar);
            Assert.Equal('y', terminal.ReadKey().KeyChar);
            Assert.Equal('z', terminal.ReadKey().KeyChar);
            Assert.False(terminal.KeyAvailable);
        }
        finally
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty,
                TerminalCapabilities.CellSizeKnown, Glyphs.CellWidth, Glyphs.CellHeight) = was;
        }
    }

    [Fact]
    public void AnAnswerIsNeverMistakenForTyping()
    {
        Assert.Empty(TerminalReply.Typing("\e_Gi=31;OK\e\\\e[6;25;11t\e]11;rgb:20/20/22\a\e[?65;4;22c"));
        Assert.Empty(TerminalReply.Typing("\\\e[?62;22c"));
        Assert.Empty(TerminalReply.Typing("\eOP\e[?62;22c"));
    }

    [Fact]
    public void TypingIsFoundWhereverItLands()
    {
        Assert.Equal([0, 1], TerminalReply.Typing("hi\e[?62;22c"));
        Assert.Equal([11, 12], TerminalReply.Typing("\e[?62;22c\e\\hi"));
        Assert.Equal([1], TerminalReply.Typing("\\h\e[?62;22c"));
    }

    [Fact]
    public void RubbishInFrontOfTheAnswersDoesNotThrowThemAway()
    {
        var was = (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty,
            TerminalCapabilities.CellSizeKnown, Glyphs.CellWidth, Glyphs.CellHeight);

        try
        {
            var terminal = new FakeTerminal(80, 24);

            terminal.EnqueueText("\\\e[6;25;11t\e]11;rgb:2020/2020/2222\e\\\e[?65;4;6;18;22;52c");

            Assert.True(TerminalProbe.Ask(terminal, TimeSpan.FromSeconds(5)));
            Assert.True(TerminalCapabilities.Sixel);
            Assert.Equal(11, Glyphs.CellWidth);
            Assert.Equal(25, Glyphs.CellHeight);
            Assert.Equal(new Rgb(0x20, 0x20, 0x22), TerminalCapabilities.Background);
        }
        finally
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty,
                TerminalCapabilities.CellSizeKnown, Glyphs.CellWidth, Glyphs.CellHeight) = was;
        }
    }

    [Fact]
    public void AutoTakesTheBestOfWhatWasAdmittedTo()
    {
        var was = (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty);

        try
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty) = (false, false);

            Assert.Equal(ImageProtocol.Blocks, TerminalCapabilities.Resolve(ImageProtocol.Auto));

            TerminalCapabilities.Sixel = true;

            Assert.Equal(ImageProtocol.Sixel, TerminalCapabilities.Resolve(ImageProtocol.Auto));

            TerminalCapabilities.Kitty = true;

            Assert.Equal(ImageProtocol.Kitty, TerminalCapabilities.Resolve(ImageProtocol.Auto));
        }
        finally
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty) = was;
        }
    }

    [Fact]
    public void ANamedProtocolIsNeverSecondGuessed()
    {
        var was = (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty);

        try
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty) = (true, true);

            Assert.Equal(ImageProtocol.Blocks, TerminalCapabilities.Resolve(ImageProtocol.Blocks));
            Assert.Equal(ImageProtocol.Sixel, TerminalCapabilities.Resolve(ImageProtocol.Sixel));
        }
        finally
        {
            (TerminalCapabilities.Sixel, TerminalCapabilities.Kitty) = was;
        }
    }
}
