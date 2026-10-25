---
title: What's new in 2026.8.4
sidebar_label: What's new in 2026.8.4
description: One fix to what 2026.8.3 added — the drawing thread's synchronization context is in force only while posted work runs.
---

# What's new in 2026.8.4

Nothing new, one fix, and nothing to migrate: take it if you took `2026.8.3`. The
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202684) is the full record.

## A context that no longer outlives its work

`2026.8.3` installed the drawing thread's synchronization context for as long as the thread was claimed.
That is right for an application, whose frame loop keeps running, and wrong for anything drawing frames
on the thread it is already on — a test, a headless host. There, an `await` in code that was never
posted tried to come back through the frame queue, and nothing was drawing frames to run it, so the wait
never ended.

The context is now in force only while posted work runs, which is where `FrameThread.Post(Func<Task>)`
needs it. A wait anywhere else is left exactly as it was. If a test of yours hung against `2026.8.3`,
this is why, and it needs no change of your own.
