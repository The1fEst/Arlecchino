---
title: "IArlecchinoLayout"
sidebar_label: "IArlecchinoLayout"
---

# IArlecchinoLayout interface

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

The frame every view is drawn inside, from one object that outlives the views. A view asks the [`Surface`](../arlecchino.rendering/Surface.md) for its content and gets the region the layout left it, so it never knows.

```csharp
public interface IArlecchinoLayout
```

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion, Action<SurfaceRegion>)`](#draw-surfaceregion-action-surfaceregion) | Draws the frame, and the view inside it. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Reads a mouse event before the view does, for a header that answers to one. A key that works on every screen is an [`IArlecchinoCommand`](../arlecchino.commands/IArlecchinoCommand.md) instead. |

## Methods in detail

### `Draw(SurfaceRegion, Action<SurfaceRegion>)` {#draw-surfaceregion-action-surfaceregion}

```csharp
public void Draw(SurfaceRegion frame, Action<SurfaceRegion> body);
```

Draws the frame, and the view inside it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `frame` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Everything there is to draw in. |
| `body` | `Action<T>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt; | Draws the view into the region it is given. Call it once. |

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public bool HandleMouse(MouseEvent mouse);
```

Reads a mouse event before the view does, for a header that answers to one. A key that works on every screen is an [`IArlecchinoCommand`](../arlecchino.commands/IArlecchinoCommand.md) instead.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** `bool` — `true` when the layout took it and the view should not see it.

