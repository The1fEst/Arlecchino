---
title: "IValueReader"
sidebar_label: "IValueReader"
---

# IValueReader interface

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

Where an atom takes back the two values a step stood between. The atom calls this, since it is the one place the kind of value is known without reflection.

```csharp
public interface IValueReader
```

**Implemented by** [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md)

## Methods

| Member | Summary |
|---|---|
| [`Read<T>(out T, out T)`](#read-t-out-t-out-t) | Reads both values of one step. |

## Methods in detail

### `Read<T>(out T, out T)` {#read-t-out-t-out-t}

```csharp
public bool Read<T>(out T undone, out T redone);
```

Reads both values of one step.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `undone` | `T` | The value undoing puts back. |
| `redone` | `T` | The value redoing puts back. |

**Returns** `bool` — `false` when this kind of value has no reader, or the step does not parse.

