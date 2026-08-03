---
title: ModalFrame
sidebar_label: ModalFrame
---

# ModalFrame class

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

Everything a dialog needs from the application for as long as it is on screen: where to draw, the words to draw in, the keys to obey, and how to close. A dialog is a value — an application writes `new TextModal { … }` and hands it over — so it cannot be given services when it is built. It is given them when it is asked to do something, which is what lets [`Modal.Draw`](../arlecchino.modals/Modal.md#draw-modalframe) and [`Modal.Handle`](../arlecchino.modals/Modal.md#handle-modalframe-consolekeyinfo) live on the dialog itself rather than in a switch somewhere that has to know every kind there will ever be.

```csharp
public sealed class ModalFrame
```

## Properties

| Member | Summary |
|---|---|
| [`Depth`](#depth) | How many dialogs are already open under this one. Each is offset a row down and three columns along, so a dialog opened from a dialog reads as being on top of it rather than instead of it. |
| [`Height`](#height) | How many rows tall. |
| [`Keymap`](#keymap) | Keys to obey. |
| [`Keys`](#keys) | Turns a key press into the character it stands for. |
| [`Screen`](#screen) | The whole screen, for a dialog that would rather place itself. |
| [`Strings`](#strings) | The words the application says things in. |
| [`Width`](#width) | How many cells wide the frame is. |

## Methods

| Member | Summary |
|---|---|
| [`Box(string, IReadOnlyList<Piece[]>, string)`](#box-string-ireadonlylist-piece-string) | Draws a titled box holding the lines given, with the hints under a rule. Every dialog the framework brings is drawn through this, which is why a colour picker and a question read as the same application. |
| [`Centered(int, int)`](#centered-int-int) | A box of the size asked for, in the middle of the screen and never off the edge of it. |
| [`Close()`](#close) | Closes the dialog on top, which while it is being handled is this one. |
| [`Copy(string)`](#copy-string) | Puts text on the clipboard, for the dialogs that offer copying. |
| [`Divider(SurfaceRegion, int)`](#divider-surfaceregion-int) | The rule that separates the body of a dialog from its hints. |

## Properties in detail

### `Depth` {#depth}

```csharp
public int Depth { get; }
```

How many dialogs are already open under this one. Each is offset a row down and three columns along, so a dialog opened from a dialog reads as being on top of it rather than instead of it.

**Type** `int`

### `Height` {#height}

```csharp
public int Height { get; }
```

How many rows tall.

**Type** `int`

### `Keymap` {#keymap}

```csharp
public ArlecchinoKeymap Keymap { get; }
```

Keys to obey.

**Type** [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md)

### `Keys` {#keys}

```csharp
public KeyText Keys { get; }
```

Turns a key press into the character it stands for.

**Type** [`KeyText`](../arlecchino.input/KeyText.md)

### `Screen` {#screen}

```csharp
public SurfaceRegion Screen { get; }
```

The whole screen, for a dialog that would rather place itself.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `Strings` {#strings}

```csharp
public ArlecchinoStrings Strings { get; }
```

The words the application says things in.

**Type** [`ArlecchinoStrings`](../arlecchino.hosting/ArlecchinoStrings.md)

### `Width` {#width}

```csharp
public int Width { get; }
```

How many cells wide the frame is.

**Type** `int`

## Methods in detail

### `Box(string, IReadOnlyList<Piece[]>, string)` {#box-string-ireadonlylist-piece-string}

```csharp
public ValueTuple<SurfaceRegion, SurfaceRegion> Box(
    string title,
    IReadOnlyList<Piece[]> body,
    string footer);
```

Draws a titled box holding the lines given, with the hints under a rule. Every dialog the framework brings is drawn through this, which is why a colour picker and a question read as the same application.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | What the dialog is called. |
| `body` | `IReadOnlyList<T>`&lt;[`Piece`](../arlecchino.modals/Piece.md)\[\]&gt; | The lines, each a run of pieces. |
| `footer` | `string` | What the keys do, along the bottom. |

**Returns** `ValueTuple<T1, T2>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md), [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt; — The whole box, and the region inside it the lines were written in.

### `Centered(int, int)` {#centered-int-int}

```csharp
public SurfaceRegion Centered(int contentWidth, int contentHeight);
```

A box of the size asked for, in the middle of the screen and never off the edge of it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `contentWidth` | `int` | How wide what goes in it is. |
| `contentHeight` | `int` | How tall. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — Where the box goes.

### `Close()` {#close}

```csharp
public void Close();
```

Closes the dialog on top, which while it is being handled is this one.

### `Copy(string)` {#copy-string}

```csharp
public void Copy(string text);
```

Puts text on the clipboard, for the dialogs that offer copying.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to copy. |

### `Divider(SurfaceRegion, int)` {#divider-surfaceregion-int}

```csharp
public static void Divider(SurfaceRegion box, int insideRow);
```

The rule that separates the body of a dialog from its hints.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `box` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | The whole box, border included. |
| `insideRow` | `int` | Which row inside the box it goes under. |

