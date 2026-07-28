---
title: What's new in 2.0
sidebar_label: What's new in 2.0
description: The three breaking changes 1.x announced, delivered together, plus what came with them.
---

# What's new in 2.0

The three breaking changes `1.x` announced, delivered together. Nothing here needs more than a rename,
a delete or a decision about colour — [Migrating to 2.0](migrating-to-2.0.md) is the edit list, and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#200) is the full record.

## Widgets draw and hand back what is left

`IArlecchinoWidget.Place` is now `Draw`, and the interface has one member:

```csharp
SurfaceRegion Draw(SurfaceRegion region);
```

It paints the widget and answers what is left of the region underneath, so a view stacks things
without counting rows:

```csharp
private readonly StatusBar _header;
private readonly Tabs _tabs;
private readonly ListBox<Mod> _list;
private readonly Surface surface;

var rest = _header.Draw(surface.Content);
var below = _tabs.Draw(rest);

_list.Draw(below);
```

The obsolete `void Draw` is gone, along with the `ARL0001` diagnostic id that existed to let its
deprecation be silenced on its own. See [Widgets](widgets.md).

## The framework's own colours are the default

`new ThemePalette()` is now crimson titles, bone text, ash borders and an ink cursor row rather than
the terminal's plain sixteen, so an application that never called `UseTheme` looks like Arlecchino.

`ThemePalette.Basic` is exactly the old defaults, and `UseTheme(ThemePalette.Basic)` is the whole of
the way back. `ThemePalette.Arlecchino` still exists and still means the same thing; it is only
redundant now. See [Theming](theming.md#the-frameworks-own-palette).

## The dispatcher is gone

`UiDispatcher` is removed. The queue it held moved into `FrameThread`, the type that already knew
which thread draws, so handing a result back from background work is one static call with nothing
injected:

```csharp
private IReadOnlyList<Mod> _rows = [];

FrameThread.Post(() => _rows = loaded);
```

`AsyncAtom<T>` and `ViewLifetime` no longer take a dispatcher either: `new AsyncAtom<T>(initial)` and
`new ViewLifetime()`.

Everything else about posting is unchanged: it is safe from any thread, runs in order just before the
next frame, asks for that frame by itself, and reports an action that threw without dropping the rest.
See [The frame loop](frame-loop.md#which-thread-draws).

## Added

- `FrameThread.DiscardPending()` drops work that was posted and can no longer run — which is what
  giving up the last claim on the drawing thread does by itself, and what
  [`ArlecchinoTestHost`](testing.md) does as it is disposed, so one test's leftovers never run inside
  the next.
- `ThemePalette.Basic`, the sixteen plain colours that were the default before this release.

## Fixed

Posting work while nothing is drawing no longer runs it inline. `FrameThread.Post` used to run the
action on the calling thread when no frame loop had claimed one, so an action that posted itself — the
ordinary way to say "again next frame" — recursed until the stack ended instead of queueing. It always
queues now.

## What came just before it

`1.3.0` is worth knowing about even coming from `1.2`, because it changed where input runs. The reader
thread used to route what it read there and then, so a key press changed the selection, the modal
stack, the route and any atom it touched *while* the drawing loop was reading the same things.

The reader now queues what it reads and the frame loop drains the queue at the top of each turn,
before the ticker and before drawing. Everything an application writes is touched by one thread, which
is what the documentation already claimed, and a key press costs at most one frame of latency — 16 ms
at the default rate. `FrameThread.Verify` is what turns that from a claim into a check.
