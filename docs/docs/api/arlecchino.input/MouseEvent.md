---
title: "MouseEvent"
sidebar_label: "MouseEvent"
---

# MouseEvent struct

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

A mouse report from the terminal. Coordinates are frame cells — the same ones [`Surface.WriteAt`](../arlecchino.rendering/Surface.md#writeat-int-int-string-iarlecchinocolor) and [`SurfaceRegion.Contains`](../arlecchino.rendering/SurfaceRegion.md#contains-int-int) use, so hit-testing is comparing numbers.

```csharp
public readonly struct MouseEvent : IEquatable<MouseEvent>
```

**Implements** `IEquatable<T>`&lt;[`MouseEvent`](../arlecchino.input/MouseEvent.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`MouseEvent(MouseAction, MouseButton, int, int, KeyModifiers)`](#mouseevent-mouseaction-mousebutton-int-int-keymodifiers) | A mouse report from the terminal. Coordinates are frame cells — the same ones [`Surface.WriteAt`](../arlecchino.rendering/Surface.md#writeat-int-int-string-iarlecchinocolor) and [`SurfaceRegion.Contains`](../arlecchino.rendering/SurfaceRegion.md#contains-int-int) use, so hit-testing is comparing numbers. |

## Properties

| Member | Summary |
|---|---|
| [`Action`](#action) | What the mouse did. |
| [`Button`](#button) | Which button, or [`MouseButton.None`](../arlecchino.input/MouseButton.md) for the wheel. |
| [`Column`](#column) | Zero-based column in the frame. |
| [`IsLeftClick`](#isleftclick) | Whether this is the left button going down — the usual "click" test. |
| [`IsScroll`](#isscroll) | Whether this is a wheel event in either direction. |
| [`Modifiers`](#modifiers) | Modifiers held at the time. |
| [`Row`](#row) | Zero-based row in the frame. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out MouseAction, out MouseButton, out int, out int, out KeyModifiers)`](#deconstruct-out-mouseaction-out-mousebutton-out-int-out-int-out-keymodifiers) |  |

## Constructors in detail

### `MouseEvent(MouseAction, MouseButton, int, int, KeyModifiers)` {#mouseevent-mouseaction-mousebutton-int-int-keymodifiers}

```csharp
public MouseEvent(
    MouseAction Action,
    MouseButton Button,
    int Row,
    int Column,
    KeyModifiers Modifiers);
```

A mouse report from the terminal. Coordinates are frame cells — the same ones [`Surface.WriteAt`](../arlecchino.rendering/Surface.md#writeat-int-int-string-iarlecchinocolor) and [`SurfaceRegion.Contains`](../arlecchino.rendering/SurfaceRegion.md#contains-int-int) use, so hit-testing is comparing numbers.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Action` | [`MouseAction`](../arlecchino.input/MouseAction.md) | What the mouse did. |
| `Button` | [`MouseButton`](../arlecchino.input/MouseButton.md) | Which button, or [`MouseButton.None`](../arlecchino.input/MouseButton.md) for the wheel. |
| `Row` | `int` | Zero-based row in the frame. |
| `Column` | `int` | Zero-based column in the frame. |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | Modifiers held at the time. |

## Properties in detail

### `Action` {#action}

```csharp
public MouseAction Action { get; init; }
```

What the mouse did.

**Type** [`MouseAction`](../arlecchino.input/MouseAction.md)

### `Button` {#button}

```csharp
public MouseButton Button { get; init; }
```

Which button, or [`MouseButton.None`](../arlecchino.input/MouseButton.md) for the wheel.

**Type** [`MouseButton`](../arlecchino.input/MouseButton.md)

### `Column` {#column}

```csharp
public int Column { get; init; }
```

Zero-based column in the frame.

**Type** `int`

### `IsLeftClick` {#isleftclick}

```csharp
public bool IsLeftClick { get; }
```

Whether this is the left button going down — the usual "click" test.

**Type** `bool`

### `IsScroll` {#isscroll}

```csharp
public bool IsScroll { get; }
```

Whether this is a wheel event in either direction.

**Type** `bool`

### `Modifiers` {#modifiers}

```csharp
public KeyModifiers Modifiers { get; init; }
```

Modifiers held at the time.

**Type** [`KeyModifiers`](../arlecchino.input/KeyModifiers.md)

### `Row` {#row}

```csharp
public int Row { get; init; }
```

Zero-based row in the frame.

**Type** `int`

## Methods in detail

### `Deconstruct(out MouseAction, out MouseButton, out int, out int, out KeyModifiers)` {#deconstruct-out-mouseaction-out-mousebutton-out-int-out-int-out-keymodifiers}

```csharp
public void Deconstruct(
    out MouseAction Action,
    out MouseButton Button,
    out int Row,
    out int Column,
    out KeyModifiers Modifiers);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Action` | [`MouseAction`](../arlecchino.input/MouseAction.md) |  |
| `Button` | [`MouseButton`](../arlecchino.input/MouseButton.md) |  |
| `Row` | `int` |  |
| `Column` | `int` |  |
| `Modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) |  |

