using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Arlecchino.Views.FilePicker;

/// <summary>
/// The drives the picker offers. Unix mounts a dozen file systems the kernel keeps for itself, and the
/// system's own folders on partitions of their own, beside the volumes anyone means to browse.
/// </summary>
internal static class PickerDrives
{
    private static readonly HashSet<string> KernelFormats = new(StringComparer.Ordinal)
    {
        "proc", "sysfs", "devtmpfs", "tmpfs", "devpts", "cgroup", "cgroup2", "securityfs", "debugfs", "tracefs",
        "pstore", "bpf", "configfs", "fusectl", "hugetlbfs", "mqueue", "autofs", "binfmt_misc", "efivarfs", "ramfs",
        "nsfs", "squashfs", "overlay",
    };

    private static readonly string[] SystemFolders =
    [
        "/proc", "/sys", "/dev", "/run", "/boot", "/efi", "/tmp", "/var", "/usr", "/opt", "/srv", "/root", "/swap",
        "/.snapshots", "/System/Volumes",
    ];

    /// <summary>The drives that are mounted and worth offering.</summary>
    /// <returns>The drives, in the order the system lists them.</returns>
    public static IEnumerable<DriveInfo> GetDrives() => DriveInfo.GetDrives().Where(IsOffered);

    private static bool IsOffered(DriveInfo drive)
    {
        try
        {
            return drive.IsReady && IsBrowsable(drive.Name, drive.DriveFormat);
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Whether a mounted drive is one anyone means to browse.</summary>
    /// <param name="root">Where it is mounted.</param>
    /// <param name="format">The file system it holds, as the system names it.</param>
    /// <returns><c>false</c> for the kernel's own file systems and the system's own folders.</returns>
    public static bool IsBrowsable(string root, string format)
    {
        if (KernelFormats.Contains(format))
        {
            return false;
        }

        var path = GetName(root);

        if (path.StartsWith("/run/media/", StringComparison.Ordinal))
        {
            return true;
        }

        if (path == "/home")
        {
            return false;
        }

        return !SystemFolders.Any(folder =>
            path == folder || path.StartsWith(folder + "/", StringComparison.Ordinal));
    }

    /// <summary>What a drive is called in a list: where it is mounted, without the trailing separator.</summary>
    /// <param name="root">Where it is mounted.</param>
    /// <returns>The name, which is the separator itself for the root of a Unix machine.</returns>
    public static string GetName(string root) =>
        root.Length > 1 ? root.TrimEnd(Path.DirectorySeparatorChar) : root;
}
