---
title: "TrackedAtomsList<T>"
sidebar_label: "TrackedAtomsList<T>"
---

# TrackedAtomsList&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Tracked` &middot; **Assembly:** `Arlecchino.Core`

A list whose changes go on the undo stack: the rows of the document being edited, the tasks of a plan, the marked files. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step — a page added with [`AtomsList.Add`](../arlecchino.atoms.collections/AtomsList-1.md#add-ireadonlylist-t) comes back as a page rather than row by row.

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

