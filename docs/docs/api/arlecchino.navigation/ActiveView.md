---
title: ActiveView
sidebar_label: ActiveView
---

# ActiveView class

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

A screen together with the container scope it was built from. Navigating away disposes both, in that order, so anything the screen took from the container — a database context, a connection, a file handle — goes away with the screen rather than living as long as the application.

```csharp
public sealed class ActiveView : IDisposable
```

**Implements** `IDisposable`

## Constructors

| Member | Summary |
|---|---|
| [`ActiveView(IArlecchinoView, IServiceScope)`](#activeview-iarlecchinoview-iservicescope) | Pairs a screen with its scope. |

## Properties

| Member | Summary |
|---|---|
| [`View`](#view) | The screen being shown. |

## Methods

| Member | Summary |
|---|---|
| [`Dispose()`](#dispose) | Disposes the screen if it asked to be, then the scope behind it. |

## Constructors in detail

### `ActiveView(IArlecchinoView, IServiceScope)` {#activeview-iarlecchinoview-iservicescope}

```csharp
public ActiveView(IArlecchinoView view, IServiceScope scope);
```

Pairs a screen with its scope.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `view` | [`IArlecchinoView`](../arlecchino.navigation/IArlecchinoView.md) | The screen. |
| `scope` | `IServiceScope` | The scope it was built from. |

## Properties in detail

### `View` {#view}

```csharp
public IArlecchinoView View { get; }
```

The screen being shown.

**Type** [`IArlecchinoView`](../arlecchino.navigation/IArlecchinoView.md)

## Methods in detail

### `Dispose()` {#dispose}

```csharp
public void Dispose();
```

Disposes the screen if it asked to be, then the scope behind it.

