---
title: "Navigator"
sidebar_label: "Navigator"
---

# Navigator class

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

Holds the screen being shown and the history behind it. Routes returned from handlers pass through here, and the view that leaves is disposed if it asked to be.

```csharp
public class Navigator
```

## Properties

| Member | Summary |
|---|---|
| [`CanGoBack`](#cangoback) | Whether there is somewhere to go back to. |
| [`CanGoForward`](#cangoforward) | Whether a step back can be retraced. |
| [`CurrentCommands`](#currentcommands) | Commands of the screen being shown, for the router and the palette. |
| [`CurrentHints`](#currenthints) | What the hints box should show: whatever the screen returned, or its commands when it returned nothing. |
| [`CurrentRoute`](#currentroute) | The route being shown. |
| [`CurrentUsesLayout`](#currentuseslayout) | Whether the screen being shown wants the layout drawn around it. |

## Methods

| Member | Summary |
|---|---|
| [`Apply(ViewRoute)`](#apply-viewroute) | Goes to a route. Ignores [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) and the route already shown; anything else pushes the current one onto the back stack and drops the forward stack. |
| [`Back()`](#back) | Goes back one step in the history. |
| [`Draw()`](#draw) | Draws the current screen. Called once per frame. |
| [`Forward()`](#forward) | Retraces a step that was gone back from. |
| [`Handle(KeyPress)`](#handle-keypress) | Passes a key to the current screen and applies the route it returns. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Passes a mouse event to the current screen and applies the route it returns. |
| [`HandlePaste(string)`](#handlepaste-string) | Passes pasted text to the current screen and applies the route it returns. |
| [`Reload()`](#reload) | Builds the current screen again from scratch, losing its per-screen state. |

## Properties in detail

### `CanGoBack` {#cangoback}

```csharp
public bool CanGoBack { get; }
```

Whether there is somewhere to go back to.

**Type** `bool`

### `CanGoForward` {#cangoforward}

```csharp
public bool CanGoForward { get; }
```

Whether a step back can be retraced.

**Type** `bool`

### `CurrentCommands` {#currentcommands}

```csharp
public IReadOnlyList<ViewCommand> CurrentCommands { get; }
```

Commands of the screen being shown, for the router and the palette.

**Type** `IReadOnlyList<T>`&lt;[`ViewCommand`](../arlecchino.commands/ViewCommand.md)&gt;

### `CurrentHints` {#currenthints}

```csharp
public ValueTuple<string, string>[] CurrentHints { get; }
```

What the hints box should show: whatever the screen returned, or its commands when it returned nothing.

**Type** `ValueTuple<T1, T2>`&lt;`string`, `string`&gt;\[\]

### `CurrentRoute` {#currentroute}

```csharp
public ViewRoute CurrentRoute { get; }
```

The route being shown.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

### `CurrentUsesLayout` {#currentuseslayout}

```csharp
public bool CurrentUsesLayout { get; }
```

Whether the screen being shown wants the layout drawn around it.

**Type** `bool`

## Methods in detail

### `Apply(ViewRoute)` {#apply-viewroute}

```csharp
public void Apply(ViewRoute route);
```

Goes to a route. Ignores [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) and the route already shown; anything else pushes the current one onto the back stack and drops the forward stack.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `route` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | Where to go. |

### `Back()` {#back}

```csharp
public bool Back();
```

Goes back one step in the history.

**Returns** `bool` — `false` when there was nothing to go back to.

### `Draw()` {#draw}

```csharp
public void Draw();
```

Draws the current screen. Called once per frame.

### `Forward()` {#forward}

```csharp
public bool Forward();
```

Retraces a step that was gone back from.

**Returns** `bool` — `false` when there was nothing to retrace.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public void Handle(KeyPress key);
```

Passes a key to the current screen and applies the route it returns.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public void HandleMouse(MouseEvent mouse);
```

Passes a mouse event to the current screen and applies the route it returns.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event, in frame coordinates. |

### `HandlePaste(string)` {#handlepaste-string}

```csharp
public void HandlePaste(string text);
```

Passes pasted text to the current screen and applies the route it returns.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was pasted. |

### `Reload()` {#reload}

```csharp
public void Reload();
```

Builds the current screen again from scratch, losing its per-screen state.

