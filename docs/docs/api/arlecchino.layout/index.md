---
title: Arlecchino.Layout
sidebar_label: Arlecchino.Layout
sidebar_position: 0
---

# Arlecchino.Layout

## Classes

| Type | Summary |
|---|---|
| [`PaneTree`](PaneTree.md) | A screen described once, by binary space partitioning: every branch hands its region to two halves, and every leaf is what goes in that half. Where a chain of [`SurfaceRegion.SplitLeft`](../arlecchino.rendering/SurfaceRegion.md#splitleft-int) and [`SurfaceRegion.SplitTop`](../arlecchino.rendering/SurfaceRegion.md#splittop-int) calls spreads the shape of a screen through the whole of `Draw`, a tree states it in one place — and the view then draws itself in a line. Two members build it, so a tree reads as a tree: [`PaneTree.Branch`](../arlecchino.layout/PaneTree.md#branch-panetree-panetree) and [`PaneTree.Leaf`](../arlecchino.layout/PaneTree.md#leaf-iarlecchinowidget). Only the two halves of a branch are ever required — say which way it cuts, or how much the first half takes, only where it matters:  ```csharp _layout = Branch(Rows, 3, Leaf(_toolbar), Branch(Columns, 0.25, Leaf(_tree, () => "files"), Branch(Leaf(_editor, () => "editor"), Leaf(_log, () => "log")))).Gaps(inner: 1);  _focus = new FocusRing(options.Keymap); _focus.AddAll(_layout.Focusables);  public void Draw() => _layout.Draw(_surface.Content);  ```  A `using static` of this type and of [`PaneSplit`](../arlecchino.layout/PaneSplit.md) is what lets it read that way. The tree holds what it draws, so it is built where the widgets are — in the view's constructor — and lives as long as the view does. Sizes are worked out per frame, which is what lets one tree fit any terminal; a region too small for what it holds leaves the panes that did not fit empty rather than overlapping them. |

## Structs

| Type | Summary |
|---|---|
| [`PaneSize`](PaneSize.md) | How much of a region a branch gives to its first half. It is a share of what there is, a fixed number of cells, or — for the toolbars and status bars that sit at the far edge — a fixed number of cells measured from the other end. The unit is the literal, not the number. A `double` is a share and an `int` is a count of cells, and both convert on their own, so the call site says which it meant by whether it has a decimal point:  ```csharp Branch(Rows, 3, header, body);      // three rows Branch(Rows, 0.3, header, body);    // three tenths of the height Branch(Columns, 3, side, main);     // three columns — a count follows the direction of the cut  ```  The pair worth remembering is `1` and `1.0`: the first is one row, the second is all of them. A bare `0` is rejected by the compiler rather than guessed at — it fits both a [`PaneSplit`](../arlecchino.layout/PaneSplit.md) and a size — so write [`PaneSize.Fraction`](../arlecchino.layout/PaneSize.md#fraction-double) or [`PaneSize.Cells`](../arlecchino.layout/PaneSize.md#cells-int) when nothing is what you mean. |

## Enums

| Type | Summary |
|---|---|
| [`PaneSplit`](PaneSplit.md) | Which way a branch cuts the space it was given. |

