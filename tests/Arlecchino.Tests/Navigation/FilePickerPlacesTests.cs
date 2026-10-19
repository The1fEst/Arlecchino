using System;
using System.IO;
using Arlecchino.Navigation;
using Arlecchino.State;
using Arlecchino.Views.FilePicker;
using Xunit;
using Arlecchino.Tests.Support;

namespace Arlecchino.Tests.Navigation;

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

    [Theory]
    [InlineData("/", "btrfs", true)]
    [InlineData("/home/user/Data", "btrfs", true)]
    [InlineData("/home/user/Windows", "fuseblk", true)]
    [InlineData("/run/media/user/USB", "vfat", true)]
    [InlineData("/mnt/backup", "ext4", true)]
    [InlineData("C:\\", "NTFS", true)]
    [InlineData("/proc", "proc", false)]
    [InlineData("/sys/fs/cgroup", "cgroup2", false)]
    [InlineData("/dev/shm", "tmpfs", false)]
    [InlineData("/proc/sys/fs/binfmt_misc", "autofs", false)]
    [InlineData("/run/user/1000/doc", "fuse.portal", false)]
    [InlineData("/home", "btrfs", false)]
    [InlineData("/var/log", "btrfs", false)]
    [InlineData("/var/cache/pacman/pkg", "btrfs", false)]
    [InlineData("/boot", "vfat", false)]
    [InlineData("/.snapshots", "btrfs", false)]
    [InlineData("/usr/local", "btrfs", false)]
    [InlineData("/System/Volumes/Data", "apfs", false)]
    [InlineData("/snap/core/1", "squashfs", false)]
    public void OnlyDrivesAnyoneMeansToBrowseAreOffered(string root, string format, bool isBrowsable)
    {
        Assert.Equal(isBrowsable, PickerDrives.IsBrowsable(root, format));
    }

    [Theory]
    [InlineData("/", "/")]
    [InlineData("/mnt/backup/", "/mnt/backup")]
    public void ADriveIsNamedWhereItIsMounted(string root, string name)
    {
        Assert.Equal(name, PickerDrives.GetName(root));
    }

    private TestApplication Show(FilePickerPlace[] places, string? startAt = null)
    {
        var app = new TestApplication(100, 30);

        app.State.FilePicker = new("Pick",
            PickFolder: true,
            startAt ?? _root.FullName,
            ViewRoute.None,
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
