---
title: Arlecchino.Modals.Telling
sidebar_label: Arlecchino.Modals.Telling
sidebar_position: 0
---

# Arlecchino.Modals.Telling

## Classes

| Type | Summary |
|---|---|
| [`MessageModal`](MessageModal.md) | Something the user only has to read: a result, a warning, an explanation of what just failed. It takes no input beyond the key that closes it, which is what separates it from every other dialog here. |
| [`NotificationModal`](NotificationModal.md) | One notification, read in full. The output row and the notifications screen have one line each to give a message, which is not enough for the errors a copy collected or the output of a command — opening the entry shows the whole of it, and offers whatever the entry said could be done about it. The notifications screen opens this itself, so an application only fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions) when it raises the entry. |

