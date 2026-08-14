---
title: "Notification"
sidebar_label: "Notification"
---

# Notification class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

One thing the application said, and when it said it. Work still running fills in [`Notification.ProgressText`](../arlecchino.diagnostics/Notification.md#progresstext), and anything longer fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions).

```csharp
public sealed class Notification
```

## Constructors

| Member | Summary |
|---|---|
| [`Notification(DateTimeOffset, NotificationLevel, string)`](#notification-datetimeoffset-notificationlevel-string) | Raises an entry, which is as much as a plain message ever needs. |

## Properties

| Member | Summary |
|---|---|
| [`Actions`](#actions) | What can be done about it, offered when the entry is opened. Work that has ended clears these, since stopping something that is over is not an offer worth making. |
| [`Detail`](#detail) | The whole story, shown when the entry is opened: the errors a copy collected, the output of a command, the stack of what failed. Falls back to [`Notification.Text`](../arlecchino.diagnostics/Notification.md#text) when it is not set. |
| [`EndedText`](#endedtext) | What came of the work, once it ended. Set through [`Notifications.Settle`](../arlecchino.diagnostics/Notifications.md#settle-notification-string-notificationlevel). |
| [`IsRunning`](#isrunning) | Whether the work this entry reports is still going. |
| [`Level`](#level) | How loud it is now: what it was raised as, or what it turned out to be once the work it reports ended — a copy that starts as a plain message and finds three files locked says so. |
| [`Line`](#line) | The single line to draw: what came of it, what is happening now, or what was said. |
| [`Progress`](#progress) | How far along the work is, from `0` to `1`, read every frame beside [`Notification.ProgressText`](../arlecchino.diagnostics/Notification.md#progresstext). A bar is drawn for it wherever the entry is shown; work whose size is not known answers `null` and gets the text alone. |
| [`ProgressText`](#progresstext) | The line to show while something is still running, read every frame. Left alone for anything that is already over, which is most notifications. |
| [`Since`](#since) | When the entry last had something new to say — raised, or ended. Work that ran for an hour is not stale the moment it finishes, so the timeouts are counted from here rather than from the moment it was raised. |
| [`Text`](#text) | What it was raised saying, kept as it was written. [`Notification.Line`](../arlecchino.diagnostics/Notification.md#line) is the line to draw, which is this one only while nothing newer has been said. |

## Methods

| Member | Summary |
|---|---|
| [`Filled()`](#filled) | How full a bar for this entry should be, or `null` when there is nothing to draw. |
| [`Whole()`](#whole) | The full text to read: [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) when there is one, the line otherwise. |

## Constructors in detail

### `Notification(DateTimeOffset, NotificationLevel, string)` {#notification-datetimeoffset-notificationlevel-string}

```csharp
public Notification(DateTimeOffset since, NotificationLevel level, string text);
```

Raises an entry, which is as much as a plain message ever needs.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `since` | `DateTimeOffset` | When it was raised. |
| `level` | [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md) | How loud it is. |
| `text` | `string` | What it says in one line. |

## Properties in detail

### `Actions` {#actions}

```csharp
public IReadOnlyList<NotificationAction> Actions { get; set; }
```

What can be done about it, offered when the entry is opened. Work that has ended clears these, since stopping something that is over is not an offer worth making.

**Type** `IReadOnlyList<T>`&lt;[`NotificationAction`](../arlecchino.diagnostics/NotificationAction.md)&gt;

### `Detail` {#detail}

```csharp
public Func<string>? Detail { get; set; }
```

The whole story, shown when the entry is opened: the errors a copy collected, the output of a command, the stack of what failed. Falls back to [`Notification.Text`](../arlecchino.diagnostics/Notification.md#text) when it is not set.

**Type** `Func<TResult>`&lt;`string`&gt;

### `EndedText` {#endedtext}

```csharp
public string? EndedText { get; }
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
public NotificationLevel Level { get; }
```

How loud it is now: what it was raised as, or what it turned out to be once the work it reports ended — a copy that starts as a plain message and finds three files locked says so.

**Type** [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md)

### `Line` {#line}

```csharp
public string Line { get; }
```

The single line to draw: what came of it, what is happening now, or what was said.

**Type** `string`

### `Progress` {#progress}

```csharp
public Func<Nullable<double>> Progress { get; init; }
```

How far along the work is, from `0` to `1`, read every frame beside [`Notification.ProgressText`](../arlecchino.diagnostics/Notification.md#progresstext). A bar is drawn for it wherever the entry is shown; work whose size is not known answers `null` and gets the text alone.

**Type** `Func<TResult>`&lt;`Nullable<T>`&lt;`double`&gt;&gt;

### `ProgressText` {#progresstext}

```csharp
public Func<string>? ProgressText { get; init; }
```

The line to show while something is still running, read every frame. Left alone for anything that is already over, which is most notifications.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Since` {#since}

```csharp
public DateTimeOffset Since { get; }
```

When the entry last had something new to say — raised, or ended. Work that ran for an hour is not stale the moment it finishes, so the timeouts are counted from here rather than from the moment it was raised.

**Type** `DateTimeOffset`

### `Text` {#text}

```csharp
public string Text { get; }
```

What it was raised saying, kept as it was written. [`Notification.Line`](../arlecchino.diagnostics/Notification.md#line) is the line to draw, which is this one only while nothing newer has been said.

**Type** `string`

## Methods in detail

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

