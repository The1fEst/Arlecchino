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
