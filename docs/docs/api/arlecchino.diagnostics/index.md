---
title: Arlecchino.Diagnostics
sidebar_label: Arlecchino.Diagnostics
sidebar_position: 0
---

# Arlecchino.Diagnostics

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoReport`](ArlecchinoReport.md) | What the application looks like right now, as text worth attaching to a bug report: the version, the platform, what the terminal said it can do, the screen being shown and the modals above it. Resolve it from the container and call [`ArlecchinoReport.Describe`](../arlecchino.diagnostics/ArlecchinoReport.md#describe) — a command that copies the result to the clipboard costs three lines and makes a report from a user useful. |
| [`LogBuffer`](LogBuffer.md) | The last few log lines, held in memory. A terminal application cannot write logs to the console — they would land in the middle of the frame — so they are collected here instead and shown in an overlay on request. Oldest lines are dropped once the buffer is full. Logging happens on whatever thread did the work, so the lines live in a concurrent queue and the overlay draws from a snapshot rather than from the live collection. Dropping the oldest is done under a lock: the check and the removal have to be one step, or two threads trimming at once take the buffer below its capacity. |
| [`LogEntry`](LogEntry.md) | One line of log, kept for the overlay. |
| [`Notification`](Notification.md) | One thing the application said, and when it said it. A plain message needs no more than the three values it is built with; something still running fills in [`Notification.Progress`](../arlecchino.diagnostics/Notification.md#progress), and something worth reading in full fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions), which the notifications screen offers when the entry is opened. |
| [`NotificationAction`](NotificationAction.md) | Something the user can do about a notification, offered when the entry is opened: stop the copy that is running, retry what failed, go to what it is about. |
| [`Notifications`](Notifications.md) | What the application has to say, and for how long. The newest line sits on the output row until it times out, so a message does not stay on screen for the rest of the session; it stays in the list for much longer, so opening the notifications screen still shows what went past while the user was looking elsewhere. Both timeouts come from [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md), and both are counted by the [`Ticker`](../arlecchino.hosting/Ticker.md) — nothing here runs on its own thread. |

## Enums

| Type | Summary |
|---|---|
| [`NotificationLevel`](NotificationLevel.md) | How loud a notification is, which decides how it is coloured. |

