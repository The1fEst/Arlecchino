---
title: What's new in 2026.8.1
sidebar_label: What's new in 2026.8.1
description: A fourth package reads picture files, the terminal can be lent to an editor or a pager, an application can draw the keys itself, and a version now says when it was cut.
---

# What's new in 2026.8.1

The release that put pictures on the screen and let the terminal be lent out — and the one where the
numbering changed, so it is worth reading that part even if nothing else here applies.
[Migrating to 2026.8.1](migrating-to-2026.8.1.md) is what needs an edit, and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202681) is the full record.

## A version says when it was cut

Numbering is `year.month.build`: this was the first release of August 2026, and the number no longer
claims anything about what is safe to upgrade across. A break is written down in the changelog whichever
digit moved, so the entry is what to read rather than the digits. See
[Versioning](packages-and-building.md).

## A fourth package reads picture files

`Arlecchino.Pictures` turns PNG, JPEG, BMP, Netpbm, QOI and Targa into the pixels a
[picture](pictures.md) draws, with no dependency of its own:

```csharp
var raster = PictureFormats.Read(bytes, PictureLimits.For(region.Width * region.Height * 4));
```

The limits are the point of it. A header claiming more pixels than the ceiling is refused before
anything is allocated against it, and a format that can read itself smaller does — a photograph drawn
into a pane is decoded at a quarter or an eighth of its side rather than in full.

## The terminal can be lent to another program

An editor, a pager or a shell cannot share a screen with a frame loop. Now one is handed the terminal
outright and handed it back afterward, with the alternate screen, the mouse and bracketed paste left
and re-entered around it — see [Lending the terminal out](frame-loop.md#lending-the-terminal-out).

## An application can draw the keys itself

`ArlecchinoOptions.Hints` replaces `ShowHints` with three answers rather than two: the framework's box,
nothing at all, or a box of your own. A screen with a status bar of its own no longer has to choose
between the framework's corner and its own row.

Two smaller things went the same way. A binding can be a character rather than a key —
`new KeyBinding('!')` answers wherever that character can be typed, which is the only dependable way to
name punctuation across layouts. And the hints box follows the focus: a screen of unlike panes lists the
keys of the pane being used rather than the same list wherever the cursor is.

## Clicks and rings

A click now goes to the pane it landed in, worked out from the same tree that laid the panes out, so a
mouse event reaches the widget under it without a screen routing it by hand. And a `FocusRing` is
itself focusable, so one goes inside another: a ring of panes, each with a ring of fields, steps the way
a reader expects. See [Focus](focus.md).
