---
title: Arlecchino.Rendering
sidebar_label: Arlecchino.Rendering
sidebar_position: 0
---

# Arlecchino.Rendering

## Classes

| Type | Summary |
|---|---|
| [`Glyphs`](Glyphs.md) | The symbols in use, reachable from anywhere that draws — the same arrangement as [`Theme`](../arlecchino.rendering/Theme.md), and for the same reason: a widget picks the look up rather than being told it. Assigned from `ArlecchinoOptions` when the container resolves them; set it directly when drawing without a host. It is process-wide and settable, so an application can offer the choice in its own settings and have every graph follow on the next frame. A frame reads all of it, so all of it is written on the drawing thread and asks for a frame by itself; hand the change over with [`FrameThread.Post`](../arlecchino/FrameThread.md#post-action) from anywhere else. |
| [`Joinery`](Joinery.md) | Lines that know about one another. Boxes and rules are recorded first and painted at the end, so where two of them meet the shared cell becomes the glyph that joins them — `┬`, `├`, `┼` — instead of one line drawn over the other. [`SurfaceRegion.Border`](../arlecchino.rendering/SurfaceRegion.md#border-iarlecchinocolor-string) draws a box that knows nothing of its neighbours, which is right for a box standing on its own and wrong for panes that touch: two of those side by side put two verticals where the eye expects one. Recording them here instead costs one object per frame and gives the drawing of a window manager.  ```csharp var joinery = new Joinery();  var files = joinery.Box(left, Theme.Info, "files"); var log = joinery.Box(right, Theme.Active, "log");  joinery.Draw(surface.Content, Theme.Info);  ```  A cell takes the style of the last thing recorded over it, so the pane that holds the focus is recorded last and its edges win where they are shared. |
| [`PaneFlow`](PaneFlow.md) | A flow cursor inside one region: it writes the next line and remembers where the next one goes, so a pane filled from a loop does not have to count rows. [`Surface`](../arlecchino.rendering/Surface.md) has flow calls of its own, but they belong to the whole frame — reaching for `region.Surface.AppendLine(...)` inside a pane writes at the top of the screen and paints over borders and neighbours. This is the same idea, bounded by the region: everything is written in its coordinates, clipped to it, and once it is full the calls stop doing anything.  ```csharp var flow = region.Flow();  flow.AppendLine("PLAYERS", Theme.TableHeader);  foreach (var player in players) { flow.AppendLine(player.Name, Theme.Default); }  ```  It is a class, so passing it to a helper that writes a few more lines carries the cursor along. A second flow over the same region starts again at its first row. |
| [`RgbTermColor`](RgbTermColor.md) | A style built from exact colours. Use it where the colour itself is the point — a swatch, a chart, syntax highlighting — and keep chrome on [`Theme`](../arlecchino.rendering/Theme.md), which follows the terminal theme. Falls back to the nearest palette colour when the terminal cannot do 24-bit. |
| [`Surface`](Surface.md) | The drawing target: a grid of cells, each holding one symbol and one style, serialized into a single write per frame. Needs nothing but an [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md), so it works outside a hosted application too. |
| [`TermColor`](TermColor.md) | A style built from the sixteen-colour palette. This is what the roles on [`Theme`](../arlecchino.rendering/Theme.md) are made of and what chrome should use, because those colours follow the terminal's own theme. |
| [`TerminalCapabilities`](TerminalCapabilities.md) | What the terminal can actually show. Detected once at startup and consulted by every style when it builds its escape sequence; assign [`TerminalCapabilities.Color`](../arlecchino.rendering/TerminalCapabilities.md#color) to override the guess. |
| [`TerminalProbe`](TerminalProbe.md) | Asks the terminal what it can do, once, before the application starts reading keys. Everything here rests on one arrangement: the questions go out in an order that ends with the one every terminal answers — primary device attributes — so the reply to it is the signal that no other reply is coming. Without that fence there is nothing to wait for but a guess at how long a terminal takes to stay silent. The fence stops the waiting, not the reading: once it has arrived, whatever is already buffered is taken too, and only a lull or a keystroke ends it. A terminal that answers out of the order it was asked would otherwise have its last answer cut off, and nothing in any specification says it must answer in order. A terminal that answers nothing costs the deadline and leaves every setting as it was, which is the behaviour an application already had to live with. Nothing a person typed is swallowed either, and the rule for that is the fence again rather than the shape of what arrives: whatever was read is handed straight back unless the fence came, because only then is it certain that what arrived was answers. Judging it by shape does not work. On Windows the console layer eats the kitty query's reply and leaves the last character of it behind, so the first thing a terminal says can be a lone backslash — and treating that as something a person typed threw away every answer behind it. |
| [`TextWidth`](TextWidth.md) | Measures text the way a terminal shows it: in columns, not in `char` values. CJK and emoji take two columns, combining marks take none, and a surrogate pair is one symbol. Use these instead of `string.Length`, `PadRight` and slicing whenever the result lands on screen. |
| [`Theme`](Theme.md) | The palette in use, reachable from anywhere that draws. Views pick a role here rather than a colour, so swapping [`Theme.Palette`](../arlecchino.rendering/Theme.md#palette) restyles the whole application, chrome included. |
| [`ThemePalette`](ThemePalette.md) | The colours behind the roles in [`Theme`](../arlecchino.rendering/Theme.md). Every role has a default, so a palette that overrides two of them is a valid palette — and what it does not override is the framework's own colours, described on [`ThemePalette.Arlecchino`](../arlecchino.rendering/ThemePalette.md#arlecchino). |

## Structs

| Type | Summary |
|---|---|
| [`Margin`](Margin.md) | Blank space around something, measured in cells. Used both by the flow calls on [`Surface`](../arlecchino.rendering/Surface.md) and by [`SurfaceRegion.Inset`](../arlecchino.rendering/SurfaceRegion.md#inset-margin). |
| [`Rgb`](Rgb.md) | A 24-bit colour. Shown exactly only where the terminal supports true colour; otherwise it is mapped to the nearest palette entry — see [`TerminalCapabilities`](../arlecchino.rendering/TerminalCapabilities.md). |
| [`SurfaceRegion`](SurfaceRegion.md) | A rectangle on a [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) with its own coordinates and its own clipping: writing outside it is dropped rather than spilled onto a neighbour. Split a frame into regions instead of counting columns by hand, and the same geometry answers "was this click inside". |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoColor`](IArlecchinoColor.md) | Anything that can style a cell. The frame writer only ever asks for [`IArlecchinoColor.Ansi`](../arlecchino.rendering/IArlecchinoColor.md#ansi) and compares styles by reference, so hold on to instances instead of building one per cell. |

## Enums

| Type | Summary |
|---|---|
| [`Align`](Align.md) | Where text or a block sits inside the space it is drawn into. Horizontal and vertical flags combine, so `Align.Right \| Align.Bottom` anchors to a corner. |
| [`ColorSupport`](ColorSupport.md) | How much colour the terminal can show. Detected once at startup by [`TerminalCapabilities.DetectColor`](../arlecchino.rendering/TerminalCapabilities.md#detectcolor) and used by every style when it builds its escape sequence. |
| [`GraphSymbols`](GraphSymbols.md) | Which characters a graph is drawn with. The choice is about the font the terminal was given rather than about taste: the denser the symbols, the more of them a font has to carry. |
| [`ImageProtocol`](ImageProtocol.md) | How a picture reaches the terminal. Like [`GraphSymbols`](../arlecchino.rendering/GraphSymbols.md), this is a question of what the terminal can do rather than of taste. |
| [`TerminalColor`](TerminalColor.md) | The sixteen ANSI colours plus the terminal's own default. Exact shades belong to the terminal theme, which is why chrome should pick a role from [`Theme`](../arlecchino.rendering/Theme.md) rather than a colour here. |
| [`TextStyle`](TextStyle.md) | Text attributes a style carries on top of its colours. Combine them with `\|`; a terminal that does not support one simply ignores it. |

