![Arlecchino](https://raw.githubusercontent.com/The1fEst/Arlecchino/master/assets/arlecchino-banner.png)

[![NuGet](https://img.shields.io/nuget/v/Arlecchino.Core?logo=nuget&label=Arlecchino.Core&color=C9382B&labelColor=141317)](https://www.nuget.org/packages/Arlecchino.Core)
[![Downloads](https://img.shields.io/nuget/dt/Arlecchino.Core?color=C9382B&labelColor=141317)](https://www.nuget.org/packages/Arlecchino.Core)
[![Build](https://img.shields.io/github/actions/workflow/status/The1fEst/Arlecchino/build.yml?branch=master&logo=github&labelColor=141317)](https://github.com/The1fEst/Arlecchino/actions/workflows/build.yml)
![Target frameworks](https://img.shields.io/badge/net8.0%20%7C%20net10.0-512BD4?logo=dotnet&logoColor=white&labelColor=141317)
[![MIT](https://img.shields.io/badge/license-MIT-EDE6D9?labelColor=141317)](https://github.com/The1fEst/Arlecchino/blob/master/LICENSE)

The cell grid the [Arlecchino](https://www.nuget.org/packages/Arlecchino) terminal UI framework draws
on, packaged on its own: a double-buffered surface, ANSI styling, flow and canvas layout, text
measured in grapheme clusters, and the atoms that hold state with an undo history. No dependency
injection, no hosting, no views — those are the `Arlecchino` package, and it brings this one with it.

**Writing an application? Install [`Arlecchino`](https://www.nuget.org/packages/Arlecchino)
instead.** This package is for code that wants the renderer and nothing else: a tool that draws one
screen, a widget library, a host of its own.

## Quick start

```
dotnet add package Arlecchino.Core
```

A `Surface` composes a frame in memory and sends it to a terminal:

```csharp
using Arlecchino;
using Arlecchino.Rendering;

var surface = new Surface(new SystemTerminal());

surface.StartFrame();
surface.AppendLine("hello", Theme.Header, Align.Center);
surface.Build();
```

`StartFrame` reads the terminal size and clears every cell; nothing reaches the terminal until
`Build`, which writes only what changed since the last frame — an idle frame writes nothing at all.
`SetFixedSize` pins the size instead of asking the terminal, which is what makes headless rendering
possible.

Anything larger than a stack of lines is drawn into a region:

```csharp
var (left, right) = surface.Content.SplitLeft(surface.FrameWidth / 2);

var pane = left.Border(Theme.Accent, "files").Flow();
pane.AppendLine("README.md", Theme.Default);
pane.AppendLine("CHANGELOG.md", Theme.Muted);

right.Fill(Theme.Default);
```

A `SurfaceRegion` splits, insets, borders and clips; `Flow()` turns one into a cursor that appends
lines and knows how many it has left. The region is a struct over the same grid — carving a layout up
allocates nothing.

## What is in it

- **`Surface`, `SurfaceRegion`, `PaneFlow`** — the double-buffered grid, region arithmetic and the
  flow cursor, plus `WriteTableRow` and `ListWindow` for lists that scroll.
- **`Theme`, `ThemePalette`, `TermColor`, `Rgb`** — one look per process, either of the two built-in
  palettes or one of your own; colours are written once and degrade to what the terminal has.
- **`TerminalCapabilities`** — truecolor, 256 colours or none, detected from `NO_COLOR`, `TERM` and
  `COLORTERM`, and overridable.
- **`TextWidth`** — width, truncation, padding and wrapping measured in grapheme clusters, so
  emoji, combining marks and East Asian characters do not tear the grid.
- **Atoms** — `Atom<T>`, `AtomsList<T>` and `AtomsMap<TKey, TValue>` in a tracked and a local
  flavour, `Computed<T>` that follows what it reads, and `AtomHistory` with `Undo`, `Redo` and
  `Group`. Writing an atom notifies whatever reads it and marks the frame stale.
- **`IArlecchinoTerminal` and `SystemTerminal`** — the seam the surface writes through, and the
  implementation over `System.Console`. A terminal of your own is fourteen members.
- **`FrameThread`** — the single thread every write is checked against, so background work that
  touches state is caught rather than tolerated.

Every type is trimming- and Native AOT-compatible, and the framework's own build publishes a native
binary each run and asks it for a frame.

## Packages

| Package | Contents |
|---|---|
| [`Arlecchino.Core`](https://www.nuget.org/packages/Arlecchino.Core) | this one — the renderer, no DI, and atoms with their undo history |
| [`Arlecchino`](https://www.nuget.org/packages/Arlecchino) | views, navigation, modals, commands, hosting, DI, async stores, and the generator |
| [`Arlecchino.Testing`](https://www.nuget.org/packages/Arlecchino.Testing) | `ArlecchinoTestHost` — the headless host applications write their tests against |

The three ship together and always carry the same version.

## Links

[Documentation](https://the1fest.github.io/Arlecchino.Docs/) ·
[Rendering](https://the1fest.github.io/Arlecchino.Docs/docs/rendering) ·
[Changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md) ·
[Source and issues](https://github.com/The1fEst/Arlecchino)

MIT.
