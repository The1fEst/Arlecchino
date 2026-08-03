---
title: LocalAtomsStack&lt;T&gt;
sidebar_label: LocalAtomsStack&lt;T&gt;
---

# LocalAtomsStack&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms.Local` &middot; **Assembly:** `Arlecchino.Core`

A stack the undo history never sees: where the user has been, the folders walked into, the screens over one another. It notifies and asks for a frame exactly as a [`TrackedAtomsStack`](../arlecchino.atoms.tracked/TrackedAtomsStack-1.md) does.

```csharp
public sealed class LocalAtomsStack<T> : AtomsStack<T>, IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsStack`](../arlecchino.atoms.collections/AtomsStack-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`LocalAtomsStack(IReadOnlyList<T>)`](#localatomsstack-ireadonlylist-t) | Creates a stack outside the undo history. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `LocalAtomsStack(IReadOnlyList<T>)` {#localatomsstack-ireadonlylist-t}

```csharp
public LocalAtomsStack(IReadOnlyList<T> initial);
```

Creates a stack outside the undo history.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `initial` | `IReadOnlyList<T>`&lt;`T`&gt; | What is already on it, top first; empty when omitted. |

## Properties in detail

### `RecordsHistory` {#recordshistory}

```csharp
public virtual bool RecordsHistory { get; }
```

**Type** `bool`

