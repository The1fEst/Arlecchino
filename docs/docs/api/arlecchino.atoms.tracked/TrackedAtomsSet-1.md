---
title: "TrackedAtomsSet<T>"
sidebar_label: "TrackedAtomsSet<T>"
---

# TrackedAtomsSet&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Tracked` &middot; **Assembly:** `Arlecchino.Core`

A set whose changes go on the undo stack: what the user marked, the columns they turned on, the tags they put on something. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step — including one that puts several in at once.

```csharp
public sealed class TrackedAtomsSet<T> : AtomsSet<T>, IReadableAtom<IReadOnlySet<T>>
```

**Inherits from** [`AtomsSet`](../arlecchino.atoms.collections/AtomsSet-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlySet<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`TrackedAtomsSet(IReadOnlyList<T>, IEqualityComparer<T>)`](#trackedatomsset-ireadonlylist-t-iequalitycomparer-t) | Creates an undoable set. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `TrackedAtomsSet(IReadOnlyList<T>, IEqualityComparer<T>)` {#trackedatomsset-ireadonlylist-t-iequalitycomparer-t}

```csharp
public TrackedAtomsSet(IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates an undoable set.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What it starts with; empty when omitted. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How items are compared; the default comparer when omitted. |

## Properties in detail

### `RecordsHistory` {#recordshistory}

```csharp
public virtual bool RecordsHistory { get; }
```

**Type** `bool`

