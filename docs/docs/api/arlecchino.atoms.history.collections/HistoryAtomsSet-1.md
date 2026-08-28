---
title: "HistoryAtomsSet<T>"
sidebar_label: "HistoryAtomsSet<T>"
---

# HistoryAtomsSet&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.History.Collections` &middot; **Assembly:** `Arlecchino.Atoms.History`

A set whose changes go on the undo stack. A plain [`AtomsSet`](../arlecchino.atoms.collections/AtomsSet-1.md) is the one whose changes do not.

```csharp
public sealed class HistoryAtomsSet<T> : AtomsSet<T>, IReadableAtom, IReadableAtom<IReadOnlySet<T>>
```

**Inherits from** [`AtomsSet`](../arlecchino.atoms.collections/AtomsSet-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md), [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlySet<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`HistoryAtomsSet(IReadOnlyList<T>, IEqualityComparer<T>)`](#historyatomsset-ireadonlylist-t-iequalitycomparer-t) | Creates a set that keeps history. |

## Properties

| Member | Summary |
|---|---|
| [`IsObserved`](#isobserved) |  |

## Methods

| Member | Summary |
|---|---|
| [`OnAdded(T[])`](#onadded-t) |  |
| [`OnRemoved(T[])`](#onremoved-t) |  |
| [`OnReset(T[], T[])`](#onreset-t-t) |  |

## Constructors in detail

### `HistoryAtomsSet(IReadOnlyList<T>, IEqualityComparer<T>)` {#historyatomsset-ireadonlylist-t-iequalitycomparer-t}

```csharp
public HistoryAtomsSet(IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates a set that keeps history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What it starts with; empty when omitted. It is copied, not held. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How items are compared. |

## Properties in detail

### `IsObserved` {#isobserved}

```csharp
public virtual bool IsObserved { get; }
```

**Type** `bool`

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

