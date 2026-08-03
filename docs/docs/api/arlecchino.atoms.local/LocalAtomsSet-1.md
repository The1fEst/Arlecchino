---
title: LocalAtomsSet&lt;T&gt;
sidebar_label: LocalAtomsSet&lt;T&gt;
---

# LocalAtomsSet&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Local` &middot; **Assembly:** `Arlecchino.Core`

A set the undo history never sees: the rows expanded, the folders already walked, the hosts that answered. It notifies and asks for a frame exactly as a [`TrackedAtomsSet`](../arlecchino.atoms.tracked/TrackedAtomsSet-1.md) does.

```csharp
public sealed class LocalAtomsSet<T> : AtomsSet<T>, IReadableAtom<IReadOnlySet<T>>
```

**Inherits from** [`AtomsSet`](../arlecchino.atoms.collections/AtomsSet-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlySet<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`LocalAtomsSet(IReadOnlyList<T>, IEqualityComparer<T>)`](#localatomsset-ireadonlylist-t-iequalitycomparer-t) | Creates a set outside the undo history. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `LocalAtomsSet(IReadOnlyList<T>, IEqualityComparer<T>)` {#localatomsset-ireadonlylist-t-iequalitycomparer-t}

```csharp
public LocalAtomsSet(IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates a set outside the undo history.

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

