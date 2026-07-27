---
title: IArlecchinoStore
sidebar_label: IArlecchinoStore
---

# IArlecchinoStore interface

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A holder of application state — a class of atoms that outlive the screens reading them. Marking it is all the registration there is: the generator finds every store in the project and `AddGeneratedStores()` puts them in the container as singletons, built from their public constructor with the most parameters. Implement [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md) instead for state that belongs to one screen.

```csharp
public interface IArlecchinoStore
```

**Implemented by** [`IArlecchinoScopedStore`](../arlecchino.atoms/IArlecchinoScopedStore.md)

