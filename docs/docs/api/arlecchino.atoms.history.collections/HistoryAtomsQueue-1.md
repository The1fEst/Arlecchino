---
title: "HistoryAtomsQueue<T>"
sidebar_label: "HistoryAtomsQueue<T>"
---

# HistoryAtomsQueue&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.History.Collections` &middot; **Assembly:** `Arlecchino.Atoms.History`

A queue whose changes go on the undo stack. A plain [`AtomsQueue`](../arlecchino.atoms.collections/AtomsQueue-1.md) is the one whose changes do not.

```csharp
public sealed class HistoryAtomsQueue<T> :
    AtomsQueue<T>,
    IReadableAtom,
    IReadableAtom<IReadOnlyList<T>>,
    IRestorableAtom,
    IHistoryCollection<T>
```

**Inherits from** [`AtomsQueue`](../arlecchino.atoms.collections/AtomsQueue-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md), [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;, [`IRestorableAtom`](../arlecchino.atoms.history/IRestorableAtom.md), `IHistoryCollection<T>`&lt;`T`&gt;

## Constructors

| Member | Summary |
|---|---|
| [`HistoryAtomsQueue(string, IReadOnlyList<T>)`](#historyatomsqueue-string-ireadonlylist-t) | Creates a queue that keeps history. |

## Properties

| Member | Summary |
|---|---|
| [`IsObserved`](#isobserved) |  |
| [`Key`](#key) | What the queue is called in a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md), written in full — usually the store and the property — so that it outlives a rename. |

## Methods

| Member | Summary |
|---|---|
| [`OnInserted(int, T[])`](#oninserted-int-t) |  |
| [`OnRemoved(int, T[])`](#onremoved-int-t) |  |
| [`OnReset(T[], T[])`](#onreset-t-t) |  |
| [`Restore(IValueReader)`](#restore-ivaluereader) |  |

## Constructors in detail

### `HistoryAtomsQueue(string, IReadOnlyList<T>)` {#historyatomsqueue-string-ireadonlylist-t}

```csharp
public HistoryAtomsQueue(string key, IReadOnlyList<T> initial);
```

Creates a queue that keeps history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `string` | What the queue is called; see [`HistoryAtomsQueue.Key`](../arlecchino.atoms.history.collections/HistoryAtomsQueue-1.md#key). |
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What is already waiting, front first; empty when omitted. It is copied, not held. |

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | The name is taken by an atom holding something else. |

## Properties in detail

### `IsObserved` {#isobserved}

```csharp
public virtual bool IsObserved { get; }
```

**Type** `bool`

### `Key` {#key}

```csharp
public string Key { get; }
```

What the queue is called in a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md), written in full — usually the store and the property — so that it outlives a rename.

**Type** `string`

## Methods in detail

### `OnInserted(int, T[])` {#oninserted-int-t}

```csharp
public virtual void OnInserted(int index, T[] items);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` |  |
| `items` | `T`\[\] |  |

### `OnRemoved(int, T[])` {#onremoved-int-t}

```csharp
public virtual void OnRemoved(int index, T[] items);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` |  |
| `items` | `T`\[\] |  |

### `OnReset(T[], T[])` {#onreset-t-t}

```csharp
public virtual void OnReset(T[] previous, T[] current);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `previous` | `T`\[\] |  |
| `current` | `T`\[\] |  |

### `Restore(IValueReader)` {#restore-ivaluereader}

```csharp
public IAtomEdit Restore(IValueReader reader);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `reader` | [`IValueReader`](../arlecchino.atoms.history/IValueReader.md) |  |

**Returns** [`IAtomEdit`](../arlecchino.atoms.history/IAtomEdit.md)

