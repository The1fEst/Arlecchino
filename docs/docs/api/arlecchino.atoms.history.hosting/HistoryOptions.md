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
| [`FileName`](#filename) | What the history file is called. It sits beside the program unless the name has a folder in it. |
| [`Keep`](#keep) | Whether the file outlives the run. The default throws it away on the way out and again on the way in, so it is only ever a scratch file of the run that is up. |

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
public string FileName { get; set; }
```

What the history file is called. It sits beside the program unless the name has a folder in it.

**Type** `string`

### `Keep` {#keep}

```csharp
public bool Keep { get; set; }
```

Whether the file outlives the run. The default throws it away on the way out and again on the way in, so it is only ever a scratch file of the run that is up.

**Type** `bool`

