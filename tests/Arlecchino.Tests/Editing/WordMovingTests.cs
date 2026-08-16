using Arlecchino.Editing;
using Arlecchino.Tests.Support;
using Xunit;

namespace Arlecchino.Tests.Editing;

/// <summary>
/// Where the keys that go a word at a time stop: on spaces, and on the dots, slashes and dashes that a
/// name is held together with.
/// </summary>
public sealed class WordMovingTests
{
    [Fact]
    public void AWordEndsWhereTheSpaceDoes()
    {
        var entry = new TestEntry { Text = "one two" };

        TextEditing.MoveWord(entry, -1);

        Assert.Equal(4, entry.Caret);
    }

    [Fact]
    public void ADotEndsAWordTheWayASpaceDoes()
    {
        var entry = new TestEntry { Text = "Arlecchino.Commander" };

        TextEditing.MoveWord(entry, -1);

        Assert.Equal(11, entry.Caret);

        TextEditing.MoveWord(entry, -1);

        Assert.Equal(0, entry.Caret);
    }

    [Fact]
    public void GoingRightStopsAtTheDotAsWell()
    {
        var entry = new TestEntry { Text = "Arlecchino.Commander", Caret = 0 };

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(10, entry.Caret);

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(20, entry.Caret);
    }

    [Fact]
    public void EverythingBetweenTwoWordsIsSteppedOverAtOnce()
    {
        var entry = new TestEntry { Text = "src/../lib", Caret = 0 };

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(3, entry.Caret);

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(10, entry.Caret);
    }

    [Fact]
    public void AnUnderscoreJoinsAWordRatherThanBreakingIt()
    {
        var entry = new TestEntry { Text = "read_dir.txt", Caret = 0 };

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(8, entry.Caret);

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(12, entry.Caret);
    }

    [Fact]
    public void AWordIsAWordInAnyWriting()
    {
        var entry = new TestEntry { Text = "文件.txt", Caret = 0 };

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(2, entry.Caret);

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(6, entry.Caret);
    }

    [Fact]
    public void TheMarksOnALetterStayWithIt()
    {
        var entry = new TestEntry { Text = "café.txt" };

        TextEditing.MoveWord(entry, -1);

        Assert.Equal(6, entry.Caret);

        TextEditing.MoveWord(entry, -1);

        Assert.Equal(0, entry.Caret);
    }

    [Fact]
    public void ALetterOutsideTheCommonRangeIsStillALetter()
    {
        var entry = new TestEntry { Text = "\U00020000\U00020001.txt", Caret = 0 };

        TextEditing.MoveWord(entry, 1);

        Assert.Equal(4, entry.Caret);
    }

    [Fact]
    public void RubbingOutAWordLeavesWhatSeparatedIt()
    {
        var entry = new TestEntry { Text = "lib/Arlecchino" };

        TextEditing.EraseWord(entry);

        Assert.Equal("lib/", entry.Text);
    }
}
