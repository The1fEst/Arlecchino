---
title: What's new in 2026.8.5
sidebar_label: What's new in 2026.8.5
description: The console is caught rather than left to scroll the frame away, a palette can be worked out against the background the terminal turned out to be, and a sweep of renames the compiler names for you.
---

# What's new in 2026.8.5

A release about names, about what a program says out loud, and about the color it says it in. The break
is the sweep of renames — [Migrating to 2026.8.5](migrating-to-2026.8.5.md) lists every one, and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202685) is the full record.

## The console is caught

A line written to standard output lands on the frame and scrolls it away, and the frame is drawn as the
difference from the last one, so nothing tells the drawing that the screen has moved: some runs came up
clean, some came up three lines out of place and stayed that way. The host's own
`Microsoft.Hosting.Lifetime` did it, and so did a stray `Console.WriteLine` from any library that never
knew it was in a terminal application.

`AddArlecchino` now stands in front of standard output and standard error. While a frame is on the
screen, what is written there is logged under `stdout` or `stderr` and shown in the
[log overlay](diagnostics.md#the-log-overlay) with escape sequences taken out of it. Before the terminal
is taken and after it is given back, the console works as it always did, so `--help`, a startup failure
and the shutdown lines still print.

One habit changes with it: `builder.Logging.ClearProviders()` used to be how a console provider was kept
off the frame, and is now how an application ends up with an empty overlay. Arlecchino registers no
logging provider of its own — the overlay draws what a provider writes to the console, and the default
host already has one.

## A palette for the terminal you landed on

A palette is written against a background nobody promised. `ArlecchinoOptions.PaletteForBackground` is
handed the color the terminal turned out to be and answers with the palette to wear:

```csharp
builder.Services.AddArlecchino(options =>
    options.PaletteForBackground = background => new ThemePalette
    {
        Secondary = new RgbTermColor { Foreground = Shade.Against(background, hue: 25, chroma: 0.02, contrast: 3) },
        Selection = new RgbTermColor { Background = Shade.Lifted(background, 0.06) },
    });
```

Behind it are `Oklch`, `Contrast` and `Shade`: the space where lightness, chroma and hue come apart, the
ratio the accessibility guidelines are written in, and the solver that finds the color reaching a wanted
contrast against a given background. Two of the numbers are worth knowing — the turn from a dark theme
to a light one is at luminance `0.179`, not at the half-way gray, and a background near the middle can
only reach about 5:1 in either direction, which is what `Shade.Scaled` brings a ladder down to. See
[Working a color out against the background](colors.md#working-a-color-out-against-the-background).

It runs once, as the application starts and the terminal is asked what color it is. A terminal that will
not say leaves `Theme` exactly as it was given.

## Names that say what a thing is

`Selected` for an index, `Muted` for a color, `Since` for a moment: a name that says what was done to a
thing rather than what it is costs a release to correct, so they were corrected together —
`SelectedIndex`, `Secondary`, `RaisedAt`, and a dozen more. Every one of them is a rename the compiler
names for you.

## Two smaller fixes

`Ctrl+Shift+C` copies on Windows instead of stopping the application: the console used to decide for
itself which of it and `Ctrl+C` had been pressed, and now the application is handed the key rather than
the verdict. And a copy no longer goes nowhere in a terminal with OSC 52 switched off — the text also
goes down the standard input of the first clipboard program the machine has. Both are on
[Escape sequences](ansi.md#what-goes-out).
