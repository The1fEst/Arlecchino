---
title: "IArlecchinoStore"
sidebar_label: "IArlecchinoStore"
---

# IArlecchinoStore interface

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A holder of application state: a class of atoms that outlive the screens reading them. Marking it is all the registration there is, and [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md) is the one for a single screen.

```csharp
public interface IArlecchinoStore
```

**Implemented by** [`ArlecchinoAsyncStore`](../arlecchino.atoms/ArlecchinoAsyncStore.md), [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md)

