---
title: TableColumn&lt;T&gt;
sidebar_label: TableColumn&lt;T&gt;
---

# TableColumn&lt;T&gt; class

**Namespace:** `Arlecchino.Widgets.Lists` &middot; **Assembly:** `Arlecchino`

One column of a table: its heading, what it shows and how it behaves.

```csharp
public sealed class TableColumn<T>
```

## Constructors

| Member | Summary |
|---|---|
| [`TableColumn()`](#tablecolumn) |  |

## Properties

| Member | Summary |
|---|---|
| [`AlignRight`](#alignright) | Whether the cell hugs the right edge, which is what numbers want. |
| [`Cell`](#cell) | Reads the cell for one row. |
| [`Header`](#header) | The heading, as a delegate so it can be localised. |
| [`Sort`](#sort) | How to order rows by this column. Without it the column cannot be sorted at all, which is how a column of free-form text opts out. |
| [`Width`](#width) | Fixed width in columns. Leave at zero to share out whatever the fixed columns leave over. |

## Constructors in detail

### `TableColumn()` {#tablecolumn}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public TableColumn();
```

## Properties in detail

### `AlignRight` {#alignright}

```csharp
public bool AlignRight { get; init; }
```

Whether the cell hugs the right edge, which is what numbers want.

**Type** `bool`

### `Cell` {#cell}

```csharp
public Func<T, string> Cell { get; init; }
```

Reads the cell for one row.

**Type** `Func<T, TResult>`&lt;`T`, `string`&gt;

### `Header` {#header}

```csharp
public Func<string> Header { get; init; }
```

The heading, as a delegate so it can be localised.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Sort` {#sort}

```csharp
public Comparison<T> Sort { get; init; }
```

How to order rows by this column. Without it the column cannot be sorted at all, which is how a column of free-form text opts out.

**Type** `Comparison<T>`&lt;`T`&gt;

### `Width` {#width}

```csharp
public int Width { get; init; }
```

Fixed width in columns. Leave at zero to share out whatever the fixed columns leave over.

**Type** `int`

