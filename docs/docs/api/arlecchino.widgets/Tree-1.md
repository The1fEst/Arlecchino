---
title: Tree&lt;T&gt;
sidebar_label: Tree&lt;T&gt;
---

# Tree&lt;T&gt; class

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

A hierarchy drawn as indented rows. Only the expanded parts are laid out, and that layout is recomputed on demand rather than cached, so nodes may be added or expanded between frames.

```csharp
public sealed class Tree<T> : IArlecchinoInteractiveWidget, IArlecchinoWidget, IArlecchinoFocusable
```

**Implements** [`IArlecchinoInteractiveWidget`](../arlecchino.widgets/IArlecchinoInteractiveWidget.md), [`IArlecchinoWidget`](../arlecchino.widgets/IArlecchinoWidget.md), [`IArlecchinoFocusable`](../arlecchino.focus/IArlecchinoFocusable.md)

## Constructors

| Member | Summary |
|---|---|
| [`Tree(ArlecchinoKeymap)`](#tree-arlecchinokeymap) | Creates the tree. |

## Properties

| Member | Summary |
|---|---|
| [`IsFocused`](#isfocused) | Whether the tree has focus, which decides how strongly the selection is drawn. |
| [`ItemStyle`](#itemstyle) | Colours a node. Ignored for the selected one. |
| [`OnActivate`](#onactivate) | What confirming a leaf does. Branches toggle instead, so this is never called for a node that has children. |
| [`OnExpanding`](#onexpanding) | Called just before a branch opens, which is where its children can be filled in. It runs on the UI thread, so anything slow belongs in an [`AsyncAtom`](../arlecchino.atoms/AsyncAtom-1.md) instead. |
| [`Render`](#render) | Turns a value into its label. The marker and the indent are added around it. |
| [`Roots`](#roots) | The top-level nodes. |
| [`Selected`](#selected) | Index of the selected row, counted over the rows that are showing rather than over all nodes. |
| [`SelectedNode`](#selectednode) | The selected node, or `null` when the tree is empty. |

## Methods

| Member | Summary |
|---|---|
| [`CollapseAll()`](#collapseall) | Closes every branch, leaving only the roots showing. |
| [`Draw(SurfaceRegion)`](#draw-surfaceregion) | Draws the rows that are showing around the selection and remembers where they landed, which is what lets a click tell a marker from a label. The tree fills whatever it is given, so nothing is left underneath it. |
| [`ExpandAll()`](#expandall) | Opens every branch. Branches are opened directly, so anything relying on the expand callback to fill in its children will still look empty. |
| [`Handle(ConsoleKeyInfo)`](#handle-consolekeyinfo) | Moves through the rows and opens or closes branches. The horizontal arrows behave the way they do in a file manager: right opens a closed branch or steps into it, left closes an open one or jumps to the parent. |
| [`HandleMouse(MouseEvent)`](#handlemouse-mouseevent) | Scrolls with the wheel and selects with a click. Clicking the marker toggles the branch, while clicking the label of the already selected row activates it. |

## Constructors in detail

### `Tree(ArlecchinoKeymap)` {#tree-arlecchinokeymap}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public Tree(ArlecchinoKeymap keymap);
```

Creates the tree.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | Keys to obey, so the tree follows the application's bindings. |

## Properties in detail

### `IsFocused` {#isfocused}

```csharp
public bool IsFocused { get; set; }
```

Whether the tree has focus, which decides how strongly the selection is drawn.

**Type** `bool`

### `ItemStyle` {#itemstyle}

```csharp
public Func<T, IArlecchinoColor> ItemStyle { get; set; }
```

Colours a node. Ignored for the selected one.

**Type** `Func<T, TResult>`&lt;`T`, [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md)&gt;

### `OnActivate` {#onactivate}

```csharp
public Func<TreeNode<T>, ViewRoute> OnActivate { get; init; }
```

What confirming a leaf does. Branches toggle instead, so this is never called for a node that has children.

**Type** `Func<T, TResult>`&lt;[`TreeNode`](../arlecchino.widgets/TreeNode-1.md)&lt;`T`&gt;, [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

### `OnExpanding` {#onexpanding}

```csharp
public Action<TreeNode<T>> OnExpanding { get; init; }
```

Called just before a branch opens, which is where its children can be filled in. It runs on the UI thread, so anything slow belongs in an [`AsyncAtom`](../arlecchino.atoms/AsyncAtom-1.md) instead.

**Type** `Action<T>`&lt;[`TreeNode`](../arlecchino.widgets/TreeNode-1.md)&lt;`T`&gt;&gt;

### `Render` {#render}

```csharp
public Func<T, string> Render { get; init; }
```

Turns a value into its label. The marker and the indent are added around it.

**Type** `Func<T, TResult>`&lt;`T`, `string`&gt;

### `Roots` {#roots}

```csharp
public IReadOnlyList<TreeNode<T>> Roots { get; set; }
```

The top-level nodes.

**Type** `IReadOnlyList<T>`&lt;[`TreeNode`](../arlecchino.widgets/TreeNode-1.md)&lt;`T`&gt;&gt;

### `Selected` {#selected}

```csharp
public int Selected { get; set; }
```

Index of the selected row, counted over the rows that are showing rather than over all nodes.

**Type** `int`

### `SelectedNode` {#selectednode}

```csharp
public TreeNode<T> SelectedNode { get; }
```

The selected node, or `null` when the tree is empty.

**Type** [`TreeNode`](../arlecchino.widgets/TreeNode-1.md)&lt;`T`&gt;

## Methods in detail

### `CollapseAll()` {#collapseall}

```csharp
public void CollapseAll();
```

Closes every branch, leaving only the roots showing.

### `Draw(SurfaceRegion)` {#draw-surfaceregion}

```csharp
public SurfaceRegion Draw(SurfaceRegion region);
```

Draws the rows that are showing around the selection and remembers where they landed, which is what lets a click tell a marker from a label. The tree fills whatever it is given, so nothing is left underneath it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `region` | [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) | Where to draw. |

**Returns** [`SurfaceRegion`](../arlecchino.rendering/SurfaceRegion.md) — An empty region: the tree uses every row it is handed.

### `ExpandAll()` {#expandall}

```csharp
public void ExpandAll();
```

Opens every branch. Branches are opened directly, so anything relying on the expand callback to fill in its children will still look empty.

### `Handle(ConsoleKeyInfo)` {#handle-consolekeyinfo}

```csharp
public FocusResult Handle(ConsoleKeyInfo key);
```

Moves through the rows and opens or closes branches. The horizontal arrows behave the way they do in a file manager: right opens a closed branch or steps into it, left closes an open one or jumps to the parent.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key that was pressed. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What became of the key, including a route when a leaf was activated.

### `HandleMouse(MouseEvent)` {#handlemouse-mouseevent}

```csharp
public FocusResult HandleMouse(MouseEvent mouse);
```

Scrolls with the wheel and selects with a click. Clicking the marker toggles the branch, while clicking the label of the already selected row activates it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

**Returns** [`FocusResult`](../arlecchino.focus/FocusResult.md) — What became of the event, including a route when a leaf was activated.

