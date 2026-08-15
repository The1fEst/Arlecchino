using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arlecchino.Editing;
using Arlecchino.State;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// The folder the picker is looking at and what is in it, narrowed by what has been typed. A folder that
/// cannot be read leaves the rows empty and the reason in <see cref="Error"/>.
/// </summary>
internal sealed class PickerListing
{
    private readonly FilePickerRequest _request;

    private List<PickerEntry> _entries = [];

    /// <summary>Opens the picker on a folder.</summary>
    /// <param name="request">What was asked for, whose filter decides which files are listed.</param>
    /// <param name="start">Where browsing starts.</param>
    public PickerListing(FilePickerRequest request, string start)
    {
        _request = request;
        Folder = StartingFolder(start);

        Load();
    }

    /// <summary>The folder being listed, or an empty string for the drives.</summary>
    public string Folder { get; private set; }

    /// <summary>Why the folder could not be read, or an empty string when it could.</summary>
    public string Error { get; private set; } = "";

    /// <summary>Whatever has been typed to narrow the rows.</summary>
    public TextEntry Filter { get; } = new();

    /// <summary>The rows that pass what has been typed, in the order they are listed.</summary>
    /// <returns>The rows to draw.</returns>
    public List<PickerEntry> Matching() =>
        Filter.Text.Length == 0
            ? _entries
            : [.. _entries.Where(entry => entry.Name.Contains(Filter.Text, StringComparison.OrdinalIgnoreCase))];

    /// <summary>Lists another folder, forgetting what was typed to narrow the last one.</summary>
    /// <param name="path">The folder, or an empty string for the drives.</param>
    public void GoTo(string path)
    {
        Folder = path;
        Filter.Text = "";

        Load();
    }

    /// <summary>Lists the folder above this one, where there is one.</summary>
    /// <returns><c>true</c> when it moved.</returns>
    public bool Up()
    {
        if (Folder.Length == 0)
        {
            return false;
        }

        GoTo(Path.GetDirectoryName(Folder) ?? "");

        return true;
    }

    /// <summary>Which row a file is on, for putting the cursor on the one that was asked for.</summary>
    /// <param name="fullPath">The file.</param>
    /// <returns>The row, or <c>-1</c> when it is not listed here.</returns>
    public int IndexOf(string fullPath) =>
        _entries.FindIndex(entry => string.Equals(entry.FullPath, fullPath, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Where browsing starts: the folder asked for, or the one holding the file asked for. Anything that is
    /// no longer there falls back to the drives.
    /// </summary>
    /// <param name="requested">What the request asked for.</param>
    /// <returns>The folder to list, or an empty string for the drives.</returns>
    public static string StartingFolder(string requested)
    {
        if (Directory.Exists(requested))
        {
            return requested;
        }

        return File.Exists(requested) ? Path.GetDirectoryName(requested) ?? "" : "";
    }

    private void Load()
    {
        Error = "";

        try
        {
            _entries = Folder.Length == 0 ? Drives() : Listing(Folder);
        }
        catch (Exception failure)
        {
            _entries = [];
            Error = failure.Message;
        }
    }

    private List<PickerEntry> Listing(string path)
    {
        var directories = new DirectoryInfo(path).EnumerateDirectories()
            .Select(static folder => new PickerEntry(folder.Name, folder.FullName, true, SafeTime(folder), 0, false))
            .OrderBy(static entry => entry.Name, StringComparer.OrdinalIgnoreCase);

        var files = new DirectoryInfo(path).EnumerateFiles()
            .Where(file => _request.FileFilter is null || _request.FileFilter(file.FullName))
            .Select(static file =>
                new PickerEntry(file.Name, file.FullName, false, SafeTime(file), SafeLength(file), false))
            .OrderBy(static entry => entry.Name, StringComparer.OrdinalIgnoreCase);

        return [.. directories, .. files];
    }

    private static List<PickerEntry> Drives() =>
    [
        .. DriveInfo.GetDrives()
            .Where(static drive => drive.IsReady)
            .Select(static drive => new PickerEntry(
                drive.VolumeLabel.Length > 0
                    ? $"{drive.VolumeLabel} ({drive.Name.TrimEnd(Path.DirectorySeparatorChar)})"
                    : drive.Name,
                drive.Name,
                IsDirectory: true,
                default,
                0,
                IsVolume: true)),
    ];

    private static DateTime SafeTime(FileSystemInfo info)
    {
        try
        {
            return info.LastWriteTime;
        }
        catch (Exception)
        {
            return default;
        }
    }

    private static long SafeLength(FileInfo info)
    {
        try
        {
            return info.Length;
        }
        catch (Exception)
        {
            return -1;
        }
    }
}
