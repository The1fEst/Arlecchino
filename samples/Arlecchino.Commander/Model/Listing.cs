using System;
using System.Collections.Generic;
using System.IO;

namespace Arlecchino.Commander.Model;

public static class Listing
{
    public static IReadOnlyList<FileEntry> Read(string path, bool showHidden)
    {
        var entries = new List<FileEntry>();

        if (Parent(path) is { } parent)
        {
            entries.Add(new("..", parent, true, true, 0, default, false, false));
        }

        foreach (var found in new DirectoryInfo(path).EnumerateFileSystemInfos())
        {
            var hidden = found.Attributes.HasFlag(FileAttributes.Hidden) ||
                         found.Attributes.HasFlag(FileAttributes.System);

            if (hidden && !showHidden)
            {
                continue;
            }

            var isFolder = found.Attributes.HasFlag(FileAttributes.Directory);

            entries.Add(new(
                found.Name,
                found.FullName,
                isFolder,
                false,
                found is FileInfo file ? file.Length : 0,
                Written(found),
                hidden,
                found.Attributes.HasFlag(FileAttributes.ReadOnly)));
        }

        return entries;
    }

    public static int Compare(FileEntry first, FileEntry second, Sorting sorting, bool descending)
    {
        if (first.Rank != second.Rank)
        {
            return first.Rank - second.Rank;
        }

        var order = sorting switch
        {
            Sorting.Size => first.Size.CompareTo(second.Size),
            Sorting.Modified => first.Modified.CompareTo(second.Modified),
            _ => string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase),
        };

        if (order == 0)
        {
            order = string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);
        }

        return descending ? -order : order;
    }

    public static string? Parent(string path)
    {
        try
        {
            return new DirectoryInfo(path).Parent?.FullName;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static string Home()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        return home.Length > 0 && Directory.Exists(home) ? home : Directory.GetCurrentDirectory();
    }

    public static IReadOnlyList<string> Drives()
    {
        var roots = new List<string>();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.IsReady)
            {
                roots.Add(drive.RootDirectory.FullName);
            }
        }

        return roots;
    }

    public static string Free(string path)
    {
        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(path));

            return root is null ? "" : $"{Sizes.Short(new DriveInfo(root).AvailableFreeSpace)} free";
        }
        catch (Exception error) when (error is ArgumentException or IOException or UnauthorizedAccessException)
        {
            return "";
        }
    }

    private static DateTime Written(FileSystemInfo found)
    {
        try
        {
            return found.LastWriteTime;
        }
        catch (IOException)
        {
            return default;
        }
    }
}
