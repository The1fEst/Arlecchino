---
title: Arlecchino.Diagnostics
sidebar_label: Arlecchino.Diagnostics
sidebar_position: 0
---

# Arlecchino.Diagnostics

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoReport`](ArlecchinoReport.md) | What the application looks like right now, as text for a bug report: the version, the platform, what the terminal can do, and the screen with the modals above it. Resolve it and call [`ArlecchinoReport.Describe`](../arlecchino.diagnostics/ArlecchinoReport.md#describe). |
| [`LogBuffer`](LogBuffer.md) | The last few log lines, held in memory for the overlay to draw, since a terminal application cannot write them to the console. Logging happens on any thread, so the oldest are dropped under a lock. |
| [`LogEntry`](LogEntry.md) | One line of log, kept for the overlay. |
| [`Notification`](Notification.md) | One thing the application said, and when it said it. Work still running fills in [`Notification.ProgressText`](../arlecchino.diagnostics/Notification.md#progresstext), and anything longer fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions). |
| [`NotificationAction`](NotificationAction.md) | Something the user can do about a notification, offered when the entry is opened: stop the copy that is running, retry what failed, go to what it is about. |
| [`Notifications`](Notifications.md) | What the application has to say, and for how long. The newest line sits on the output row until it times out and stays in the list much longer, on two timeouts the [`Ticker`](../arlecchino.hosting/Ticker.md) counts. |

## Enums

| Type | Summary |
|---|---|
| [`NotificationLevel`](NotificationLevel.md) | How loud a notification is, which decides how it is colored. |

