---
title: "AtomsCollectionBase"
sidebar_label: "AtomsCollectionBase"
---

# AtomsCollectionBase class

**Namespace:** `Arlecchino.Atoms.Collections` &middot; **Assembly:** `Arlecchino.Core`

What the five collection atoms have in common: who is listening, the check that a change is being made on the drawing thread, and the seam a subclass watches changes through.

```csharp
public abstract class AtomsCollectionBase : IReadableAtom
```

**Implements** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md)  
**Derived types** [`AtomsList`](../arlecchino.atoms.collections/AtomsList-1.md), [`AtomsMap`](../arlecchino.atoms.collections/AtomsMap-2.md), [`AtomsQueue`](../arlecchino.atoms.collections/AtomsQueue-1.md), [`AtomsSet`](../arlecchino.atoms.collections/AtomsSet-1.md), [`AtomsStack`](../arlecchino.atoms.collections/AtomsStack-1.md)

## Constructors

| Member | Summary |
|---|---|
| [`AtomsCollectionBase()`](#atomscollectionbase) |  |

## Properties

| Member | Summary |
|---|---|
| [`IsObserved`](#isobserved) | Whether a subclass is listening, which decides whether the collection describes what changed. Describing a change copies what was there, so an unwatched collection skips it. |

## Methods

| Member | Summary |
|---|---|
| [`Subscribe(Action)`](#subscribe-action) | Calls back whenever the contents change. |

## Constructors in detail

### `AtomsCollectionBase()` {#atomscollectionbase}

```csharp
public AtomsCollectionBase();
```

## Properties in detail

### `IsObserved` {#isobserved}

```csharp
public virtual bool IsObserved { get; }
```

Whether a subclass is listening, which decides whether the collection describes what changed. Describing a change copies what was there, so an unwatched collection skips it.

**Type** `bool`

## Methods in detail

### `Subscribe(Action)` {#subscribe-action}

```csharp
public IDisposable Subscribe(Action listener);
```

Calls back whenever the contents change.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `listener` | `Action` | What to run on change. |

**Returns** `IDisposable` — Dispose it to stop listening.

