---
title: Piece
sidebar_label: Piece
---

# Piece struct

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

One run of a line inside a dialog, with the colour it is written in.

```csharp
public readonly struct Piece : IEquatable<Piece>
```

**Implements** `IEquatable<T>`&lt;[`Piece`](../arlecchino.modals/Piece.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`Piece(string, IArlecchinoColor)`](#piece-string-iarlecchinocolor) | One run of a line inside a dialog, with the colour it is written in. |

## Properties

| Member | Summary |
|---|---|
| [`Style`](#style) | How they are written. |
| [`Text`](#text) | The words. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out string, out IArlecchinoColor)`](#deconstruct-out-string-out-iarlecchinocolor) |  |

## Constructors in detail

### `Piece(string, IArlecchinoColor)` {#piece-string-iarlecchinocolor}

```csharp
public Piece(string Text, IArlecchinoColor Style);
```

One run of a line inside a dialog, with the colour it is written in.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Text` | `string` | The words. |
| `Style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | How they are written. |

## Properties in detail

### `Style` {#style}

```csharp
public IArlecchinoColor Style { get; init; }
```

How they are written.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Text` {#text}

```csharp
public string Text { get; init; }
```

The words.

**Type** `string`

## Methods in detail

### `Deconstruct(out string, out IArlecchinoColor)` {#deconstruct-out-string-out-iarlecchinocolor}

```csharp
public void Deconstruct(out string Text, out IArlecchinoColor Style);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Text` | `string` |  |
| `Style` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) |  |

