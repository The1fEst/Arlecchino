---
title: MouseEvent
sidebar_label: MouseEvent
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
| [`MouseEvent(MouseAction, MouseButton, int, int, ConsoleModifiers)`](#mouseevent-mouseaction-mousebutton-int-int-consolemodifiers) | A mouse report from the terminal. Coordinates are frame cells — the same ones [`Surface.WriteAt`](../arlecchino.rendering/Surface.md#writeat-int-int-string-iarlecchinocolor) and [`SurfaceRegion.Contains`](../arlecchino.rendering/SurfaceRegion.md#contains-int-int) use, so hit-testing is comparing numbers. |

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
| [`Deconstruct(MouseAction&, MouseButton&, Int32&, Int32&, ConsoleModifiers&)`](#deconstruct-mouseaction-mousebutton-int32-int32-consolemodifiers) |  |
| [`Equals(object)`](#equals-object) |  |
| [`Equals(MouseEvent)`](#equals-mouseevent) |  |
| [`GetHashCode()`](#gethashcode) |  |
| [`ToString()`](#tostring) |  |

## Operators

| Member | Summary |
|---|---|
| [`operator Inequality(MouseEvent, MouseEvent)`](#operator-inequality-mouseevent-mouseevent) |  |
| [`operator Equality(MouseEvent, MouseEvent)`](#operator-equality-mouseevent-mouseevent) |  |

## Constructors in detail

### `MouseEvent(MouseAction, MouseButton, int, int, ConsoleModifiers)` {#mouseevent-mouseaction-mousebutton-int-int-consolemodifiers}

```csharp
public MouseEvent(MouseAction Action, MouseButton Button, int Row, int Column, ConsoleModifiers Modifiers);
```

A mouse report from the terminal. Coordinates are frame cells — the same ones [`Surface.WriteAt`](../arlecchino.rendering/Surface.md#writeat-int-int-string-iarlecchinocolor) and [`SurfaceRegion.Contains`](../arlecchino.rendering/SurfaceRegion.md#contains-int-int) use, so hit-testing is comparing numbers.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Action` | [`MouseAction`](../arlecchino.input/MouseAction.md) | What the mouse did. |
| `Button` | [`MouseButton`](../arlecchino.input/MouseButton.md) | Which button, or [`MouseButton.None`](../arlecchino.input/MouseButton.md) for the wheel. |
| `Row` | `int` | Zero-based row in the frame. |
| `Column` | `int` | Zero-based column in the frame. |
| `Modifiers` | `ConsoleModifiers` | Modifiers held at the time. |

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
public ConsoleModifiers Modifiers { get; init; }
```

Modifiers held at the time.

**Type** `ConsoleModifiers`

### `Row` {#row}

```csharp
public int Row { get; init; }
```

Zero-based row in the frame.

**Type** `int`

## Methods in detail

### `Deconstruct(MouseAction&, MouseButton&, Int32&, Int32&, ConsoleModifiers&)` {#deconstruct-mouseaction-mousebutton-int32-int32-consolemodifiers}

```csharp
public void Deconstruct(out MouseAction Action, out MouseButton Button, out int Row, out int Column, out ConsoleModifiers Modifiers);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Action` | [`MouseAction`](../arlecchino.input/MouseAction.md) |  |
| `Button` | [`MouseButton`](../arlecchino.input/MouseButton.md) |  |
| `Row` | `int` |  |
| `Column` | `int` |  |
| `Modifiers` | `ConsoleModifiers` |  |

### `Equals(object)` {#equals-object}

```csharp
public virtual bool Equals(object obj);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `obj` | `object` |  |

**Returns** `bool`

### `Equals(MouseEvent)` {#equals-mouseevent}

```csharp
public bool Equals(MouseEvent other);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) |  |

**Returns** `bool`

### `GetHashCode()` {#gethashcode}

```csharp
public override int GetHashCode();
```

**Returns** `int`

### `ToString()` {#tostring}

```csharp
public virtual string ToString();
```

**Returns** `string`

## Operators in detail

### `operator Inequality(MouseEvent, MouseEvent)` {#operator-inequality-mouseevent-mouseevent}

```csharp
public static bool op_Inequality(MouseEvent left, MouseEvent right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) |  |
| `right` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) |  |

**Returns** `bool`

### `operator Equality(MouseEvent, MouseEvent)` {#operator-equality-mouseevent-mouseevent}

```csharp
public static bool op_Equality(MouseEvent left, MouseEvent right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) |  |
| `right` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) |  |

**Returns** `bool`

