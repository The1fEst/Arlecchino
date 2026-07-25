using System;
using System.Collections.Generic;
using System.Text;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Navigation;
using Arlecchino.Rendering;

namespace Arlecchino.Widgets;

/// <summary>One column of a table: its heading, what it shows and how it behaves.</summary>
/// <typeparam name="T">The type of row the column reads from.</typeparam>
public sealed class TableColumn<T>
{
    /// <summary>The heading, as a delegate so it can be localised.</summary>
    public required Func<string> Header { get; init; }

    /// <summary>Reads the cell for one row.</summary>
    public required Func<T, string> Cell { get; init; }

    /// <summary>Fixed width in columns. Leave at zero to share out whatever the fixed columns leave over.</summary>
    public int Width { get; init; }

    /// <summary>Whether the cell hugs the right edge, which is what numbers want.</summary>
    public bool AlignRight { get; init; }

    /// <summary>
    /// How to order rows by this column. Without it the column cannot be sorted at all, which is how a
    /// column of free-form text opts out.
    /// </summary>
    public Comparison<T>? Sort { get; init; }
}

/// <summary>
/// Rows in aligned columns, with a heading and optional sorting. Selection and scrolling are a list
/// box underneath, so a table behaves exactly like a list that happens to draw more per row. Sorting
/// reorders a copy, leaving whatever was assigned to <see cref="Rows"/> untouched.
/// </summary>
/// <typeparam name="T">What each row holds.</typeparam>
public sealed class Table<T> : IFocusable
{
    private const int ColumnGap = 1;

    private readonly ListBox<T> _rows;
    private readonly List<T> _sorted = [];

    private IReadOnlyList<T> _source = [];
    private int[] _widths = [];

    /// <summary>Creates the table.</summary>
    /// <param name="keymap">Keys to obey, so the table follows the application's bindings.</param>
    public Table(ArlecchinoKeymap keymap)
    {
        _rows = new(keymap)
        {
            Render = row => RenderRow(row),
            OnActivate = row => OnActivate?.Invoke(row) ?? ViewRoute.None,
        };
    }

    /// <summary>The columns, left to right.</summary>
    public required IReadOnlyList<TableColumn<T>> Columns { get; init; }

    /// <summary>What confirming a row does. Returning a route navigates.</summary>
    public Func<T, ViewRoute>? OnActivate { get; init; }

    /// <summary>Colours a whole row. Ignored for the selected one.</summary>
    public Func<T, ITermColor>? Style
    {
        get => _rows.Style;
        init => _rows.Style = value;
    }

    /// <summary>What to show. Assigning re-applies the current sort.</summary>
    public IReadOnlyList<T> Rows
    {
        get => _source;
        set
        {
            _source = value;
            Resort();
        }
    }

    /// <summary>Index of the column being sorted by, or <c>-1</c> when the rows are in their original order.</summary>
    public int SortedBy { get; private set; } = -1;

    /// <summary>Which way the sort runs.</summary>
    public bool SortsDescending { get; private set; }

    /// <summary>Index of the selected row within the sorted order, not within what was assigned.</summary>
    public int Selected
    {
        get => _rows.Selected;
        set => _rows.Selected = value;
    }

    /// <summary>The selected row, or the type's default when the table is empty.</summary>
    public T? SelectedRow => _rows.SelectedItem;

    /// <summary>Whether the table has focus, which decides how strongly the selection is drawn.</summary>
    public bool IsFocused
    {
        get => _rows.IsFocused;
        set => _rows.IsFocused = value;
    }

    /// <summary>
    /// Sorts by a column, or flips the direction when it is already the one being sorted by. Columns
    /// without a comparison, and indexes outside the table, are ignored.
    /// </summary>
    /// <param name="column">Index of the column to sort by.</param>
    public void SortBy(int column)
    {
        if (column < 0 || column >= Columns.Count || Columns[column].Sort is null)
        {
            return;
        }

        SortsDescending = SortedBy == column && !SortsDescending;
        SortedBy = column;
        Resort();
    }

    /// <summary>
    /// Works out the column widths for the space available, then draws the heading on the first row and
    /// the rows below it.
    /// </summary>
    /// <param name="region">Where to draw, heading included.</param>
    public void Draw(Region region)
    {
        if (region.IsEmpty)
        {
            return;
        }

        _widths = MeasureColumns(region.Width);
        region.Write(0, 0, HeaderLine(), Theme.TableHeader);
        _rows.Draw(region.Rows(1, region.Height - 1));
    }

    /// <summary>Moves the selection or confirms it. Sorting is not bound to a key; call <see cref="SortBy"/>.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>What became of the key.</returns>
    public FocusResult Handle(ConsoleKeyInfo key) => _rows.Handle(key);

    /// <summary>Scrolls and selects. Clicks on the heading are not routed here.</summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>What became of the event.</returns>
    public FocusResult HandleMouse(MouseEvent mouse) => _rows.HandleMouse(mouse);

    private void Resort()
    {
        _sorted.Clear();
        _sorted.AddRange(_source);

        if (SortedBy >= 0 && Columns[SortedBy].Sort is { } comparison)
        {
            _sorted.Sort(SortsDescending ? (first, second) => comparison(second, first) : comparison);
        }

        _rows.Items = _sorted;
    }

    private string HeaderLine()
    {
        var line = new StringBuilder();

        for (var i = 0; i < Columns.Count && i < _widths.Length; i++)
        {
            var marker = i == SortedBy ? SortsDescending ? " ↓" : " ↑" : "";
            line.Append(Fit(Columns[i].Header() + marker, _widths[i], Columns[i].AlignRight));

            if (i < Columns.Count - 1)
            {
                line.Append(' ', ColumnGap);
            }
        }

        return line.ToString();
    }

    private string RenderRow(T row)
    {
        var line = new StringBuilder();

        for (var i = 0; i < Columns.Count && i < _widths.Length; i++)
        {
            line.Append(Fit(Columns[i].Cell(row), _widths[i], Columns[i].AlignRight));

            if (i < Columns.Count - 1)
            {
                line.Append(' ', ColumnGap);
            }
        }

        return line.ToString();
    }

    private int[] MeasureColumns(int available)
    {
        var widths = new int[Columns.Count];
        var gaps = Math.Max(0, Columns.Count - 1) * ColumnGap;
        var fixedTotal = 0;
        var flexible = 0;

        for (var i = 0; i < Columns.Count; i++)
        {
            widths[i] = Columns[i].Width;

            if (widths[i] > 0)
            {
                fixedTotal += widths[i];
            }
            else
            {
                flexible++;
            }
        }

        if (flexible == 0)
        {
            return widths;
        }

        var left = Math.Max(0, available - gaps - fixedTotal);
        var share = left / flexible;

        for (var i = 0; i < widths.Length; i++)
        {
            if (widths[i] > 0)
            {
                continue;
            }

            flexible--;
            widths[i] = flexible == 0 ? left : share;
            left -= widths[i];
        }

        return widths;
    }

    private static string Fit(string text, int width, bool alignRight)
    {
        var clipped = TextWidth.Truncate(text, width);
        return alignRight ? TextWidth.PadLeft(clipped, width) : TextWidth.PadRight(clipped, width);
    }
}
