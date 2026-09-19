---
title: "MarkdownStyles"
sidebar_label: "MarkdownStyles"
---

# MarkdownStyles class

**Namespace:** `Arlecchino.Widgets.Readouts` &middot; **Assembly:** `Arlecchino`

What each part of a document is written in. Every one of them is `null` to begin with, which leaves it to [`Theme`](../arlecchino.rendering.colors/Theme.md); what is worth setting is what a palette has no role for.

```csharp
public sealed class MarkdownStyles
```

## Constructors

| Member | Summary |
|---|---|
| [`MarkdownStyles()`](#markdownstyles) |  |

## Properties

| Member | Summary |
|---|---|
| [`Code`](#code) | Code, both a span of it and a block, [`Theme.Info`](../arlecchino.rendering.colors/Theme.md#info) when left alone. A background of its own is worth giving this one: a block is painted across the pane with it. |
| [`Emphasis`](#emphasis) | What it put between one, the ordinary text in italic when left alone. |
| [`Heading`](#heading) | The first two levels of heading, [`Theme.Header`](../arlecchino.rendering.colors/Theme.md#header) when left alone. |
| [`Link`](#link) | What a link is written as, the ordinary text underlined when left alone. |
| [`Marker`](#marker) | Bullets, the numbers of a list and the bar down the side of a quote, [`Theme.Active`](../arlecchino.rendering.colors/Theme.md#active) when left alone. |
| [`Quote`](#quote) | What a quote says, [`Theme.Secondary`](../arlecchino.rendering.colors/Theme.md#secondary) when left alone. |
| [`Rule`](#rule) | Rules and the lines between the columns of a table, [`Theme.Info`](../arlecchino.rendering.colors/Theme.md#info) when left alone. |
| [`Strong`](#strong) | What the document put between two stars, the ordinary text in bold when left alone. |
| [`Subheading`](#subheading) | The four below them, [`Theme.Accent`](../arlecchino.rendering.colors/Theme.md#accent) when left alone. |
| [`TableHeader`](#tableheader) | The first row of a table, [`Theme.TableHeader`](../arlecchino.rendering.colors/Theme.md#tableheader) when left alone. |
| [`Url`](#url) | Where it goes, [`Theme.Secondary`](../arlecchino.rendering.colors/Theme.md#secondary) when left alone. |

## Constructors in detail

### `MarkdownStyles()` {#markdownstyles}

```csharp
public MarkdownStyles();
```

## Properties in detail

### `Code` {#code}

```csharp
public IArlecchinoColor? Code { get; init; }
```

Code, both a span of it and a block, [`Theme.Info`](../arlecchino.rendering.colors/Theme.md#info) when left alone. A background of its own is worth giving this one: a block is painted across the pane with it.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Emphasis` {#emphasis}

```csharp
public IArlecchinoColor? Emphasis { get; init; }
```

What it put between one, the ordinary text in italic when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Heading` {#heading}

```csharp
public IArlecchinoColor? Heading { get; init; }
```

The first two levels of heading, [`Theme.Header`](../arlecchino.rendering.colors/Theme.md#header) when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Link` {#link}

```csharp
public IArlecchinoColor? Link { get; init; }
```

What a link is written as, the ordinary text underlined when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Marker` {#marker}

```csharp
public IArlecchinoColor? Marker { get; init; }
```

Bullets, the numbers of a list and the bar down the side of a quote, [`Theme.Active`](../arlecchino.rendering.colors/Theme.md#active) when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Quote` {#quote}

```csharp
public IArlecchinoColor? Quote { get; init; }
```

What a quote says, [`Theme.Secondary`](../arlecchino.rendering.colors/Theme.md#secondary) when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Rule` {#rule}

```csharp
public IArlecchinoColor? Rule { get; init; }
```

Rules and the lines between the columns of a table, [`Theme.Info`](../arlecchino.rendering.colors/Theme.md#info) when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Strong` {#strong}

```csharp
public IArlecchinoColor? Strong { get; init; }
```

What the document put between two stars, the ordinary text in bold when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Subheading` {#subheading}

```csharp
public IArlecchinoColor? Subheading { get; init; }
```

The four below them, [`Theme.Accent`](../arlecchino.rendering.colors/Theme.md#accent) when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `TableHeader` {#tableheader}

```csharp
public IArlecchinoColor? TableHeader { get; init; }
```

The first row of a table, [`Theme.TableHeader`](../arlecchino.rendering.colors/Theme.md#tableheader) when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Url` {#url}

```csharp
public IArlecchinoColor? Url { get; init; }
```

Where it goes, [`Theme.Secondary`](../arlecchino.rendering.colors/Theme.md#secondary) when left alone.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

