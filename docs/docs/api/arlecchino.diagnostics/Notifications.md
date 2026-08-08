---
title: "Notifications"
sidebar_label: "Notifications"
---

# Notifications class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

What the application has to say, and for how long. The newest line sits on the output row until it times out, so a message does not hold the screen for the rest of the session. It stays in the list for much longer, so opening the notifications screen still shows what went past while the user was looking elsewhere. Both timeouts come from [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md), and both are counted by the [`Ticker`](../arlecchino.hosting/Ticker.md) — nothing here runs on its own thread.

```csharp
public sealed class Notifications
```

## Constructors

| Member | Summary |
|---|---|
| [`Notifications(ArlecchinoOptions, TimeProvider, Ticker)`](#notifications-arlecchinooptions-timeprovider-ticker) | Creates the list. |

## Properties

| Member | Summary |
|---|---|
| [`Capacity`](#capacity) | How many messages to keep at most, however young they are. A list bounded only by time grows without limit when something reports in a loop, so the oldest fall off once this many are held. |
| [`Current`](#current) | The line the output row shows, or `null` once it has timed out. The entry itself stays in [`Notifications.Entries`](../arlecchino.diagnostics/Notifications.md#entries) until the longer timeout takes it. |
| [`Entries`](#entries) | Everything still held, the newest first. |
| [`Recent`](#recent) | What is worth showing right now, the newest first: everything still running, and everything that ended recently enough not to have timed out yet. [`Notifications.Current`](../arlecchino.diagnostics/Notifications.md#current) answers the same question for one row at the bottom of the screen, which can only hold the newest. An application that shows its work as a stack of cards rather than a line wants all of them, and wants each one to stay up however long the work takes. That is why running work is here whatever its age. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Throws away everything that has been said, the output row included — except work that is still running, which keeps its line. A copy does not stop because its line was cleared, and a job running with nothing on screen to show for it is worse than a list that would not empty. |
| [`Notify(string, NotificationLevel)`](#notify-string-notificationlevel) | Says something. The newest line replaces whatever the output row was showing. |
| [`Raise(Notification)`](#raise-notification) | Says something that carries more than a line — work still running, a report to read in full, something to do about it. The entry is built by the caller, so it can be kept and taken back with [`Notifications.Withdraw`](../arlecchino.diagnostics/Notifications.md#withdraw-notification) once whatever it reports is over. |
| [`Settle(Notification, string, NotificationLevel)`](#settle-notification-string-notificationlevel) | Turns a line that was reporting work into what came of that work, in place. The entry keeps its spot in the list and its identity, so a dialog someone already has open changes under them rather than going stale, and the entry starts aging like any other message. |
| [`Withdraw(Notification)`](#withdraw-notification) | Takes one entry back, for work whose line should not be kept at all. |

## Constructors in detail

### `Notifications(ArlecchinoOptions, TimeProvider, Ticker)` {#notifications-arlecchinooptions-timeprovider-ticker}

```csharp
public Notifications(ArlecchinoOptions options, TimeProvider time, Ticker ticker);
```

Creates the list.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `options` | [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md) | Supplies both timeouts. |
| `time` | `TimeProvider` | Where the current time comes from. |
| `ticker` | [`Ticker`](../arlecchino.hosting/Ticker.md) | Counts the timeouts between frames. |

## Properties in detail

### `Capacity` {#capacity}

```csharp
public int Capacity { get; set; }
```

How many messages to keep at most, however young they are. A list bounded only by time grows without limit when something reports in a loop, so the oldest fall off once this many are held.

**Type** `int`

### `Current` {#current}

```csharp
public Notification? Current { get; }
```

The line the output row shows, or `null` once it has timed out. The entry itself stays in [`Notifications.Entries`](../arlecchino.diagnostics/Notifications.md#entries) until the longer timeout takes it.

**Type** [`Notification`](../arlecchino.diagnostics/Notification.md)

### `Entries` {#entries}

```csharp
public IReadOnlyList<Notification> Entries { get; }
```

Everything still held, the newest first.

**Type** `IReadOnlyList<T>`&lt;[`Notification`](../arlecchino.diagnostics/Notification.md)&gt;

### `Recent` {#recent}

```csharp
public IReadOnlyList<Notification> Recent { get; }
```

What is worth showing right now, the newest first: everything still running, and everything that ended recently enough not to have timed out yet. [`Notifications.Current`](../arlecchino.diagnostics/Notifications.md#current) answers the same question for one row at the bottom of the screen, which can only hold the newest. An application that shows its work as a stack of cards rather than a line wants all of them, and wants each one to stay up however long the work takes. That is why running work is here whatever its age.

**Type** `IReadOnlyList<T>`&lt;[`Notification`](../arlecchino.diagnostics/Notification.md)&gt;

## Methods in detail

### `Clear()` {#clear}

```csharp
public void Clear();
```

Throws away everything that has been said, the output row included — except work that is still running, which keeps its line. A copy does not stop because its line was cleared, and a job running with nothing on screen to show for it is worse than a list that would not empty.

### `Notify(string, NotificationLevel)` {#notify-string-notificationlevel}

```csharp
public void Notify(string text, NotificationLevel level = Information);
```

Says something. The newest line replaces whatever the output row was showing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to say; an empty string clears the row instead. |
| `level` | [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md) | How loud it is. |

### `Raise(Notification)` {#raise-notification}

```csharp
public Notification Raise(Notification entry);
```

Says something that carries more than a line — work still running, a report to read in full, something to do about it. The entry is built by the caller, so it can be kept and taken back with [`Notifications.Withdraw`](../arlecchino.diagnostics/Notifications.md#withdraw-notification) once whatever it reports is over.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`Notification`](../arlecchino.diagnostics/Notification.md) | The notification to raise. |

**Returns** [`Notification`](../arlecchino.diagnostics/Notification.md) — The same entry, so a caller can hold on to it in one expression.

### `Settle(Notification, string, NotificationLevel)` {#settle-notification-string-notificationlevel}

```csharp
public void Settle(Notification entry, string text, NotificationLevel level = Information);
```

Turns a line that was reporting work into what came of that work, in place. The entry keeps its spot in the list and its identity, so a dialog someone already has open changes under them rather than going stale, and the entry starts aging like any other message.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`Notification`](../arlecchino.diagnostics/Notification.md) | The entry that was reporting. |
| `text` | `string` | What came of the work. |
| `level` | [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md) | How loud that is. |

### `Withdraw(Notification)` {#withdraw-notification}

```csharp
public void Withdraw(Notification entry);
```

Takes one entry back, for work whose line should not be kept at all.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`Notification`](../arlecchino.diagnostics/Notification.md) | The entry to remove; one that is no longer held is ignored. |

