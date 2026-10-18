using System;
using System.IO;
using Arlecchino.Navigation;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Navigation;

public sealed class FilePickerStartTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("arlecchino-start");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void AFolderOpensThere()
    {
        Directory.CreateDirectory(Path.Combine(_root.FullName, "saves"));

        using var app = Show(_root.FullName);

        Assert.Contains("saves", app.Frame(), StringComparison.Ordinal);
    }

    [Fact]
    public void AFileOpensInTheFolderThatHoldsIt()
    {
        var file = Path.Combine(_root.FullName, "game.sav");
        File.WriteAllText(file, "");
        File.WriteAllText(Path.Combine(_root.FullName, "other.sav"), "");

        using var app = Show(file);

        var frame = app.Frame();

        Assert.Contains("game.sav", frame, StringComparison.Ordinal);
        Assert.Contains("other.sav", frame, StringComparison.Ordinal);
    }

    [Fact]
    public void TheFileItStartedFromIsTheOneUnderTheCursor()
    {
        File.WriteAllText(Path.Combine(_root.FullName, "alpha.sav"), "");
        File.WriteAllText(Path.Combine(_root.FullName, "omega.sav"), "");

        using var app = Show(Path.Combine(_root.FullName, "omega.sav"));

        app.Press(ConsoleKey.Enter);

        Assert.Equal(Path.Combine(_root.FullName, "omega.sav"), Choice);
    }

    [Fact]
    public void APathThatIsGoneLandsOnTheDrives()
    {
        using var app = Show(Path.Combine(_root.FullName, "moved", "away.sav"));

        Assert.DoesNotContain("away.sav", app.Frame(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("/saves/..")]
    public void EnterPicksTheFolderItWasAskedFor(string suffix)
    {
        Directory.CreateDirectory(Path.Combine(_root.FullName, "saves"));

        using var app = Show(_root.FullName + suffix, pickFolder: true);

        app.Press(ConsoleKey.Enter);

        Assert.Equal(_root.FullName, Choice);
    }

    [Fact]
    public void RightOpensAFolderAndEnterPicksTheOneUnderTheCursor()
    {
        var saves = Path.Combine(_root.FullName, "saves");
        Directory.CreateDirectory(saves);

        using var app = Show(_root.FullName, pickFolder: true);

        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(saves, Choice);
    }

    [Fact]
    public void LeftPutsTheCursorOnTheFolderItLeft()
    {
        Directory.CreateDirectory(Path.Combine(_root.FullName, "alpha"));
        var saves = Path.Combine(_root.FullName, "saves");
        Directory.CreateDirectory(saves);

        using var app = Show(_root.FullName, pickFolder: true);

        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.DownArrow);
        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.LeftArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(saves, Choice);
    }

    [Fact]
    public void EnterOnAFileDoesNothingWhileFoldersArePicked()
    {
        File.WriteAllText(Path.Combine(_root.FullName, "notes.txt"), "");

        using var app = Show(_root.FullName, pickFolder: true);

        app.Press(ConsoleKey.RightArrow);
        app.Press(ConsoleKey.Enter);

        Assert.Equal("", Choice);
    }

    [Fact]
    public void EnterOnAFolderStillOpensItWhileFilesArePicked()
    {
        var saves = Directory.CreateDirectory(Path.Combine(_root.FullName, "saves"));
        var game = Path.Combine(saves.FullName, "game.sav");
        File.WriteAllText(game, "");

        using var app = Show(_root.FullName);

        app.Press(ConsoleKey.Enter);
        app.Press(ConsoleKey.Enter);

        Assert.Equal(game, Choice);
    }

    private string Choice { get; set; } = "";

    private TestApplication Show(string startAt, bool pickFolder = false)
    {
        var app = new TestApplication(100, 30);

        app.State.FilePicker = new("Pick", pickFolder, startAt, ViewRoute.None, choice => Choice = choice);
        app.Navigator.Apply(Routes.FilePicker);

        return app;
    }
}
