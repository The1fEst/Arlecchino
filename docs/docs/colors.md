---
title: Colors
sidebar_label: Colors
description: TermColor, RgbTermColor and Rgb, and what TerminalCapabilities decides a terminal can actually show.
---

# Colors

[Theming](theming.md) is about the twelve roles a view draws with. This page is about what a role is
made of, and what happens when the terminal cannot show it.

## IArlecchinoColor

The contract is a single member:

```csharp
public interface IArlecchinoColor
{
    string Ansi { get; }
}
```

A style is anything that can produce an escape sequence. Two implementations ship.

## TermColor

```csharp
public sealed class TermColor : IArlecchinoColor
{
    public TerminalColor Foreground { get; init; } = TerminalColor.Default;
    public TerminalColor Background { get; init; } = TerminalColor.Default;
    public Rgb? ExactForeground { get; init; }
    public Rgb? ExactBackground { get; init; }
    public TextStyle Style { get; init; } = TextStyle.None;
}
```

The two `Exact` colors are drawn where the terminal can do 24-bit, and the palette colors beside
them are what a terminal without it gets. Setting both is how a palette says a brand color and still
degrades to something its author chose:

```csharp
Header = new TermColor
{
    Foreground = TerminalColor.BrightRed,
    ExactForeground = new Rgb(0xC9, 0x38, 0x2B),
    Style = TextStyle.Bold,
};
```

`TerminalColor` is the sixteen-color ANSI set — `Default`, the eight base colors, and their
`Bright` counterparts. `TextStyle` is described on [Text and width](text.md#styles).

## RgbTermColor

Where an exact color is the point — a swatch, a chart, a syntax highlighter — reach for the other
implementation:

```csharp
private readonly Surface _surface;

_surface.WriteAt(row, column, "████", new RgbTermColor { Foreground = new Rgb(63, 169, 245) });
```

`Foreground` and `Background` are both optional, so a swatch is a background with no foreground. On a
terminal without 24-bit it falls back to the nearest of the sixteen colors.

Keep chrome on `Theme`. Palette roles stay on the sixteen-color set because they have to look right
on a terminal the application does not control.

## Rgb

`Rgb` is a `(Red, Green, Blue)` record struct — what the [color modal](modals.md#color) edits and
hands back.

| Member | Meaning |
|---|---|
| `Hex` | `#RRGGBB` |
| `TryParseHex(text, out rgb)` | Parses `#RRGGBB` and `RRGGBB` |
| `FromHsl(hue, saturation, lightness)` | Builds from HSL, which is what a color picker moves along |
| `ToHsl()` | The other direction |

## Working a color out against the background

A color is readable or not against the background it lands on, and a terminal's background is whatever
its user picked. `Oklch`, `Contrast` and `Shade` are the arithmetic for that, and they are what
[`PaletteForBackground`](theming.md#a-palette-for-the-terminal-you-landed-on) is written with.

`Oklch` is the space where lightness, chroma and hue come apart, so a color can be made lighter without
becoming a different color:

| Member | Meaning |
|---|---|
| `Oklch.Of(rgb)` | The color in `(Lightness, Chroma, Hue)`, lightness and chroma from 0, hue in degrees |
| `ToRgb()` | Back again, trimmed to what sRGB holds |
| `FitsScreen` | Whether sRGB holds it at all, before trimming moves it |
| `Trimmed()` | The same lightness and hue with the chroma cut back to what sRGB holds |

`Contrast` is the ratio the accessibility guidelines are written in:

| Member | Meaning |
|---|---|
| `Contrast.Between(one, other)` | The ratio, from 1 to 21 |
| `Contrast.Luminance(color)` | The luminance those ratios are built on |
| `Contrast.IsDark(background)` | Whether a background wants light text |
| `Contrast.Pivot` | Where that turn is: luminance `0.179`, not the half-way gray, that being where white and black are equally far off |
| `Contrast.Reach(background)` | The most any color can reach against it |

`Shade` solves for the color rather than checking one:

```csharp
var background = TerminalCapabilities.Background ?? new Rgb(0x14, 0x13, 0x17);
var warning = Shade.Against(background, hue: 70, chroma: 0.14, contrast: 5.5);
```

| Member | Meaning |
|---|---|
| `Shade.Against(background, hue, chroma, contrast)` | The color at that hue which reaches the contrast, chroma cut only where sRGB cannot hold it |
| `Shade.Against(background, sample, contrast)` | The same, taking the hue and chroma off a color you already drew |
| `Shade.Lifted(background, step)` | A background a step away from this one, for panels and borders |
| `Shade.Scaled(contrast, lowest, highest, background)` | A wanted contrast brought down to the room a background actually has |
| `Shade.Pull(background)` | How much a background's own color should sway a design drawn for a gray one |
| `Shade.Turn(background, anchor, offset)` | How far to turn a design's hues so they sit at an angle from the background rather than across from it |

Two of these are worth knowing before you reach for them. A background near the middle can only reach
about 5:1 in either direction, so a ladder of contrasts written for a near-black terminal flattens on
it — `Scaled` brings the whole ladder down instead of clamping three of its steps onto white. And
`Pull` is nothing under a chroma of 0.06 and everything over 0.16, which is not a guess: every terminal
theme in wide use falls under the first number, while a background somebody picked a color for starts
around the second. A design is therefore left exactly as drawn on every theme anyone actually runs.

## Caching

Both implementations cache their escape sequence, and the frame writer compares styles **by
reference**. Hold on to a style instance rather than building one per cell:

```csharp
private static readonly RgbTermColor Swatch = new() { Background = new Rgb(63, 169, 245) };
```

## What the terminal can actually do

`TerminalCapabilities.Color` decides how styles are emitted, and is detected once at startup:

| Level | When | Effect |
|---|---|---|
| `TrueColor` | `COLORTERM` is `truecolor` or `24bit`, or `WT_SESSION` is set | `RgbTermColor` and the `Exact` colors emit 24-bit sequences |
| `Palette` | anything else | Both fall back to the nearest of the sixteen colors |
| `None` | `NO_COLOR` is set, `TERM=dumb`, or the Windows console refused virtual terminal mode | no style sequence is emitted at all, including the per-line reset |

Set it yourself to override the guess:

```csharp
TerminalCapabilities.Color = ColorSupport.Palette;
```

`TerminalCapabilities.NearestPaletteColor(rgb)` is the same conversion the fallback uses, available
for your own rendering.

:::note[Windows]

`SystemTerminal` turns on `ENABLE_VIRTUAL_TERMINAL_PROCESSING` when it starts. If the console
refuses — an old `conhost`, say — color drops to `None` and the alternate-screen sequences are not
written either, so the application degrades to plain text instead of spraying escape codes across it.

:::

## Testing what came out

`NO_COLOR=1` is the fastest way to see whether a screen still reads without color, and the
[test host](testing.md) asserts on frames with the color level pinned so a run on a developer's
machine and a run on CI produce the same text.
