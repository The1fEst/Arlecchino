---
title: IArlecchinoLayout
sidebar_label: IArlecchinoLayout
---

# IArlecchinoLayout interface

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

The frame every view is drawn inside: a band along the top, a bar along the bottom, whatever a screen of this application always has around it. It is one object for the whole application rather than one per screen, so what it holds outlives the view — a row of tabs keeps its scroll position when a screen is left and come back to, which is the whole reason a header is worth having in one place instead of drawn again by every view. [`IArlecchinoLayout.Draw`](../arlecchino.navigation/IArlecchinoLayout.md#draw-surfaceregion-action-surfaceregion) is handed the room there is and a delegate that draws the view. Where that delegate is called is where the view goes, and how much it is given is what the view thinks its screen is — a view asks the [`Surface`](../arlecchino.rendering/Surface.md) for its content and gets the region the layout left it, so no view has to know it is inside one.

```csharp
public interface IArlecchinoLayout
```

## Methods

| Member | Summary |
|---|---|
| [`Draw(SurfaceRegion, Action<SurfaceRegion>)`](#draw-surfaceregion-action-surfaceregion) | Draws the frame, and the view inside it. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Reads a mouse event before the view does, for a header that answers to one. Keys are not here: a key that works on every screen is an [`IArlecchinoCommand`](../arlecchino.commands/IArlecchinoCommand.md), which the framework already had, and two ways to say the same thing is one too many. |

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

Reads a mouse event before the view does, for a header that answers to one. Keys are not here: a key that works on every screen is an [`IArlecchinoCommand`](../arlecchino.commands/IArlecchinoCommand.md), which the framework already had, and two ways to say the same thing is one too many.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** `bool` — `true` when the layout took it and the view should not see it.

