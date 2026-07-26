using System;
using System.IO;
using Arlecchino.Navigation;
using Xunit;
using Arlecchino.Modals;

namespace Arlecchino.Tests;

public sealed class ModalMouseTests
{
    private static readonly string[] Options = ["alpha", "beta", "gamma"];

    private static (int Row, int Column) RowOf(TestApplication app, string text)
    {
        var lines = app.FrameLines();
        for (var row = 0; row < lines.Length; row++)
        {
            var column = lines[row].IndexOf(text, StringComparison.Ordinal);
            if (column >= 0)
            {
                return (row, column);
            }
        }

        return (-1, -1);
    }

    [Fact]
    public void ClickingAnOptionSelectsItAndTheSecondClickPicks()
    {
        using var app = new TestApplication();
        var picked = "";

        app.State.RequestChoice("Pick", Options, value => picked = value);
        var (row, column) = RowOf(app, "gamma");

        app.Click(row, column);
        Assert.Equal(2, ((ChoiceModal)app.State.Modal!).Index);
        Assert.Equal("", picked);

        app.Click(row, column);
        Assert.Equal("gamma", picked);
        Assert.Null(app.State.Modal);
    }

    [Fact]
    public void ClickingAMultiChoiceRowMarksIt()
    {
        using var app = new TestApplication();

        app.State.RequestMultiChoice("Columns", Options, [], static _ => { });
        var (row, column) = RowOf(app, "beta");

        app.Click(row, column);
        app.Click(row, column);

        Assert.Contains("[×] beta", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void ClickingOnTheSliderTrackSetsTheValue()
    {
        using var app = new TestApplication();

        app.State.RequestSlider("Volume", 0, 0, 100, static _ => { });
        var modal = (SliderModal)app.State.Modal!;
        app.Frame();

        app.Click(modal.Track.Top, modal.Track.Left + modal.Track.Width - 1);
        Assert.Equal(100m, modal.Value);

        app.Click(modal.Track.Top, modal.Track.Left);
        Assert.Equal(0m, modal.Value);
    }

    [Fact]
    public void DraggingTheSliderKeepsUpdatingIt()
    {
        using var app = new TestApplication();

        app.State.RequestSlider("Volume", 0, 0, 100, static _ => { });
        var modal = (SliderModal)app.State.Modal!;
        app.Frame();

        var middle = modal.Track.Left + (modal.Track.Width - 1) / 2;
        app.Click(modal.Track.Top, middle);

        Assert.InRange(modal.Value, 45m, 55m);
    }

    [Fact]
    public void ClickingAToggleChipPicksThatSide()
    {
        using var app = new TestApplication();

        app.State.RequestToggle("Fullscreen", true, static _ => { });
        var modal = (ToggleModal)app.State.Modal!;
        app.Frame();

        app.Click(modal.NoChip.Top, modal.NoChip.Left);
        Assert.False(modal.Value);

        app.Click(modal.YesChip.Top, modal.YesChip.Left);
        Assert.True(modal.Value);
    }

    [Fact]
    public void ClickingAColorChannelSelectsItAndSetsTheValue()
    {
        using var app = new TestApplication();

        app.State.RequestColor("Accent", new(255, 0, 0), static _ => { });
        var modal = (ColorModal)app.State.Modal!;
        app.Frame();

        var lightness = modal.ChannelTracks[(int)ColorChannel.Lightness];
        app.Click(lightness.Top, lightness.Left);

        Assert.Equal(ColorChannel.Lightness, modal.Channel);
        Assert.Equal(0, modal.Lightness);
    }

    [Fact]
    public void ClickingACommandInThePaletteRunsIt()
    {
        using var app = new TestApplication(configure: static builder => builder.AddCommand<ProbeCommand>());

        app.Press(ConsoleKey.Oem1, shift: true);
        var (row, column) = RowOf(app, "Probe command");

        app.Click(row, column);

        Assert.Null(app.State.Modal);
        Assert.Equal("probe command", app.State.Output);
    }

    [Fact]
    public void ClicksOutsideAModalChangeNothing()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });
        app.Frame();

        app.Click(0, 0);

        Assert.NotNull(app.State.Modal);
        Assert.Equal(0, ((ChoiceModal)app.State.Modal!).Index);
    }

    [Fact]
    public void ClickingAFolderRowSelectsItAndTheSecondClickOpensIt()
    {
        var root = Directory.CreateTempSubdirectory("arlecchino-picker");

        try
        {
            Directory.CreateDirectory(Path.Combine(root.FullName, "alpha-folder"));
            Directory.CreateDirectory(Path.Combine(root.FullName, "beta-folder"));

            using var app = new TestApplication(100, 26);

            app.State.FilePicker = new("Pick", PickFolder: true, root.FullName, ViewRoute.None, static _ => { });
            app.Navigator.Apply(Routes.FilePicker);

            var (row, column) = RowOf(app, "beta-folder");
            Assert.True(row > 0);

            app.Click(row, column);
            Assert.Contains("alpha-folder", app.Frame(), StringComparison.Ordinal);

            app.Click(row, column);

            var opened = app.Frame();
            Assert.Contains("beta-folder", opened, StringComparison.Ordinal);
            Assert.DoesNotContain("alpha-folder", opened, StringComparison.Ordinal);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void ClickingThePlacesSidebarMovesFocusThere()
    {
        using var app = new TestApplication(100, 26);

        app.State.FilePicker = new("Pick", PickFolder: true, Path.GetTempPath(), ViewRoute.None, static _ => { });
        app.Navigator.Apply(Routes.FilePicker);

        var before = app.Styles();
        var (row, column) = RowOf(app, app.Options.Strings.FilePicker.Drives());

        Assert.True(row > 0);
        app.Click(row, column);

        Assert.NotEqual(before, app.Styles());
    }

    [Fact]
    public void WheelStillScrollsAnOpenList()
    {
        using var app = new TestApplication();

        app.State.RequestChoice("Pick", Options, static _ => { });
        app.Frame();

        app.Scroll(0, 0, down: true);

        Assert.Equal(1, ((ChoiceModal)app.State.Modal!).Index);
    }
}
