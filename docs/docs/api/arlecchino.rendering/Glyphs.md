---
title: Glyphs
sidebar_label: Glyphs
---

# Glyphs class

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

The symbols in use, reachable from anywhere that draws — the same arrangement as [`Theme`](../arlecchino.rendering/Theme.md), and for the same reason: a widget picks the look up rather than being told it. Assigned from `ArlecchinoOptions` when the container resolves them; set it directly when drawing without a host. It is process-wide and settable, so an application can offer the choice in its own settings and have every graph follow on the next frame. A change made outside the input path should ask for a frame with `Repaint.Request()`, since nothing else will.

```csharp
public static class Glyphs
```

## Properties

| Member | Summary |
|---|---|
| [`Graph`](#graph) | What graphs are drawn with when a widget does not say otherwise. |

## Properties in detail

### `Graph` {#graph}

```csharp
public static GraphSymbols Graph { get; set; }
```

What graphs are drawn with when a widget does not say otherwise.

**Type** [`GraphSymbols`](../arlecchino.rendering/GraphSymbols.md)

