---
title: FocusResult
sidebar_label: FocusResult
---

# FocusResult struct

**Namespace:** `Arlecchino.Focus` &middot; **Assembly:** `Arlecchino`

What a focusable element did with an event: whether it claimed it, and whether that means going somewhere.

```csharp
public readonly struct FocusResult : IEquatable<FocusResult>
```

**Implements** `IEquatable<T>`&lt;[`FocusResult`](../arlecchino.focus/FocusResult.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`FocusResult(bool, ViewRoute)`](#focusresult-bool-viewroute) | What a focusable element did with an event: whether it claimed it, and whether that means going somewhere. |

## Properties

| Member | Summary |
|---|---|
| [`Handled`](#handled) | The event was mine and nothing else should see it. |
| [`Ignored`](#ignored) | The event was not mine; whoever asked should keep looking. |
| [`Route`](#route) | Where to navigate, if anywhere. |
| [`WasHandled`](#washandled) | Whether the element claimed the event. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out bool, out ViewRoute)`](#deconstruct-out-bool-out-viewroute) |  |
| [`Navigate(ViewRoute)`](#navigate-viewroute) | The event was mine and the screen should navigate. |

## Constructors in detail

### `FocusResult(bool, ViewRoute)` {#focusresult-bool-viewroute}

```csharp
public FocusResult(bool WasHandled, ViewRoute Route);
```

What a focusable element did with an event: whether it claimed it, and whether that means going somewhere.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `WasHandled` | `bool` | Whether the element claimed the event. |
| `Route` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | Where to navigate, if anywhere. |

## Properties in detail

### `Handled` {#handled}

```csharp
public static FocusResult Handled { get; }
```

The event was mine and nothing else should see it.

**Type** [`FocusResult`](../arlecchino.focus/FocusResult.md)

### `Ignored` {#ignored}

```csharp
public static FocusResult Ignored { get; }
```

The event was not mine; whoever asked should keep looking.

**Type** [`FocusResult`](../arlecchino.focus/FocusResult.md)

### `Route` {#route}

```csharp
public ViewRoute Route { get; init; }
```

Where to navigate, if anywhere.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

### `WasHandled` {#washandled}

```csharp
public bool WasHandled { get; init; }
```

Whether the element claimed the event.

**Type** `bool`

## Methods in detail

### `Deconstruct(out bool, out ViewRoute)` {#deconstruct-out-bool-out-viewroute}

```csharp
public void Deconstruct(out bool WasHandled, out ViewRoute Route);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `WasHandled` | `bool` |  |
| `Route` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) |  |

### `Navigate(ViewRoute)` {#navigate-viewroute}

```csharp
public static FocusResult Navigate(ViewRoute route);
```

The event was mine and the screen should navigate.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `route` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | Where to go. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — The result to return.

