using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Testing;
using Arlecchino.Tests.Views;
using Xunit;

using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Rendering;

public sealed class TextWidthTests
{
    private const string Wide = "日本語";
    private const string Emoji = "🚀";
    private const string Combining = "é";

    [Fact]
    public void EastAsianCharactersTakeTwoColumns()
    {
        Assert.Equal(6, TextWidth.Of(Wide));
        Assert.Equal(2, TextWidth.Of(Emoji));
        Assert.Equal(5, TextWidth.Of("ab" + Emoji + "c"));
    }

    [Fact]
    public void CombiningMarksTakeNone()
    {
        Assert.Equal(2, Combining.Length);
        Assert.Equal(1, TextWidth.Of(Combining));
        Assert.Equal(4, TextWidth.Of("a" + Combining + "bc"));
    }

    [Fact]
    public void TruncateNeverSplitsACharacter()
    {
        Assert.Equal("日本", TextWidth.Truncate(Wide, 4));
        Assert.Equal("日本", TextWidth.Truncate(Wide, 5));
        Assert.Equal("", TextWidth.Truncate(Emoji, 1));
        Assert.Equal(Emoji, TextWidth.Truncate(Emoji, 2));
    }

    [Fact]
    public void PaddingCountsColumnsNotCharacters()
    {
        Assert.Equal(10, TextWidth.Of(TextWidth.PadRight(Wide, 10)));
        Assert.Equal(10, TextWidth.Of(TextWidth.PadLeft(Emoji, 10)));
    }

    [Fact]
    public void WideTextKeepsTheFrameWidth()
    {
        var terminal = new FakeTerminal(20, 3);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        surface.AppendLine(Wide + "|", Theme.Default);
        surface.Build();

        var line = FrameText.Lines(terminal.Written)[0];
        Assert.Equal("日本語|", line.TrimEnd());
        Assert.Equal(20, TextWidth.Of(line));
    }

    [Fact]
    public void OverwritingHalfOfAWideCharacterLeavesNoDebris()
    {
        var terminal = new FakeTerminal(10, 2);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        surface.AppendLine(Wide, Theme.Default);
        surface.WriteAt(0, 1, "x", Theme.Default);
        surface.Build();

        var line = FrameText.Lines(terminal.Written)[0];
        Assert.Equal(" x本語", line.TrimEnd());
        Assert.Equal(10, TextWidth.Of(line));
    }

    [Fact]
    public void WideCharacterIsDroppedRatherThanSplitAtTheEdge()
    {
        var terminal = new FakeTerminal(5, 2);
        var surface = new Surface(terminal) { HorizontalPadding = 0, VerticalPadding = 0 };

        surface.StartFrame();
        surface.WriteAt(0, 4, Emoji, Theme.Default);
        surface.Build();

        Assert.Equal(5, TextWidth.Of(FrameText.Lines(terminal.Written)[0]));
    }

    [Fact]
    public void ModalBoxesStayRectangularWithWideTitlesAndOptions()
    {
        using var app = new TestApplication(60, 20);

        app.State.RequestChoice("日本語のタイトル", ["日本語", "ascii", Emoji + " rocket"], static _ => { });

        var widths = new HashSet<int>();
        foreach (var line in app.FrameLines())
        {
            var width = BoxWidthInColumns(line);
            if (width > 0)
            {
                widths.Add(width);
            }
        }

        Assert.Single(widths);
    }

    [Fact]
    public void HintsBoxStaysRectangularWithWideDescriptions()
    {
        using var app = new TestApplication(60, 20);

        app.Navigator.Apply(ViewKind.Wide);

        var widths = new HashSet<int>();
        foreach (var line in app.FrameLines())
        {
            var width = BoxWidthInColumns(line);
            if (width > 0)
            {
                widths.Add(width);
            }
        }

        Assert.Single(widths);
    }

    private static int BoxWidthInColumns(string line)
    {
        var first = line.IndexOfAny(['╭', '│', '├', '╰']);
        var last = line.LastIndexOfAny(['╮', '│', '┤', '╯']);

        return first < 0 || last <= first ? -1 : TextWidth.Of(line[first..(last + 1)]);
    }
}
