---
title: IArlecchinoStartup
sidebar_label: IArlecchinoStartup
---

# IArlecchinoStartup interface

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Work to do once before the first frame. Several may be registered; they run in registration order, and the last route that is not [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) decides where the application opens.

```csharp
public interface IArlecchinoStartup
```

## Methods

| Member | Summary |
|---|---|
| [`Start()`](#start) | Runs the work. Return a route to open somewhere other than the configured start, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) to leave that alone. |

## Methods in detail

### `Start()` {#start}

```csharp
public ViewRoute Start();
```

Runs the work. Return a route to open somewhere other than the configured start, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) to leave that alone.

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — Where to open, or none.

