---
title: "FocusRing"
sidebar_label: "FocusRing"
---

# FocusRing class

**Namespace:** `Arlecchino.Focus` &middot; **Assembly:** `Arlecchino`

The cycle of focusable elements inside one view: `Tab` and `Shift+Tab` move between them, everything else goes to the one that holds the focus.

```csharp
public sealed class FocusRing
```

## Constructors

| Member | Summary |
|---|---|
| [`FocusRing(ArlecchinoKeymap)`](#focusring-arlecchinokeymap) | Creates an empty ring. |

## Properties

| Member | Summary |
|---|---|
| [`Current`](#current) | The focused element, or `null` when the ring is empty. |
| [`Index`](#index) | Position of the focused element. |
| [`Items`](#items) | The elements, in the order they were added. |

## Methods

| Member | Summary |
|---|---|
| [`Add(IArlecchinoFocusable)`](#add-iarlecchinofocusable) | Adds an element. The first one added starts focused. |
| [`Focus(IArlecchinoFocusable)`](#focus-iarlecchinofocusable) | Moves the focus to a particular element, if it belongs to this ring. |
| [`FocusNext()`](#focusnext) | Moves the focus to the next element, wrapping around at the end. |
| [`FocusPrevious()`](#focusprevious) | Moves the focus to the previous element, wrapping around at the start. |
| [`Handle(KeyPress)`](#handle-keypress) | Moves the focus on the field keys, and otherwise hands the key to the focused element. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Offers the event to each element and moves the focus to whichever one claims it, so a click both selects a pane and acts inside it. |

## Constructors in detail

### `FocusRing(ArlecchinoKeymap)` {#focusring-arlecchinokeymap}

```csharp
public FocusRing(ArlecchinoKeymap keymap);
```

Creates an empty ring.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | Where the keys that move the focus come from. |

## Properties in detail

### `Current` {#current}

```csharp
public IArlecchinoFocusable? Current { get; }
```

The focused element, or `null` when the ring is empty.

**Type** [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

### `Index` {#index}

```csharp
public int Index { get; }
```

Position of the focused element.

**Type** `int`

### `Items` {#items}

```csharp
public IReadOnlyList<IArlecchinoFocusable> Items { get; }
```

The elements, in the order they were added.

**Type** `IReadOnlyList<T>`&lt;[`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)&gt;

## Methods in detail

### `Add(IArlecchinoFocusable)` {#add-iarlecchinofocusable}

```csharp
public void Add(IArlecchinoFocusable item);
```

Adds an element. The first one added starts focused.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md) | The element to add. |

### `Focus(IArlecchinoFocusable)` {#focus-iarlecchinofocusable}

```csharp
public void Focus(IArlecchinoFocusable item);
```

Moves the focus to a particular element, if it belongs to this ring.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `item` | [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md) | The element to focus. |

### `FocusNext()` {#focusnext}

```csharp
public void FocusNext();
```

Moves the focus to the next element, wrapping around at the end.

### `FocusPrevious()` {#focusprevious}

```csharp
public void FocusPrevious();
```

Moves the focus to the previous element, wrapping around at the start.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public ViewRoute Handle(KeyPress key);
```

Moves the focus on the field keys, and otherwise hands the key to the focused element.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — The route the element asked for, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none).

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public ViewRoute HandleMouse(MouseEvent mouse);
```

Offers the event to each element and moves the focus to whichever one claims it, so a click both selects a pane and acts inside it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event, in frame coordinates. |

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — The route the element asked for, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none).

