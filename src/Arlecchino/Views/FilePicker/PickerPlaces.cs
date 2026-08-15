using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.State;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// The shortcuts down the left of the picker: what the application offered, the folders every system has,
/// and the drives. The headings between them are rows the cursor steps over.
/// </summary>
internal sealed class PickerPlaces
{
    private readonly ArlecchinoStrings.FilePickerStrings _strings;
    private readonly List<Place> _rows;

    private int _firstVisible;

    /// <summary>Builds the shortcuts.</summary>
    /// <param name="request">What was asked for, whose own places come first.</param>
    /// <param name="strings">The words the picker says things in.</param>
    public PickerPlaces(FilePickerRequest request, ArlecchinoStrings.FilePickerStrings strings)
    {
        _strings = strings;
        _rows = Built(request);
        Selected = _rows.FindIndex(static row => row.Path is not null);
    }

    /// <summary>Which shortcut the cursor is on.</summary>
    public int Selected { get; private set; }

    /// <summary>Where the shortcut under the cursor leads, or <c>null</c> when it is a heading.</summary>
    public string? Current => At(Selected);

    /// <summary>Where the shortcut on a row leads, or <c>null</c> when there is none.</summary>
    /// <param name="row">Row on screen, counted from the top of the pane.</param>
    /// <returns>The folder, or <c>null</c>.</returns>
    public string? ClickedAt(int row)
    {
        var index = _firstVisible + row;

        if (index < 0 || index >= _rows.Count || _rows[index].Path is not { } path)
        {
            return null;
        }

        Selected = index;

        return path;
    }

    /// <summary>Steps the cursor to the next shortcut, over any heading in between.</summary>
    /// <param name="delta">Which way to step.</param>
    public void Move(int delta)
    {
        var next = Selected;

        foreach (var _ in _rows)
        {
            next += delta;

            if (next < 0 || next >= _rows.Count)
            {
                return;
            }

            if (_rows[next].Path is null)
            {
                continue;
            }

            Selected = next;

            return;
        }
    }

    /// <summary>Puts the cursor on the shortcut that leads where the picker is now, when there is one.</summary>
    /// <param name="path">The folder being listed.</param>
    public void SyncTo(string path)
    {
        for (var index = 0; index < _rows.Count; index++)
        {
            if (_rows[index].Path is not { } place || !string.Equals(place, path, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Selected = index;

            return;
        }
    }

    /// <summary>Draws the shortcuts, scrolled to keep the one under the cursor in view.</summary>
    /// <param name="sidebar">The pane to draw in.</param>
    /// <param name="focused">Whether the pane has the keyboard.</param>
    public void Draw(SurfaceRegion sidebar, bool focused)
    {
        var start = Math.Clamp(Selected - sidebar.Height / 2, 0, Math.Max(0, _rows.Count - sidebar.Height));

        _firstVisible = start;

        for (var offset = 0; offset < sidebar.Height && start + offset < _rows.Count; offset++)
        {
            var row = _rows[start + offset];

            if (row.Path is null)
            {
                sidebar.Write(offset, 0, PickerText.Pad(row.Label, sidebar.Width), Theme.Muted);

                continue;
            }

            var style = start + offset == Selected
                ? focused ? Theme.ActiveSelected : Theme.Selected
                : Theme.Default;

            sidebar.Write(offset, 0, PickerText.Pad($" {row.Icon} {row.Label}", sidebar.Width), style);
        }
    }

    private string? At(int index) => index >= 0 && index < _rows.Count ? _rows[index].Path : null;

    private List<Place> Built(FilePickerRequest request)
    {
        var rows = new List<Place> { new(_strings.Favorites(), null, "") };

        foreach (var place in request.Places)
        {
            rows.Add(new(place.Name, place.Path, place.Icon.Length > 0 ? place.Icon : "▪"));
        }

        AddSpecial(rows, Environment.SpecialFolder.Desktop, "▪");
        AddSpecial(rows, Environment.SpecialFolder.MyDocuments, "▪");
        AddSpecial(rows, Environment.SpecialFolder.UserProfile, "▪", "Downloads");
        AddSpecial(rows, Environment.SpecialFolder.MyPictures, "▪");
        AddSpecial(rows, Environment.SpecialFolder.MyMusic, "▪");
        AddSpecial(rows, Environment.SpecialFolder.MyVideos, "▪");

        rows.Add(new(_strings.Locations(), null, ""));

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (Directory.Exists(home))
        {
            rows.Add(new(Environment.UserName, home, "⌂"));
        }

        rows.Add(new(_strings.Drives(), "", "▣"));

        foreach (var drive in DriveInfo.GetDrives().Where(static drive => drive.IsReady))
        {
            rows.Add(new(drive.Name.TrimEnd(Path.DirectorySeparatorChar), drive.Name, "▤"));
        }

        return rows;
    }

    private static void AddSpecial(
        List<Place> rows,
        Environment.SpecialFolder folder,
        string icon,
        string? childName = null)
    {
        var path = Environment.GetFolderPath(folder);

        if (path.Length == 0)
        {
            return;
        }

        if (childName is not null)
        {
            path = Path.Combine(path, childName);
        }

        if (!Directory.Exists(path))
        {
            return;
        }

        var name = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar));

        if (name.Length == 0 || rows.Any(row => string.Equals(row.Path, path, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        rows.Add(new(name, path, icon));
    }

    private sealed record Place(string Label, string? Path, string Icon);
}
