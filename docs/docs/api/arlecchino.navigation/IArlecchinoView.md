---
title: "IArlecchinoView"
sidebar_label: "IArlecchinoView"
---

# IArlecchinoView interface

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

A screen, built from the container and living as long as its route is shown. Implement `IDisposable` to be told when it goes away.

```csharp
public interface IArlecchinoView
```

## Properties

| Member | Summary |
|---|---|
| [`Focus`](#focus) | What holds the focus inside this screen, normally the [`FocusRing`](../arlecchino.focus/FocusRing.md) the view built. It puts the keys of the focused widget at the top of the hints box, and keeps them in step as `Tab` moves. |
| [`UsesLayout`](#useslayout) | Whether the [`IArlecchinoLayout`](../arlecchino.navigation/IArlecchinoLayout.md) is drawn around this screen, where the application has one. Answer `false` for a screen that wants the whole terminal. |

## Methods

| Member | Summary |
|---|---|
| [`Commands()`](#commands) | Keys this screen reacts to, declared as data. They are checked before [`IArlecchinoView.Handle`](../arlecchino.navigation/IArlecchinoView.md#handle-keypress), listed in the command palette, and checked for conflicts with application commands. |
| [`Draw()`](#draw) | Draws the screen. Called once per frame against the shared surface. |
| [`Handle(KeyPress)`](#handle-keypress) | Handles a key the router did not claim: typing, arrows, list filters. Keys that belong to a command should be declared in [`IArlecchinoView.Commands`](../arlecchino.navigation/IArlecchinoView.md#commands) instead. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Handles a mouse event in frame coordinates. Only views that care about the mouse implement this. |
| [`HandlePaste(string)`](#handlepaste-string) | Handles text pasted into the terminal. It arrives as one block however long it is, so a screen that takes typed input should take this too rather than leaving pastes on the floor. |
| [`Hints()`](#hints) | What the hints box shows. Leave it empty and the box is built from [`IArlecchinoView.Commands`](../arlecchino.navigation/IArlecchinoView.md#commands), so a rebound key relabels itself. |

## Properties in detail

### `Focus` {#focus}

```csharp
public IArlecchinoFocusable? Focus { get; }
```

What holds the focus inside this screen, normally the [`FocusRing`](../arlecchino.focus/FocusRing.md) the view built. It puts the keys of the focused widget at the top of the hints box, and keeps them in step as `Tab` moves.

**Type** [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

### `UsesLayout` {#useslayout}

```csharp
public bool UsesLayout { get; }
```

Whether the [`IArlecchinoLayout`](../arlecchino.navigation/IArlecchinoLayout.md) is drawn around this screen, where the application has one. Answer `false` for a screen that wants the whole terminal.

**Type** `bool`

## Methods in detail

### `Commands()` {#commands}

```csharp
public IReadOnlyList<ViewCommand> Commands();
```

Keys this screen reacts to, declared as data. They are checked before [`IArlecchinoView.Handle`](../arlecchino.navigation/IArlecchinoView.md#handle-keypress), listed in the command palette, and checked for conflicts with application commands.

**Returns** `IReadOnlyList<T>`&lt;[`ViewCommand`](../arlecchino.commands/ViewCommand.md)&gt; — The commands of this screen.

### `Draw()` {#draw}

```csharp
public void Draw();
```

Draws the screen. Called once per frame against the shared surface.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public ViewRoute Handle(KeyPress key);
```

Handles a key the router did not claim: typing, arrows, list filters. Keys that belong to a command should be declared in [`IArlecchinoView.Commands`](../arlecchino.navigation/IArlecchinoView.md#commands) instead.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — A route to navigate to, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) to stay.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public ViewRoute HandleMouse(MouseEvent mouse);
```

Handles a mouse event in frame coordinates. Only views that care about the mouse implement this.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event, with coordinates as frame cells. |

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — A route to navigate to, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) to stay.

### `HandlePaste(string)` {#handlepaste-string}

```csharp
public ViewRoute HandlePaste(string text);
```

Handles text pasted into the terminal. It arrives as one block however long it is, so a screen that takes typed input should take this too rather than leaving pastes on the floor.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was pasted, with the terminal's markers already stripped. |

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — A route to navigate to, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) to stay.

### `Hints()` {#hints}

```csharp
public virtual ValueTuple<string, string>[] Hints();
```

What the hints box shows. Leave it empty and the box is built from [`IArlecchinoView.Commands`](../arlecchino.navigation/IArlecchinoView.md#commands), so a rebound key relabels itself.

**Returns** `ValueTuple<T1, T2>`&lt;`string`, `string`&gt;\[\] — Pairs of key and description.

