---
title: "IValueWriter"
sidebar_label: "IValueWriter"
---

# IValueWriter interface

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

Where an edit hands over the two values it stands between. The edit calls this, since it is the one place the kind of value is known without reflection.

```csharp
public interface IValueWriter
```

**Implemented by** [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md)

## Methods

| Member | Summary |
|---|---|
| [`Write<T>(T, T)`](#write-t-t-t) | Takes both values of one edit. |

## Methods in detail

### `Write<T>(T, T)` {#write-t-t-t}

```csharp
public bool Write<T>(T undone, T redone);
```

Takes both values of one edit.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `undone` | `T` | The value undoing puts back. |
| `redone` | `T` | The value redoing puts back. |

**Returns** `bool` — `false` when this kind of value cannot be written, which abandons the whole step.

