---
title: Arlecchino.Navigation
sidebar_label: Arlecchino.Navigation
sidebar_position: 0
---

# Arlecchino.Navigation

## Classes

| Type | Summary |
|---|---|
| [`ActiveView`](ActiveView.md) | A screen together with the container scope it was built from. Navigating away disposes the screen and then the scope, so whatever it took from the container goes with it. |
| [`Navigator`](Navigator.md) | Holds the screen being shown and the history behind it. Routes returned from handlers pass through here, and the view that leaves is disposed if it asked to be. |
| [`Routes`](Routes.md) | Routes of the screens that ship with the framework. |
| [`ViewLifetime`](ViewLifetime.md) | How long the screen is on. Take it in a view's constructor to tie background work and subscriptions to the screen, and navigating away cancels the token and releases everything registered here. |
| [`ViewResolver`](ViewResolver.md) | Turns a route into a view by asking each registered factory in turn. Each screen is built inside its own container scope, so a scoped service is released when the screen goes away. |

## Structs

| Type | Summary |
|---|---|
| [`ViewRoute`](ViewRoute.md) | Where to navigate: a name in a struct, compared ordinally. Routes are strings rather than an enum because the framework has to name a screen without seeing the application's types — the generated `ViewKind` lives in your assembly, not in Arlecchino. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoLayout`](IArlecchinoLayout.md) | The frame every view is drawn inside, from one object that outlives the views. A view asks the [`Surface`](../arlecchino.rendering/Surface.md) for its content and gets the region the layout left it, so it never knows. |
| [`IArlecchinoView`](IArlecchinoView.md) | A screen, built from the container and living as long as its route is shown. Implement `IDisposable` to be told when it goes away. |
| [`IArlecchinoViewFactory`](IArlecchinoViewFactory.md) | Builds views for routes. Register one with `AddViewFactory<T>()` to serve a whole family of routes at once — a plugin directory, or routes carrying an ID in the name. |

