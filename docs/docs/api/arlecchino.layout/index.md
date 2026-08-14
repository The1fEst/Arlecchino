---
title: Arlecchino.Layout
sidebar_label: Arlecchino.Layout
sidebar_position: 0
---

# Arlecchino.Layout

## Classes

| Type | Summary |
|---|---|
| [`PaneTree`](PaneTree.md) | A screen described once, by binary space partitioning: every branch hands its region to two halves, and every leaf is what goes in that half. It is built from [`PaneTree.Branch`](../arlecchino.layout/PaneTree.md#branch-panetree-panetree) and [`PaneTree.Leaf`](../arlecchino.layout/PaneTree.md#leaf-iarlecchinowidget).  ```csharp _layout = Branch(Rows, 3, Leaf(_toolbar), Branch(Columns, 0.25, Leaf(_tree, () => "files"), Branch(Leaf(_editor, () => "editor"), Leaf(_log, () => "log")))).Gaps(inner: 1);  _focus = new FocusRing(options.Keymap); _focus.AddAll(_layout.Focusables);  public void Draw() => _layout.Draw(_surface.Content);  ``` |

## Structs

| Type | Summary |
|---|---|
| [`PaneSize`](PaneSize.md) | How much of a region a branch gives to its first half: a share, a count of cells, or a count from the other end. The literal says which, since a `double` is a share and an `int` is a count.  ```csharp Branch(Rows, 3, header, body);      // three rows Branch(Rows, 0.3, header, body);    // three tenths of the height Branch(Columns, 3, side, main);     // three columns — a count follows the direction of the cut  ``` |

## Enums

| Type | Summary |
|---|---|
| [`PaneSplit`](PaneSplit.md) | Which way a branch cuts the space it was given. |

