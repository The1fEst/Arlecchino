---
title: Arlecchino
sidebar_label: Arlecchino
sidebar_position: 0
---

# Arlecchino

## Classes

| Type | Summary |
|---|---|
| [`FrameThread`](FrameThread.md) | Which thread draws, claimed by the frame loop as it starts. Views, widgets, atoms and the surface are written without locks, and this is what turns that convention into something the framework checks. |
| [`InputRouter`](InputRouter.md) | Decides who gets a key or a mouse event, in order: an open dialog, the palette key, the view's commands, the commands available everywhere, then the view. A handler that throws is reported on the output line. |
| [`Repaint`](Repaint.md) | The "this frame is stale" signal the render loop waits on. Input, navigation, state changes and atom writes raise it for you; raise it yourself when something else changes what a view draws. |
| [`Screen`](Screen.md) | Draws the frames: the current view first, inside the [`IArlecchinoLayout`](../arlecchino.navigation/IArlecchinoLayout.md) when there is one, then the output line, the keys and any dialog on top. A view that throws is reported on the output line rather than taking the application down. |
| [`SystemTerminal`](SystemTerminal.md) | The real console, registered by default and replaceable through `UseTerminal<T>()`. On Windows it turns virtual terminal output on and virtual terminal input off at startup. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoTerminal`](IArlecchinoTerminal.md) | Everything the framework needs from a console. Replace it with `UseTerminal<T>()` to drive a test harness or a remote session; `SystemTerminal` is the real one. |

