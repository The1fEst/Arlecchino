---
title: TrackedAtomsStack&lt;T&gt;
sidebar_label: TrackedAtomsStack&lt;T&gt;
---

# TrackedAtomsStack&lt;T&gt; class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A stack whose changes go on the undo stack of their own: the steps the user piled up, a draft being unwound. [`AtomHistory`](../arlecchino.atoms/AtomHistory.md) picks it up with nothing to register, and each call is one step.

```csharp
public sealed class TrackedAtomsStack<T> : AtomsStack<T>, IReadableAtom<IReadOnlyList<T>>
```

**Inherits from** [`AtomsStack`](../arlecchino.atoms/AtomsStack-1.md)&lt;`T`&gt;  
**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md)&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

## Constructors

| Member | Summary |
|---|---|
| [`TrackedAtomsStack(IReadOnlyList<T>)`](#trackedatomsstack-ireadonlylist-t) | Creates an undoable stack. |

## Properties

| Member | Summary |
|---|---|
| [`RecordsHistory`](#recordshistory) |  |

## Constructors in detail

### `TrackedAtomsStack(IReadOnlyList<T>)` {#trackedatomsstack-ireadonlylist-t}

```csharp
public TrackedAtomsStack(IReadOnlyList<T> initial);
```

Creates an undoable stack.

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

