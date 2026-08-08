---
title: Arlecchino.Testing
sidebar_label: Arlecchino.Testing
sidebar_position: 0
---

# Arlecchino.Testing

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoTestHost`](ArlecchinoTestHost.md) | A whole application wired up for a test: real services, a terminal in memory, and no loop running in the background. Frames are drawn when asked for rather than on a timer, so a test presses keys and then looks at what the screen would be showing, with nothing to wait for and nothing to race against. |
| [`FakeTerminal`](FakeTerminal.md) | A terminal that keeps everything in memory: keys are queued in, output is collected as text, and the size is whatever a test sets it to. Nothing is written anywhere, so tests can run side by side and assert on what would have been drawn. The input queues are concurrent, so a test can deliver keys late — the way a real terminal splits an escape sequence across two reads. |
| [`FrameText`](FrameText.md) | Pulls apart what was written to a terminal. A frame is text with escape sequences woven through it, which is unreadable in an assertion message, so these separate the content from the styling and let a test assert on either. |
| [`ScreenGrid`](ScreenGrid.md) | A terminal screen as the terminal itself would hold it: a grid of cells that output is applied to rather than collected in. Where [`FrameText`](../arlecchino.testing/FrameText.md) strips escapes out of what was written, this obeys them — a cursor jump moves the cursor, a style sticks to the cells that follow, a wide symbol takes two columns. That difference is the point. Frames are written as the difference from the last one, so what reaches the terminal is a handful of jumps and runs which say nothing on their own about what the screen holds afterward. Applying them here answers that, and makes the invariant worth asserting: a screen built from diffs is the screen a whole repaint would have drawn. |
| [`SessionTape`](SessionTape.md) | A session written down: every event that goes in, how long the application waits for it, and where a frame is worth looking at. Playing a tape draws the same frames every time, because a screen here is a function of state, state only changes on an event, and the time comes from a provider rather than from the clock on the wall. What it is for is writing a test as the session it describes, rather than as a dozen calls with the assertions lost among them. What it is deliberately not for is recording a running application. The framework has a password modal and a paste step, so a tape captured from a real session would hold whatever the user typed into them. A file like that must not be something an application writes on their behalf.  ```csharp var frames = new SessionTape() .Type(":") .Shot() .Type("copy") .Wait(200) .Shot() .Play(host);  Assert.Contains("Copy files", frames[^1], StringComparison.Ordinal);  ```  A tape holds what the terminal reported rather than what it meant, so it replays the same whatever the keyboard layout, and it holds no application state at all — only what was done to it. [`SessionTape.Read`](../arlecchino.testing/SessionTape.md#read-string) takes back what [`SessionTape.ToString`](../arlecchino.testing/SessionTape.md#tostring) wrote, so a tape travels as a file. |
| [`TestClock`](TestClock.md) | A clock a test moves by hand. Scheduled work runs when the clock passes its due time, so a test that would otherwise wait for a second of real time moves a second instead and sees the result on the next frame. |

