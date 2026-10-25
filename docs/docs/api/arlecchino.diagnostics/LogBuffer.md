---
title: "LogBuffer"
sidebar_label: "LogBuffer"
---

# LogBuffer class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

The last few log lines, held in memory for the overlay to draw, since a terminal application cannot write them to the console. Logging happens on any thread, so the oldest are dropped under a lock.

```csharp
public sealed class LogBuffer
```

## Constructors

| Member | Summary |
|---|---|
| [`LogBuffer(Repaint)`](#logbuffer-repaint) | Creates the buffer. |

## Properties

| Member | Summary |
|---|---|
| [`Capacity`](#capacity) | How many lines to keep before the oldest start falling off the back. |
| [`Count`](#count) | How many lines are held. Can change between two reads if something logs meanwhile. |

## Methods

| Member | Summary |
|---|---|
| [`Add(LogEntry)`](#add-logentry) | Records a line, dropping the oldest when the buffer is full. Safe from any thread. |
| [`Clear()`](#clear) | Throws away every line held. |
| [`Snapshot()`](#snapshot) | The lines held, the oldest first, as they were at this moment. A copy, because anything may be logging while the overlay draws. |

## Constructors in detail

### `LogBuffer(Repaint)` {#logbuffer-repaint}

```csharp
public LogBuffer(Repaint repaint);
```

Creates the buffer.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `repaint` | [`Repaint`](../arlecchino/Repaint.md) | Asked for a frame when a line arrives, so the overlay stays current. |

## Properties in detail

### `Capacity` {#capacity}

```csharp
public int Capacity { get; set; }
```

How many lines to keep before the oldest start falling off the back.

**Type** `int`

### `Count` {#count}

```csharp
public int Count { get; }
```

How many lines are held. Can change between two reads if something logs meanwhile.

**Type** `int`

## Methods in detail

### `Add(LogEntry)` {#add-logentry}

```csharp
public void Add(LogEntry entry);
```

Records a line, dropping the oldest when the buffer is full. Safe from any thread.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`LogEntry`](../arlecchino.diagnostics/LogEntry.md) | The line. |

### `Clear()` {#clear}

```csharp
public void Clear();
```

Throws away every line held.

### `Snapshot()` {#snapshot}

```csharp
public IReadOnlyList<LogEntry> Snapshot();
```

The lines held, the oldest first, as they were at this moment. A copy, because anything may be logging while the overlay draws.

**Returns** `IReadOnlyList<T>`&lt;[`LogEntry`](../arlecchino.diagnostics/LogEntry.md)&gt; — The lines.

