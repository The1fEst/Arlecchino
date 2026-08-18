using System;
using System.Collections.Generic;
using Arlecchino.Editing;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Modals.Asking;

/// <summary>
/// Several lines of text, edited in place, where <c>Enter</c> starts a new line and the <c>Submit</c> binding
/// confirms. The text is one line with newlines in it, so every edit is the one a field of one line has.
/// </summary>
public sealed class TextAreaModal : Modal, ITextEntry
{
    private string _text = "";
    private string[] _lines = [""];
    private int _caret;
    private int _anchor;

    /// <summary>The whole text, lines joined with a newline. Assigning it puts the caret at the end.</summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value.Replace("\r", "", StringComparison.Ordinal);
            _lines = _text.Split('\n');
            _caret = _text.Length;
            _anchor = _text.Length;
        }
    }

    /// <summary>Where the caret sits, counted from the start of the whole text.</summary>
    public int Caret
    {
        get => _caret;
        set => _caret = Math.Clamp(value, 0, _text.Length);
    }

    /// <summary>Where the selection was started from, on the caret while nothing is selected.</summary>
    public int Anchor
    {
        get => _anchor;
        set => _anchor = Math.Clamp(value, 0, _text.Length);
    }

    /// <summary>The lines as they stand, top to bottom.</summary>
    public IReadOnlyList<string> Lines => _lines;

    /// <summary>Row the caret is on.</summary>
    public int Row => Placed(_caret).Row;

    /// <summary>Where the caret sits inside its row, as an index into that line.</summary>
    public int Column => Placed(_caret).Column;

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

    /// <summary>Where in the whole text a row begins.</summary>
    /// <param name="row">The row, clamped to the rows there are.</param>
    /// <returns>The index of the first character on it.</returns>
    public int StartOf(int row)
    {
        var room = Math.Clamp(row, 0, _lines.Length - 1);
        var start = 0;

        for (var index = 0; index < room; index++)
        {
            start += _lines[index].Length + 1;
        }

        return start;
    }

    /// <summary>Inserts text where the caret is, over whatever was selected.</summary>
    /// <param name="text">What to insert; a newline in it starts a new line.</param>
    public void InsertText(string text) =>
        TextEditing.InsertText(this, text.Replace("\r", "", StringComparison.Ordinal));

    /// <summary>The line being typed into, which here is the whole text with its newlines in it.</summary>
    public override ITextEntry Typing => this;

    /// <summary>
    /// Takes pasted text whole, line breaks included, since this is the one dialog that holds more than
    /// one row of it.
    /// </summary>
    /// <param name="frame">The keys to obey, and how to close.</param>
    /// <param name="text">What was pasted.</param>
    public override void HandlePaste(ModalFrame frame, string text) => InsertText(text);

    /// <summary>Puts the caret at a row and a position inside it, clamped to what exists.</summary>
    /// <param name="row">Row to move to.</param>
    /// <param name="column">Index inside that row.</param>
    public void MoveCaret(int row, int column) => Put(Placed(row, column), collapse: true);

    /// <summary>Moves the caret a number of rows, keeping as much of the column as the new row has.</summary>
    /// <param name="rows">How far to move; negative goes up.</param>
    public void MoveRows(int rows) => Put(Placed(Row + rows, Column), collapse: true);

    /// <summary>Takes the selection a number of rows, dragging it along behind the caret.</summary>
    /// <param name="rows">How far to take it; negative goes up.</param>
    public void SelectRows(int rows) => Put(Placed(Row + rows, Column), collapse: false);

    /// <summary>Puts the caret at the start of its line.</summary>
    public void MoveToLineStart() => Put(StartOf(Row), collapse: true);

    /// <summary>Puts the caret at the end of its line.</summary>
    public void MoveToLineEnd() => Put(StartOf(Row) + _lines[Row].Length, collapse: true);

    /// <summary>Takes the selection back to the start of the line.</summary>
    public void SelectToLineStart() => Put(StartOf(Row), collapse: false);

    /// <summary>Takes the selection on to the end of the line.</summary>
    public void SelectToLineEnd() => Put(StartOf(Row) + _lines[Row].Length, collapse: false);

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) => frame.Paint.Area(this);

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key) => frame.Areas.Handle(this, key);

    /// <summary>Where a place in the text falls, as the row it is on and how far into that row it is.</summary>
    /// <param name="index">The place in the whole text.</param>
    /// <returns>The row and the index inside it.</returns>
    private (int Row, int Column) Placed(int index)
    {
        var row = 0;
        var start = 0;

        for (var walk = 0; walk < index && walk < _text.Length; walk++)
        {
            if (_text[walk] != '\n')
            {
                continue;
            }

            row++;
            start = walk + 1;
        }

        return (row, index - start);
    }

    private int Placed(int row, int column)
    {
        var room = Math.Clamp(row, 0, _lines.Length - 1);

        return StartOf(room) + Math.Clamp(column, 0, _lines[room].Length);
    }

    private void Put(int caret, bool collapse)
    {
        Caret = TextWidth.SnapToCluster(_text, caret);

        if (collapse)
        {
            Anchor = Caret;
        }
    }
}
