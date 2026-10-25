---
title: "HistoryAtomsSet<T>"
sidebar_label: "HistoryAtomsSet<T>"
---

# HistoryAtomsSet&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.History.Collections` &middot; **Assembly:** `Arlecchino.Atoms.History`

A set whose changes go on the undo stack. A plain [`AtomsSet`](../arlecchino.atoms.collections/AtomsSet-1.md) is the one whose changes do not.

```csharp
public sealed class HistoryAtomsSet<T> :
    AtomsSet<T>,
    IReadableAtom,
    IReadableAtom<IReadOnlySet<T>>,
    IRestorableAtom,
    IHistoryCollection<T>
```

**Inherits from** [`AtomsSet`](../arlecchino.atoms.collections/AtomsSet-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md), [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlySet<T>`&lt;`T`&gt;&gt;, [`IRestorableAtom`](../arlecchino.atoms.history/IRestorableAtom.md), `IHistoryCollection<T>`&lt;`T`&gt;

## Constructors

| Member | Summary |
|---|---|
| [`HistoryAtomsSet(string, IReadOnlyList<T>, IEqualityComparer<T>)`](#historyatomsset-string-ireadonlylist-t-iequalitycomparer-t) | Creates a set that keeps history. |

## Properties

| Member | Summary |
|---|---|
| [`IsObserved`](#isobserved) |  |
| [`Key`](#key) | What the set is called in a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md), written in full — usually the store and the property — so that it outlives a rename. |

## Methods

| Member | Summary |
|---|---|
| [`OnAdded(T[])`](#onadded-t) |  |
| [`OnRemoved(T[])`](#onremoved-t) |  |
| [`OnReset(T[], T[])`](#onreset-t-t) |  |
| [`Restore(IValueReader)`](#restore-ivaluereader) |  |

## Constructors in detail

### `HistoryAtomsSet(string, IReadOnlyList<T>, IEqualityComparer<T>)` {#historyatomsset-string-ireadonlylist-t-iequalitycomparer-t}

```csharp
public HistoryAtomsSet(string key, IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates a set that keeps history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `string` | What the set is called; see [`HistoryAtomsSet.Key`](../arlecchino.atoms.history.collections/HistoryAtomsSet-1.md#key). |
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What it starts with; empty when omitted. It is copied, not held. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How items are compared. |

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

What the set is called in a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md), written in full — usually the store and the property — so that it outlives a rename.

**Type** `string`

## Methods in detail

### `OnAdded(T[])` {#onadded-t}

```csharp
public virtual void OnAdded(T[] items);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `T`\[\] |  |

### `OnRemoved(T[])` {#onremoved-t}

```csharp
public virtual void OnRemoved(T[] items);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
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

