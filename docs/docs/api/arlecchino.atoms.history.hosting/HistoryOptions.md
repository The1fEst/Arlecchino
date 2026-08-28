---
title: "HistoryOptions"
sidebar_label: "HistoryOptions"
---

# HistoryOptions class

**Namespace:** `Arlecchino.Atoms.History.Hosting` &middot; **Assembly:** `Arlecchino.Atoms.History`

Everything the history can be told, set in the callback `UseHistory` takes.

```csharp
public sealed class HistoryOptions
```

## Constructors

| Member | Summary |
|---|---|
| [`HistoryOptions()`](#historyoptions) |  |

## Properties

| Member | Summary |
|---|---|
| [`Capacity`](#capacity) | How many steps to keep before the oldest fall off the far end. |
| [`FileName`](#filename) | What the history file is called, or `null` for a history kept in memory. A file sits beside the program, outlives the run, and is what the next one comes up in. |

## Constructors in detail

### `HistoryOptions()` {#historyoptions}

```csharp
public HistoryOptions();
```

## Properties in detail

### `Capacity` {#capacity}

```csharp
public int Capacity { get; set; }
```

How many steps to keep before the oldest fall off the far end.

**Type** `int`

### `FileName` {#filename}

```csharp
public string? FileName { get; set; }
```

What the history file is called, or `null` for a history kept in memory. A file sits beside the program, outlives the run, and is what the next one comes up in.

**Type** `string`

