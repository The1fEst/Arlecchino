---
title: "TrackedAtomsList<T>"
sidebar_label: "TrackedAtomsList<T>"
---

# TrackedAtomsList&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Tracked` &middot; **Assembly:** `Arlecchino.Core`

A list whose changes go on the undo stack, picked up by [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) with nothing to register. Each call is one step, so a page added at once comes back as a page.

```csharp
public sealed class TrackedAtomsList<T> : AtomsList<T>, IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsList`](../arlecchino.atoms.collections/AtomsList-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`TrackedAtomsList(IReadOnlyList<T>, IEqualityComparer<T>)`](#trackedatomslist-ireadonlylist-t-iequalitycomparer-t) | Creates an undoable list. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `TrackedAtomsList(IReadOnlyList<T>, IEqualityComparer<T>)` {#trackedatomslist-ireadonlylist-t-iequalitycomparer-t}

```csharp
public TrackedAtomsList(IReadOnlyList<T> initial, IEqualityComparer<T> comparer);
```

Creates an undoable list.

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

