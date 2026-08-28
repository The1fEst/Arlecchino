---
title: "HistoryAtomsList<T>"
sidebar_label: "HistoryAtomsList<T>"
---

# HistoryAtomsList&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.History.Collections` &middot; **Assembly:** `Arlecchino.Atoms.History`

A list whose changes go on the undo stack. A plain [`AtomsList`](../arlecchino.atoms.collections/AtomsList-1.md) is the one whose changes do not.

```csharp
public sealed class HistoryAtomsList<T> :
    AtomsList<T>,
    IReadableAtom,
    IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsList`](../arlecchino.atoms.collections/AtomsList-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md), [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`HistoryAtomsList(IReadOnlyList<T>, IEqualityComparer<T>)`](#historyatomslist-ireadonlylist-t-iequalitycomparer-t) | Creates a list that keeps history. |

## Properties

| Member | Summary |
|---|---|
| [`IsObserved`](#isobserved) |  |

## Methods

| Member | Summary |
|---|---|
| [`OnInserted(int, T[])`](#oninserted-int-t) |  |
| [`OnRemoved(int, T[])`](#onremoved-int-t) |  |
| [`OnReplaced(int, T, T)`](#onreplaced-int-t-t) |  |
| [`OnReset(T[], T[])`](#onreset-t-t) |  |

## Constructors in detail

### `HistoryAtomsList(IReadOnlyList<T>, IEqualityComparer<T>)` {#historyatomslist-ireadonlylist-t-iequalitycomparer-t}

```csharp
public HistoryAtomsList(IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates a list that keeps history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What it starts with; empty when omitted. It is copied, not held. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How `Remove` finds an item, and how the indexer decides nothing changed. |

## Properties in detail

### `IsObserved` {#isobserved}

```csharp
public virtual bool IsObserved { get; }
```

**Type** `bool`

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

### `OnReplaced(int, T, T)` {#onreplaced-int-t-t}

```csharp
public virtual void OnReplaced(int index, T previous, T current);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `index` | `int` |  |
| `previous` | `T` |  |
| `current` | `T` |  |

### `OnReset(T[], T[])` {#onreset-t-t}

```csharp
public virtual void OnReset(T[] previous, T[] current);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `previous` | `T`\[\] |  |
| `current` | `T`\[\] |  |

