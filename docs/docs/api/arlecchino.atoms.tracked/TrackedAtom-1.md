---
title: "TrackedAtom<T>"
sidebar_label: "TrackedAtom<T>"
---

# TrackedAtom&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Tracked` &middot; **Assembly:** `Arlecchino.Core`

An atom whose edits go on the undo stack: the draft being edited, a setting, the selected item — anything a user changed and may want back. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register.

```csharp
public sealed class TrackedAtom<T> : Atom<T>, IReadableAtom<T>
```

**Inherits from** [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`T`&gt;

## Constructors

| Member | Summary |
|---|---|
| [`TrackedAtom(T, IEqualityComparer<T>)`](#trackedatom-t-iequalitycomparer-t) | Creates an undoable atom holding a starting value. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `TrackedAtom(T, IEqualityComparer<T>)` {#trackedatom-t-iequalitycomparer-t}

```csharp
public TrackedAtom(T initial, IEqualityComparer<T> comparer);
```

Creates an undoable atom holding a starting value.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `T` | The value to start with. |
| `comparer` | `IEqualityComparer<T>`&lt;`T`&gt; | How to decide that writing to it changed nothing; the default comparer for `T` is used when omitted. |

## Properties in detail

### `RecordsHistory` {#recordshistory}

```csharp
public virtual bool RecordsHistory { get; }
```

**Type** `bool`

