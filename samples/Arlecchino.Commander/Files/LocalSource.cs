using System.Collections.Generic;
using System.IO;
using Arlecchino.Commander.Model;

namespace Arlecchino.Commander.Files;

public sealed class LocalSource : IFileSource
{
    public string Label => "local";

    public bool IsRemote => false;

    public int Concurrency => 1;

    public bool TryDeleteTree(FileEntry entry) => false;

    public string Home => Listing.Home();

    public string Combine(string folder, string name) => Path.Combine(folder, name);

    public string? Parent(string folder) => Listing.Parent(folder);

    public string NameOf(string path) => Path.GetFileName(path);

    public bool FolderExists(string folder) => Directory.Exists(folder);

    public string Free(string folder) => Listing.Free(folder);

    public IReadOnlyList<FileEntry> List(string folder, bool showHidden) => Listing.Read(folder, showHidden);

    public Stream OpenRead(string path) => File.OpenRead(path);

    public Stream Create(string path) => File.Create(path);

    public void CreateFolder(string path) => Directory.CreateDirectory(path);

    public void Delete(FileEntry entry)
    {
        if (entry.IsFolder)
        {
            Directory.Delete(entry.Path, true);
            return;
        }

        File.Delete(entry.Path);
    }

    public void Move(string from, string to)
    {
        if (Directory.Exists(from))
        {
            Directory.Move(from, to);
            return;
        }

        File.Move(from, to, true);
    }

    public void Dispose()
    {
    }
}
