namespace Arlecchino.Atoms.History.Hosting;

/// <summary>Everything the history can be told, set in the callback <c>UseHistory</c> takes.</summary>
public sealed class HistoryOptions
{
    /// <summary>What the history file is called. It sits beside the program unless the name has a folder in it.</summary>
    public string FileName { get; set; } = HistoryFile.DefaultName;

    /// <summary>
    /// Whether the file outlives the run. The default throws it away on the way out and again on the way
    /// in, so it is only ever a scratch file of the run that is up.
    /// </summary>
    public bool Keep { get; set; }

    /// <summary>How many steps to keep before the oldest fall off the far end.</summary>
    public int Capacity { get; set; } = 200;
}
