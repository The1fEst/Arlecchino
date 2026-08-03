---
title: Arlecchino.Navigation
sidebar_label: Arlecchino.Navigation
sidebar_position: 0
---

# Arlecchino.Navigation

## Classes

| Type | Summary |
|---|---|
| [`ActiveView`](ActiveView.md) | A screen together with the container scope it was built from. Navigating away disposes both, in that order, so anything the screen took from the container — a database context, a connection, a file handle — goes away with the screen rather than living as long as the application. |
| [`Navigator`](Navigator.md) | Holds the screen being shown and the history behind it. Routes returned from handlers pass through here, and the view that leaves is disposed if it asked to be. |
| [`Routes`](Routes.md) | Routes of the screens that ship with the framework. |
| [`ViewLifetime`](ViewLifetime.md) | How long the screen is on. Take it in a view's constructor to tie background work, subscriptions and anything else disposable to the screen: it is scoped, so navigating away cancels the token and releases everything registered here. Without it a load that outlives its screen keeps running and hands its result to a view nobody can see any more. |
| [`ViewResolver`](ViewResolver.md) | Turns a route into a view by asking each registered factory in registration order. Each screen is built inside its own container scope, which is what lets a view take a scoped service and have it released the moment the screen goes away. |

## Structs

| Type | Summary |
|---|---|
| [`ViewRoute`](ViewRoute.md) | Where to navigate: a name in a struct, compared ordinally. Routes are strings rather than an enum because the framework has to name a screen without seeing the application's types — the generated `ViewKind` lives in your assembly, not in Arlecchino. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoLayout`](IArlecchinoLayout.md) | The frame every view is drawn inside: a band along the top, a bar along the bottom, whatever a screen of this application always has around it. It is one object for the whole application rather than one per screen, so what it holds outlives the view — a row of tabs keeps its scroll position when a screen is left and come back to, which is the whole reason a header is worth having in one place instead of drawn again by every view. [`IArlecchinoLayout.Draw`](../arlecchino.navigation/IArlecchinoLayout.md#draw-surfaceregion-action-surfaceregion) is handed the room there is and a delegate that draws the view. Where that delegate is called is where the view goes, and how much it is given is what the view thinks its screen is — a view asks the [`Surface`](../arlecchino.rendering/Surface.md) for its content and gets the region the layout left it, so no view has to know it is inside one. |
| [`IArlecchinoView`](IArlecchinoView.md) | A screen. Constructor parameters come from the container, and the instance lives as long as the route is shown — navigating away and back builds a new one, so per-screen state can live in fields. Implement `IDisposable` to be told when the screen goes away. |
| [`IArlecchinoViewFactory`](IArlecchinoViewFactory.md) | Builds views for routes. Register one with `AddViewFactory<T>()` to serve a whole family of routes at once — a plugin directory, or routes carrying an id in the name. |

