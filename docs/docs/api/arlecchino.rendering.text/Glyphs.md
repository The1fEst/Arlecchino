---
title: "Glyphs"
sidebar_label: "Glyphs"
---

# Glyphs class

**Namespace:** `Arlecchino.Rendering.Text` &middot; **Assembly:** `Arlecchino.Core`

The symbols in use, reachable from anywhere that draws — the same arrangement as [`Theme`](../arlecchino.rendering.colors/Theme.md), and for the same reason: a widget picks the look up rather than being told it. Assigned from `ArlecchinoOptions` when the container resolves them; set it directly when drawing without a host. It is process-wide and settable, so an application can offer the choice in its own settings and have every graph follow on the next frame. A frame reads all of it, so all of it is written on the drawing thread and asks for a frame by itself; hand the change over with [`FrameThread.Post`](../arlecchino/FrameThread.md#post-action) from anywhere else.

```csharp
public static class Glyphs
```

## Properties

| Member | Summary |
|---|---|
| [`CellHeight`](#cellheight) | How many pixels tall a cell is taken to be. See [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth). |
| [`CellWidth`](#cellwidth) | How many pixels wide a cell is taken to be. Only [`ImageProtocol.Sixel`](../arlecchino.rendering.terminals/ImageProtocol.md) needs it, because sixel is measured in pixels and knows nothing of cells: a picture is resampled to however many pixels the cells it was given come to. [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan) sets it from what the terminal reports. Ten by twenty is the standing guess for a terminal that does not answer, and a wrong guess shows as a picture that does not quite fill its pane rather than as a broken one. |
| [`Graph`](#graph) | What graphs are drawn with when a widget does not say otherwise. |
| [`Picture`](#picture) | How pictures reach the terminal when a widget does not say otherwise. [`ImageProtocol.Auto`](../arlecchino.rendering.terminals/ImageProtocol.md) by default, which is the best of what the terminal admitted to when it was asked and cells when it admitted to nothing. Name a protocol to decide yourself — a terminal that cannot speak the one you name shows the escape sequence as text. |

## Properties in detail

### `CellHeight` {#cellheight}

```csharp
public static int CellHeight { get; set; }
```

How many pixels tall a cell is taken to be. See [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth).

**Type** `int`

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Assigned from off the drawing thread. |

### `CellWidth` {#cellwidth}

```csharp
public static int CellWidth { get; set; }
```

How many pixels wide a cell is taken to be. Only [`ImageProtocol.Sixel`](../arlecchino.rendering.terminals/ImageProtocol.md) needs it, because sixel is measured in pixels and knows nothing of cells: a picture is resampled to however many pixels the cells it was given come to. [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan) sets it from what the terminal reports. Ten by twenty is the standing guess for a terminal that does not answer, and a wrong guess shows as a picture that does not quite fill its pane rather than as a broken one.

**Type** `int`

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Assigned from off the drawing thread. |

### `Graph` {#graph}

```csharp
public static GraphSymbols Graph { get; set; }
```

What graphs are drawn with when a widget does not say otherwise.

**Type** [`GraphSymbols`](../arlecchino.rendering.text/GraphSymbols.md)

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Assigned from off the drawing thread. |

### `Picture` {#picture}

```csharp
public static ImageProtocol Picture { get; set; }
```

How pictures reach the terminal when a widget does not say otherwise. [`ImageProtocol.Auto`](../arlecchino.rendering.terminals/ImageProtocol.md) by default, which is the best of what the terminal admitted to when it was asked and cells when it admitted to nothing. Name a protocol to decide yourself — a terminal that cannot speak the one you name shows the escape sequence as text.

**Type** [`ImageProtocol`](../arlecchino.rendering.terminals/ImageProtocol.md)

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Assigned from off the drawing thread. |

