---
title: IArlecchinoViewFactory
sidebar_label: IArlecchinoViewFactory
---

# IArlecchinoViewFactory interface

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

Builds views for routes. Register one with `AddViewFactory<T>()` to serve a whole family of routes at once — a plugin directory, or routes carrying an id in the name.

```csharp
public interface IArlecchinoViewFactory
```

## Methods

| Member | Summary |
|---|---|
| [`TryCreate(IServiceProvider, ViewRoute, IArlecchinoView&)`](#trycreate-iserviceprovider-viewroute-iarlecchinoview) | Creates the view for a route. |

## Methods in detail

### `TryCreate(IServiceProvider, ViewRoute, IArlecchinoView&)` {#trycreate-iserviceprovider-viewroute-iarlecchinoview}

```csharp
public bool TryCreate(IServiceProvider services, ViewRoute route, out IArlecchinoView? view);
```

Creates the view for a route.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `services` | `IServiceProvider` | The scope this screen lives in. Resolve from it rather than from a captured container, so scoped services belong to the screen and go away with it. |
| `route` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | The route being shown. |
| `view` | [`IArlecchinoView`](../arlecchino.navigation/IArlecchinoView.md) | The view, when this factory owns the route. |

**Returns** `bool` — `false` for routes you do not own, so the next factory gets a turn.

