---
title: LogEntry
sidebar_label: LogEntry
---

# LogEntry class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

One line of log, kept for the overlay.

```csharp
public sealed class LogEntry : IEquatable<LogEntry>
```

**Implements** `IEquatable<T>`&lt;[`LogEntry`](../arlecchino.diagnostics/LogEntry.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`LogEntry(DateTimeOffset, LogLevel, string, string)`](#logentry-datetimeoffset-loglevel-string-string) | One line of log, kept for the overlay. |

## Properties

| Member | Summary |
|---|---|
| [`Category`](#category) | Where it came from, already shortened to the last part of the name. |
| [`Level`](#level) | How bad it is. |
| [`Message`](#message) | What was logged, exception message included. |
| [`Time`](#time) | When it was written. |

## Methods

| Member | Summary |
|---|---|
| [`<Clone>$()`](#clone) |  |
| [`Deconstruct(DateTimeOffset&, LogLevel&, String&, String&)`](#deconstruct-datetimeoffset-loglevel-string-string) |  |
| [`Equals(object)`](#equals-object) |  |
| [`Equals(LogEntry)`](#equals-logentry) |  |
| [`GetHashCode()`](#gethashcode) |  |
| [`ToString()`](#tostring) |  |

## Operators

| Member | Summary |
|---|---|
| [`operator Inequality(LogEntry, LogEntry)`](#operator-inequality-logentry-logentry) |  |
| [`operator Equality(LogEntry, LogEntry)`](#operator-equality-logentry-logentry) |  |

## Constructors in detail

### `LogEntry(DateTimeOffset, LogLevel, string, string)` {#logentry-datetimeoffset-loglevel-string-string}

```csharp
public LogEntry(DateTimeOffset Time, LogLevel Level, string Category, string Message);
```

One line of log, kept for the overlay.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Time` | `DateTimeOffset` | When it was written. |
| `Level` | `LogLevel` | How bad it is. |
| `Category` | `string` | Where it came from, already shortened to the last part of the name. |
| `Message` | `string` | What was logged, exception message included. |

## Properties in detail

### `Category` {#category}

```csharp
public string Category { get; init; }
```

Where it came from, already shortened to the last part of the name.

**Type** `string`

### `Level` {#level}

```csharp
public LogLevel Level { get; init; }
```

How bad it is.

**Type** `LogLevel`

### `Message` {#message}

```csharp
public string Message { get; init; }
```

What was logged, exception message included.

**Type** `string`

### `Time` {#time}

```csharp
public DateTimeOffset Time { get; init; }
```

When it was written.

**Type** `DateTimeOffset`

## Methods in detail

### `<Clone>$()` {#clone}

```csharp
public LogEntry <Clone>$();
```

**Returns** [`LogEntry`](../arlecchino.diagnostics/LogEntry.md)

### `Deconstruct(DateTimeOffset&, LogLevel&, String&, String&)` {#deconstruct-datetimeoffset-loglevel-string-string}

```csharp
public void Deconstruct(out DateTimeOffset Time, out LogLevel Level, out string Category, out string Message);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Time` | `DateTimeOffset` |  |
| `Level` | `LogLevel` |  |
| `Category` | `string` |  |
| `Message` | `string` |  |

### `Equals(object)` {#equals-object}

```csharp
public override bool Equals(object? obj);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `obj` | `object` |  |

**Returns** `bool`

### `Equals(LogEntry)` {#equals-logentry}

```csharp
public bool Equals(LogEntry? other);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`LogEntry`](../arlecchino.diagnostics/LogEntry.md) |  |

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

### `operator Inequality(LogEntry, LogEntry)` {#operator-inequality-logentry-logentry}

```csharp
public static bool op_Inequality(LogEntry left, LogEntry right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`LogEntry`](../arlecchino.diagnostics/LogEntry.md) |  |
| `right` | [`LogEntry`](../arlecchino.diagnostics/LogEntry.md) |  |

**Returns** `bool`

### `operator Equality(LogEntry, LogEntry)` {#operator-equality-logentry-logentry}

```csharp
public static bool op_Equality(LogEntry left, LogEntry right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`LogEntry`](../arlecchino.diagnostics/LogEntry.md) |  |
| `right` | [`LogEntry`](../arlecchino.diagnostics/LogEntry.md) |  |

**Returns** `bool`

