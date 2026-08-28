namespace Arlecchino.Atoms.History.Hosting;

/// <summary>Everything the history can be told, set in the callback <c>UseHistory</c> takes.</summary>
public sealed class HistoryOptions
{
    /// <summary>
    /// What the history file is called, or <c>null</c> for a history kept in memory. A file sits beside
    /// the program, outlives the run, and is what the next one comes up in.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>How many steps to keep before the oldest fall off the far end.</summary>
    public int Capacity { get; set; } = 200;
}
