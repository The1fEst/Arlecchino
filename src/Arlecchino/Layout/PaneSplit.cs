namespace Arlecchino.Layout;

/// <summary>Which way a branch cuts the space it was given.</summary>
public enum PaneSplit : byte
{
    /// <summary>Top from bottom; the first half is the upper one.</summary>
    Rows,

    /// <summary>Left from right; the first half is the left one.</summary>
    Columns,
}
