namespace Arlecchino.Atoms.History.Hosting;

/// <summary>Everything the history can be told, set in the callback <c>UseHistory</c> takes.</summary>
public sealed class HistoryOptions
{
    /// <summary>What the history file is called. It sits beside the program unless the name has a folder in it.</summary>
    public string FileName { get; set; } = HistoryFile.DefaultName;

    /// <summary>
    /// Whether a named atom starts out holding what the file last saw it hold, rather than the value its
    /// store gives it. It is also what keeps the file, there being no other reason to have one outlive
    /// the run.
    /// </summary>
    public bool Resume { get; set; }

    /// <summary>How many steps to keep before the oldest fall off the far end.</summary>
    public int Capacity { get; set; } = 200;
}
