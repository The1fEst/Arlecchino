---
title: Tree
sidebar_label: Tree
description: Tree and TreeNode — a hierarchy that indents itself, expands on demand, and fills in its children just before it opens.
---

# Tree

```csharp
private readonly Tree<VltNode> _nodes;
private readonly VltFile _vlt;

_nodes = new Tree<VltNode>(options.Keymap)
{
    Render = node => node.Name,
    OnExpanding = node => node.Children = _vlt.LoadChildren(node.Value),
    OnActivate = node => ViewKind.Node,
    Roots = roots,
};
```

Widgets are built in the view's constructor, so `options` is the `ArlecchinoOptions` the container
hands it, and `region` is the [region](layout.md) the view draws the widget into.

## Nodes

`TreeNode<T>` carries the value, its children and whether it is open:

```csharp
var roots = projects
    .Select(project => new TreeNode<Project>(project))
    .ToList();
```

A node with no children is a leaf and gets no marker. Rows are indented two columns per level and
prefixed with `▾` / `▸`.

## Keys and clicks

| Input | Does |
|---|---|
| `→` | Expands the node under the cursor, then steps into it |
| `←` | Collapses it, then walks up to the parent |
| `Confirm` | Toggles a branch, or activates a leaf through `OnActivate` |
| Click on the marker | Toggles that node without activating it |
| Click elsewhere | Selects the row; a second click activates it |

Movement, paging and the wheel come from the [`ListBox`](lists.md) inside.

## Filling children in late

`OnExpanding` fires just before a node opens, which is where children are filled in for a tree that
loads level by level:

```csharp
OnExpanding = node => node.Children = _vlt.LoadChildren(node.Value),
```

That keeps a large hierarchy cheap: nothing below a closed node is ever read. Work that has to leave
the thread goes through an [async atom](async-atoms.md) and posts the children back rather than
blocking the frame.

`ExpandAll()` and `CollapseAll()` walk the whole thing.

## A worked example

The dependency screen of `samples/Arlecchino.Packages` is a tree of projects beside a per-project
table, with transitive branches folded in:

```bash
dotnet run --project samples/Arlecchino.Packages -- --frame projects 120x30
```
