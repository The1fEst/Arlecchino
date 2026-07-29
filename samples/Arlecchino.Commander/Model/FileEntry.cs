using System;

namespace Arlecchino.Commander.Model;

public sealed record FileEntry(
    string Name,
    string Path,
    bool IsFolder,
    bool IsParent,
    long Size,
    DateTime Modified,
    bool IsHidden,
    bool IsReadOnly)
{
    public int Rank => IsParent ? 0 : IsFolder ? 1 : 2;
}
