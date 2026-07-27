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
| [`<Clone>$()`](#clone) |  |
| [`Deconstruct(DateTimeOffset&, NotificationLevel&, String&)`](#deconstruct-datetimeoffset-notificationlevel-string) |  |
| [`Equals(object)`](#equals-object) |  |
| [`Equals(Notification)`](#equals-notification) |  |
| [`GetHashCode()`](#gethashcode) |  |
| [`ToString()`](#tostring) |  |

## Operators

| Member | Summary |
|---|---|
| [`operator Inequality(Notification, Notification)`](#operator-inequality-notification-notification) |  |
| [`operator Equality(Notification, Notification)`](#operator-equality-notification-notification) |  |

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

### `<Clone>$()` {#clone}

```csharp
public Notification <Clone>$();
```

**Returns** [`Notification`](../arlecchino.diagnostics/Notification.md)

### `Deconstruct(DateTimeOffset&, NotificationLevel&, String&)` {#deconstruct-datetimeoffset-notificationlevel-string}

```csharp
public void Deconstruct(out DateTimeOffset Time, out NotificationLevel Level, out string Text);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Time` | `DateTimeOffset` |  |
| `Level` | [`NotificationLevel`](../arlecchino.diagnostics/NotificationLevel.md) |  |
| `Text` | `string` |  |

### `Equals(object)` {#equals-object}

```csharp
public override bool Equals(object? obj);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `obj` | `object` |  |

**Returns** `bool`

### `Equals(Notification)` {#equals-notification}

```csharp
public bool Equals(Notification? other);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`Notification`](../arlecchino.diagnostics/Notification.md) |  |

**Returns** `bool`

### `GetHashCode()` {#gethashcode}

```csharp
public override int GetHashCode();
```

**Returns** `int`

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

**Returns** `string`

## Operators in detail

### `operator Inequality(Notification, Notification)` {#operator-inequality-notification-notification}

```csharp
public static bool op_Inequality(Notification left, Notification right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`Notification`](../arlecchino.diagnostics/Notification.md) |  |
| `right` | [`Notification`](../arlecchino.diagnostics/Notification.md) |  |

**Returns** `bool`

### `operator Equality(Notification, Notification)` {#operator-equality-notification-notification}

```csharp
public static bool op_Equality(Notification left, Notification right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`Notification`](../arlecchino.diagnostics/Notification.md) |  |
| `right` | [`Notification`](../arlecchino.diagnostics/Notification.md) |  |

**Returns** `bool`

