---
title: Notification
sidebar_label: Notification
---

# Notification class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

One thing the application said, and when it said it.

```csharp
public sealed class Notification : IEquatable<Notification>
```

**Implements** `IEquatable<T>`&lt;[`Notification`](../arlecchino.diagnostics/Notification.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Notification(DateTimeOffset, NotificationLevel, string)`](#notification-datetimeoffset-notificationlevel-string) | One thing the application said, and when it said it. |

## Properties

| Member | Summary |
|---|---|
| [`Level`](#level) | How loud it is. |
| [`Text`](#text) | What it says. |
| [`Time`](#time) | When it was raised. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out DateTimeOffset, out NotificationLevel, out string)`](#deconstruct-out-datetimeoffset-out-notificationlevel-out-string) |  |

## Constructors in detail

### `Notification(DateTimeOffset, NotificationLevel, string)` {#notification-datetimeoffset-notificationlevel-string}

```csharp
public Notification(DateTimeOffset Time, NotificationLevel Level, string Text);
```

One thing the application said, and when it said it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Time` | `DateTimeOffset` | When it was raised. |
| `Level` | [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md) | How loud it is. |
| `Text` | `string` | What it says. |

## Properties in detail

### `Level` {#level}

```csharp
public NotificationLevel Level { get; init; }
```

How loud it is.

**Type** [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md)

### `Text` {#text}

```csharp
public string Text { get; init; }
```

What it says.

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

