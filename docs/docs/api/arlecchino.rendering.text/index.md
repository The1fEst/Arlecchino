---
title: Arlecchino.Rendering.Text
sidebar_label: Arlecchino.Rendering.Text
sidebar_position: 0
---

# Arlecchino.Rendering.Text

## Classes

| Type | Summary |
|---|---|
| [`Glyphs`](Glyphs.md) | The symbols in use, reachable from anywhere that draws — the same arrangement as [`Theme`](../arlecchino.rendering.colors/Theme.md), and for the same reason: a widget picks the look up rather than being told it. Assigned from `ArlecchinoOptions` when the container resolves them; set it directly when drawing without a host. It is process-wide and settable, so an application can offer the choice in its own settings and have every graph follow on the next frame. A frame reads all of it, so all of it is written on the drawing thread and asks for a frame by itself; hand the change over with [`FrameThread.Post`](../arlecchino/FrameThread.md#post-action) from anywhere else. |
| [`Joinery`](Joinery.md) | Lines that know about one another. Boxes and rules are recorded first and painted at the end, so where two of them meet the shared cell becomes the glyph that joins them — `┬`, `├`, `┼` — instead of one line drawn over the other. [`SurfaceRegion.Border`](../arlecchino.rendering/SurfaceRegion.md#border-iarlecchinocolor-string) draws a box that knows nothing of its neighbors, which is right for a box standing on its own and wrong for panes that touch: two of those side by side put two verticals where the eye expects one. Recording them here instead costs one object per frame and gives the drawing of a window manager.  ```csharp var joinery = new Joinery();  var files = joinery.Box(left, Theme.Info, "files"); var log = joinery.Box(right, Theme.Active, "log");  joinery.Draw(surface.Content, Theme.Info);  ```  A cell takes the style of the last thing recorded over it, so the pane that holds the focus is recorded last and its edges win where they are shared. |
| [`TextWidth`](TextWidth.md) | Measures text the way a terminal shows it: in columns, not in `char` values. CJK and emoji take two columns, combining marks take none, and a surrogate pair is one symbol. Use these instead of `string.Length`, `PadRight` and slicing whenever the result lands on screen. |

## Enums

| Type | Summary |
|---|---|
| [`GraphSymbols`](GraphSymbols.md) | Which characters a graph is drawn with. The choice is about the font the terminal was given rather than about taste: the denser the symbols, the more of them a font has to carry. |

