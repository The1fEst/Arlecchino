---
title: IArlecchinoScopedStore
sidebar_label: IArlecchinoScopedStore
---

# IArlecchinoScopedStore interface

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino.Core`

A store that lives as long as the screen that asked for it: navigating away disposes the scope and with it the store, and navigating back builds a fresh one. Registered by `AddGeneratedStores()` exactly as an [`IArlecchinoStore`](../arlecchino.atoms/IArlecchinoStore.md) is, only scoped.

```csharp
public interface IArlecchinoScopedStore : IArlecchinoStore
```

**Implements** [`IArlecchinoStore`](../arlecchino.atoms/IArlecchinoStore.md)

