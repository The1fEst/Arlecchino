---
title: PaneFlow
sidebar_label: PaneFlow
---

# PaneFlow class

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

A flow cursor inside one region: it writes the next line and remembers where the next one goes, so a pane filled from a loop does not have to count rows. [`Surface`](../arlecchino.rendering/Surface.md) has flow calls of its own, but they belong to the whole frame — reaching for `region.Surface.AppendLine(...)` inside a pane writes at the top of the screen and paints over borders and neighbours. This is the same idea, bounded by the region: everything is written in its coordinates, clipped to it, and once it is full the calls stop doing anything.

```csharp
var flow = region.Flow();

flow.AppendLine("PLAYERS", Theme.TableHeader);

foreach (var player in players)
{
flow.AppendLine(player.Name, Theme.Default);
}

```

It is a class, so passing it to a helper that writes a few more lines carries the cursor along. A second flow over the same region starts again at its first row.

```csharp
public sealed class PaneFlow
```

## Properties

| Member | Summary |
|---|---|
| [`FreeLines`](#freelines) | How many rows are left. Zero once the region is full. |
| [`IsFull`](#isfull) | Whether there is any room left to write in. |
| [`Region`](#region) | The region being written into. |
| [`Row`](#row) | The row the next line goes on, counted from the top of the region. |

## Methods

| Member | Summary |
|---|---|
| [`AppendLine(string, IArlecchinoColor, Align)`](#appendline-string-iarlecchinocolor-align) | Writes one line at the cursor and moves it down. Once the region is full the call does nothing, so a loop over more rows than fit needs no bound of its own. |
| [`FillLine(IArlecchinoColor)`](#fillline-iarlecchinocolor) | Draws a rule of `-` across the region and moves the cursor down. |
| [`Rest()`](#rest) | The rows the cursor has not reached yet, as a region of their own — for handing what is left of a pane to a widget once the lines above it are written. |
| [`Rewind()`](#rewind) | Puts the cursor back on the first row of the region. |
| [`Skip(int)`](#skip-int) | Leaves several rows blank. |
| [`SkipLine()`](#skipline) | Leaves the next row blank. |

## Properties in detail

### `FreeLines` {#freelines}

```csharp
public int FreeLines { get; }
```

How many rows are left. Zero once the region is full.

**Type** `int`

### `IsFull` {#isfull}

```csharp
public bool IsFull { get; }
```

Whether there is any room left to write in.

**Type** `bool`

### `Region` {#region}

```csharp
public SurfaceRegion Region { get; }
```

The region being written into.

**Type** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)

### `Row` {#row}

```csharp
public int Row { get; }
```

The row the next line goes on, counted from the top of the region.

**Type** `int`

## Methods in detail

### `AppendLine(string, IArlecchinoColor, Align)` {#appendline-string-iarlecchinocolor-align}

```csharp
public void AppendLine(string line, IArlecchinoColor? style = null, Align align = Left);
```

Writes one line at the cursor and moves it down. Once the region is full the call does nothing, so a loop over more rows than fit needs no bound of its own.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `line` | `string` | Text to write. |
| `style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) | Style for the line; the default role when omitted. |
| `align` | [`Align`](../arlecchino.rendering/Align.md) | Horizontal alignment inside the region. |

### `FillLine(IArlecchinoColor)` {#fillline-iarlecchinocolor}

```csharp
public void FillLine(IArlecchinoColor? style = null);
```

Draws a rule of `-` across the region and moves the cursor down.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) | Style for the rule; the default role when omitted. |

### `Rest()` {#rest}

```csharp
public SurfaceRegion Rest();
```

The rows the cursor has not reached yet, as a region of their own — for handing what is left of a pane to a widget once the lines above it are written.

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the cursor, empty when there is nothing left.

### `Rewind()` {#rewind}

```csharp
public void Rewind();
```

Puts the cursor back on the first row of the region.

### `Skip(int)` {#skip-int}

```csharp
public void Skip(int rows);
```

Leaves several rows blank.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `rows` | `int` | How many to leave. |

### `SkipLine()` {#skipline}

```csharp
public void SkipLine();
```

Leaves the next row blank.

