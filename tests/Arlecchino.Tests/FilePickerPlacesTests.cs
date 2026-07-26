using System;
using System.IO;
using Arlecchino.Navigation;
using Arlecchino.State;
using Xunit;

namespace Arlecchino.Tests;

public sealed class FilePickerPlacesTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("arlecchino-places");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void APlaceIsListedInTheSidebarWithItsIcon()
    {
        using var app = Show([new("Projects", _root.FullName, "★")]);

        var frame = app.Frame();

        Assert.Contains("★", frame, StringComparison.Ordinal);
        Assert.Contains("Projects", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void APlaceWithoutAnIconGetsTheDefaultOne()
    {
        using var app = Show([new("Projects", _root.FullName)]);

        Assert.Contains("▪ Projects", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void PlacesComeBeforeTheFoldersTheFrameworkOffers()
    {
        using var app = Show([new("Projects", _root.FullName)]);
        var frame = app.Frame();

        Assert.True(
            frame.IndexOf("Projects", StringComparison.Ordinal) <
            frame.IndexOf(app.Options.Strings.FilePicker.Locations(), StringComparison.Ordinal));
    }

    [Fact]
    public void ClickingAPlaceBrowsesToIt()
    {
        Directory.CreateDirectory(Path.Combine(_root.FullName, "inside-the-place"));

        using var app = Show([new("Projects", _root.FullName)], startAt: Path.GetTempPath());

        var (row, column) = RowOf(app, "Projects");
        Assert.True(row > 0);

        app.Click(row, column);
        app.Click(row, column);

        Assert.Contains("inside-the-place", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void EveryPlaceIsListed()
    {
        using var app = Show([
            new("First", _root.FullName),
            new("Second", Path.GetTempPath()),
        ]);

        var frame = app.Frame();

        Assert.Contains("First", frame, StringComparison.Ordinal);
        Assert.Contains("Second", frame, StringComparison.Ordinal);
    }

    private TestApplication Show(FilePickerPlace[] places, string? startAt = null)
    {
        var app = new TestApplication(100, 30);

        app.State.FilePicker = new("Pick", PickFolder: true, startAt ?? _root.FullName, ViewRoute.None,
            static _ => { })
        {
            Places = places,
        };

        app.Navigator.Apply(Routes.FilePicker);
        return app;
    }

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
}
