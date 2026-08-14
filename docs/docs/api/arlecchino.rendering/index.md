---
title: Arlecchino.Rendering
sidebar_label: Arlecchino.Rendering
sidebar_position: 0
---

# Arlecchino.Rendering

## Classes

| Type | Summary |
|---|---|
| [`PaneFlow`](PaneFlow.md) | A flow cursor inside one region: it writes the next line and remembers where the one after it goes, clipped to the region.  ```csharp var flow = region.Flow();  flow.AppendLine("PLAYERS", Theme.TableHeader);  foreach (var player in players) { flow.AppendLine(player.Name, Theme.Default); }  ``` |
| [`Surface`](Surface.md) | The drawing target: a grid of cells, each holding one symbol and one style, serialized into a single write per frame. Needs nothing but an [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md), so it works outside a hosted application too. |

## Structs

| Type | Summary |
|---|---|
| [`Margin`](Margin.md) | Blank space around something, measured in cells. Used both by the flow calls on [`Surface`](../arlecchino.rendering/Surface.md) and by [`SurfaceRegion.Inset`](../arlecchino.rendering/SurfaceRegion.md#inset-margin). |
| [`SurfaceRegion`](SurfaceRegion.md) | A rectangle on a [`SurfaceRegion.Surface`](../arlecchino.rendering/SurfaceRegion.md#surface) with its own coordinates and clipping, so writing outside it is dropped. The same geometry answers where a click landed. |

## Enums

| Type | Summary |
|---|---|
| [`Align`](Align.md) | Where text or a block sits inside the space it is drawn into. Horizontal and vertical flags combine, so `Align.Right \| Align.Bottom` anchors to a corner. |

