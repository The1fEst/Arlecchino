---
title: "Form"
sidebar_label: "Form"
---

# Form class

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

A column of fields with their values lined up, and a help line under the selected one. It holds no values of its own, and the atom behind each field decides whether an edit can be undone.

```csharp
public sealed class Form : IArlecchinoInteractiveWidget, IArlecchinoWidget, IArlecchinoFocusable
```

**Implements** [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`Form(ArlecchinoState, ArlecchinoOptions)`](#form-arlecchinostate-arlecchinooptions) | Creates the form. |

## Properties

| Member | Summary |
|---|---|
| [`Current`](#current) | The selected field, or `null` when the form has none. |
| [`Fields`](#fields) | The rows, top to bottom. |
| [`IsFocused`](#isfocused) | Whether the form has focus, which decides how strongly the selection is drawn. |
| [`SelectedIndex`](#selectedindex) | Index of the selected field. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the fields with their labels aligned, scrolled, so the selection stays in view, and returns the rows below the last one written. Buttons are left out of the alignment, since they have no value to line up against. |
| [`Handle(KeyPress)`](#handle-keypress) | Moves through the fields, opens one, or clears it. For a view that is nothing but a form; views that mix a form with other panes hand it to the focus ring instead. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Scrolls with the wheel and selects with a click. Clicking the already selected field opens it, so a double click reads as select-then-edit. |
| [`Hints()`](#hints) | What the form does with keys, worded and bound as the application configured it, ready to be shown in the hint line. |

## Constructors in detail

### `Form(ArlecchinoState, ArlecchinoOptions)` {#form-arlecchinostate-arlecchinooptions}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public Form(ArlecchinoState state, ArlecchinoOptions options);
```

Creates the form.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `state` | [`ArlecchinoState`](../arlecchino.state/ArlecchinoState.md) | Where fields open their dialogs. |
| `options` | [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md) | Supplies the keymap and the wording. |

## Properties in detail

### `Current` {#current}

```csharp
public Field? Current { get; }
```

The selected field, or `null` when the form has none.

**Type** [`Field`](../arlecchino.forms/Field.md)

### `Fields` {#fields}

```csharp
public IReadOnlyList<Field> Fields { get; init; }
```

The rows, top to bottom.

**Type** `IReadOnlyList<T>`&lt;[`Field`](../arlecchino.forms/Field.md)&gt;

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Whether the form has focus, which decides how strongly the selection is drawn.

**Type** `bool`

### `SelectedIndex` {#selectedindex}

```csharp
public int SelectedIndex { get; }
```

Index of the selected field.

**Type** `int`

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the fields with their labels aligned, scrolled, so the selection stays in view, and returns the rows below the last one written. Buttons are left out of the alignment, since they have no value to line up against.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw, help line included. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the fields, which is empty when they filled it.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public FocusResult Handle(KeyPress key);
```

Moves through the fields, opens one, or clears it. For a view that is nothing but a form; views that mix a form with other panes hand it to the focus ring instead.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What was done with it, and where to go.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Scrolls with the wheel and selects with a click. Clicking the already selected field opens it, so a double click reads as select-then-edit.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What was done with it, and where to go.

### `Hints()` {#hints}

```csharp
public ValueTuple<string, string>[] Hints();
```

What the form does with keys, worded and bound as the application configured it, ready to be shown in the hint line.

**Returns** `ValueTuple<T1, T2>`&lt;`string`, `string`&gt;\[\] — The key and its description, one pair per action.

