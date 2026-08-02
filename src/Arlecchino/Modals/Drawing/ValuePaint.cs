using System;
using System.Collections.Generic;
using Arlecchino.Hosting;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Rendering.Text;
using Arlecchino.Modals.Setting;

namespace Arlecchino.Modals.Drawing;

/// <summary>
/// The dialogs that show a value rather than ask for one in words: a slider, a yes-or-no, a date or a
/// time, a colour. Each says where its parts were drawn as it draws them, so that a click can be
/// answered without anyone having to work out where the track must have gone.
/// </summary>
internal sealed class ValuePaint
{
    private const int TrackCells = 24;
    private const int SwatchRows = 2;
    private const int ChipGap = 3;
    private const char Filled = '█';
    private const char Empty = '░';

    private readonly ArlecchinoStrings _strings;
    private readonly ModalBox _box;

    /// <summary>Draws them.</summary>
    /// <param name="strings">The words the application says things in.</param>
    /// <param name="box">The box they are drawn in.</param>
    public ValuePaint(ArlecchinoStrings strings, ModalBox box)
    {
        _strings = strings;
        _box = box;
    }

    /// <summary>Draws a slider.</summary>
    /// <param name="modal">The dialog.</param>
    public void Slider(SliderModal modal)
    {
        var filled = Math.Clamp((int)Math.Round(modal.Fraction * TrackCells), 0, TrackCells);

        List<Piece[]> body =
        [
            [
                new(Track(filled), Theme.Active),
                new($"  {modal.Display(modal.Value)}", Theme.Accent),
            ],
        ];

        var (box, inside) = _box.Draw(modal.Title, body, _strings.ModalSliderHints());

        modal.Box = box;
        modal.Track = inside.Rows(0, 1).Inset(new Margin(1, 0, inside.Width - TrackCells - 1, 0));
    }

    /// <summary>Draws a yes-or-no.</summary>
    /// <param name="modal">The dialog.</param>
    public void Toggle(ToggleModal modal)
    {
        var yes = $" {_strings.Yes()} ";
        var no = $" {_strings.No()} ";

        List<Piece[]> body =
        [
            [
                new(yes, modal.Value ? Theme.ActiveSelected : Theme.Muted),
                new(new(' ', ChipGap), Theme.Default),
                new(no, modal.Value ? Theme.Muted : Theme.ActiveSelected),
            ],
        ];

        var (box, inside) = _box.Draw(modal.Title, body, _strings.ModalToggleHints());
        var yesWidth = TextWidth.Of(yes);
        var noAt = yesWidth + ChipGap;

        modal.Box = box;
        modal.YesChip = inside.Rows(0, 1).Inset(new Margin(0, 0, inside.Width - yesWidth, 0));
        modal.NoChip = inside.Rows(0, 1)
            .Inset(new Margin(noAt, 0, Math.Max(0, inside.Width - noAt - TextWidth.Of(no)), 0));
    }

    /// <summary>Draws a date or a time, which are one dialog with different segments in it.</summary>
    /// <param name="modal">The dialog.</param>
    /// <param name="hints">What the keys do.</param>
    public void Segmented(SegmentedModal modal, string hints)
    {
        var texts = modal.EditedSegmentTexts();
        var pieces = new List<Piece>();

        for (var index = 0; index < texts.Length; index++)
        {
            if (index > 0)
            {
                pieces.Add(new(modal.Separator, Theme.Muted));
            }

            pieces.Add(new(texts[index], index == modal.Segment ? Theme.Input : Theme.Default));
        }

        _box.Draw(modal.Title, [[.. pieces]], hints);
    }

    /// <summary>Draws a colour: what it looks like, and the three sliders that make it.</summary>
    /// <param name="modal">The dialog.</param>
    public void Color(ColorModal modal)
    {
        string[] labels = [_strings.ColorHue(), _strings.ColorSaturation(), _strings.ColorLightness()];
        var labelWidth = 0;

        foreach (var label in labels)
        {
            labelWidth = Math.Max(labelWidth, TextWidth.Of(label));
        }

        List<Piece[]> body =
        [
            [
                new(new(' ', TrackCells + 2), new RgbTermColor { Background = modal.Value }),
                new($"  {modal.Value.Hex}", Theme.Accent),
            ],
            [],
        ];

        for (var channel = ColorChannel.Hue; channel <= ColorChannel.Lightness; channel++)
        {
            var value = modal.ValueOf(channel);
            var maximum = ColorModal.MaximumOf(channel);
            var lit = channel == modal.Channel;

            body.Add(
            [
                new(TextWidth.PadRight(labels[(int)channel], labelWidth + 2), lit ? Theme.Accent : Theme.Muted),
                new(Track(maximum > 0 ? value * TrackCells / maximum : 0), lit ? Theme.Active : Theme.Muted),
                new($"  {value,3}", lit ? Theme.Accent : Theme.Muted),
            ]);
        }

        var (box, inside) = _box.Draw(modal.Title, body, _strings.ModalColorHints());
        var trackStart = labelWidth + 3;

        modal.Box = box;
        modal.ChannelRows = new SurfaceRegion[labels.Length];
        modal.ChannelTracks = new SurfaceRegion[labels.Length];

        for (var channel = 0; channel < labels.Length; channel++)
        {
            var row = inside.Rows(SwatchRows + channel, 1);

            modal.ChannelRows[channel] = row;
            modal.ChannelTracks[channel] = row.Inset(
                new Margin(trackStart, 0, Math.Max(0, inside.Width - trackStart - TrackCells), 0));
        }
    }

    /// <summary>
    /// The bar of an entry that reports how far along it is, drawn as pieces rather than by the widget
    /// because a dialog is laid out line by line.
    /// </summary>
    /// <param name="share">How full it is, from <c>0</c> to <c>1</c>.</param>
    /// <param name="width">How wide the box is.</param>
    /// <returns>The pieces that make up the row.</returns>
    public static Piece[] Bar(double share, int width)
    {
        var readout = $" {share * 100:0}%";
        var track = Math.Max(1, width - TextWidth.Of(readout));
        var filled = (int)Math.Round(share * track);

        return
        [
            new(new(Filled, filled), Theme.Active),
            new(new(Empty, Math.Max(0, track - filled)), Theme.Muted),
            new(readout, Theme.Accent),
        ];
    }

    private static string Track(int filled) =>
        $"[{new string(Filled, filled)}{new string(Empty, TrackCells - filled)}]";
}
