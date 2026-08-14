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
| [`NotificationModal`](NotificationModal.md) | One notification, read in full, with whatever the entry said could be done about it. The notifications screen opens it, so an application only fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and its actions. |

