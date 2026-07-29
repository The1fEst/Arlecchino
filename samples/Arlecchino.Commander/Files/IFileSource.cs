using System;
using System.Collections.Generic;
using System.IO;
using Arlecchino.Commander.Model;

namespace Arlecchino.Commander.Files;

public interface IFileSource : IDisposable
{
    string Label { get; }

    bool IsRemote { get; }

    /// <summary>
    /// How many requests may be in flight at once. A local disk answers immediately and wants one; a
    /// server spends the whole time waiting for the network, where asking for the next thing before
    /// the last one has answered is the difference between a minute and a few seconds.
    /// </summary>
    int Concurrency { get; }

    /// <summary>
    /// Removes a folder and everything under it in one go, when the source can do that itself. A
    /// server reached over SSH can, and one command beats one round trip per file by a long way.
    /// </summary>
    /// <param name="entry">The folder to remove.</param>
    /// <returns><c>false</c> when the source has no such shortcut and the tree must be walked.</returns>
    bool TryDeleteTree(FileEntry entry);

    string Home { get; }

    string Combine(string folder, string name);

    string? Parent(string folder);

    string NameOf(string path);

    bool FolderExists(string folder);

    string Free(string folder);

    IReadOnlyList<FileEntry> List(string folder, bool showHidden);

    Stream OpenRead(string path);

    Stream Create(string path);

    void CreateFolder(string path);

    void Delete(FileEntry entry);

    void Move(string from, string to);
}
