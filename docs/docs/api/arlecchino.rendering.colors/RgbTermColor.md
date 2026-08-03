---
title: RgbTermColor
sidebar_label: RgbTermColor
---

# RgbTermColor class

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

A style built from exact colours. Use it where the colour itself is the point — a swatch, a chart, syntax highlighting — and keep chrome on [`Theme`](../arlecchino.rendering.colors/Theme.md), which follows the terminal theme. Falls back to the nearest palette colour when the terminal cannot do 24-bit.

```csharp
public sealed class RgbTermColor : IArlecchinoColor
```

**Implements** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

## Constructors

| Member | Summary |
|---|---|
| [`RgbTermColor()`](#rgbtermcolor) |  |

## Properties

| Member | Summary |
|---|---|
| [`Ansi`](#ansi) | The escape sequence for this style: 24-bit where the terminal supports it, the nearest palette colour where it does not, and empty when colour is off. |
| [`Background`](#background) | Colour behind the glyphs, or `null` to leave the background alone. |
| [`Foreground`](#foreground) | Colour of the glyphs, or `null` to leave the foreground alone. |
| [`Style`](#style) | Bold, italic, underline and dim, in any combination. |

## Methods

| Member | Summary |
|---|---|
| [`ToString()`](#tostring) | Returns [`RgbTermColor.Ansi`](../arlecchino.rendering.colors/RgbTermColor.md#ansi). |

## Constructors in detail

### `RgbTermColor()` {#rgbtermcolor}

```csharp
public RgbTermColor();
```

## Properties in detail

### `Ansi` {#ansi}

```csharp
public string Ansi { get; }
```

The escape sequence for this style: 24-bit where the terminal supports it, the nearest palette colour where it does not, and empty when colour is off.

**Type** `string`

### `Background` {#background}

```csharp
public Nullable<Rgb> Background { get; init; }
```

Colour behind the glyphs, or `null` to leave the background alone.

**Type** `Nullable<T>`&lt;[`Rgb`](../arlecchino.rendering.colors/Rgb.md)&gt;

### `Foreground` {#foreground}

```csharp
public Nullable<Rgb> Foreground { get; init; }
```

Colour of the glyphs, or `null` to leave the foreground alone.

**Type** `Nullable<T>`&lt;[`Rgb`](../arlecchino.rendering.colors/Rgb.md)&gt;

### `Style` {#style}

```csharp
public TextStyle Style { get; init; }
```

Bold, italic, underline and dim, in any combination.

**Type** [`TextStyle`](../arlecchino.rendering.colors/TextStyle.md)

## Methods in detail

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

Returns [`RgbTermColor.Ansi`](../arlecchino.rendering.colors/RgbTermColor.md#ansi).

**Returns** `string`

