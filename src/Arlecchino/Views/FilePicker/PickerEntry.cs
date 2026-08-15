using System;

namespace Arlecchino.Views.FilePicker;

/// <summary>One row of the folder being browsed: a file, a folder below it, or a drive.</summary>
/// <param name="Name">What it is called, as the row shows it.</param>
/// <param name="FullPath">Where it is, which is what picking it hands back.</param>
/// <param name="IsDirectory">Whether opening it browses into it rather than picking it.</param>
/// <param name="Modified">When it was last written to, or the default when that could not be read.</param>
/// <param name="Length">How many bytes it holds, or <c>-1</c> when that could not be read.</param>
/// <param name="IsVolume">Whether it is a drive rather than a folder inside one.</param>
internal sealed record PickerEntry(
    string Name,
    string FullPath,
    bool IsDirectory,
    DateTime Modified,
    long Length,
    bool IsVolume);
