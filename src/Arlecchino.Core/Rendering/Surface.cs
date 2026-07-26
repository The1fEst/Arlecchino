using System;
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

        if (CanDrawChangesOnly())
        {
            AppendChangedRuns();
        }
        else
        {
            AppendWholeFrame();
        }

        RememberFrame();

        if (_stringBuilder.Length > 0)
        {
            _terminal.Write(_stringBuilder.ToString());
        }
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
