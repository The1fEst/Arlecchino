---
title: "HistoryAtomsStack<T>"
sidebar_label: "HistoryAtomsStack<T>"
---

# HistoryAtomsStack&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.History.Collections` &middot; **Assembly:** `Arlecchino.Atoms.History`

A stack whose changes go on the undo stack. A plain [`AtomsStack`](../arlecchino.atoms.collections/AtomsStack-1.md) is the one whose changes do not.

```csharp
public sealed class HistoryAtomsStack<T> :
    AtomsStack<T>,
    IReadableAtom,
    IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsStack`](../arlecchino.atoms.collections/AtomsStack-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md), [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`HistoryAtomsStack(IReadOnlyList<T>)`](#historyatomsstack-ireadonlylist-t) | Creates a stack that keeps history. |

## Properties

| Member | Summary |
|---|---|
| [`IsObserved`](#isobserved) |  |

## Methods

| Member | Summary |
|---|---|
| [`OnInserted(int, T[])`](#oninserted-int-t) |  |
| [`OnRemoved(int, T[])`](#onremoved-int-t) |  |
| [`OnReset(T[], T[])`](#onreset-t-t) |  |

## Constructors in detail

### `HistoryAtomsStack(IReadOnlyList<T>)` {#historyatomsstack-ireadonlylist-t}

```csharp
public HistoryAtomsStack(IReadOnlyList<T> initial);
```

Creates a stack that keeps history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What is already on it, top first; empty when omitted. It is copied, not held. |

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

### `OnReset(T[], T[])` {#onreset-t-t}

```csharp
public virtual void OnReset(T[] previous, T[] current);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `previous` | `T`\[\] |  |
| `current` | `T`\[\] |  |

