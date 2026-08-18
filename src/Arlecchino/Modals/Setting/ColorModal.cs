using System;
using Arlecchino.Rendering;
using Arlecchino.Rendering.Colors;
using Arlecchino.Input;

namespace Arlecchino.Modals.Setting;

/// <summary>One of the three sliders in the color dialog.</summary>
public enum ColorChannel : byte
{
    /// <summary>Position on the color wheel, in degrees.</summary>
    Hue,

    /// <summary>How far the color is from gray, in percent.</summary>
    Saturation,

    /// <summary>How far the color is from black or white, in percent.</summary>
    Lightness,
}

/// <summary>
/// A color picked on three sliders: hue, saturation and lightness, converted to <see cref="Rgb"/> on the way
/// out. Both directions round to whole units, so feeding a color back in can shift it by one.
/// </summary>
public sealed class ColorModal : Modal
{
    private const int HueDegrees = 360;
    private const int PercentMaximum = 100;

    /// <summary>Position on the color wheel, from <c>0</c> to <c>359</c>. It wraps rather than stopping.</summary>
    public int Hue { get; set; }

    /// <summary>Distance from gray, in percent.</summary>
    public int Saturation { get; set; } = PercentMaximum;

    /// <summary>Distance from black toward white, in percent. Fifty is the pure color.</summary>
    public int Lightness { get; set; } = 50;

    /// <summary>Which of the three sliders the arrows move.</summary>
    public ColorChannel Channel { get; set; }

    /// <summary>How far the arrow keys move the active slider.</summary>
    public int Step { get; init; } = 1;

    /// <summary>How far the page keys move the active slider.</summary>
    public int LargeStep { get; init; } = 10;

    /// <summary>Called with the color that was confirmed.</summary>
    public required Action<Rgb> OnPicked { get; init; }

    /// <summary>The three sliders resolved into a color, as drawn in the swatch.</summary>
    public Rgb Value => Rgb.FromHsl(Hue, Saturation, Lightness);

    /// <summary>Where each slider's row was drawn last frame, used to turn a click into a channel.</summary>
    public SurfaceRegion[] ChannelRows { get; set; } = [];

    /// <summary>Where each slider's track was drawn last frame, used to turn a click into a value.</summary>
    public SurfaceRegion[] ChannelTracks { get; set; } = [];

    /// <summary>Places a slider's handle at a position along its track.</summary>
    /// <param name="channel">The slider to move.</param>
    /// <param name="fraction">Position from <c>0</c> at the left end to <c>1</c> at the right; anything outside is pulled in.</param>
    public void SetChannelFromFraction(ColorChannel channel, decimal fraction)
    {
        var value = (int)Math.Round(Math.Clamp(fraction, 0m, 1m) * MaximumOf(channel));

        switch (channel)
        {
            case ColorChannel.Hue:
                Hue = value;
                return;
            case ColorChannel.Saturation:
                Saturation = value;
                return;
            default:
                Lightness = value;
                return;
        }
    }

    /// <summary>Value of the slider the arrows move.</summary>
    public int ChannelValue => Channel switch
    {
        ColorChannel.Hue => Hue,
        ColorChannel.Saturation => Saturation,
        _ => Lightness,
    };

    /// <summary>Upper end of the slider the arrows move.</summary>
    public int ChannelMaximum => Channel == ColorChannel.Hue ? HueDegrees - 1 : PercentMaximum;

    /// <summary>Loads a color into the three sliders, which is how an existing value is edited.</summary>
    /// <param name="color">The color to start from.</param>
    public void SetValue(Rgb color)
    {
        var (hue, saturation, lightness) = color.ToHsl();
        Hue = hue;
        Saturation = saturation;
        Lightness = lightness;
    }

    /// <summary>Moves between the sliders, stopping at the first and the last.</summary>
    /// <param name="delta">How far to move; negative goes up.</param>
    public void MoveChannel(int delta)
    {
        Channel = (ColorChannel)Math.Clamp((int)Channel + delta, 0, (int)ColorChannel.Lightness);
    }

    /// <summary>Moves the active slider. Hue wraps around the wheel; the other two halt at their ends.</summary>
    /// <param name="delta">How far to move; negative goes left.</param>
    public void Add(int delta)
    {
        switch (Channel)
        {
            case ColorChannel.Hue:
                Hue = (Hue + delta % HueDegrees + HueDegrees) % HueDegrees;
                return;
            case ColorChannel.Saturation:
                Saturation = Math.Clamp(Saturation + delta, 0, PercentMaximum);
                return;
            default:
                Lightness = Math.Clamp(Lightness + delta, 0, PercentMaximum);
                return;
        }
    }

    /// <summary>Jumps the active slider to its left end.</summary>
    public void MoveToMinimum() => SetChannelValue(0);

    /// <summary>Jumps the active slider to its right end.</summary>
    public void MoveToMaximum() => SetChannelValue(ChannelMaximum);

    /// <summary>Upper end of a slider, since hue counts degrees and the others count percent.</summary>
    /// <param name="channel">The slider to ask about.</param>
    /// <returns>The largest value it accepts.</returns>
    public static int MaximumOf(ColorChannel channel) => channel == ColorChannel.Hue ? HueDegrees - 1 : PercentMaximum;

    /// <summary>Value of a slider, for drawing all three at once.</summary>
    /// <param name="channel">The slider to read.</param>
    /// <returns>Its current value.</returns>
    public int ValueOf(ColorChannel channel) => channel switch
    {
        ColorChannel.Hue => Hue,
        ColorChannel.Saturation => Saturation,
        _ => Lightness,
    };

    private void SetChannelValue(int value)
    {
        switch (Channel)
        {
            case ColorChannel.Hue:
                Hue = value;
                return;
            case ColorChannel.Saturation:
                Saturation = value;
                return;
            default:
                Lightness = value;
                return;
        }
    }

    /// <inheritdoc/>
    public override void Draw(ModalFrame frame) => frame.Values.Color(this);

    /// <inheritdoc/>
    public override void Handle(ModalFrame frame, KeyPress key) => frame.Steps.Color(this, key);

    /// <inheritdoc/>
    public override void HandleMouse(ModalFrame frame, MouseEvent mouse)
    {
        if (mouse.Action is not (MouseAction.Pressed or MouseAction.Moved) || mouse.Button != MouseButton.Left)
        {
            return;
        }

        for (var row = 0; row < ChannelRows.Length; row++)
        {
            if (!ChannelRows[row].Contains(mouse.Row, mouse.Column))
            {
                continue;
            }

            var channel = (ColorChannel)row;

            Channel = channel;

            Tracking.Follow(ChannelTracks[row], mouse, fraction => SetChannelFromFraction(channel, fraction));

            return;
        }
    }
}
