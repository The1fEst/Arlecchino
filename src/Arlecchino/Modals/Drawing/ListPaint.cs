using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Widgets.Lists;
using Arlecchino.Modals.Choosing;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// The dialogs that are a list: pick one, pick several, or the palette of every key.
///
/// A list is drawn as its own box rather than through the shared one, because the row of what has
/// been typed to narrow it sits above a rule of its own — and because the rows have to be reported
/// back to the dialog so that a click can land on one.
/// </summary>
internal sealed class ListPaint
{
    private const int MostRows = 12;
    private const int FewestRows = 3;
    private const int RoomAround = 10;
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
            $"{modal.Title} — {_strings.SelectedCount(modal.Selected.Count)}",
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

        var (box, inside) = _box.Draw(modal.Title, body, _strings.ModalCommandHints());

        modal.Box = box;
        modal.Rows = inside.Rows(0, modal.Commands.Count);
    }

    private void Options(OptionListModal modal, string title, string hints, Func<string, string> formatRow)
    {
        var matching = modal.MatchingOptions();

        modal.Index = Math.Clamp(modal.Index, 0, Math.Max(0, matching.Count - 1));

        var maxRows = Math.Min(MostRows, Math.Max(FewestRows, _surface.FrameHeight - RoomAround));
        var visible = Math.Min(matching.Count, maxRows);
        var start = Math.Clamp(modal.Index - (maxRows / 2), 0, Math.Max(0, matching.Count - maxRows));

        var rows = new string[visible];
        var content = Math.Max(Math.Max(TextWidth.Of(title) + 4, LeastInner), TextWidth.Of(hints));

        for (var index = 0; index < visible; index++)
        {
            rows[index] = formatRow(matching[start + index]);
            content = Math.Max(content, TextWidth.Of(rows[index]));
        }

        var notice = matching.Count == 0 ? 1 : 0;
        var box = _box.Centered(content, visible + notice + 4);
        var inside = box.Border(Theme.Info, title).Inset(new Margin(1, 0, 1, 0));

        modal.Box = box;
        modal.Rows = inside.Rows(2 + notice, visible);
        modal.FirstVisible = start;

        inside.WriteLine(0, _strings.Filter(modal.Filter), Theme.Info);
        ModalBox.Divider(box, 1);

        if (notice == 1)
        {
            inside.WriteLine(2, _strings.NothingMatches(), Theme.Muted);
        }

        var scrolled = ScrollBar.IsNeeded(matching.Count, visible);

        Rows(modal, inside, rows, start, notice, scrolled);

        if (scrolled)
        {
            ScrollBar.Draw(modal.Rows, start, matching.Count);
            Position(inside, modal.Index + 1, matching.Count);
        }

        var footer = 2 + notice + visible;

        ModalBox.Divider(box, footer);
        inside.WriteLine(footer + 1, hints, Theme.Muted);
    }

    private static void Rows(
        OptionListModal modal,
        SurfaceRegion inside,
        string[] rows,
        int start,
        int notice,
        bool scrolled)
    {
        var width = scrolled ? Math.Max(0, inside.Width - 1) : inside.Width;

        for (var index = 0; index < rows.Length; index++)
        {
            inside.Write(
                2 + notice + index,
                0,
                TextWidth.PadRight(TextWidth.Truncate(rows[index], width), width),
                start + index == modal.Index ? Theme.Selected : Theme.Default);
        }
    }

    private void Position(SurfaceRegion inside, int position, int count)
    {
        var text = _strings.ListPosition(position, count);
        var column = inside.Width - TextWidth.Of(text);

        if (column > 0)
        {
            inside.Write(0, column, text, Theme.Muted);
        }
    }
}
