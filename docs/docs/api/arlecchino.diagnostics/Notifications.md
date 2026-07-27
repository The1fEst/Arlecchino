---
title: Notifications
sidebar_label: Notifications
---

# Notifications class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

What the application has to say, and for how long. The newest line sits on the output row until it times out, so a message does not stay on screen for the rest of the session; it stays in the list for much longer, so opening the notifications screen still shows what went past while the user was looking elsewhere. Both timeouts come from [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md), and both are counted by the [`Ticker`](../arlecchino.hosting/Ticker.md) — nothing here runs on its own thread.

```csharp
public sealed class Notifications
```

## Constructors

| Member | Summary |
|---|---|
| [`Notifications(ArlecchinoOptions, TimeProvider, Ticker, Repaint)`](#notifications-arlecchinooptions-timeprovider-ticker-repaint) | Creates the list. |

## Properties

| Member | Summary |
|---|---|
| [`Capacity`](#capacity) | How many messages to keep at most, however young they are. A list bounded only by time grows without limit when something reports in a loop, so the oldest fall off once this many are held. |
| [`Current`](#current) | The line the output row shows, or `null` once it has timed out. The entry itself stays in [`Notifications.Entries`](../arlecchino.diagnostics/Notifications.md#entries) until the longer timeout takes it. |
| [`Entries`](#entries) | Everything still held, newest first. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Throws everything away, the output row included. |
| [`Notify(string, NotificationLevel)`](#notify-string-notificationlevel) | Says something. The newest line replaces whatever the output row was showing. |

## Constructors in detail

### `Notifications(ArlecchinoOptions, TimeProvider, Ticker, Repaint)` {#notifications-arlecchinooptions-timeprovider-ticker-repaint}

```csharp
public Notifications(ArlecchinoOptions options, TimeProvider time, Ticker ticker, Repaint repaint);
```

Creates the list.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `options` | [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md) | Supplies both timeouts. |
| `time` | `TimeProvider` | Where the current time comes from. |
| `ticker` | [`Ticker`](../arlecchino.hosting/Ticker.md) | Counts the timeouts between frames. |
| `repaint` | [`Repaint`](../arlecchino/Repaint.md) | Asked for a frame whenever something arrives or expires. |

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

Everything still held, newest first.

**Type** `IReadOnlyList<T>`&lt;[`Notification`](../arlecchino.diagnostics/Notification.md)&gt;

## Methods in detail

### `Clear()` {#clear}

```csharp
public void Clear();
```

Throws everything away, the output row included.

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

