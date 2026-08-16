using System;
using System.IO;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Tests.Support;
using Xunit;

namespace Arlecchino.Tests.Navigation;

/// <summary>
/// The picker's filter is a line of text like any other once something is being filtered by. With nothing
/// typed the same keys still browse, so an empty filter never takes a key the listing wants.
/// </summary>
public sealed class FilePickerFilterTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("arlecchino-filter");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void TheCaretWalksTheFilterRatherThanLeavingTheFolder()
    {
        File.WriteAllText(Path.Combine(_root.FullName, "notes.txt"), "");

        using var app = Show();

        app.Type("ns");
        app.Press(ConsoleKey.LeftArrow);
        app.Type("ote");

        Assert.Contains("notes", app.Frame(), StringComparison.Ordinal);
        Assert.Equal(_root.FullName, Folder(app));
    }

    [Fact]
    public void CopyingTheFilterReachesTheClipboard()
    {
        using var app = Show();

        app.Type("note");
        app.Press(ConsoleKey.Insert, KeyModifiers.Control);

        Assert.Equal("note", app.Terminal.Copied);
    }

    [Fact]
    public void PastedTextNarrowsTheListing()
    {
        File.WriteAllText(Path.Combine(_root.FullName, "alpha.txt"), "");
        File.WriteAllText(Path.Combine(_root.FullName, "omega.txt"), "");

        using var app = Show();

        app.ReadFromTerminal("\e[200~omeg\e[201~");

        var frame = app.Frame();

        Assert.Contains("omega.txt", frame, StringComparison.Ordinal);
        Assert.DoesNotContain("alpha.txt", frame, StringComparison.Ordinal);
    }

    /// <summary>With nothing typed, the left arrow is still the key that leaves a folder.</summary>
    [Fact]
    public void AnEmptyFilterLeavesTheBrowsingKeysAlone()
    {
        Directory.CreateDirectory(Path.Combine(_root.FullName, "inside"));

        using var app = Show(Path.Combine(_root.FullName, "inside"));

        app.Press(ConsoleKey.LeftArrow);

        Assert.Equal(_root.FullName, Folder(app));
    }

    private string Picked { get; set; } = "";

    /// <summary>
    /// Which folder is being listed, read from the picker itself by picking it. The view keeps the folder
    /// to itself, so asking for it means asking what would be picked.
    /// </summary>
    /// <param name="app">The application under test.</param>
    /// <returns>The folder the picker is looking at.</returns>
    private string Folder(TestApplication app)
    {
        app.Press(ConsoleKey.Enter, KeyModifiers.Control);

        return Picked;
    }

    private TestApplication Show(string? startAt = null)
    {
        var app = new TestApplication(100, 30);

        app.State.FilePicker = new(
            "Pick",
            PickFolder: true,
            startAt ?? _root.FullName,
            ViewRoute.None,
            picked => Picked = picked);
        app.Navigator.Apply(Routes.FilePicker);

        return app;
    }
}
