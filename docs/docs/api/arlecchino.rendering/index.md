---
title: Arlecchino.Rendering
sidebar_label: Arlecchino.Rendering
sidebar_position: 0
---

# Arlecchino.Rendering

## Classes

| Type | Summary |
|---|---|
| [`PaneFlow`](PaneFlow.md) | A flow cursor inside one region: it writes the next line and remembers where the next one goes, so a pane filled from a loop does not have to count rows. [`Surface`](../arlecchino.rendering/Surface.md) has flow calls of its own, but they belong to the whole frame — reaching for `region.Surface.AppendLine(...)` inside a pane writes at the top of the screen and paints over borders and neighbours. This is the same idea, bounded by the region: everything is written in its coordinates, clipped to it, and once it is full the calls stop doing anything.  ```csharp var flow = region.Flow();  flow.AppendLine("PLAYERS", Theme.TableHeader);  foreach (var player in players) { flow.AppendLine(player.Name, Theme.Default); }  ```  It is a class, so passing it to a helper that writes a few more lines carries the cursor along. A second flow over the same region starts again at its first row. |
| [`Surface`](Surface.md) | The drawing target: a grid of cells, each holding one symbol and one style, serialized into a single write per frame. Needs nothing but an [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md), so it works outside a hosted application too. |

## Structs

| Type | Summary |
|---|---|
| [`Margin`](Margin.md) | Blank space around something, measured in cells. Used both by the flow calls on [`Surface`](../arlecchino.rendering/Surface.md) and by [`SurfaceRegion.Inset`](../arlecchino.rendering/SurfaceRegion.md#inset-margin). |
| [`SurfaceRegion`](SurfaceRegion.md) | A rectangle on a [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) with its own coordinates and its own clipping: writing outside it is dropped rather than spilled onto a neighbour. Split a frame into regions instead of counting columns by hand, and the same geometry answers "was this click inside". |

## Enums

| Type | Summary |
|---|---|
| [`Align`](Align.md) | Where text or a block sits inside the space it is drawn into. Horizontal and vertical flags combine, so `Align.Right \| Align.Bottom` anchors to a corner. |

