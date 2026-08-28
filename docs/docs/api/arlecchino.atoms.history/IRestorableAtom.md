---
title: "IRestorableAtom"
sidebar_label: "IRestorableAtom"
---

# IRestorableAtom interface

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

An atom a step can be handed back to, which is how a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md) written by an earlier run reaches the atoms of this one. Named atoms answer to their [`HistoryAtom.Key`](../arlecchino.atoms.history/HistoryAtom-1.md#key).

```csharp
public interface IRestorableAtom
```

**Implemented by** [`HistoryAtom`](../arlecchino.atoms.history/HistoryAtom-1.md)

## Methods

| Member | Summary |
|---|---|
| [`Restore(IValueReader)`](#restore-ivaluereader) | Builds the step the reader is holding, for this atom. |

## Methods in detail

### `Restore(IValueReader)` {#restore-ivaluereader}

```csharp
public IAtomEdit? Restore(IValueReader reader);
```

Builds the step the reader is holding, for this atom.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `reader` | [`IValueReader`](../arlecchino.atoms.history/IValueReader.md) | Where the two values come from. |

**Returns** [`IAtomEdit`](../arlecchino.atoms.history/IAtomEdit.md) — The step, or `null` when the values could not be read.

