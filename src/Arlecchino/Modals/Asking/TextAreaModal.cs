using System;
using System.Collections.Generic;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Modals.Asking;

/// <summary>
/// Several lines of text, edited in place: a description, a commit message, a snippet of
/// configuration. <c>Enter</c> starts a new line here rather than accepting the dialog, so confirming
/// is a key of its own — the <c>Submit</c> binding.
///
/// The caret is a row and a position inside that row, and every move and edit goes by symbols rather
/// than <c>char</c> values, so emoji and combining marks survive a backspace.
/// </summary>
public sealed class TextAreaModal : Modal
{
    private readonly List<string> _lines = [""];

    private int _row;
    private int _column;

    /// <summary>The whole text, lines joined with a newline. Assigning it puts the caret at the end.</summary>
    public string Text
    {
        get => string.Join("\n", _lines);
        init => SetText(value);
    }

    /// <summary>The lines as they stand, top to bottom.</summary>
    public IReadOnlyList<string> Lines => _lines;

    /// <summary>Row the caret is on.</summary>
    public int Row => _row;

    /// <summary>Where the caret sits inside its row, as an index into that line.</summary>
    public int Column => _column;

    /// <summary>How many rows of text the dialog shows before it starts scrolling.</summary>
    public int VisibleRows { get; init; } = 8;

    /// <summary>
    /// Checked when the text is submitted; return a message to keep the dialog open, or <c>null</c> to
    /// accept.
    /// </summary>
    public Func<string, string?>? Validate { get; init; }

    /// <summary>Called with the accepted text.</summary>
    public required Action<string> OnSubmit { get; init; }

    /// <summary>Why the last attempt to submit was refused, drawn under the text.</summary>
    public string Message { get; set; } = "";

    /// <summary>Where the text area was drawn last frame, for turning a click into a caret position.</summary>
    public SurfaceRegion Rows { get; set; }

    /// <summary>First visible row, kept in step with the caret while drawing.</summary>
    public int FirstVisible { get; set; }

    /// <summary>Replaces the whole text and puts the caret at its end.</summary>
    /// <param name="text">The text to hold.</param>
    public void SetText(string text)
    {
        _lines.Clear();
        _lines.AddRange(text.Replace("\r", "").Split('\n'));

        _row = _lines.Count - 1;
        _column = _lines[_row].Length;
    }

    /// <summary>Puts the caret at a row and a position inside it, clamped to what exists.</summary>
    /// <param name="row">Row to move to.</param>
    /// <param name="column">Index inside that row.</param>
    public void MoveCaret(int row, int column)
    {
        _row = Math.Clamp(row, 0, _lines.Count - 1);
        _column = TextWidth.SnapToCluster(_lines[_row], Math.Clamp(column, 0, _lines[_row].Length));
    }

    /// <summary>Inserts a character where the caret is.</summary>
    /// <param name="character">What to insert.</param>
    public void Insert(char character) => InsertText(character.ToString());

    /// <summary>Inserts text where the caret is, starting a new line for every newline in it.</summary>
    /// <param name="text">What to insert.</param>
    public void InsertText(string text)
    {
        var parts = text.Replace("\r", "").Split('\n');

        for (var index = 0; index < parts.Length; index++)
        {
            var line = _lines[_row];

            _lines[_row] = line[.._column] + parts[index] + line[_column..];
            _column += parts[index].Length;

            if (index < parts.Length - 1)
            {
                Break();
            }
        }
    }

    /// <summary>Splits the current line at the caret, which is what <c>Enter</c> does here.</summary>
    public void Break()
    {
        var line = _lines[_row];

        _lines[_row] = line[.._column];
        _lines.Insert(_row + 1, line[_column..]);

        _row++;
        _column = 0;
    }

    /// <summary>
    /// Deletes the symbol before the caret, joining this line onto the one above when the caret is at
    /// the start of a line.
    /// </summary>
    public void Erase()
    {
        if (_column > 0)
        {
            var line = _lines[_row];
            var start = TextWidth.PreviousClusterStart(line, _column);

            _lines[_row] = line[..start] + line[_column..];
            _column = start;
            return;
        }

        if (_row == 0)
        {
            return;
        }

        var above = _lines[_row - 1];

        _column = above.Length;
        _lines[_row - 1] = above + _lines[_row];
        _lines.RemoveAt(_row);
        _row--;
    }

    /// <summary>
    /// Deletes the symbol after the caret, pulling the next line up when the caret is at the end of a
    /// line.
    /// </summary>
    public void DeleteForward()
    {
        var line = _lines[_row];

        if (_column < line.Length)
        {
            _lines[_row] = line[.._column] + line[TextWidth.NextClusterEnd(line, _column)..];
            return;
        }

        if (_row + 1 >= _lines.Count)
        {
            return;
        }

        _lines[_row] = line + _lines[_row + 1];
        _lines.RemoveAt(_row + 1);
    }

    /// <summary>Moves the caret one symbol left, wrapping to the end of the line above.</summary>
    public void MoveLeft()
    {
        if (_column > 0)
        {
            _column = TextWidth.PreviousClusterStart(_lines[_row], _column);
            return;
        }

        if (_row == 0)
        {
            return;
        }

        _row--;
        _column = _lines[_row].Length;
    }

    /// <summary>Moves the caret one symbol right, wrapping to the start of the line below.</summary>
    public void MoveRight()
    {
        var line = _lines[_row];

        if (_column < line.Length)
        {
            _column = TextWidth.NextClusterEnd(line, _column);
            return;
        }

        if (_row + 1 >= _lines.Count)
        {
            return;
        }

        _row++;
        _column = 0;
    }

    /// <summary>Moves the caret a number of rows, keeping as much of the column as the new row has.</summary>
    /// <param name="rows">How far to move; negative goes up.</param>
    public void MoveRows(int rows) => MoveCaret(_row + rows, _column);

    /// <summary>Puts the caret at the start of its line.</summary>
    public void MoveToLineStart() => _column = 0;

    /// <summary>Puts the caret at the end of its line.</summary>
    public void MoveToLineEnd() => _column = _lines[_row].Length;
}
