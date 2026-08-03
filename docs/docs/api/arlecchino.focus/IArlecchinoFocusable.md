---
title: IArlecchinoFocusable
sidebar_label: IArlecchinoFocusable
---

# IArlecchinoFocusable interface

**Namespace:** `Arlecchino.Focus` &middot; **Assembly:** `Arlecchino`

Something inside a view that can hold the cursor: a pane, a list, a form. Put them in a [`FocusRing`](../arlecchino.focus/FocusRing.md) and the cycling, the routing and the mouse are handled for you.

```csharp
public interface IArlecchinoFocusable
```

**Implemented by** [`FocusablePane`](../arlecchino.focus/FocusablePane.md), [`Form`](../arlecchino.forms/Form.md), [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`ListBox`](../arlecchino.widgets.lists/ListBox-1.md), [`ScrollPane`](../arlecchino.widgets.lists/ScrollPane.md), [`Table`](../arlecchino.widgets.lists/Table-1.md), [`Tabs`](../arlecchino.widgets.lists/Tabs.md), [`Tree`](../arlecchino.widgets.lists/Tree-1.md), [`TextView`](../arlecchino.widgets.readouts/TextView.md)

## Properties

| Member | Summary |
|---|---|
| [`IsFocused`](#isfocused) | Set by the ring. Draw the element differently while it is false. |

## Methods

| Member | Summary |
|---|---|
| [`Handle(ConsoleKeyInfo)`](#handle-consolekeyinfo) | Handles a key while this element has the focus. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Handles a mouse event wherever it landed. Claiming one also moves the focus here, so a click selects the pane it hit. |

## Properties in detail

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Set by the ring. Draw the element differently while it is false.

**Type** `bool`

## Methods in detail

### `Handle(ConsoleKeyInfo)` {#handle-consolekeyinfo}

```csharp
public FocusResult Handle(ConsoleKeyInfo key);
```

Handles a key while this element has the focus.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What was done with it.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Handles a mouse event wherever it landed. Claiming one also moves the focus here, so a click selects the pane it hit.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event, in frame coordinates. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What was done with it.

