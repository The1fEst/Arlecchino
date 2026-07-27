---
title: Arlecchino.Testing
sidebar_label: Arlecchino.Testing
sidebar_position: 0
---

# Arlecchino.Testing

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoTestHost`](ArlecchinoTestHost.md) | A whole application wired up for a test: real services, a terminal in memory, and no loop running in the background. Frames are drawn when asked for rather than on a timer, so a test presses keys and then looks at what would be on screen, with nothing to wait for and nothing to race against. |
| [`FakeTerminal`](FakeTerminal.md) | A terminal that keeps everything in memory: keys are queued in, output is collected as text, and the size is whatever a test sets it to. Nothing is written anywhere, so tests can run side by side and assert on what would have been drawn. The input queues are concurrent, so a test can deliver keys late — the way a real terminal splits an escape sequence across two reads. |
| [`FrameText`](FrameText.md) | Pulls apart what was written to a terminal. A frame is text with escape sequences woven through it, which is unreadable in an assertion message, so these separate the content from the styling and let a test assert on either. |
| [`TestClock`](TestClock.md) | A clock a test moves by hand. Scheduled work runs when the clock passes its due time, so a test that would otherwise wait for a second of real time moves a second instead and sees the result on the next frame. |

