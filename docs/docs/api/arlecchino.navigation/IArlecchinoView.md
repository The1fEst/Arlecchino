---
title: IArlecchinoView
sidebar_label: IArlecchinoView
---

# IArlecchinoView interface

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

A screen. Constructor parameters come from the container, and the instance lives as long as the route is shown — navigating away and back builds a new one, so per-screen state can live in fields. Implement `IDisposable` to be told when the screen goes away.

```csharp
public interface IArlecchinoView
```

## Properties

| Member | Summary |
|---|---|
| [`UsesLayout`](#useslayout) | Whether the [`IArlecchinoLayout`](../arlecchino.navigation/IArlecchinoLayout.md) is drawn around this screen, when the application has one. Answer `false` for a screen that wants the whole terminal — a file being read, a picture, anything where the band along the top is in the way rather than in the frame. |

## Methods

| Member | Summary |
|---|---|
| [`Commands()`](#commands) | Keys this screen reacts to, declared as data. They are checked before [`IArlecchinoView.Handle`](../arlecchino.navigation/IArlecchinoView.md#handle-consolekeyinfo), listed in the command palette, and checked for conflicts with application commands. |
| [`Draw()`](#draw) | Draws the screen. Called once per frame against the shared surface. |
| [`Handle(ConsoleKeyInfo)`](#handle-consolekeyinfo) | Handles a key the router did not claim: typing, arrows, list filters. Keys that belong to a command should be declared in [`IArlecchinoView.Commands`](../arlecchino.navigation/IArlecchinoView.md#commands) instead. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Handles a mouse event in frame coordinates. Only views that care about the mouse implement this. |
| [`HandlePaste(string)`](#handlepaste-string) | Handles text pasted into the terminal. It arrives as one block however long it is, so a screen that takes typed input should take this too rather than leaving pastes on the floor. |
| [`Hints()`](#hints) | What the hints box shows. Leave it empty and the box is built from [`IArlecchinoView.Commands`](../arlecchino.navigation/IArlecchinoView.md#commands), so a rebound key relabels itself. |

## Properties in detail

### `UsesLayout` {#useslayout}

```csharp
public bool UsesLayout { get; }
```

Whether the [`IArlecchinoLayout`](../arlecchino.navigation/IArlecchinoLayout.md) is drawn around this screen, when the application has one. Answer `false` for a screen that wants the whole terminal — a file being read, a picture, anything where the band along the top is in the way rather than in the frame.

**Type** `bool`

## Methods in detail

### `Commands()` {#commands}

```csharp
public IReadOnlyList<ViewCommand> Commands();
```

Keys this screen reacts to, declared as data. They are checked before [`IArlecchinoView.Handle`](../arlecchino.navigation/IArlecchinoView.md#handle-consolekeyinfo), listed in the command palette, and checked for conflicts with application commands.

**Returns** `IReadOnlyList<T>`&lt;[`ViewCommand`](../arlecchino.commands/ViewCommand.md)&gt; — The commands of this screen.

### `Draw()` {#draw}

```csharp
public void Draw();
```

Draws the screen. Called once per frame against the shared surface.

### `Handle(ConsoleKeyInfo)` {#handle-consolekeyinfo}

```csharp
public ViewRoute Handle(ConsoleKeyInfo key);
```

Handles a key the router did not claim: typing, arrows, list filters. Keys that belong to a command should be declared in [`IArlecchinoView.Commands`](../arlecchino.navigation/IArlecchinoView.md#commands) instead.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key that was pressed. |

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

