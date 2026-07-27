---
title: AtomHistory
sidebar_label: AtomHistory
---

# AtomHistory class

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

Undo and redo over every [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md). It collects from the moment it exists, so the hosted service resolves it at startup; a headless run has to create it before the edits it wants to undo. The undo stack is bounded: a long-running application would otherwise hold on to every edit it has ever made, and each of those keeps the old value alive too. Steps past [`AtomHistory.Capacity`](../arlecchino.atoms/AtomHistory.md#capacity) fall off the far end, which is the end nobody is going to reach.

```csharp
public sealed class AtomHistory : IDisposable
```

**Implements** `IDisposable`

## Constructors

| Member | Summary |
|---|---|
| [`AtomHistory()`](#atomhistory) | Starts collecting edits. |

## Properties

| Member | Summary |
|---|---|
| [`CanRedo`](#canredo) | Whether an undone step can be applied again. |
| [`CanUndo`](#canundo) | Whether there is a step to undo. |
| [`Capacity`](#capacity) | How many steps to keep. Lowering it drops the oldest straight away; anything below one step is treated as one, since a history that remembers nothing is what [`LocalAtom`](../arlecchino.atoms/LocalAtom-1.md) is for. |
| [`Depth`](#depth) | How many steps are on the undo stack. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Forgets both stacks. The hosted service does this once the application is up, so wiring does not end up as the first undo step. |
| [`Dispose()`](#dispose) | Stops collecting edits. |
| [`Group()`](#group) | Collects everything written until the scope is disposed into a single undo step, so related edits go back together. Groups nest: a group opened inside another joins it rather than closing it early, so wrapping code that groups edits of its own still yields one step. |
| [`Redo()`](#redo) | Applies an undone step again. Writing something new drops this branch. |
| [`Undo()`](#undo) | Takes back the last step. Undoing does not itself become a step. |

## Constructors in detail

### `AtomHistory()` {#atomhistory}

```csharp
public AtomHistory();
```

Starts collecting edits.

## Properties in detail

### `CanRedo` {#canredo}

```csharp
public bool CanRedo { get; }
```

Whether an undone step can be applied again.

**Type** `bool`

### `CanUndo` {#canundo}

```csharp
public bool CanUndo { get; }
```

Whether there is a step to undo.

**Type** `bool`

### `Capacity` {#capacity}

```csharp
public int Capacity { get; set; }
```

How many steps to keep. Lowering it drops the oldest straight away; anything below one step is treated as one, since a history that remembers nothing is what [`LocalAtom`](../arlecchino.atoms/LocalAtom-1.md) is for.

**Type** `int`

### `Depth` {#depth}

```csharp
public int Depth { get; }
```

How many steps are on the undo stack.

**Type** `int`

## Methods in detail

### `Clear()` {#clear}

```csharp
public void Clear();
```

Forgets both stacks. The hosted service does this once the application is up, so wiring does not end up as the first undo step.

### `Dispose()` {#dispose}

```csharp
public void Dispose();
```

Stops collecting edits.

### `Group()` {#group}

```csharp
public IDisposable Group();
```

Collects everything written until the scope is disposed into a single undo step, so related edits go back together. Groups nest: a group opened inside another joins it rather than closing it early, so wrapping code that groups edits of its own still yields one step.

**Returns** `IDisposable` — The scope to dispose when the group is complete.

### `Redo()` {#redo}

```csharp
public bool Redo();
```

Applies an undone step again. Writing something new drops this branch.

**Returns** `bool` — `false` when there was nothing to redo.

### `Undo()` {#undo}

```csharp
public bool Undo();
```

Takes back the last step. Undoing does not itself become a step.

**Returns** `bool` — `false` when there was nothing to undo.

