---
title: Arlecchino.Testing
sidebar_label: Arlecchino.Testing
sidebar_position: 0
---

# Arlecchino.Testing

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoTestHost`](ArlecchinoTestHost.md) | A whole application wired up for a test: real services, a terminal in memory, and no loop in the background. Frames are drawn when asked for, so a test presses keys and then reads the screen. |
| [`FakeTerminal`](FakeTerminal.md) | A terminal that keeps everything in memory: keys queued in, output collected as text, and the size a test sets. The input queues are concurrent, so keys can be delivered late as a real terminal delivers them. |
| [`FrameText`](FrameText.md) | Pulls apart what was written to a terminal. A frame is text with escape sequences woven through it, which is unreadable in an assertion message, so these separate the content from the styling and let a test assert on either. |
| [`ScreenGrid`](ScreenGrid.md) | A terminal screen as the terminal itself would hold it: a grid of cells that output is applied to rather than collected in, obeying the escapes [`FrameText`](../arlecchino.testing/FrameText.md) strips out. |
| [`SessionTape`](SessionTape.md) | A session written down: the events that go in, the waits between them, and where a frame is worth looking at. A tape is written by hand rather than recorded.  ```csharp var frames = new SessionTape() .Type(":") .Shot() .Type("copy") .Wait(200) .Shot() .Play(host);  Assert.Contains("Copy files", frames[^1], StringComparison.Ordinal);  ``` |
| [`TestClock`](TestClock.md) | A clock a test moves by hand. Scheduled work runs when the clock passes its due time, so a second is moved rather than waited for. |

