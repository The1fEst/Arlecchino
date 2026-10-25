---
title: "HistoryAtom<T>"
sidebar_label: "HistoryAtom<T>"
---

# HistoryAtom&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

An atom whose writes go on the undo stack: a draft, a setting, the selected item. A plain [`Atom`](../arlecchino.atoms/Atom-1.md) is the one whose writes do not.

```csharp
public sealed class HistoryAtom<T> :
    Atom<T>,
    IReadableAtom<T>,
    IReadableAtom,
    IRestorableAtom,
    IResettableAtom
```

**Inherits from** [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`T`&gt;, [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md), [`IRestorableAtom`](../arlecchino.atoms.history/IRestorableAtom.md), [`IResettableAtom`](../arlecchino.atoms/IResettableAtom.md)

## Constructors

| Member | Summary |
|---|---|
| [`HistoryAtom(T, string, IEqualityComparer<T>)`](#historyatom-t-string-iequalitycomparer-t) | Creates an atom that keeps history. |

## Properties

| Member | Summary |
|---|---|
| [`Key`](#key) | What the atom is called in a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md), written in full rather than worked out: it is the name a later run finds its atom by, so it outlives any rename. Usually the store and the property, as `"SettingsStore.Server"`. |

## Methods

| Member | Summary |
|---|---|
| [`OnWritten(T, T)`](#onwritten-t-t) |  |
| [`Restore(IValueReader)`](#restore-ivaluereader) |  |
| [`TryReset()`](#tryreset) | Writes back what the atom held before the first edit the history took, as one more edit rather than by unwinding the stack: the step it makes is itself undoable, and no other atom moves. |

## Constructors in detail

### `HistoryAtom(T, string, IEqualityComparer<T>)` {#historyatom-t-string-iequalitycomparer-t}

```csharp
public HistoryAtom(T initial, string key, IEqualityComparer<T> comparer);
```

Creates an atom that keeps history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `T` | The value to start with. |
| `key` | `string` | What the atom is called; see [`HistoryAtom.Key`](../arlecchino.atoms.history/HistoryAtom-1.md#key). |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How to decide that writing to it changed nothing; the default comparer for `T` is used when omitted. |

**Exceptions**

| Type | Thrown when |
|---|---|
| `ArgumentException` | The name is empty. |
| `InvalidOperationException` | The name is taken by an atom holding something else. |

## Properties in detail

### `Key` {#key}

```csharp
public string Key { get; }
```

What the atom is called in a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md), written in full rather than worked out: it is the name a later run finds its atom by, so it outlives any rename. Usually the store and the property, as `"SettingsStore.Server"`.

**Type** `string`

## Methods in detail

### `OnWritten(T, T)` {#onwritten-t-t}

```csharp
public virtual void OnWritten(T previous, T current);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `previous` | `T` |  |
| `current` | `T` |  |

### `Restore(IValueReader)` {#restore-ivaluereader}

```csharp
public IAtomEdit Restore(IValueReader reader);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `reader` | [`IValueReader`](../arlecchino.atoms.history/IValueReader.md) |  |

**Returns** [`IAtomEdit`](../arlecchino.atoms.history/IAtomEdit.md)

### `TryReset()` {#tryreset}

```csharp
public bool TryReset();
```

Writes back what the atom held before the first edit the history took, as one more edit rather than by unwinding the stack: the step it makes is itself undoable, and no other atom moves.

**Returns** `bool` — `false` when no edit of this atom was ever taken, leaving nowhere to go back to.

