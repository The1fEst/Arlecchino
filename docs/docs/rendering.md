---
title: Rendering
sidebar_label: Rendering
description: Surface — the cell grid every frame is composed in, what a frame costs, and which terminals have actually run it.
---

# Rendering

`Surface` is the drawing target: a cell grid of one symbol plane and one style plane, serialized into
a single write per frame. It lives in `Arlecchino.Core` and needs nothing but an
[`IArlecchinoTerminal`](api/arlecchino/IArlecchinoTerminal.md), so it can be used on its own, outside
the hosted application.

Everything a view draws goes through it. [Layout](layout.md) is the API for placing things,
[Text and width](text.md) is how it measures them, and [Theming](theming.md) is where the styles come
from. When a frame happens and which thread composes it is [The frame loop](frame-loop.md).

## What a frame costs

```csharp
_surface.StartFrame();
// … drawing …
_surface.Build();
```

`StartFrame` reads the terminal size, reallocates the planes if it changed, clears every cell to a
space styled `Theme.Default`, and skips `VerticalPadding` rows.

`Build` walks the grid, emits an ANSI sequence only where the style changes, and hands the whole frame
to `IArlecchinoTerminal.Write` as one string. It also compares the composed frame against the previous
one and writes only what changed, jumping the cursor to each changed run — an idle frame writes
nothing at all.

Four things fall back to sending the whole screen: the first frame, a resize, a fixed size, and
`ForgetPreviousFrame()`.

Styles are compared by reference, and both style implementations cache their escape sequence, so hold
on to a style instance rather than building one per cell.

## Geometry

| Member | Meaning |
|---|---|
| `FrameWidth` / `FrameHeight` | Size of the current frame in cells |
| `HorizontalPadding` / `VerticalPadding` | Gutters applied by the flow calls; set from `ArlecchinoOptions` |
| `Frame` | The whole frame as a [region](layout.md#regions) |
| `Content` | The frame minus the configured padding |
| `ListWindow()` | How many rows a scrolling list may use: the free lines minus room for the chrome, never fewer than four |
| `SetFixedSize(width, height)` | Pins the frame size and stops the surface asking the terminal |

## Rendering without a terminal

`SetFixedSize` is what makes headless rendering work: pin a size, resolve `Screen`, call `DrawOnce()`,
and the frame goes to stdout as plain ANSI text. That is how the samples render a single screen:

```bash
dotnet run --project samples/Arlecchino.Sample -- --frame picker 130x30
```

A fixed-size surface always sends whole frames, so the output is a complete screen rather than a
difference against something that was never there. [Hosting and options](hosting-and-options.md#running-without-the-hosted-service)
has the wiring, and [Testing](testing.md) has the version that asserts on it.

## Where this has been run

A terminal UI is only as good as the terminals it was tried in, so here is what has actually executed
the escape sequences rather than a test double, and what came out:

| Where | What it showed |
|---|---|
| Windows Terminal, Windows 11 | Day-to-day use — this is where the framework is developed |
| Arch on WSL2, `TERM=xterm-256color` | Alternate screen, SGR mouse reporting and bracketed paste all requested; colour stays inside the sixteen ANSI entries |
| The same with `COLORTERM=truecolor` | 24-bit sequences (`48;2;…`) where a screen actually asks for an exact colour |
| The same with `NO_COLOR=1` or `TERM=dumb` | Not one colour sequence emitted, and the frame still drawn — the notice, the layout and the box drawing are all intact |
| tmux, 100×30 | Frames, keys (`F1` opens the keys screen, `End` scrolls it) and the alternate screen all survive the multiplexer |
| macOS 26 on arm64, over ssh | The same sequences and a 120×34 frame; the only place the framework has run on Arm |
| Ubuntu and Windows on CI | The suite on both target frameworks, and a natively compiled binary that has to draw a frame |

What has **not** been tried, in case one of them is your terminal: the old Windows console host
without virtual terminal support (the path that drops colour entirely exists and is tested, but no
real conhost has run it), Terminal.app, PuTTY, and kitty, alacritty or WezTerm. Mouse reporting has
not been exercised inside a multiplexer either. If something misbehaves there,
[an issue](https://github.com/The1fEst/Arlecchino/issues) with the terminal and `TERM` is useful.
