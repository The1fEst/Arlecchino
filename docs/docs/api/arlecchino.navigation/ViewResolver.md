---
title: "ViewResolver"
sidebar_label: "ViewResolver"
---

# ViewResolver class

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

Turns a route into a view by asking each registered factory in turn. Each screen is built inside its own container scope, so a scoped service is released when the screen goes away.

```csharp
public sealed class ViewResolver
```

## Constructors

| Member | Summary |
|---|---|
| [`ViewResolver(IEnumerable<IArlecchinoViewFactory>, IServiceScopeFactory)`](#viewresolver-ienumerable-iarlecchinoviewfactory-iservicescopefactory) | Creates the resolver. |

## Methods

| Member | Summary |
|---|---|
| [`Create(ViewRoute)`](#create-viewroute) | Builds the view for a route, in a scope of its own. |

## Constructors in detail

### `ViewResolver(IEnumerable<IArlecchinoViewFactory>, IServiceScopeFactory)` {#viewresolver-ienumerable-iarlecchinoviewfactory-iservicescopefactory}

```csharp
public ViewResolver(IEnumerable<IArlecchinoViewFactory> factories, IServiceScopeFactory scopes);
```

Creates the resolver.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `factories` | `IEnumerable<T>`&lt;[`IArlecchinoViewFactory`](../arlecchino.navigation/IArlecchinoViewFactory.md)&gt; | Factories to ask, in the order they were registered. |
| `scopes` | `IServiceScopeFactory` | Where the per-screen scopes come from. |

## Methods in detail

### `Create(ViewRoute)` {#create-viewroute}

```csharp
public ActiveView Create(ViewRoute route);
```

Builds the view for a route, in a scope of its own.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `route` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | The route to show. |

**Returns** [`ActiveView`](../arlecchino.navigation/ActiveView.md) — The view and the scope it lives in; dispose it when navigating away.

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | No factory owns the route; the message names both ways to register it. |

