using System;

namespace Arlecchino.Layout;

/// <summary>
/// How much of a region a split gives to its first half: a share of what there is, a fixed number of
/// cells, or — for the toolbars and status bars that sit at the far edge — a fixed number of cells
/// measured from the other end.
///
/// A <c>double</c> and an <c>int</c> both convert on their own, so <c>0.25</c> reads as a quarter and
/// <c>3</c> reads as three cells at the call site.
/// </summary>
public readonly record struct PaneSize
{
    private readonly double _value;
    private readonly Kind _measure;

    private PaneSize(double value, Kind measure)
    {
        _value = value;
        _measure = measure;
    }

    private enum Kind : byte
    {
        Fraction,
        Cells,
        CellsFromEnd,
    }

    /// <summary>A share of the space, between nothing and all of it.</summary>
    /// <param name="value">The share, clamped to <c>0..1</c>.</param>
    /// <returns>The size.</returns>
    public static PaneSize Fraction(double value) => new(Math.Clamp(value, 0, 1), Kind.Fraction);

    /// <summary>A fixed number of cells, however big the region is.</summary>
    /// <param name="count">Columns or rows, whichever way the split runs.</param>
    /// <returns>The size.</returns>
    public static PaneSize Cells(int count) => new(Math.Max(0, count), Kind.Cells);

    /// <summary>
    /// Everything except a fixed number of cells, which is how a one-row status bar at the bottom is
    /// written: the first half takes the rest, the second half takes what was reserved.
    /// </summary>
    /// <param name="count">Columns or rows to leave for the second half.</param>
    /// <returns>The size.</returns>
    public static PaneSize CellsFromEnd(int count) => new(Math.Max(0, count), Kind.CellsFromEnd);

    /// <summary>Reads a share as a size, so <c>0.25</c> is a quarter.</summary>
    /// <param name="value">The share.</param>
    public static implicit operator PaneSize(double value) => Fraction(value);

    /// <summary>Reads a count as a size, so <c>3</c> is three cells.</summary>
    /// <param name="count">The count.</param>
    public static implicit operator PaneSize(int count) => Cells(count);

    internal int Of(int available) => _measure switch
    {
        Kind.Cells => Math.Min((int)_value, available),
        Kind.CellsFromEnd => Math.Max(0, available - (int)_value),
        _ => (int)Math.Round(available * _value),
    };
}
