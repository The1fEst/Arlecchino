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

    private string Choice { get; set; } = "";

    private TestApplication Show(string startAt)
    {
        var app = new TestApplication(100, 30);

        app.State.FilePicker = new("Pick", PickFolder: false, startAt, ViewRoute.None, choice => Choice = choice);
        app.Navigator.Apply(Routes.FilePicker);

        return app;
    }
}
