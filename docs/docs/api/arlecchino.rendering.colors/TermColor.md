---
title: "TermColor"
sidebar_label: "TermColor"
---

# TermColor class

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

A style built from the sixteen-color palette. This is what the roles on [`Theme`](../arlecchino.rendering.colors/Theme.md) are made of and what chrome should use, because those colors follow the terminal's own theme.

```csharp
public sealed class TermColor : IArlecchinoColor
```

**Implements** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

## Constructors

| Member | Summary |
|---|---|
| [`TermColor()`](#termcolor) |  |

## Properties

| Member | Summary |
|---|---|
| [`Ansi`](#ansi) | The escape sequence for this style, built once and rebuilt only if [`TerminalCapabilities.Color`](../arlecchino.rendering.terminals/TerminalCapabilities.md#color) changes. Empty when color is turned off. |
| [`Background`](#background) | Color behind the glyphs. [`TerminalColor.Default`](../arlecchino.rendering.colors/TerminalColor.md) leaves it to the terminal. |
| [`ExactBackground`](#exactbackground) | The same for what is behind the glyphs, falling back to [`TermColor.Background`](../arlecchino.rendering.colors/TermColor.md#background). |
| [`ExactForeground`](#exactforeground) | An exact color for the glyphs, used where the terminal can do 24-bit. Set [`TermColor.Foreground`](../arlecchino.rendering.colors/TermColor.md#foreground) as well, and it becomes the chosen fallback rather than the nearest one arithmetic finds. |
| [`Foreground`](#foreground) | Color of the glyphs. [`TerminalColor.Default`](../arlecchino.rendering.colors/TerminalColor.md) leaves it to the terminal. |
| [`Style`](#style) | Bold, italic, underline and dim, in any combination. |

## Methods

| Member | Summary |
|---|---|
| [`ToString()`](#tostring) | Writes the style as the sequence that puts it in force, so it can be appended directly. |

## Constructors in detail

### `TermColor()` {#termcolor}

```csharp
public TermColor();
```

## Properties in detail

### `Ansi` {#ansi}

```csharp
public string Ansi { get; }
```

The escape sequence for this style, built once and rebuilt only if [`TerminalCapabilities.Color`](../arlecchino.rendering.terminals/TerminalCapabilities.md#color) changes. Empty when color is turned off.

**Type** `string`

### `Background` {#background}

```csharp
public TerminalColor Background { get; init; }
```

Color behind the glyphs. [`TerminalColor.Default`](../arlecchino.rendering.colors/TerminalColor.md) leaves it to the terminal.

**Type** [`TerminalColor`](../arlecchino.rendering.colors/TerminalColor.md)

### `ExactBackground` {#exactbackground}

```csharp
public Nullable<Rgb> ExactBackground { get; init; }
```

The same for what is behind the glyphs, falling back to [`TermColor.Background`](../arlecchino.rendering.colors/TermColor.md#background).

**Type** `Nullable<T>`&lt;[`Rgb`](../arlecchino.rendering.colors/Rgb.md)&gt;

### `ExactForeground` {#exactforeground}

```csharp
public Nullable<Rgb> ExactForeground { get; init; }
```

An exact color for the glyphs, used where the terminal can do 24-bit. Set [`TermColor.Foreground`](../arlecchino.rendering.colors/TermColor.md#foreground) as well, and it becomes the chosen fallback rather than the nearest one arithmetic finds.

**Type** `Nullable<T>`&lt;[`Rgb`](../arlecchino.rendering.colors/Rgb.md)&gt;

### `Foreground` {#foreground}

```csharp
public TerminalColor Foreground { get; init; }
```

Color of the glyphs. [`TerminalColor.Default`](../arlecchino.rendering.colors/TerminalColor.md) leaves it to the terminal.

**Type** [`TerminalColor`](../arlecchino.rendering.colors/TerminalColor.md)

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

Writes the style as the sequence that puts it in force, so it can be appended directly.

**Returns** `string` — [`TermColor.Ansi`](../arlecchino.rendering.colors/TermColor.md#ansi).

