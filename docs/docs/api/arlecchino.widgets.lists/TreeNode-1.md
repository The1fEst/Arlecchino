---
title: TreeNode&lt;T&gt;
sidebar_label: TreeNode&lt;T&gt;
---

# TreeNode&lt;T&gt; class

**Namespace:** `Arlecchino.Widgets.Lists` &middot; **Assembly:** `Arlecchino`

One node of a tree. Children are settable so a branch can be filled in when it is first opened rather than up front.

```csharp
public sealed class TreeNode<T>
```

## Constructors

| Member | Summary |
|---|---|
| [`TreeNode()`](#treenode) |  |

## Properties

| Member | Summary |
|---|---|
| [`Children`](#children) | What sits under it. |
| [`HasChildren`](#haschildren) | Whether the node has children right now. A branch that has not been filled in yet reads as a leaf, so fill it in before it is first drawn. |
| [`IsExpanded`](#isexpanded) | Whether the children are showing. |
| [`Value`](#value) | What this node stands for. |

## Constructors in detail

### `TreeNode()` {#treenode}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public TreeNode();
```

## Properties in detail

### `Children` {#children}

```csharp
public IReadOnlyList<TreeNode<T>> Children { get; set; }
```

What sits under it.

**Type** `IReadOnlyList<T>`&lt;[`TreeNode`](../arlecchino.widgets.lists/TreeNode-1.md)&lt;`T`&gt;&gt;

### `HasChildren` {#haschildren}

```csharp
public bool HasChildren { get; }
```

Whether the node has children right now. A branch that has not been filled in yet reads as a leaf, so fill it in before it is first drawn.

**Type** `bool`

### `IsExpanded` {#isexpanded}

```csharp
public bool IsExpanded { get; set; }
```

Whether the children are showing.

**Type** `bool`

### `Value` {#value}

```csharp
public T Value { get; init; }
```

What this node stands for.

**Type** `T`

