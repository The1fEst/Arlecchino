---
title: FocusablePane
sidebar_label: FocusablePane
---

# FocusablePane class

**Namespace:** `Arlecchino.Focus` &middot; **Assembly:** `Arlecchino`

Wraps delegates as a focusable element, for a view that keeps its logic in methods rather than in objects — that is how the file picker holds its list and its places sidebar.

```csharp
public sealed class FocusablePane : IArlecchinoFocusable
```

**Implements** [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`FocusablePane(Func<ConsoleKeyInfo, FocusResult>, Func<MouseEvent, FocusResult>)`](#focusablepane-func-consolekeyinfo-focusresult-func-mouseevent-focusresult) | Creates the element. |

## Properties

| Member | Summary |
|---|---|
| [`IsFocused`](#isfocused) | Whether this element currently holds the focus. |

## Methods

| Member | Summary |
|---|---|
| [`Handle(ConsoleKeyInfo)`](#handle-consolekeyinfo) | Passes the key to the delegate. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Passes the mouse event to the delegate, if one was given. |

## Constructors in detail

### `FocusablePane(Func<ConsoleKeyInfo, FocusResult>, Func<MouseEvent, FocusResult>)` {#focusablepane-func-consolekeyinfo-focusresult-func-mouseevent-focusresult}

```csharp
public FocusablePane(
    Func<ConsoleKeyInfo, FocusResult> handle,
    Func<MouseEvent, FocusResult>? handleMouse = null);
```

Creates the element.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `handle` | `Func<T, TResult>`&lt;`ConsoleKeyInfo`, [`FocusResult`](../arlecchino.focus/FocusResult.md)&gt; | What to do with a key while focused. |
| `handleMouse` | `Func<T, TResult>`&lt;[`MouseEvent`](../arlecchino.input/MouseEvent.md), [`FocusResult`](../arlecchino.focus/FocusResult.md)&gt; | What to do with a mouse event; omit to ignore the mouse. |

## Properties in detail

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Whether this element currently holds the focus.

**Type** `bool`

## Methods in detail

### `Handle(ConsoleKeyInfo)` {#handle-consolekeyinfo}

```csharp
public FocusResult Handle(ConsoleKeyInfo key);
```

Passes the key to the delegate.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What the delegate decided.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Passes the mouse event to the delegate, if one was given.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event, in frame coordinates. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What the delegate decided, or ignored.

