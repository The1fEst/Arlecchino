---
title: Arlecchino
sidebar_label: Arlecchino
sidebar_position: 0
---

# Arlecchino

## Classes

| Type | Summary |
|---|---|
| [`FrameThread`](FrameThread.md) | Which thread draws. Views, widgets, atoms and the surface are written without locks because one thread touches them, and this is what turns that from a convention into something the framework can check. The frame loop claims the thread it runs on, and everything that must happen there asks before it changes anything. Nothing claims it outside a running application — a headless host, a test, a single `DrawOnce` — so the checks stay quiet there and cost a null comparison. |
| [`InputRouter`](InputRouter.md) | Decides who gets a key or a mouse event. The order is what keeps the application predictable: an open dialog takes everything, then the palette key, then the view's own commands, then commands available everywhere, and only then the view itself. A handler that throws is reported on the output line rather than allowed to stop the loop. What a key means once a dialog has it is not decided here. This file is the order and nothing else, so that the order can be read at a sitting. |
| [`Repaint`](Repaint.md) | The "this frame is stale" signal the render loop waits on. Input, navigation, state changes and atom writes raise it for you; raise it yourself when something else changes what a view draws. |
| [`Screen`](Screen.md) | Draws the frames: the current view first — inside the [`IArlecchinoLayout`](../arlecchino.navigation/IArlecchinoLayout.md) when the application registered one — then the output line, the hints and any dialog on top. A view that throws while drawing is reported on the output line instead of taking the application down, since a half-drawn frame is easier to recover from than a dead process. |
| [`SystemTerminal`](SystemTerminal.md) | The real console. Registered by default and replaceable through `UseTerminal<T>()`. On Windows it turns on virtual terminal output at startup and turns off virtual terminal input, because that flag stops `Console.ReadKey` from delivering keys at all. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoTerminal`](IArlecchinoTerminal.md) | Everything the framework needs from a console. Replace it with `UseTerminal<T>()` to drive a test harness or a remote session; `SystemTerminal` is the real one. |

