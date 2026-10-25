---
title: "Glyphs"
sidebar_label: "Glyphs"
---

# Glyphs class

**Namespace:** `Arlecchino.Rendering.Text` &middot; **Assembly:** `Arlecchino.Core`

The symbols in use, reachable from anywhere that draws, the way [`Theme`](../arlecchino.rendering.colors/Theme.md) is. It is written on the drawing thread and asks for a frame itself, so every graph follows on the next one.

```csharp
public static class Glyphs
```

## Properties

| Member | Summary |
|---|---|
| [`CellHeight`](#cellheight) | How many pixels tall a cell is taken to be. See [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth). |
| [`CellWidth`](#cellwidth) | How many pixels wide a cell is taken to be, which only [`ImageProtocol.Sixel`](../arlecchino.rendering.terminals/ImageProtocol.md) needs. [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan) sets it, and ten by twenty is the guess for a silent terminal. |
| [`Graph`](#graph) | What graphs are drawn with when a widget does not say otherwise. |
| [`Picture`](#picture) | How pictures reach the terminal when a widget does not say otherwise, which is [`ImageProtocol.Auto`](../arlecchino.rendering.terminals/ImageProtocol.md) by default. A terminal that cannot speak a named protocol shows the escape sequence as text. |

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

How many pixels wide a cell is taken to be, which only [`ImageProtocol.Sixel`](../arlecchino.rendering.terminals/ImageProtocol.md) needs. [`TerminalProbe.Ask`](../arlecchino.rendering.terminals/TerminalProbe.md#ask-iarlecchinoterminal-timespan) sets it, and ten by twenty is the guess for a silent terminal.

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

How pictures reach the terminal when a widget does not say otherwise, which is [`ImageProtocol.Auto`](../arlecchino.rendering.terminals/ImageProtocol.md) by default. A terminal that cannot speak a named protocol shows the escape sequence as text.

**Type** [`ImageProtocol`](../arlecchino.rendering.terminals/ImageProtocol.md)

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Assigned from off the drawing thread. |

