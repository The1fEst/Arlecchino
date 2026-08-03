---
title: StatusBar
sidebar_label: StatusBar
---

# StatusBar class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

A line of short readouts pinned to an edge. Items are delegates because a status line is redrawn every frame and is expected to show what is true now, not what was true when it was built.

```csharp
public sealed class StatusBar : IArlecchinoWidget
```

**Implements** [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md)

## Constructors

| Member | Summary |
|---|---|
| [`StatusBar()`](#statusbar) |  |

## Properties

| Member | Summary |
|---|---|
| [`Left`](#left) | Items shown from the left edge. Empty results are skipped, separators and all. |
| [`Right`](#right) | Items shown from the right edge. |
| [`Style`](#style) | Colour to draw in. The muted theme colour when left alone. |

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws both groups on the first row and returns the rows below. The left side is truncated to fit, and the right side is dropped entirely when the two would collide, so the bar never overlaps itself. |

## Constructors in detail

### `StatusBar()` {#statusbar}

```csharp
public StatusBar();
```

## Properties in detail

### `Left` {#left}

```csharp
public IReadOnlyList<Func<string>> Left { get; init; }
```

Items shown from the left edge. Empty results are skipped, separators and all.

**Type** `IReadOnlyList<T>`&lt;`Func<TResult>`&lt;`string`&gt;&gt;

### `Right` {#right}

```csharp
public IReadOnlyList<Func<string>> Right { get; init; }
```

Items shown from the right edge.

**Type** `IReadOnlyList<T>`&lt;`Func<TResult>`&lt;`string`&gt;&gt;

### `Style` {#style}

```csharp
public IArlecchinoColor? Style { get; init; }
```

Colour to draw in. The muted theme colour when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

## Methods in detail

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws both groups on the first row and returns the rows below. The left side is truncated to fit, and the right side is dropped entirely when the two would collide, so the bar never overlaps itself.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw; only its first row is used. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — The region below the bar.

