---
title: What's new in 3.0
sidebar_label: What's new in 3.0
description: Pictures, a terminal that is asked what it can do, panes that share a line, and every global write checked against the drawing thread.
---

# What's new in 3.0

A release about what the terminal can be talked into. Most of it is new surface that an application
gets without editing a line — [Migrating to 3.0](migrating-to-3.0.md) is the short list of what does
need an edit, and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#300) is the full record.

## Pictures

`Picture` draws an image. By default it needs nothing of the terminal beyond the colour it already
draws in: a cell carries two pixels, the upper half block painted as the one above and its background
as the one below.

```csharp
private readonly Picture _preview = new();

_preview.Show(pixels, width, height);
_preview.Draw(region);
```

Where the terminal speaks a graphics protocol, the pixels themselves go out instead — the kitty
graphics protocol on kitty, WezTerm and Ghostty, and sixel on Windows Terminal, xterm and foot. The
default is `Auto`: whichever the terminal admitted to, and cells when it admitted to nothing, so a
picture appears either way.

Underneath is `Surface.Passthrough`, for what the cell grid cannot express. It takes what removes the
payload as well as the payload, so a picture whose view left the screen is undrawn rather than left
behind — kitty deletes by number, sixel paints over, cells need nothing. See
[Pictures](pictures.md).

## The terminal is asked what it can do

Once, before the first frame, and it decides more than pictures: which graphics protocols the
terminal speaks, how many pixels a cell is, and what colour is behind the text.

The arrangement that makes it safe is the order. The questions end with the one every terminal
answers — primary device attributes — so the reply to it is the signal that no other reply is coming.
Without that fence there is nothing to wait for but a guess at how long a terminal takes to stay
silent. A terminal that answers nothing costs `TerminalAnswer`, 120 ms by default, and leaves every
setting as it was. Nothing a person typed is swallowed either: whatever was read is handed straight
back unless the fence came.

```csharp
options.AskTerminal = false;   // decide yourself instead
```

## Panes that touch share a line

A `PaneTree` with `Gaps(inner: 0)` used to put `╮╭` where the eye expects `┬`, because each titled
pane drew its own box. The tree now records its boxes in a `Joinery` and paints them together, and
panes that touch are pulled onto one another's edge so the line is shared. Nothing to change — a
layout that already asked for no gap simply looks right.

## The look and the state are checked

Everything process-wide that a frame reads is now written on the drawing thread and says so when it
is not. `Theme.Palette`, `Glyphs.Graph`, `Glyphs.Picture` and the cell size join `ArlecchinoState`,
its modals and the notification list: each asks which thread it is on and each asks for a frame by
itself.

The practical effect is that a background task that changes the theme or opens a dialog now fails
loudly instead of tearing a frame in half. Hand the change over the way an atom write is handed over:

```csharp
FrameThread.Post(() => Theme.Palette = ThemePalette.Basic);
```

The dialog stack became a `LocalAtomsList<Modal>` at the same time, so what watches it is told when it
changes rather than after the fact.

## Any language can be typed without asking

`TextInputMode.Native` is the default, so a layout that is not Latin works out of the box. What was
`UseLatinOnlyInput()` is `UseKeysByPosition()`, and it now does without exception what it used to do
only sometimes — the position of a key decides, whatever the layout makes of it. See
[Keyboard](keyboard.md).

## A test written as the session it describes

`SessionTape` in `Arlecchino.Testing` records the events that go in, how long the application waits
for them, and where a frame is worth looking at:

```csharp
var frames = new SessionTape()
    .Type(":")
    .Shot()
    .Type("copy")
    .Wait(200)
    .Shot()
    .Play(host);
```

Playing it draws the same frames every time, because a screen is a function of state, state changes
only on an event, and the clock comes from a provider. See [Testing](testing.md).

## Added

| What | Where |
|---|---|
| `Picture`, `ImageProtocol` | [Pictures](pictures.md) |
| `Surface.Passthrough(row, column, payload, undraw)` | [Rendering](rendering.md) |
| `TerminalProbe`, `TerminalCapabilities` | [Pictures](pictures.md#what-the-terminal-was-asked) |
| `Joinery` | [Layout](layout.md) |
| `SessionTape` | [Testing](testing.md#sessiontape) |
| `IArlecchinoTerminal.Unread(key)` | [API](api/arlecchino/IArlecchinoTerminal.md) |
| `CellWidth`, `CellHeight`, `ImageProtocol`, `AskTerminal`, `TerminalAnswer` options | [Hosting and options](hosting-and-options.md) |

## Fixed

- A picture could vanish from a frame that was written whole — writing every cell is what removes the
  pixels over it in some terminals, so every payload goes out again when the frame does.
- A picture drawn in cells was written out again every frame, however still it was: it built its
  colours afresh and the frame diff tells cells apart by reference.
- Undrawing a sixel could paint up to five rows below it, because bands are six rows whatever the
  picture's height.
- The probe assumed answers came back in the order they were asked for; nothing in any specification
  says they must.
- A console read that failed reached views as a key press of NUL, which no one presses.

## What came just before it

`2.13.0` added `AreaChart` — a series drawn as a filled area over as many rows as it is given, which
is the shape a system monitor shows and the one thing `Sparkline` cannot be. See
[Charts](charts.md).
