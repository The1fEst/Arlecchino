---
title: "IArlecchinoFocusable"
sidebar_label: "IArlecchinoFocusable"
---

# IArlecchinoFocusable interface

**Namespace:** `Arlecchino.Focus` &middot; **Assembly:** `Arlecchino`

Something inside a view that can hold the cursor: a pane, a list, a form. Put them in a [`FocusRing`](../arlecchino.focus/FocusRing.md) and the cycling, the routing and the mouse are handled for you.

```csharp
public interface IArlecchinoFocusable
```

**Implemented by** [`FocusRing`](../arlecchino.focus/FocusRing.md), [`FocusablePane`](../arlecchino.focus/FocusablePane.md), [`Form`](../arlecchino.forms/Form.md), [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`ListBox`](../arlecchino.widgets.lists/ListBox-1.md), [`ScrollPane`](../arlecchino.widgets.lists/ScrollPane.md), [`Table`](../arlecchino.widgets.lists/Table-1.md), [`Tabs`](../arlecchino.widgets.lists/Tabs.md), [`Tree`](../arlecchino.widgets.lists/Tree-1.md), [`TextView`](../arlecchino.widgets.readouts/TextView.md)

## Properties

| Member | Summary |
|---|---|
| [`IsFocused`](#isfocused) | Set by the ring. Draw the element differently while it is false. |

## Methods

| Member | Summary |
|---|---|
| [`Handle(KeyPress)`](#handle-keypress) | Handles a key while this element has the focus. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Handles a mouse event wherever it landed. Claiming one also moves the focus here, so a click selects the pane it hit. |
| [`Hints()`](#hints) | Keys this element reacts to while it holds the focus. The screen collects them down the chain of focus, so the hints box follows the cursor instead of listing the same keys everywhere. |
| [`MoveFocus(FocusDirection)`](#movefocus-focusdirection) | Moves the focus one step inside this element, for something that holds focusable parts of its own — a [`FocusRing`](../arlecchino.focus/FocusRing.md) nested in another one, or a widget made of several fields. Answering `false` hands the step back to the surrounding ring, which is what lets `Tab` walk into a composite element, through its parts and out the far side. An element with nothing inside it leaves this alone: the default says the step was not taken, and the ring moves to the next element as it always has. |

## Properties in detail

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Set by the ring. Draw the element differently while it is false.

**Type** `bool`

## Methods in detail

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public FocusResult Handle(KeyPress key);
```

Handles a key while this element has the focus.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

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

### `Hints()` {#hints}

```csharp
public virtual ValueTuple<string, string>[] Hints();
```

Keys this element reacts to while it holds the focus. The screen collects them down the chain of focus, so the hints box follows the cursor instead of listing the same keys everywhere.

**Returns** `ValueTuple<T1, T2>`&lt;`string`, `string`&gt;\[\] — Pairs of key and description, empty when the element has nothing to say.

### `MoveFocus(FocusDirection)` {#movefocus-focusdirection}

```csharp
public bool MoveFocus(FocusDirection direction);
```

Moves the focus one step inside this element, for something that holds focusable parts of its own — a [`FocusRing`](../arlecchino.focus/FocusRing.md) nested in another one, or a widget made of several fields. Answering `false` hands the step back to the surrounding ring, which is what lets `Tab` walk into a composite element, through its parts and out the far side. An element with nothing inside it leaves this alone: the default says the step was not taken, and the ring moves to the next element as it always has.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `direction` | [`FocusDirection`](../arlecchino.focus/FocusDirection.md) | Which way the focus is going. |

**Returns** `bool` — Whether the focus moved inside this element.

