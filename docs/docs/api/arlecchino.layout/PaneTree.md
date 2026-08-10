---
title: "PaneTree"
sidebar_label: "PaneTree"
---

# PaneTree class

**Namespace:** `Arlecchino.Layout` &middot; **Assembly:** `Arlecchino`

A screen described once, by binary space partitioning: every branch hands its region to two halves, and every leaf is what goes in that half. Where a chain of [`SurfaceRegion.SplitLeft`](../arlecchino.rendering/SurfaceRegion.md#splitleft-int) and [`SurfaceRegion.SplitTop`](../arlecchino.rendering/SurfaceRegion.md#splittop-int) calls spreads the shape of a screen through the whole of `Draw`, a tree states it in one place — and the view then draws itself in a line. Two members build it, so a tree reads as a tree: [`PaneTree.Branch`](../arlecchino.layout/PaneTree.md#branch-panetree-panetree) and [`PaneTree.Leaf`](../arlecchino.layout/PaneTree.md#leaf-iarlecchinowidget). Only the two halves of a branch are ever required — say which way it cuts, or how much the first half takes, only where it matters:

```csharp
_layout = Branch(Rows, 3,
Leaf(_toolbar),
Branch(Columns, 0.25,
Leaf(_tree, () => "files"),
Branch(Leaf(_editor, () => "editor"), Leaf(_log, () => "log")))).Gaps(inner: 1);

_focus = new FocusRing(options.Keymap);
_focus.AddAll(_layout.Focusables);

public void Draw() => _layout.Draw(_surface.Content);

```

A `using static` of this type and of [`PaneSplit`](../arlecchino.layout/PaneSplit.md) is what lets it read that way. The tree holds what it draws, so it is built where the widgets are — in the view's constructor — and lives as long as the view does. Sizes are worked out per frame, which is what lets one tree fit any terminal; a region too small for what it holds leaves the panes that did not fit empty rather than overlapping them.

```csharp
public sealed class PaneTree
```

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many panes the tree draws. |
| [`InnerGap`](#innergap) | Cells left empty between the two halves of every branch. Set by [`PaneTree.Gaps`](../arlecchino.layout/PaneTree.md#gaps-int-int). |
| [`OuterGap`](#outergap) | Cells left empty around the whole layout. Set by [`PaneTree.Gaps`](../arlecchino.layout/PaneTree.md#gaps-int-int). |

## Methods

| Member | Summary |
|---|---|
| [`AsFocusRing(ArlecchinoKeymap)`](#asfocusring-arlecchinokeymap) | Builds the focus ring of the screen from the tree: every widget of it that takes the focus, in the order the branches lay them out — left before right, top before bottom. `Tab` then walks the screen the way it looks, and there is no second list to keep in step with the first.  ```csharp _focus = _layout.AsFocusRing(options.Keymap);  ```  What comes back is an ordinary [`FocusRing`](../arlecchino.focus/FocusRing.md), so anything focusable that lives outside the tree is added to it afterward, and lands at the end of the walk. The tree keeps the ring it built, which is what lets [`PaneTree.HandleMouse`](../arlecchino.layout/PaneTree.md#handlemouse-mouseevent) move the focus to the pane that was clicked. |
| [`Branch(PaneTree, PaneTree)`](#branch-panetree-panetree) | A branch that decides everything itself: it cuts along the longer side of whatever region it is given and halves it. The longer side is measured in what the eye sees rather than in cells — a cell is about twice as tall as it is wide, so 80×24 is a wide region and gets two columns. Because the side is measured per frame, such a branch can turn from columns into rows when the terminal is resized. That is what makes it right for panes of equal standing, and wrong for chrome, which should be pinned with a [`PaneSplit`](../arlecchino.layout/PaneSplit.md) of its own. |
| [`Branch(PaneSplit, PaneTree, PaneTree)`](#branch-panesplit-panetree-panetree) | A branch that cuts the way it is told and halves the space. |
| [`Branch(PaneSize, PaneTree, PaneTree)`](#branch-panesize-panetree-panetree) | A branch of a given size that still cuts along the longer side, for a split that is uneven but has no reason to prefer an axis. |
| [`Branch(PaneSplit, PaneSize, PaneTree, PaneTree)`](#branch-panesplit-panesize-panetree-panetree) | A branch that says both: the space is cut the given way and each half goes to a subtree, which is itself either a branch or a leaf. Three bands is therefore a branch inside a branch. |
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws every pane where the branches put it. This is the whole of a view's `Draw` when the screen is a tree. |
| [`Gaps(int, int)`](#gaps-int-int) | Sets the spacing of the whole layout, rather than of one branch, so a screen is loosened or tightened in one place. The names are the ones a tiling window manager uses. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Sends a mouse event to the pane it landed in, and moves the focus there when that pane claims it. The tree already works out which pane owns which cells in order to draw them, so the same knowledge tells a click where to go. The event walks down the branches that contain the point and reaches one pane, instead of being offered to every widget on the screen in turn. This is the whole of a view's `HandleMouse` when the screen is a tree:  ```csharp public ViewRoute HandleMouse(MouseEvent mouse) => _layout.HandleMouse(mouse);  ```  A click in the gap between panes, in the surrounding space, or before the first frame was drawn belongs to no pane and is left alone. The focus follows the click only for a tree that built its ring with [`PaneTree.AsFocusRing`](../arlecchino.layout/PaneTree.md#asfocusring-arlecchinokeymap); without one the pane still sees the event. |
| [`Leaf(IArlecchinoWidget)`](#leaf-iarlecchinowidget) | A pane holding a widget, drawn into whatever region the tree gives it. |
| [`Leaf(IArlecchinoWidget, Func<string>)`](#leaf-iarlecchinowidget-func-string) | A pane holding a widget, in a box with a title. The widget is drawn in the room left inside the box. The box itself is drawn `Theme.Active` while the widget holds the focus and `Theme.Info` while it does not, so a screen of panes shows where the cursor is without the view saying anything about it. |
| [`Leaf(Action<SurfaceRegion>)`](#leaf-action-surfaceregion) | A pane the view draws itself, for the parts of a screen that are not a widget — a title, a box, a row of readouts. |
| [`Leaf(Action<SurfaceRegion>, Func<string>)`](#leaf-action-surfaceregion-func-string) | A pane the view draws itself, in a box with a title. |
| [`Leaf()`](#leaf) | A pane that draws nothing, for space deliberately left blank. |

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many panes the tree draws.

**Type** `int`

### `InnerGap` {#innergap}

```csharp
public int InnerGap { get; }
```

Cells left empty between the two halves of every branch. Set by [`PaneTree.Gaps`](../arlecchino.layout/PaneTree.md#gaps-int-int).

**Type** `int`

### `OuterGap` {#outergap}

```csharp
public int OuterGap { get; }
```

Cells left empty around the whole layout. Set by [`PaneTree.Gaps`](../arlecchino.layout/PaneTree.md#gaps-int-int).

**Type** `int`

## Methods in detail

### `AsFocusRing(ArlecchinoKeymap)` {#asfocusring-arlecchinokeymap}

```csharp
public FocusRing AsFocusRing(ArlecchinoKeymap keymap);
```

Builds the focus ring of the screen from the tree: every widget of it that takes the focus, in the order the branches lay them out — left before right, top before bottom. `Tab` then walks the screen the way it looks, and there is no second list to keep in step with the first.

```csharp
_focus = _layout.AsFocusRing(options.Keymap);

```

What comes back is an ordinary [`FocusRing`](../arlecchino.focus/FocusRing.md), so anything focusable that lives outside the tree is added to it afterward, and lands at the end of the walk. The tree keeps the ring it built, which is what lets [`PaneTree.HandleMouse`](../arlecchino.layout/PaneTree.md#handlemouse-mouseevent) move the focus to the pane that was clicked.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | Where the keys that move the focus come from. |

**Returns** [`FocusRing`](../arlecchino.focus/FocusRing.md) — A ring holding the panes that take the focus.

### `Branch(PaneTree, PaneTree)` {#branch-panetree-panetree}

```csharp
public static PaneTree Branch(PaneTree first, PaneTree second);
```

A branch that decides everything itself: it cuts along the longer side of whatever region it is given and halves it. The longer side is measured in what the eye sees rather than in cells — a cell is about twice as tall as it is wide, so 80×24 is a wide region and gets two columns. Because the side is measured per frame, such a branch can turn from columns into rows when the terminal is resized. That is what makes it right for panes of equal standing, and wrong for chrome, which should be pinned with a [`PaneSplit`](../arlecchino.layout/PaneSplit.md) of its own.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `first` | [`PaneTree`](../arlecchino.layout/PaneTree.md) | The upper half, or the left one. |
| `second` | [`PaneTree`](../arlecchino.layout/PaneTree.md) | The lower half, or the right one. |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The branch.

### `Branch(PaneSplit, PaneTree, PaneTree)` {#branch-panesplit-panetree-panetree}

```csharp
public static PaneTree Branch(PaneSplit split, PaneTree first, PaneTree second);
```

A branch that cuts the way it is told and halves the space.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `split` | [`PaneSplit`](../arlecchino.layout/PaneSplit.md) | Which way to cut. |
| `first` | [`PaneTree`](../arlecchino.layout/PaneTree.md) | The upper half, or the left one. |
| `second` | [`PaneTree`](../arlecchino.layout/PaneTree.md) | The lower half, or the right one. |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The branch.

### `Branch(PaneSize, PaneTree, PaneTree)` {#branch-panesize-panetree-panetree}

```csharp
public static PaneTree Branch(PaneSize size, PaneTree first, PaneTree second);
```

A branch of a given size that still cuts along the longer side, for a split that is uneven but has no reason to prefer an axis.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `size` | [`PaneSize`](../arlecchino.layout/PaneSize.md) | How much of it the first half takes; the second half takes the rest. A count of cells when written as an `int`, a share of the space when written with a decimal point — `3` is three rows or columns, `0.3` is three tenths. See [`PaneSize`](../arlecchino.layout/PaneSize.md). |
| `first` | [`PaneTree`](../arlecchino.layout/PaneTree.md) | The upper half, or the left one. |
| `second` | [`PaneTree`](../arlecchino.layout/PaneTree.md) | The lower half, or the right one. |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The branch.

### `Branch(PaneSplit, PaneSize, PaneTree, PaneTree)` {#branch-panesplit-panesize-panetree-panetree}

```csharp
public static PaneTree Branch(PaneSplit split, PaneSize size, PaneTree first, PaneTree second);
```

A branch that says both: the space is cut the given way and each half goes to a subtree, which is itself either a branch or a leaf. Three bands is therefore a branch inside a branch.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `split` | [`PaneSplit`](../arlecchino.layout/PaneSplit.md) | Which way to cut. |
| `size` | [`PaneSize`](../arlecchino.layout/PaneSize.md) | How much of it the first half takes; the second half takes the rest. A count of cells when written as an `int`, a share of the space when written with a decimal point — `3` is three rows or columns, `0.3` is three tenths. See [`PaneSize`](../arlecchino.layout/PaneSize.md). |
| `first` | [`PaneTree`](../arlecchino.layout/PaneTree.md) | The upper half, or the left one. |
| `second` | [`PaneTree`](../arlecchino.layout/PaneTree.md) | The lower half, or the right one. |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The branch.

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public void Draw(SurfaceRegion region);
```

Draws every pane where the branches put it. This is the whole of a view's `Draw` when the screen is a tree.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | The space to fill, usually `surface.Content`. |

### `Gaps(int, int)` {#gaps-int-int}

```csharp
public PaneTree Gaps(int inner, int outer = 0);
```

Sets the spacing of the whole layout, rather than of one branch, so a screen is loosened or tightened in one place. The names are the ones a tiling window manager uses.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `inner` | `int` | Cells left empty between the two halves of every branch. |
| `outer` | `int` | Cells left empty around everything, inside the region handed to [`PaneTree.Draw`](../arlecchino.layout/PaneTree.md#draw-surfaceregion). |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The same tree, so the call finishes the expression that built it.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public ViewRoute HandleMouse(MouseEvent mouse);
```

Sends a mouse event to the pane it landed in, and moves the focus there when that pane claims it. The tree already works out which pane owns which cells in order to draw them, so the same knowledge tells a click where to go. The event walks down the branches that contain the point and reaches one pane, instead of being offered to every widget on the screen in turn. This is the whole of a view's `HandleMouse` when the screen is a tree:

```csharp
public ViewRoute HandleMouse(MouseEvent mouse) => _layout.HandleMouse(mouse);

```

A click in the gap between panes, in the surrounding space, or before the first frame was drawn belongs to no pane and is left alone. The focus follows the click only for a tree that built its ring with [`PaneTree.AsFocusRing`](../arlecchino.layout/PaneTree.md#asfocusring-arlecchinokeymap); without one the pane still sees the event.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event, in frame coordinates. |

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — The route the pane asked for, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none).

### `Leaf(IArlecchinoWidget)` {#leaf-iarlecchinowidget}

```csharp
public static PaneTree Leaf(IArlecchinoWidget widget);
```

A pane holding a widget, drawn into whatever region the tree gives it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `widget` | [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md) | What goes in the pane. |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The leaf.

### `Leaf(IArlecchinoWidget, Func<string>)` {#leaf-iarlecchinowidget-func-string}

```csharp
public static PaneTree Leaf(IArlecchinoWidget widget, Func<string> title);
```

A pane holding a widget, in a box with a title. The widget is drawn in the room left inside the box. The box itself is drawn `Theme.Active` while the widget holds the focus and `Theme.Info` while it does not, so a screen of panes shows where the cursor is without the view saying anything about it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `widget` | [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md) | What goes in the pane. |
| `title` | `Func<TResult>`&lt;`string`&gt; | What to write in the top border. A delegate rather than a string, like every other piece of user-visible text in the framework, so a translated application translates it too. |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The leaf.

### `Leaf(Action<SurfaceRegion>)` {#leaf-action-surfaceregion}

```csharp
public static PaneTree Leaf(Action<SurfaceRegion> draw);
```

A pane the view draws itself, for the parts of a screen that are not a widget — a title, a box, a row of readouts.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `draw` | `Action<T>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt; | What to draw, given the region the pane was allotted. |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The leaf.

### `Leaf(Action<SurfaceRegion>, Func<string>)` {#leaf-action-surfaceregion-func-string}

```csharp
public static PaneTree Leaf(Action<SurfaceRegion> draw, Func<string> title);
```

A pane the view draws itself, in a box with a title.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `draw` | `Action<T>`&lt;[`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md)&gt; | What to draw, given the room left inside the box. |
| `title` | `Func<TResult>`&lt;`string`&gt; | What to write in the top border. |

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The leaf.

### `Leaf()` {#leaf}

```csharp
public static PaneTree Leaf();
```

A pane that draws nothing, for space deliberately left blank.

**Returns** [`PaneTree`](../arlecchino.layout/PaneTree.md) — The leaf.

