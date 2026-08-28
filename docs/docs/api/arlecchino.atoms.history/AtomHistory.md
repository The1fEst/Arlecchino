---
title: "AtomHistory"
sidebar_label: "AtomHistory"
---

# AtomHistory class

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

Undo and redo over every [`HistoryAtom`](../arlecchino.atoms.history/HistoryAtom-1.md) and history collection. Recent steps are held in memory, and older ones are read back from the [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md) a step at a time.

```csharp
public sealed class AtomHistory : IDisposable
```

**Implements** `IDisposable`

## Properties

| Member | Summary |
|---|---|
| [`CanRedo`](#canredo) | Whether an undone step can be applied again. |
| [`CanUndo`](#canundo) | Whether there is a step to undo, in memory or in the file. |
| [`Capacity`](#capacity) | How many steps to keep in memory. Lowering it drops the oldest straight away, and what was written down is read back from the file as it is needed. |
| [`Depth`](#depth) | How many steps can be taken back, counting the ones only the file holds. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Forgets what this run holds in memory, so that wiring does not end up as the first undo step. What the file holds stays where it is. |
| [`Dispose()`](#dispose) | Stops collecting edits. |
| [`Group()`](#group) | Collects everything written until the scope is disposed into a single undo step. A group opened inside another joins it rather than closing it early. |
| [`Redo()`](#redo) | Applies an undone step again. Writing something new drops this branch. |
| [`Undo()`](#undo) | Takes back the last step. Undoing does not itself become a step. |

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

Whether there is a step to undo, in memory or in the file.

**Type** `bool`

### `Capacity` {#capacity}

```csharp
public int Capacity { get; set; }
```

How many steps to keep in memory. Lowering it drops the oldest straight away, and what was written down is read back from the file as it is needed.

**Type** `int`

### `Depth` {#depth}

```csharp
public int Depth { get; }
```

How many steps can be taken back, counting the ones only the file holds.

**Type** `int`

## Methods in detail

### `Clear()` {#clear}

```csharp
public void Clear();
```

Forgets what this run holds in memory, so that wiring does not end up as the first undo step. What the file holds stays where it is.

### `Dispose()` {#dispose}

```csharp
public void Dispose();
```

Stops collecting edits.

### `Group()` {#group}

```csharp
public IDisposable Group();
```

Collects everything written until the scope is disposed into a single undo step. A group opened inside another joins it rather than closing it early.

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

