---
title: Notification
sidebar_label: Notification
---

# Notification class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

One thing the application said, and when it said it. A plain message needs no more than the three values it is built with; something still running fills in [`Notification.Progress`](../arlecchino.diagnostics/Notification.md#progress), and something worth reading in full fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions), which the notifications screen offers when the entry is opened.

```csharp
public sealed class Notification : IEquatable<Notification>
```

**Implements** `IEquatable<T>`&lt;[`Notification`](../arlecchino.diagnostics/Notification.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Notification(DateTimeOffset, NotificationLevel, string)`](#notification-datetimeoffset-notificationlevel-string) | One thing the application said, and when it said it. A plain message needs no more than the three values it is built with; something still running fills in [`Notification.Progress`](../arlecchino.diagnostics/Notification.md#progress), and something worth reading in full fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions), which the notifications screen offers when the entry is opened. |

## Properties

| Member | Summary |
|---|---|
| [`Actions`](#actions) | What can be done about it, offered when the entry is opened. Work that has ended clears these, since stopping something that is over is not an offer worth making. |
| [`Detail`](#detail) | The whole story, shown when the entry is opened: the errors a copy collected, the output of a command, the stack of what failed. Falls back to [`Notification.Text`](../arlecchino.diagnostics/Notification.md#text) when it is not set. |
| [`Ended`](#ended) | What came of the work, once it ended. Set through [`Notifications.Settle`](../arlecchino.diagnostics/Notifications.md#settle-notification-string-notificationlevel). |
| [`IsRunning`](#isrunning) | Whether the work this entry reports is still going. |
| [`Level`](#level) | How loud it is. |
| [`Line`](#line) | The single line to draw: what came of it, what is happening now, or what was said. |
| [`Loudness`](#loudness) | How loud it is now: what it was raised as, or what it turned out to be once the work it reports ended — a copy that starts as a plain message and finds three files locked says so. |
| [`Progress`](#progress) | The line to show while something is still running, read every frame. Left alone for anything that is already over, which is most notifications. |
| [`Share`](#share) | How far along the work is, from `0` to `1`, read every frame beside [`Notification.Progress`](../arlecchino.diagnostics/Notification.md#progress). A bar is drawn for it wherever the entry is shown; work whose size is not known answers `null` and gets the text alone. |
| [`Since`](#since) | When the entry last had something new to say — raised, or ended. Work that ran for an hour is not stale the moment it finishes, so the timeouts are counted from here rather than from [`Notification.Time`](../arlecchino.diagnostics/Notification.md#time). |
| [`Text`](#text) | What it says in one line. |
| [`Time`](#time) | When it was raised. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out DateTimeOffset, out NotificationLevel, out string)`](#deconstruct-out-datetimeoffset-out-notificationlevel-out-string) |  |
| [`Filled()`](#filled) | How full a bar for this entry should be, or `null` when there is nothing to draw. |
| [`Whole()`](#whole) | The full text to read: [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) when there is one, the line otherwise. |

## Constructors in detail

### `Notification(DateTimeOffset, NotificationLevel, string)` {#notification-datetimeoffset-notificationlevel-string}

```csharp
public Notification(DateTimeOffset Time, NotificationLevel Level, string Text);
```

One thing the application said, and when it said it. A plain message needs no more than the three values it is built with; something still running fills in [`Notification.Progress`](../arlecchino.diagnostics/Notification.md#progress), and something worth reading in full fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions), which the notifications screen offers when the entry is opened.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Time` | `DateTimeOffset` | When it was raised. |
| `Level` | [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md) | How loud it is. |
| `Text` | `string` | What it says in one line. |

## Properties in detail

### `Actions` {#actions}

```csharp
public IReadOnlyList<NotificationAction> Actions { get; set; }
```

What can be done about it, offered when the entry is opened. Work that has ended clears these, since stopping something that is over is not an offer worth making.

**Type** `IReadOnlyList<T>`&lt;[`NotificationAction`](../arlecchino.diagnostics/NotificationAction.md)&gt;

### `Detail` {#detail}

```csharp
public Func<string> Detail { get; set; }
```

The whole story, shown when the entry is opened: the errors a copy collected, the output of a command, the stack of what failed. Falls back to [`Notification.Text`](../arlecchino.diagnostics/Notification.md#text) when it is not set.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Ended` {#ended}

```csharp
public string Ended { get; }
```

What came of the work, once it ended. Set through [`Notifications.Settle`](../arlecchino.diagnostics/Notifications.md#settle-notification-string-notificationlevel).

**Type** `string`

### `IsRunning` {#isrunning}

```csharp
public bool IsRunning { get; }
```

Whether the work this entry reports is still going.

**Type** `bool`

### `Level` {#level}

```csharp
public NotificationLevel Level { get; init; }
```

How loud it is.

**Type** [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md)

### `Line` {#line}

```csharp
public string Line { get; }
```

The single line to draw: what came of it, what is happening now, or what was said.

**Type** `string`

### `Loudness` {#loudness}

```csharp
public NotificationLevel Loudness { get; }
```

How loud it is now: what it was raised as, or what it turned out to be once the work it reports ended — a copy that starts as a plain message and finds three files locked says so.

**Type** [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md)

### `Progress` {#progress}

```csharp
public Func<string> Progress { get; init; }
```

The line to show while something is still running, read every frame. Left alone for anything that is already over, which is most notifications.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Share` {#share}

```csharp
public Func<Nullable<double>> Share { get; init; }
```

How far along the work is, from `0` to `1`, read every frame beside [`Notification.Progress`](../arlecchino.diagnostics/Notification.md#progress). A bar is drawn for it wherever the entry is shown; work whose size is not known answers `null` and gets the text alone.

**Type** `Func<TResult>`&lt;`Nullable<T>`&lt;`double`&gt;&gt;

### `Since` {#since}

```csharp
public DateTimeOffset Since { get; }
```

When the entry last had something new to say — raised, or ended. Work that ran for an hour is not stale the moment it finishes, so the timeouts are counted from here rather than from [`Notification.Time`](../arlecchino.diagnostics/Notification.md#time).

**Type** `DateTimeOffset`

### `Text` {#text}

```csharp
public string Text { get; init; }
```

What it says in one line.

**Type** `string`

### `Time` {#time}

```csharp
public DateTimeOffset Time { get; init; }
```

When it was raised.

**Type** `DateTimeOffset`

## Methods in detail

### `Deconstruct(out DateTimeOffset, out NotificationLevel, out string)` {#deconstruct-out-datetimeoffset-out-notificationlevel-out-string}

```csharp
public void Deconstruct(out DateTimeOffset Time, out NotificationLevel Level, out string Text);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Time` | `DateTimeOffset` |  |
| `Level` | [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md) |  |
| `Text` | `string` |  |

### `Filled()` {#filled}

```csharp
public Nullable<double> Filled();
```

How full a bar for this entry should be, or `null` when there is nothing to draw.

**Returns** `Nullable<T>`&lt;`double`&gt; — A fraction between `0` and `1`.

### `Whole()` {#whole}

```csharp
public string Whole();
```

The full text to read: [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) when there is one, the line otherwise.

**Returns** `string` — What to show in the dialog.

