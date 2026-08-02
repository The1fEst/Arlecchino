using System;
using Arlecchino.Rendering;

namespace Arlecchino.Testing;

/// <summary>
/// A terminal screen as the terminal itself would hold it: a grid of cells that output is applied to
/// rather than collected in. Where <see cref="FrameText"/> strips escapes out of what was written,
/// this obeys them — a cursor jump moves the cursor, a style sticks to the cells that follow, a wide
/// symbol takes two columns.
///
/// That difference is the point. Frames are written as the difference from the last one, so what
/// reaches the terminal is a handful of jumps and runs which say nothing on their own about what is
/// on screen afterwards. Applying them here answers that, and makes the invariant worth asserting:
/// a screen built from diffs is the screen a whole repaint would have drawn.
/// </summary>
public sealed class ScreenGrid
{
    private const string Blank = " ";
    private const string WideTail = "";
    private const int TabStop = 8;

    private string[][] _cells = [];
    private string[][] _styles = [];
    private Kept? _normal;
    private string _style = "";
    private int _row;
    private int _column;
    private bool _wraps = true;

    /// <summary>Creates a blank screen at a fixed size.</summary>
    /// <param name="width">Columns.</param>
    /// <param name="height">Rows.</param>
    public ScreenGrid(int width, int height)
    {
        Resize(width, height);
    }

    /// <summary>Columns.</summary>
    public int Width { get; private set; }

    /// <summary>Rows.</summary>
    public int Height { get; private set; }

    /// <summary>The row the cursor sits on, counted from the top.</summary>
    public int CursorRow => _row;

    /// <summary>
    /// The column the cursor sits on, counted from the left. A symbol written into the last column
    /// while wrapping is on leaves it one past the right edge, waiting to wrap: the next symbol goes
    /// to the row below, and a terminal asked where its cursor is answers the same way.
    /// </summary>
    public int CursorColumn => Math.Min(_column, Width);

    /// <summary>Whether the cursor was left visible.</summary>
    public bool IsCursorVisible { get; private set; } = true;

    /// <summary>
    /// Applies what was written to the terminal. Call it as often as there are frames — the screen
    /// carries over, which is what makes a run of diffed frames add up to a picture.
    /// </summary>
    /// <param name="output">The bytes written, escapes and all.</param>
    public void Apply(string output)
    {
        ArgumentNullException.ThrowIfNull(output);

        var index = 0;
        while (index < output.Length)
        {
            index = output[index] switch
            {
                '\e' => Escape(output, index),
                '\r' => Return(index),
                '\n' => Feed(index),
                '\b' => Back(index),
                '\t' => Tab(index),
                _ => Put(output, index),
            };
        }
    }

    /// <summary>
    /// Resizes the screen, keeping what fits and dropping the rest, the way a terminal does. The
    /// cursor is pulled back inside.
    /// </summary>
    /// <param name="width">Columns.</param>
    /// <param name="height">Rows.</param>
    public void Resize(int width, int height)
    {
        var cells = new string[height][];
        var styles = new string[height][];

        for (var row = 0; row < height; row++)
        {
            cells[row] = new string[width];
            styles[row] = new string[width];

            for (var column = 0; column < width; column++)
            {
                var kept = row < Height && column < Width;
                cells[row][column] = kept ? _cells[row][column] : Blank;
                styles[row][column] = kept ? _styles[row][column] : "";
            }
        }

        _cells = cells;
        _styles = styles;
        Width = width;
        Height = height;
        _row = Math.Clamp(_row, 0, Math.Max(0, height - 1));
        _column = Math.Clamp(_column, 0, Math.Max(0, width));
    }

    /// <summary>The symbol standing on a cell, or an empty string for the second half of a wide one.</summary>
    /// <param name="row">Row, counted from the top.</param>
    /// <param name="column">Column, counted from the left.</param>
    /// <returns>The symbol.</returns>
    public string CellAt(int row, int column) => _cells[row][column];

    /// <summary>
    /// The style sequence in force on a cell, empty where the style was reset. Compare it against
    /// <see cref="TermColor.Ansi"/> to assert that something was drawn in the colour it should be.
    /// </summary>
    /// <param name="row">Row, counted from the top.</param>
    /// <param name="column">Column, counted from the left.</param>
    /// <returns>The sequence.</returns>
    public string StyleAt(int row, int column) => _styles[row][column];

    /// <summary>One row as it reads, padded out to the full width.</summary>
    /// <param name="row">Row, counted from the top.</param>
    /// <returns>The row as text.</returns>
    public string Line(int row) => string.Concat(_cells[row]);

    /// <summary>Every row as it reads.</summary>
    /// <returns>One string per row.</returns>
    public string[] Lines()
    {
        var lines = new string[Height];
        for (var row = 0; row < Height; row++)
        {
            lines[row] = Line(row);
        }

        return lines;
    }

    /// <summary>
    /// Whether another screen holds the same symbols in the same styles. Text alone is the readable
    /// half of a frame, so a screen that matches on <see cref="ToString"/> can still differ in colour.
    /// </summary>
    /// <param name="other">The screen to compare against.</param>
    /// <returns><c>true</c> when both the symbols and the styles agree.</returns>
    public bool Matches(ScreenGrid other)
    {
        ArgumentNullException.ThrowIfNull(other);

        if (other.Width != Width || other.Height != Height)
        {
            return false;
        }

        for (var row = 0; row < Height; row++)
        {
            for (var column = 0; column < Width; column++)
            {
                if (!string.Equals(_cells[row][column], other._cells[row][column], StringComparison.Ordinal) ||
                    !string.Equals(_styles[row][column], other._styles[row][column], StringComparison.Ordinal))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>The whole screen as text, rows separated by line feeds.</summary>
    /// <returns>The screen as it reads.</returns>
    public override string ToString() => string.Join('\n', Lines());

    private int Return(int index)
    {
        _column = 0;

        return index + 1;
    }

    private int Feed(int index)
    {
        Down();

        return index + 1;
    }

    private int Back(int index)
    {
        _column = Math.Max(0, _column - 1);

        return index + 1;
    }

    private int Tab(int index)
    {
        var from = Math.Min(_column, Math.Max(0, Width - 1));

        _column = Math.Min((from / TabStop + 1) * TabStop, Math.Max(0, Width - 1));

        return index + 1;
    }

    private int Escape(string output, int index)
    {
        if (index + 1 >= output.Length)
        {
            return output.Length;
        }

        return output[index + 1] switch
        {
            '[' => ControlSequence(output, index + 2),
            ']' or 'P' or '_' or '^' or 'X' => StringSequence(output, index + 2),
            _ => index + 2,
        };
    }

    private int ControlSequence(string output, int start)
    {
        var at = start;
        while (at < output.Length && !char.IsBetween(output[at], '@', '~'))
        {
            at++;
        }

        if (at >= output.Length)
        {
            return output.Length;
        }

        Dispatch(output[at], output[start..at]);

        return at + 1;
    }

    /// <summary>
    /// Skips a payload handed to the terminal whole — an image in one of the graphics protocols, most
    /// of all. It is not text and never lands on a cell, so the screen has to step over it rather
    /// than spell it out.
    ///
    /// What the pixels themselves come to is not modelled and cannot be: a grid of cells has nowhere to
    /// put them, and a terminal that draws a sixel inline moves the cursor by however tall the picture
    /// turned out. Nothing here depends on that — every frame positions the cursor outright before it
    /// writes anything — but a screen read back after a picture was drawn is a screen with the picture
    /// missing.
    /// </summary>
    /// <param name="output">What was written.</param>
    /// <param name="start">The first character after the introducer.</param>
    /// <returns>The index just past the terminator.</returns>
    private static int StringSequence(string output, int start)
    {
        for (var at = start; at < output.Length; at++)
        {
            if (output[at] == '\a')
            {
                return at + 1;
            }

            if (output[at] == '\e' && at + 1 < output.Length && output[at + 1] == '\\')
            {
                return at + 2;
            }
        }

        return output.Length;
    }

    private void Dispatch(char final, string parameters)
    {
        if (parameters.StartsWith('?'))
        {
            PrivateMode(parameters[1..], final);

            return;
        }

        switch (final)
        {
            case 'H' or 'f':
                MoveTo(Parameter(parameters, 0, 1) - 1, Parameter(parameters, 1, 1) - 1);
                break;
            case 'A':
                MoveTo(_row - Parameter(parameters, 0, 1), _column);
                break;
            case 'B':
                MoveTo(_row + Parameter(parameters, 0, 1), _column);
                break;
            case 'C':
                MoveTo(_row, _column + Parameter(parameters, 0, 1));
                break;
            case 'D':
                MoveTo(_row, _column - Parameter(parameters, 0, 1));
                break;
            case 'K':
                EraseLine(Parameter(parameters, 0, 0));
                break;
            case 'J':
                EraseScreen(Parameter(parameters, 0, 0));
                break;
            case 'm':
                Style(parameters);
                break;
        }
    }

    private void PrivateMode(string parameters, char final)
    {
        var on = final == 'h';

        foreach (var mode in parameters.Split(';'))
        {
            switch (mode)
            {
                case "7":
                    _wraps = on;
                    break;
                case "25":
                    IsCursorVisible = on;
                    break;
                case "1049":
                    AlternateScreen(on);
                    break;
            }
        }
    }

    /// <summary>
    /// Takes the screen over, or gives it back. An application that draws full screen asks for a screen
    /// of its own, which arrives blank and hands the one underneath back untouched when it leaves — the
    /// shell scrollback a user sees again on exit. Asking twice for what is already in force changes
    /// nothing, which is what a terminal does with it.
    /// </summary>
    /// <param name="on">Whether the screen is being taken over.</param>
    private void AlternateScreen(bool on)
    {
        if (_normal is not { } kept)
        {
            if (!on)
            {
                return;
            }

            _normal = new(_cells, _styles, _row, _column);
            _cells = new string[Height][];
            _styles = new string[Height][];

            for (var row = 0; row < Height; row++)
            {
                _cells[row] = new string[Width];
                _styles[row] = new string[Width];
                Array.Fill(_cells[row], Blank);
                Array.Fill(_styles[row], "");
            }

            return;
        }

        if (on)
        {
            return;
        }

        _cells = kept.Cells;
        _styles = kept.Styles;
        _row = Math.Clamp(kept.Row, 0, Math.Max(0, Height - 1));
        _column = Math.Clamp(kept.Column, 0, Math.Max(0, Width));
        _normal = null;
    }

    private void Style(string parameters)
    {
        if (parameters is "" or "0")
        {
            _style = "";

            return;
        }

        var sequence = $"\e[{parameters}m";
        _style = Head(parameters) is "" or "0" ? sequence : _style + sequence;
    }

    private void MoveTo(int row, int column)
    {
        _row = Math.Clamp(row, 0, Math.Max(0, Height - 1));
        _column = Math.Clamp(column, 0, Math.Max(0, Width - 1));
    }

    private void EraseLine(int kind) =>
        Fill(_row, kind == 0 ? Math.Min(_column, Width) : 0, kind == 1 ? Math.Min(_column + 1, Width) : Width);

    private void EraseScreen(int kind)
    {
        var first = kind == 0 ? _row : 0;
        var last = kind == 1 ? _row : Height - 1;

        for (var row = first; row <= last; row++)
        {
            if (row != _row)
            {
                Fill(row, 0, Width);
            }
        }

        EraseLine(kind);
    }

    private void Fill(int row, int from, int to)
    {
        for (var column = from; column < to; column++)
        {
            _cells[row][column] = Blank;
            _styles[row][column] = _style;
        }
    }

    private void Down()
    {
        if (++_row < Height)
        {
            return;
        }

        _row = Math.Max(0, Height - 1);
        Scroll();
    }

    private void Scroll()
    {
        for (var row = 0; row + 1 < Height; row++)
        {
            _cells[row] = _cells[row + 1];
            _styles[row] = _styles[row + 1];
        }

        if (Height == 0)
        {
            return;
        }

        _cells[^1] = new string[Width];
        _styles[^1] = new string[Width];
        Array.Fill(_cells[^1], Blank);
        Array.Fill(_styles[^1], "");
    }

    private int Put(string output, int index)
    {
        var length = TextWidth.NextClusterLength(output, index);
        var cluster = output.Substring(index, length);
        var width = TextWidth.OfCluster(cluster);

        if (width > 0 && Width > 0 && Height > 0 && Fits(width))
        {
            Write(cluster, width);
        }

        return index + length;
    }

    /// <summary>
    /// Makes room for a symbol about to be written. A symbol that runs past the right edge moves to
    /// the next row; with wrapping off there is nowhere to move it to, so the cursor stays in the last
    /// column and the symbol is dropped rather than pushed inwards — which is what makes a frame drawn
    /// past its own width show up as a defect rather than as a row that quietly grew.
    ///
    /// Terminals do not agree about that last case, and there is nothing to be right about: a wide
    /// symbol written into the last column is dropped by tmux and shifted a column inwards by kitty, so
    /// whichever this does contradicts one of them. It is dropped here because a frame never asks for
    /// it — the surface refuses a wide symbol the right edge would split, and writes a blank instead —
    /// so the only way to reach this is by hand, and a symbol that vanishes is easier to notice than one
    /// that moved.
    /// </summary>
    /// <param name="width">Columns the symbol takes.</param>
    /// <returns><c>false</c> when there is nowhere to put it.</returns>
    private bool Fits(int width)
    {
        if (_column + width <= Width)
        {
            return true;
        }

        if (!_wraps)
        {
            return false;
        }

        _column = 0;
        Down();

        return width <= Width;
    }

    private void Write(string cluster, int width)
    {
        var cells = _cells[_row];

        if (_column > 0 && cells[_column].Length == 0)
        {
            cells[_column - 1] = Blank;
        }

        cells[_column] = cluster;
        _styles[_row][_column] = _style;

        if (width == 2)
        {
            cells[_column + 1] = WideTail;
            _styles[_row][_column + 1] = _style;
        }

        var next = _column + width;

        if (next < Width && cells[next].Length == 0)
        {
            cells[next] = Blank;
        }

        _column = _wraps ? next : Math.Min(next, Width - 1);
    }

    private static string Head(string parameters)
    {
        var end = parameters.IndexOf(';');

        return end < 0 ? parameters : parameters[..end];
    }

    /// <summary>The screen an application took over, kept until it gives it back.</summary>
    /// <param name="Cells">The symbols that were on it.</param>
    /// <param name="Styles">The styles they were in.</param>
    /// <param name="Row">Where the cursor was.</param>
    /// <param name="Column">Where the cursor was.</param>
    private readonly record struct Kept(string[][] Cells, string[][] Styles, int Row, int Column);

    private static int Parameter(string parameters, int position, int fallback)
    {
        var at = 0;
        foreach (var part in parameters.Split(';'))
        {
            if (at++ == position)
            {
                return int.TryParse(part, out var value) ? value : fallback;
            }
        }

        return fallback;
    }
}
