using System;
using System.Collections.Generic;
using System.Text;

namespace Arlecchino.Rendering;

/// <summary>
/// The drawing target: a grid of cells, each holding one symbol and one style, serialized into a
/// single write per frame. Needs nothing but an <see cref="IArlecchinoTerminal"/>, so it works outside a
/// hosted application too.
/// </summary>
public partial class Surface
{
    private const string BlankCell = " ";
    private const string WideCellTail = "";
    private const int CursorMoveCost = 6;

    private static readonly string[] AsciiCells = BuildAsciiCells();

    private readonly IArlecchinoTerminal _terminal;
    private readonly StringBuilder _stringBuilder = new();

    private string[][] _cells = [];
    private IArlecchinoColor[][] _styles = [];
    private string[][] _previousCells = [];
    private IArlecchinoColor[][] _previousStyles = [];
    private readonly List<(int Row, int Column, string Payload, string Undraw)> _passthrough = [];
    private (int Row, int Column, string Payload, string Undraw)[] _previousPassthrough = [];
    private int _width;
    private int _height;
    private int _lines;
    private int _fixedWidth;
    private int _fixedHeight;
    private SurfaceRegion? _clip;

    /// <summary>Creates a surface that draws to a terminal.</summary>
    /// <param name="terminal">Where composed frames are written.</param>
    public Surface(IArlecchinoTerminal terminal)
    {
        _terminal = terminal;
    }

    /// <summary>Cells kept free on the left and right by the flow calls.</summary>
    public int HorizontalPadding { get; set; } = 2;

    /// <summary>Rows kept free above and below by the flow calls.</summary>
    public int VerticalPadding { get; set; } = 1;

    /// <summary>Width of the current frame in cells.</summary>
    public int FrameWidth => _width;

    /// <summary>Height of the current frame in rows.</summary>
    public int FrameHeight => _height;

    /// <summary>The whole frame as a region.</summary>
    public SurfaceRegion Frame => new(this, 0, 0, _width, _height);

    /// <summary>The frame minus the configured padding — where a view normally draws.</summary>
    public SurfaceRegion Content => Frame.Inset(new Margin(HorizontalPadding, VerticalPadding, HorizontalPadding, VerticalPadding));

    private int FreeLines => Math.Max(0, _height - _lines);

    /// <summary>
    /// What the frame just built put in a cell. The testing package reads it to hold the terminal's
    /// screen against the frame that was composed: a diff that dropped a cell and a cell that never
    /// had anything in it look the same on screen, and only the frame itself tells them apart.
    /// </summary>
    /// <param name="row">Row in frame coordinates.</param>
    /// <param name="column">Column in frame coordinates.</param>
    /// <returns>The symbol, empty for the second half of a wide one, and the style it carries.</returns>
    internal (string Cell, IArlecchinoColor Style) Composed(int row, int column) =>
        (_cells[row][column], _styles[row][column]);

    /// <summary>
    /// How many rows a scrolling list may use: what is left of the frame minus room for the chrome,
    /// never fewer than four.
    /// </summary>
    /// <returns>Rows available for list content.</returns>
    public int ListWindow()
    {
        return Math.Max(4, FreeLines - 4);
    }

    /// <summary>
    /// Confines every write to a rectangle until the returned scope is disposed, whatever coordinates
    /// the writing code uses. This is what makes a scrolling pane possible: the content is drawn at an
    /// offset that reaches outside the pane, and the parts that fall outside are dropped instead of
    /// landing on a neighbour.
    ///
    /// Scopes nest, and the innermost one wins — a clip inside a clip is their intersection.
    /// </summary>
    /// <param name="region">The only part of the frame writes may reach.</param>
    /// <returns>Dispose it to go back to the clip that was in force before.</returns>
    public IDisposable Clip(SurfaceRegion region)
    {
        var previous = _clip;

        _clip = previous is { } outer
            ? Intersect(outer, region)
            : region;

        return new ClipScope(this, previous);
    }

    /// <summary>
    /// Drops the memory of the last frame, so the next <see cref="Build"/> sends the whole screen
    /// instead of the difference. Use it after something else wrote to the terminal.
    /// </summary>
    public void ForgetPreviousFrame()
    {
        _previousCells = [];
        _previousStyles = [];
    }

    /// <summary>
    /// Pins the frame size instead of asking the terminal, which is what makes headless rendering
    /// possible. A fixed-size surface always sends whole frames.
    /// </summary>
    /// <param name="width">Width in cells.</param>
    /// <param name="height">Height in rows.</param>
    public void SetFixedSize(int width, int height)
    {
        _fixedWidth = width;
        _fixedHeight = height;
    }

    /// <summary>
    /// Begins a frame: reads the terminal size, reallocates if it changed, clears every cell and
    /// skips the vertical padding. Nothing reaches the terminal until <see cref="Build"/>.
    /// </summary>
    public void StartFrame()
    {
        _height = _fixedHeight > 0 ? _fixedHeight : Math.Max(1, _terminal.Height);
        _width = _fixedWidth > 0 ? _fixedWidth : Math.Max(1, _terminal.Width);
        _lines = 0;

        _passthrough.Clear();

        if (_cells.Length != _height || (_cells.Length > 0 && _cells[0].Length != _width))
        {
            _cells = new string[_height][];
            _styles = new IArlecchinoColor[_height][];
            for (var row = 0; row < _height; row++)
            {
                _cells[row] = new string[_width];
                _styles[row] = new IArlecchinoColor[_width];
            }
        }

        for (var row = 0; row < _height; row++)
        {
            Array.Fill(_cells[row], BlankCell);
            Array.Fill(_styles[row], Theme.Default);
        }

        for (var i = 0; i < VerticalPadding; i++)
        {
            SkipLine();
        }
    }

    /// <summary>
    /// Sends the composed frame to the terminal, writing only what changed since the last one — an
    /// idle frame writes nothing at all. The first frame, a resize and a fixed size send everything.
    /// </summary>
    public void Build()
    {
        _stringBuilder.Clear();
        _stringBuilder.EnsureCapacity(_height * (_width + 64));

        var undrawn = AppendUndraws();
        var whole = undrawn || !CanDrawChangesOnly();

        if (whole)
        {
            AppendWholeFrame();
        }
        else
        {
            AppendChangedRuns();
        }

        AppendPassthrough(whole);
        RememberFrame();

        if (_stringBuilder.Length > 0)
        {
            _terminal.Write(_stringBuilder.ToString());
        }
    }

    /// <summary>
    /// Hands the terminal something the cell grid cannot express — an image in one of the graphics
    /// protocols, most of all — to be written verbatim at a cell, after everything the frame drew.
    ///
    /// It goes out last on purpose: the cells are written first, so whatever was under or around the
    /// payload last time is repainted before it lands.
    ///
    /// Repainting the cells is not enough to remove it, though, which is what <paramref name="undraw"/>
    /// is for. A payload that was handed over last frame and is not handed over this one — the widget
    /// moved, or shrank, or is not on screen at all any more — has its undraw written at the place it used
    /// to be, and written **first**, before a single cell of the new frame. An undraw paints over what it
    /// removes, so whatever the frame draws lands on top of it; the other way round it would erase the
    /// frame instead of the picture. A frame that undraws anything is written whole rather than diffed,
    /// since the cells the undraw painted over have to be put back whether they changed or not.
    ///
    /// Whoever hands over pixels says how to take them back, because only they know: kitty deletes an
    /// image by number, a sixel has to be painted over.
    ///
    /// Nothing is re-sent while it stays the same. A frame is only composed when something asked for
    /// one, so a picture that has not changed costs nothing between frames — but a payload measured
    /// in kilobytes is still worth handing over only when it has to be.
    /// </summary>
    /// <param name="row">Row of the cell it starts at, counted from the top of the frame.</param>
    /// <param name="column">Column of that cell.</param>
    /// <param name="payload">The bytes to write, escapes and all.</param>
    /// <param name="undraw">
    /// What removes it again, written where the payload was. Empty when nothing can: a sixel on a
    /// terminal that will not say what colour is behind its text has to be left where it is.
    /// </param>
    public void Passthrough(int row, int column, string payload, string undraw = "")
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(undraw);

        if (payload.Length > 0)
        {
            _passthrough.Add((row, column, payload, undraw));
        }
    }

    /// <summary>
    /// Writes what removes the payloads this frame no longer hands over, before a single cell goes out.
    /// The order is the whole point: an undraw paints over what it removes, so anything the frame draws
    /// afterwards is drawn on top of it. Written the other way round it erases the frame instead of the
    /// picture.
    /// </summary>
    /// <returns><c>true</c> when something was undrawn, so the frame is written whole rather than diffed.</returns>
    private bool AppendUndraws()
    {
        var undrawn = false;

        foreach (var gone in _previousPassthrough)
        {
            if (gone.Undraw.Length == 0 || _passthrough.Contains(gone))
            {
                continue;
            }

            Place(gone.Row, gone.Column).Append(gone.Undraw);
            undrawn = true;
        }

        return undrawn;
    }

    /// <summary>
    /// Writes the payloads this frame hands over, after every cell.
    /// </summary>
    /// <param name="whole">
    /// Whether the frame was written whole rather than diffed, in which case every payload goes out again
    /// even if it has not changed. It has to: writing a cell is what removes the pixels over it in some
    /// terminals, so a frame that rewrote every cell rewrote the picture away. This is what a fixed-size
    /// surface, a resize and <see cref="ForgetPreviousFrame"/> all end up needing.
    /// </param>
    private void AppendPassthrough(bool whole)
    {
        if (!whole && Unchanged())
        {
            return;
        }

        foreach (var (row, column, payload, _) in _passthrough)
        {
            Place(row, column).Append(payload);
        }
    }

    private StringBuilder Place(int row, int column) =>
        _stringBuilder
            .Append("\e[")
            .Append(row + 1)
            .Append(';')
            .Append(column + 1)
            .Append('H');

    private bool Unchanged()
    {
        if (_passthrough.Count != _previousPassthrough.Length)
        {
            return false;
        }

        for (var index = 0; index < _passthrough.Count; index++)
        {
            if (_passthrough[index] != _previousPassthrough[index])
            {
                return false;
            }
        }

        return true;
    }

    private bool CanDrawChangesOnly() =>
        _fixedWidth == 0 &&
        _previousCells.Length == _height &&
        (_height == 0 || _previousCells[0].Length == _width);

    private void AppendWholeFrame()
    {
        if (_fixedWidth == 0)
        {
            _stringBuilder.Append("\e[?7l\e[H");
        }

        for (var row = 0; row < _height; row++)
        {
            var cells = _cells[row];
            var styles = _styles[row];
            IArlecchinoColor? current = null;

            for (var col = 0; col < _width; col++)
            {
                if (!ReferenceEquals(styles[col], current))
                {
                    current = styles[col];
                    _stringBuilder.Append(current.Ansi);
                }

                _stringBuilder.Append(cells[col]);
            }

            AppendStyleReset();

            if (row < _height - 1)
            {
                _stringBuilder.Append("\r\n");
            }
        }

        if (_fixedWidth == 0)
        {
            _stringBuilder.Append("\e[?7h");
        }
    }

    private void AppendChangedRuns()
    {
        for (var row = 0; row < _height; row++)
        {
            var col = 0;
            while (col < _width)
            {
                if (!CellChanged(row, col))
                {
                    col++;
                    continue;
                }

                var start = StartOfSymbol(row, col);
                var end = start;
                var unchangedRun = 0;

                for (var probe = start; probe < _width; probe++)
                {
                    if (CellChanged(row, probe))
                    {
                        end = probe;
                        unchangedRun = 0;
                        continue;
                    }

                    if (++unchangedRun > CursorMoveCost)
                    {
                        break;
                    }
                }

                AppendRun(row, start, end);
                col = end + 1;
            }
        }
    }

    private void AppendRun(int row, int start, int end)
    {
        _stringBuilder.Append("\e[").Append(row + 1).Append(';').Append(start + 1).Append('H');

        var cells = _cells[row];
        var styles = _styles[row];
        IArlecchinoColor? current = null;

        for (var col = start; col <= end; col++)
        {
            if (!ReferenceEquals(styles[col], current))
            {
                current = styles[col];
                _stringBuilder.Append(current.Ansi);
            }

            _stringBuilder.Append(cells[col]);
        }

        AppendStyleReset();
    }

    private void AppendStyleReset()
    {
        if (TerminalCapabilities.Color != ColorSupport.None)
        {
            _stringBuilder.Append("\e[0m");
        }
    }

    private bool CellChanged(int row, int col) =>
        !ReferenceEquals(_styles[row][col], _previousStyles[row][col]) ||
        !string.Equals(_cells[row][col], _previousCells[row][col], StringComparison.Ordinal);

    private int StartOfSymbol(int row, int col) =>
        col > 0 && _cells[row][col].Length == 0 ? col - 1 : col;

    private void RememberFrame()
    {
        _previousPassthrough = [.. _passthrough];

        if (_previousCells.Length != _height || (_height > 0 && _previousCells[0].Length != _width))
        {
            _previousCells = new string[_height][];
            _previousStyles = new IArlecchinoColor[_height][];
            for (var row = 0; row < _height; row++)
            {
                _previousCells[row] = new string[_width];
                _previousStyles[row] = new IArlecchinoColor[_width];
            }
        }

        for (var row = 0; row < _height; row++)
        {
            Array.Copy(_cells[row], _previousCells[row], _width);
            Array.Copy(_styles[row], _previousStyles[row], _width);
        }
    }

    private static SurfaceRegion Intersect(SurfaceRegion outer, SurfaceRegion inner)
    {
        var left = Math.Max(outer.Left, inner.Left);
        var top = Math.Max(outer.Top, inner.Top);

        return new(
            inner.Surface,
            left,
            top,
            Math.Max(0, Math.Min(outer.Right, inner.Right) - left),
            Math.Max(0, Math.Min(outer.Bottom, inner.Bottom) - top));
    }

    private bool IsInsideClip(int row, int column) =>
        _clip is not { } clip || clip.Contains(row, column);

    private void SetCell(int row, int column, string cell, int cellWidth, IArlecchinoColor style)
    {
        if (!IsInsideClip(row, column))
        {
            return;
        }

        var cells = _cells[row];

        if (column > 0 && cells[column].Length == 0)
        {
            cells[column - 1] = BlankCell;
        }

        if (cellWidth == 2 && column + 1 < _width && cells[column + 1].Length == 0)
        {
            ClearWideTail(row, column + 1);
        }

        if (cellWidth == 1 && column + 1 < _width && cells[column + 1].Length == 0)
        {
            cells[column + 1] = BlankCell;
        }

        cells[column] = cell;
        _styles[row][column] = style;

        if (cellWidth != 2 || column + 1 >= _width)
        {
            return;
        }

        cells[column + 1] = WideCellTail;
        _styles[row][column + 1] = style;
    }

    private void ClearWideTail(int row, int column)
    {
        for (var next = column; next < _width && _cells[row][next].Length == 0; next++)
        {
            _cells[row][next] = BlankCell;
        }
    }

    private static string CellOf(ReadOnlySpan<char> cluster) =>
        cluster.Length == 1 && cluster[0] < AsciiCells.Length
            ? AsciiCells[cluster[0]]
            : cluster.ToString();

    private static string[] BuildAsciiCells()
    {
        var cells = new string[128];
        for (var value = 0; value < cells.Length; value++)
        {
            cells[value] = ((char)value).ToString();
        }

        return cells;
    }

    private sealed class ClipScope : IDisposable
    {
        private readonly Surface _surface;
        private readonly SurfaceRegion? _previous;

        public ClipScope(Surface surface, SurfaceRegion? previous)
        {
            _surface = surface;
            _previous = previous;
        }

        public void Dispose() => _surface._clip = _previous;
    }
}
