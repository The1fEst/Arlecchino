---
title: "IResettableAtom"
sidebar_label: "IResettableAtom"
---

# IResettableAtom interface

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

An atom that knows where it began and can be put back there. One keeping no record of its own cannot tell the value it was built with from the one it holds.

```csharp
public interface IResettableAtom
```

**Implemented by** [`HistoryAtom`](../arlecchino.atoms.history/HistoryAtom-1.md)

## Methods

| Member | Summary |
|---|---|
| [`TryReset()`](#tryreset) | Writes back the value the atom started at. |

## Methods in detail

### `TryReset()` {#tryreset}

```csharp
public bool TryReset();
```

Writes back the value the atom started at.

**Returns** `bool` — `false` when the atom has no starting value to go back to.

