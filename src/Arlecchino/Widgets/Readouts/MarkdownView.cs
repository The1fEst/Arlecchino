using System;
using System.Collections.Generic;
using Arlecchino.Focus;
using Arlecchino.Hosting;
using Arlecchino.Input;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Widgets.Lists;
using Arlecchino.Widgets.Pictures;
using Arlecchino.Widgets.Text;

namespace Arlecchino.Widgets.Readouts;

/// <summary>
/// A Markdown document to read: headings, lists, quotes, code blocks, tables and links drawn as what they
/// are. It wraps to the width it is given and scrolls the way <see cref="TextView"/> does.
/// </summary>
public sealed class MarkdownView : IArlecchinoInteractiveWidget
{
    private const string Cut = "›";

    private readonly ScrollPane _pane;

    private List<MarkdownLine> _lines = [];
    private List<MarkdownRow> _rows = [];
    private List<MarkdownPlate> _plates = [];
    private string _readFrom = "";
    private int _wrappedTo = -1;
    private int _pictureRows = -1;

    private TermColor _derivedFrom = new();
    private TermColor _strong = new();
    private TermColor _emphasis = new();
    private TermColor _link = new();

    /// <summary>Creates the view.</summary>
    /// <param name="keymap">Keys to obey, so scrolling follows the application's bindings.</param>
    public MarkdownView(ArlecchinoKeymap keymap)
    {
        _pane = new(keymap)
        {
            ContentHeight = () => _rows.Count,
            Content = DrawRows,
        };
    }

    /// <summary>The document, as it is written in the file.</summary>
    public string Text { get; set; } = "";

    /// <summary>What each part of it is written in.</summary>
    public MarkdownStyles Styles { get; set; } = new();

    /// <summary>
    /// The picture at an address the document names, which the application reads and holds. It is asked
    /// while a frame is drawn, so it answers at once, with <c>null</c> for a picture that is not there.
    /// </summary>
    public Func<string, Picture?>? Pictures { get; set; }

    /// <summary>
    /// How many rows a picture may take. A picture is drawn no larger than it is, and no taller than
    /// this, so that what surrounds it is still on the screen with it.
    /// </summary>
    public int PictureRows { get; set; } = 16;

    /// <summary>Whether a scroll bar is drawn when the document does not fit.</summary>
    public bool ShowScrollBar
    {
        get => _pane.ShowScrollBar;
        init => _pane.ShowScrollBar = value;
    }

    /// <summary>First row shown.</summary>
    public int Offset
    {
        get => _pane.Offset;
        set => _pane.Offset = value;
    }

    /// <summary>How many rows the document takes once laid out for the last width it was drawn at.</summary>
    public int LineCount => _rows.Count;

    /// <summary>Whether the view has focus. Only a focused view answers keys.</summary>
    public bool IsFocused
    {
        get => _pane.IsFocused;
        set => _pane.IsFocused = value;
    }

    /// <summary>
    /// Lays the document out for the region and draws the part that fits. The view fills whatever it is
    /// given, so nothing is left underneath it.
    /// </summary>
    /// <param name="region">Where to draw.</param>
    /// <returns>An empty region: the view uses every row it is handed.</returns>
    public SurfaceRegion Draw(SurfaceRegion region)
    {
        if (region.IsEmpty)
        {
            return region;
        }

        var width = _pane.ShowScrollBar ? Math.Max(1, region.Width - 1) : region.Width;

        Derive();
        Relay(width);

        var drawnRegion = _pane.Draw(region);

        Place(region with { Width = width });

        return drawnRegion;
    }

    /// <summary>Scrolls the document.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>Whether the view took it.</returns>
    public FocusResult Handle(KeyPress key) => _pane.Handle(key);

    /// <summary>Scrolls with the wheel while the pointer is over the document.</summary>
    /// <param name="mouse">The event that arrived.</param>
    /// <returns>Whether the view took it.</returns>
    public FocusResult HandleMouse(MouseEvent mouse) => _pane.HandleMouse(mouse);

    /// <summary>
    /// The three styles a palette has no role for, which are the ordinary text said another way. They are
    /// worked out again whenever the palette is swapped, since a style is held by reference.
    /// </summary>
    private void Derive()
    {
        if (ReferenceEquals(_derivedFrom, Theme.Default))
        {
            return;
        }

        _derivedFrom = Theme.Default;
        _strong = Said(_derivedFrom, TextStyle.Bold);
        _emphasis = Said(_derivedFrom, TextStyle.Italic);
        _link = Said(_derivedFrom, TextStyle.Underline);
    }

    /// <summary>One style with a weight added to it, which is the same colors said another way.</summary>
    /// <param name="color">The style to take the colors from.</param>
    /// <param name="style">What to add to it.</param>
    /// <returns>The style.</returns>
    private static TermColor Said(TermColor color, TextStyle style) => new()
    {
        Foreground = color.Foreground,
        Background = color.Background,
        ExactForeground = color.ExactForeground,
        ExactBackground = color.ExactBackground,
        Style = color.Style | style,
    };

    /// <summary>
    /// Reads the document, and lays it out again where the width has moved. Reading is the slower of the
    /// two and the width is what changes, so what the document came to is kept between widths.
    /// </summary>
    /// <param name="width">How many columns there are to draw in.</param>
    private void Relay(int width)
    {
        if (!string.Equals(_readFrom, Text, StringComparison.Ordinal))
        {
            _lines = Markdown.Read(Text);
            _readFrom = Text;
            _wrappedTo = -1;
        }

        var pictureRows = RowsForPictures(width);

        if (_wrappedTo == width && _pictureRows == pictureRows)
        {
            return;
        }

        (_rows, _plates) = MarkdownWrap.Of(_lines, width, Rows);
        _wrappedTo = width;
        _pictureRows = pictureRows;
    }

    /// <summary>
    /// How much of the document its pictures take up at a width. A picture arrives after the document is
    /// read, whenever the application has it, and the rows kept for it are worked out again when it does.
    /// </summary>
    /// <param name="width">How many columns there are to draw in.</param>
    /// <returns>The rows every picture in the document takes, all told.</returns>
    private int RowsForPictures(int width)
    {
        if (Pictures is null)
        {
            return 0;
        }

        var rows = 0;

        foreach (var line in _lines)
        {
            if (line.Shape != MarkdownShape.Picture || line.Pictures.Count == 0)
            {
                continue;
            }

            var columns = Math.Max(1, width / line.Pictures.Count);

            foreach (var address in line.Pictures)
            {
                rows += Rows(address, columns);
            }
        }

        return rows;
    }

    /// <summary>
    /// How many rows a picture takes: as many as it is tall at the size a cell of this terminal is, down
    /// to what fits across the pane, and never more than <see cref="PictureRows"/>.
    /// </summary>
    /// <param name="address">Which picture.</param>
    /// <param name="columns">How many columns it is given.</param>
    /// <returns>The rows to keep for it, none at all for a picture that is not there.</returns>
    private int Rows(string address, int columns)
    {
        if (Pictures?.Invoke(address) is not { IsEmpty: false } picture || columns <= 0)
        {
            return 0;
        }

        var cellWidth = Math.Max(1, Glyphs.CellWidth);
        var cellHeight = Math.Max(1, Glyphs.CellHeight);
        var wide = Math.Min(columns, (double)picture.PixelWidth / cellWidth);
        var rows = picture.PixelHeight * wide * cellWidth / picture.PixelWidth / cellHeight;

        return Math.Clamp((int)Math.Ceiling(rows), 1, Math.Max(1, PictureRows));
    }

    /// <summary>
    /// Draws the pictures over the rows kept clear for them, and only where the whole of a picture is in
    /// the pane. A graphics protocol is placed at a cell, which no pane can clip.
    /// </summary>
    /// <param name="region">The pane, as the content was drawn into it.</param>
    private void Place(SurfaceRegion region)
    {
        foreach (var plate in _plates)
        {
            var top = region.Top + plate.Row - _pane.Offset;

            if (top < region.Top || top + plate.Rows > region.Bottom)
            {
                continue;
            }

            var columns = region.Width / plate.Addresses.Count;

            for (var at = 0; at < plate.Addresses.Count; at++)
            {
                if (Pictures?.Invoke(plate.Addresses[at]) is not { IsEmpty: false } picture)
                {
                    continue;
                }

                picture.Draw(region with
                {
                    Left = region.Left + (at * columns),
                    Top = top,
                    Width = columns,
                    Height = plate.Rows,
                });
            }
        }
    }

    /// <summary>Draws every row of the document, the pane having put the region where they belong.</summary>
    /// <param name="region">Where to draw.</param>
    private void DrawRows(SurfaceRegion region)
    {
        for (var row = 0; row < _rows.Count; row++)
        {
            var line = _rows[row];

            if (line.Shape == MarkdownShape.Code)
            {
                region.Rows(row, 1).Fill(Style(MarkdownInk.Code));
            }

            var column = line.Indent;

            foreach (var run in line.Runs)
            {
                region.Write(row, column, run.Text, Style(run.Ink));
                column += TextWidth.Of(run.Text);
            }

            if (column > region.Width)
            {
                region.Write(row, region.Width - 1, Cut, Style(Ink(line.Shape)));
            }
        }
    }

    /// <summary>
    /// What a row that runs past the edge is marked with, in its last column. A table and a code block
    /// are not wrapped, and a row that was cut reads like a row that ended.
    /// </summary>
    /// <param name="shape">What the row is, so the mark sits on the same ground as the row does.</param>
    /// <returns>What to write the mark in.</returns>
    private static MarkdownInk Ink(MarkdownShape shape) =>
        shape == MarkdownShape.Code ? MarkdownInk.Code : MarkdownInk.Rule;

    /// <summary>What a part of the document is written in: what was asked for, or what the theme says.</summary>
    /// <param name="ink">Which part.</param>
    /// <returns>The style to draw it in.</returns>
    private IArlecchinoColor Style(MarkdownInk ink) => ink switch
    {
        MarkdownInk.Strong => Styles.Strong ?? _strong,
        MarkdownInk.Emphasis => Styles.Emphasis ?? _emphasis,
        MarkdownInk.Code => Styles.Code ?? Theme.Info,
        MarkdownInk.Heading => Styles.Heading ?? Theme.Header,
        MarkdownInk.Subheading => Styles.Subheading ?? Theme.Accent,
        MarkdownInk.Marker => Styles.Marker ?? Theme.Active,
        MarkdownInk.Quote => Styles.Quote ?? Theme.Secondary,
        MarkdownInk.Link => Styles.Link ?? _link,
        MarkdownInk.Url => Styles.Url ?? Theme.Secondary,
        MarkdownInk.Rule => Styles.Rule ?? Theme.Info,
        MarkdownInk.TableHeader => Styles.TableHeader ?? Theme.TableHeader,
        _ => Theme.Default,
    };
}
