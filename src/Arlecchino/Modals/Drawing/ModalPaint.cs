using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

using Arlecchino.Modals.Asking;
using Arlecchino.Modals.Choosing;
using Arlecchino.Modals.Setting;
using Arlecchino.Modals.Telling;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// Draws whatever dialogs are open, in the order they were opened and each a little below and to the
/// right of the one under it. The application's own dialog is handed the whole screen and left to it;
/// everything the framework brings goes through the one box.
/// </summary>
internal sealed class ModalPaint
{
    private const int SmallestFieldColumns = 12;

    private readonly Surface _surface;
    private readonly ArlecchinoStrings _strings;
    private readonly ModalBox _box;
    private readonly FieldPaint _fields;
    private readonly ValuePaint _values;
    private readonly ListPaint _lists;

    /// <summary>Draws the dialogs.</summary>
    /// <param name="surface">The cell grid frames are built in.</param>
    /// <param name="strings">The words the application says things in.</param>
    public ModalPaint(Surface surface, ArlecchinoStrings strings)
    {
        _surface = surface;
        _strings = strings;
        _box = new(surface);
        _fields = new(surface, strings, _box);
        _values = new(strings, _box);
        _lists = new(surface, strings, _box);
    }

    /// <summary>Draws the whole stack.</summary>
    /// <param name="open">The dialogs, oldest first.</param>
    public void Draw(IReadOnlyList<Modal> open)
    {
        ArgumentNullException.ThrowIfNull(open);

        for (var depth = 0; depth < open.Count; depth++)
        {
            _box.Depth = depth;

            One(open[depth]);
        }

        _box.Depth = 0;
    }

    private void One(Modal? open)
    {
        switch (open)
        {
            case CustomModal modal:
                modal.Box = _surface.Content;
                modal.Draw(_surface.Content);
                return;
            case ChoiceModal modal:
                _lists.One(modal);
                return;
            case MultiChoiceModal modal:
                _lists.Several(modal);
                return;
            case CommandModal modal:
                _lists.Commands(modal);
                return;
            case NumberModal modal:
                _fields.Entry(modal, modal.Title, _strings.ModalNumberHints());
                return;
            case SliderModal modal:
                _values.Slider(modal);
                return;
            case ToggleModal modal:
                _values.Toggle(modal);
                return;
            case MessageModal modal:
                Message(modal);
                return;
            case NotificationModal modal:
                Notification(modal);
                return;
            case TextAreaModal modal:
                _fields.Area(modal);
                return;
            case DateModal modal:
                _values.Segmented(modal, _strings.ModalDateHints());
                return;
            case TimeModal modal:
                _values.Segmented(modal, _strings.ModalTimeHints());
                return;
            case ColorModal modal:
                _values.Color(modal);
                return;
            case TextModal modal:
                _fields.Entry(modal, modal.Title, _strings.ModalTextHints());
                return;
        }
    }

    private void Message(MessageModal modal)
    {
        var (box, _) = _box.Draw(modal.Title, Wrapped(modal.Text), _strings.ModalMessageHints());

        modal.Box = box;
    }

    /// <summary>
    /// A notification opened for reading: what it said, how far along it is when it reports that, and
    /// its actions as a row of chips. Where each chip was drawn is handed back, so that clicking one
    /// runs the same thing the arrows and Enter would.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    private void Notification(NotificationModal modal)
    {
        var body = Wrapped(modal.Text);

        if (modal.Entry.Filled() is { } share)
        {
            body.Add([new("", Theme.Default)]);
            body.Add(ValuePaint.Bar(share, Width()));
        }

        if (modal.Actions.Count == 0)
        {
            var (plain, _) = _box.Draw(modal.Title, body, _strings.ModalMessageHints());

            modal.Box = plain;
            modal.Chips = [];

            return;
        }

        var (pieces, offsets) = Chips(modal);

        body.Add([new("", Theme.Default)]);
        body.Add(pieces);

        var (box, inside) = _box.Draw(modal.Title, body, _strings.ModalNotificationHints());
        var row = inside.Rows(body.Count - 1, 1);
        var chips = new List<SurfaceRegion>(offsets.Count);

        foreach (var (left, width) in offsets)
        {
            chips.Add(row.Inset(new Margin(left, 0, Math.Max(0, row.Width - left - width), 0)));
        }

        modal.Box = box;
        modal.Chips = chips;
    }

    private static (Piece[] Pieces, List<(int Column, int Width)> Offsets) Chips(NotificationModal modal)
    {
        const string gap = "   ";

        var pieces = new List<Piece>();
        var offsets = new List<(int Column, int Width)>();
        var column = 0;

        for (var index = 0; index < modal.Actions.Count; index++)
        {
            if (index > 0)
            {
                pieces.Add(new(gap, Theme.Default));
                column += gap.Length;
            }

            var label = $" {modal.Actions[index].Label()} ";
            var width = TextWidth.Of(label);

            pieces.Add(new(label, index == modal.Index ? Theme.ActiveSelected : Theme.Muted));
            offsets.Add((column, width));

            column += width;
        }

        return ([.. pieces], offsets);
    }

    private List<Piece[]> Wrapped(string text)
    {
        var body = new List<Piece[]>();

        foreach (var line in TextWidth.Wrap(text, Width()))
        {
            body.Add([new(line, Theme.Default)]);
        }

        return body;
    }

    private int Width() => Math.Max(SmallestFieldColumns, _surface.FrameWidth / 2);
}
