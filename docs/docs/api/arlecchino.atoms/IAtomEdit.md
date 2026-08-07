---
title: "IAtomEdit"
sidebar_label: "IAtomEdit"
---

# IAtomEdit interface

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

One recorded change of one atom, as kept by [`AtomHistory`](../arlecchino.atoms/AtomHistory.md). Replaying it does not record a new step.

```csharp
public interface IAtomEdit
```

## Properties

| Member | Summary |
|---|---|
| [`Owner`](#owner) | The atom this edit belongs to. |

## Methods

| Member | Summary |
|---|---|
| [`Redo()`](#redo) | Applies the edit again. |
| [`Undo()`](#undo) | Puts the value back to what it was before the edit. |

## Properties in detail

### `Owner` {#owner}

```csharp
public object? Owner { get; }
```

The atom this edit belongs to.

**Type** `object`

## Methods in detail

### `Redo()` {#redo}

```csharp
public void Redo();
```

Applies the edit again.

### `Undo()` {#undo}

```csharp
public void Undo();
```

Puts the value back to what it was before the edit.

