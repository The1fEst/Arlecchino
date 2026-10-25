---
title: "IReadableAtom"
sidebar_label: "IReadableAtom"
---

# IReadableAtom interface

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

What every atom has in common, whatever it holds: it can be listened to, and it can be written out. It is what a widget takes when it shows a value whose type it has no use for.

```csharp
public interface IReadableAtom
```

**Implemented by** [`AsyncAtom`](../arlecchino.atoms/AsyncAtom-1.md), [`Atom`](../arlecchino.atoms/Atom-1.md), [`Computed`](../arlecchino.atoms/Computed-1.md), [`IReadableAtom`](../arlecchino.atoms/IReadableAtom-1.md), [`AtomsCollectionBase`](../arlecchino.atoms.collections/AtomsCollectionBase.md), [`AtomsList`](../arlecchino.atoms.collections/AtomsList-1.md), [`AtomsMap`](../arlecchino.atoms.collections/AtomsMap-2.md), [`AtomsQueue`](../arlecchino.atoms.collections/AtomsQueue-1.md), [`AtomsSet`](../arlecchino.atoms.collections/AtomsSet-1.md), [`AtomsStack`](../arlecchino.atoms.collections/AtomsStack-1.md), [`HistoryAtom`](../arlecchino.atoms.history/HistoryAtom-1.md), [`HistoryAtomsList`](../arlecchino.atoms.history.collections/HistoryAtomsList-1.md), [`HistoryAtomsMap`](../arlecchino.atoms.history.collections/HistoryAtomsMap-2.md), [`HistoryAtomsQueue`](../arlecchino.atoms.history.collections/HistoryAtomsQueue-1.md), [`HistoryAtomsSet`](../arlecchino.atoms.history.collections/HistoryAtomsSet-1.md), [`HistoryAtomsStack`](../arlecchino.atoms.history.collections/HistoryAtomsStack-1.md)

## Fields

| Member | Summary |
|---|---|
| [`UndefinedValue`](#undefinedvalue) | What an atom holding nothing writes out as. |

## Methods

| Member | Summary |
|---|---|
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the value changes. |

## Fields in detail

### `UndefinedValue` {#undefinedvalue}

```csharp
public static string UndefinedValue { get; }
```

What an atom holding nothing writes out as.

**Type** `string`

## Methods in detail

### `Subscribe(Action)` {#subscribe-action}

```csharp
public IDisposable Subscribe(Action listener);
```

Calls back whenever the value changes.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | What to run on change. |

**Returns** `IDisposable` — Dispose it to stop listening; a view that subscribes must do so when it goes away.

