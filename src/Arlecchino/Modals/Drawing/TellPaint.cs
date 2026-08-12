using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Modals.Telling;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// The dialogs that say something rather than ask it: a message to dismiss, and a notification opened in
/// full. Both wrap to half the screen, and a notification adds its bar and its row of chips.
/// </summary>
internal sealed class TellPaint
{
    private const int SmallestFieldColumns = 12;
    private const string Gap = "   ";

    private readonly Surface _surface;
    private readonly ArlecchinoStrings _strings;
    private readonly ModalBox _box;

    /// <summary>Draws them.</summary>
    /// <param name="surface">The cell grid frames are built in.</param>
    /// <param name="strings">The words the application says things in.</param>
    /// <param name="box">The box they are drawn in.</param>
    public TellPaint(Surface surface, ArlecchinoStrings strings, ModalBox box)
    {
        _surface = surface;
        _strings = strings;
        _box = box;
    }

    /// <summary>Draws something to read and dismiss.</summary>
    /// <param name="modal">The dialog.</param>
    public void Message(MessageModal modal)
    {
        var (box, _) = _box.Draw(modal.Title, Wrapped(modal.Text), _strings.ModalMessageHints());

        modal.Box = box;
    }

    /// <summary>
    /// A notification opened for reading: what it said, how far along it is, and its actions as a row of
    /// chips. Where each chip was drawn is handed back, so a click runs what the arrows would.
    /// </summary>
    /// <param name="modal">The dialog.</param>
    public void Notification(NotificationModal modal)
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
        var pieces = new List<Piece>();
        var offsets = new List<(int Column, int Width)>();
        var column = 0;

        for (var index = 0; index < modal.Actions.Count; index++)
        {
            if (index > 0)
            {
                pieces.Add(new(Gap, Theme.Default));
                column += Gap.Length;
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
