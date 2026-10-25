---
title: "IRestorableAtom"
sidebar_label: "IRestorableAtom"
---

# IRestorableAtom interface

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

An atom a step can be handed back to, which is how a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md) written by an earlier run reaches the atoms of this one. It answers to the name it was given.

```csharp
public interface IRestorableAtom
```

**Implemented by** [`HistoryAtom`](../arlecchino.atoms.history/HistoryAtom-1.md), [`HistoryAtomsList`](../arlecchino.atoms.history.collections/HistoryAtomsList-1.md), [`HistoryAtomsMap`](../arlecchino.atoms.history.collections/HistoryAtomsMap-2.md), [`HistoryAtomsQueue`](../arlecchino.atoms.history.collections/HistoryAtomsQueue-1.md), [`HistoryAtomsSet`](../arlecchino.atoms.history.collections/HistoryAtomsSet-1.md), [`HistoryAtomsStack`](../arlecchino.atoms.history.collections/HistoryAtomsStack-1.md)

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

