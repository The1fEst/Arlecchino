using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Widgets.Lists;
using Arlecchino.Widgets.Text;
using Arlecchino.Modals.Choosing;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// The dialogs that are a list: pick one, pick several, or the palette of every key. Each is drawn as its own
/// box, with the rows reported back to the dialog for a click to land on.
/// </summary>
internal sealed class ListPaint
{
    private const int MostRows = 12;
    private const int FewestRows = 3;
    private const int Padding = 10;
    private const int LeastInner = 34;

    private readonly Surface _surface;
    private readonly ArlecchinoStrings _strings;
    private readonly ModalBox _box;

    /// <summary>Draws the lists.</summary>
    /// <param name="surface">The cell grid frames are built in.</param>
    /// <param name="strings">The words the application says things in.</param>
    /// <param name="box">Where a box goes and how wide it is.</param>
    public ListPaint(Surface surface, ArlecchinoStrings strings, ModalBox box)
    {
        _surface = surface;
        _strings = strings;
        _box = box;
    }

    /// <summary>Draws a list one thing is picked from.</summary>
    /// <param name="modal">The dialog.</param>
    public void One(ChoiceModal modal) =>
        Options(modal, modal.Title, _strings.ModalChoiceHints(), static option => option);

    /// <summary>Draws a list several things are picked from, each row saying whether it is one of them.</summary>
    /// <param name="modal">The dialog.</param>
    public void Several(MultiChoiceModal modal) =>
        Options(
            modal,
            $"{modal.Title} — {_strings.SelectedCount(modal.SelectedKeys.Count)}",
            _strings.ModalMultiChoiceHints(),
            option => $"[{(modal.IsSelected(option) ? '×' : ' ')}] {option}");

    /// <summary>Draws the palette: every key, and what it does.</summary>
    /// <param name="modal">The dialog.</param>
    public void Commands(CommandModal modal)
    {
        var keyWidth = 0;

        foreach (var (key, _) in modal.Commands)
        {
            keyWidth = Math.Max(keyWidth, TextWidth.Of(key));
        }

        var body = new List<Piece[]>();

        foreach (var (key, label) in modal.Commands)
        {
            body.Add([new($"{TextWidth.PadLeft(key, keyWidth)}  {label}", Theme.Default)]);
        }

        var (box, content) = _box.Draw(modal.Title, body, _strings.ModalCommandHints());

        modal.Box = box;
        modal.Rows = content.Rows(0, modal.Commands.Count);
    }

    private void Options(OptionListModal modal, string title, string hints, Func<string, string> formatRow)
    {
        var matching = modal.MatchingOptions();

        modal.Index = Math.Clamp(modal.Index, 0, Math.Max(0, matching.Count - 1));

        var maxRows = Math.Min(MostRows, Math.Max(FewestRows, _surface.FrameHeight - Padding));
        var visible = Math.Min(matching.Count, maxRows);
        var start = Math.Clamp(modal.Index - (maxRows / 2), 0, Math.Max(0, matching.Count - maxRows));

        var rows = new string[visible];
        var width = Math.Max(Math.Max(TextWidth.Of(title) + 4, LeastInner), TextWidth.Of(hints));

        for (var index = 0; index < visible; index++)
        {
            rows[index] = formatRow(matching[start + index]);
            width = Math.Max(width, TextWidth.Of(rows[index]));
        }

        var notice = matching.Count == 0 ? 1 : 0;
        var box = _box.Centered(width, visible + notice + 4);
        var content = box.Border(Theme.Info, title).Inset(new Margin(1, 0, 1, 0));

        modal.Box = box;
        modal.Rows = content.Rows(2 + notice, visible);
        modal.FirstVisible = start;

        Filter(modal, content);
        ModalBox.Divider(box, 1);

        if (notice == 1)
        {
            content.WriteLine(2, _strings.NothingMatches(), Theme.Secondary);
        }

        var scrolled = ScrollBar.IsNeeded(matching.Count, visible);

        Rows(modal, content, rows, start, notice, scrolled);

        if (scrolled)
        {
            ScrollBar.Draw(modal.Rows, start, matching.Count);
            Position(content, modal.Index + 1, matching.Count);
        }

        var footer = 2 + notice + visible;

        ModalBox.Divider(box, footer);
        content.WriteLine(footer + 1, hints, Theme.Secondary);
    }

    /// <summary>
    /// Draws what is narrowing the list, with the caret and whatever is selected in it. It is drawn the way
    /// a field is, because it is edited the way a field is.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="content">The region inside the box.</param>
    private void Filter(OptionListModal modal, SurfaceRegion content)
    {
        var label = _strings.Filter();

        content.Write(0, 0, label, Theme.Info);
        EntryRow.Draw(
            content,
            0,
            TextWidth.Of(label) + 1,
            Math.Max(0, content.Width - TextWidth.Of(label) - 1),
            modal,
            new(Theme.Info, Theme.Selection, Theme.Caret));
    }

    private static void Rows(
        OptionListModal modal,
        SurfaceRegion content,
        string[] rows,
        int start,
        int notice,
        bool scrolled)
    {
        var width = scrolled ? Math.Max(0, content.Width - 1) : content.Width;

        for (var index = 0; index < rows.Length; index++)
        {
            content.Write(
                2 + notice + index,
                0,
                TextWidth.PadRight(TextWidth.Truncate(rows[index], width), width),
                start + index == modal.Index ? Theme.Selection : Theme.Default);
        }
    }

    private void Position(SurfaceRegion content, int position, int count)
    {
        var text = _strings.ListPosition(position, count);
        var column = content.Width - TextWidth.Of(text);

        if (column > 0)
        {
            content.Write(0, column, text, Theme.Secondary);
        }
    }
}
