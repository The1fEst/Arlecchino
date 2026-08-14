---
title: "ThemePalette"
sidebar_label: "ThemePalette"
---

# ThemePalette class

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

The colors behind the roles in [`Theme`](../arlecchino.rendering.colors/Theme.md). Every role has a default, so a palette that overrides two of them is a valid palette — and what it does not override is the framework's own colors, described on [`ThemePalette.Arlecchino`](../arlecchino.rendering.colors/ThemePalette.md#arlecchino).

```csharp
public sealed class ThemePalette
```

## Constructors

| Member | Summary |
|---|---|
| [`ThemePalette()`](#themepalette) |  |

## Properties

| Member | Summary |
|---|---|
| [`Accent`](#accent) | Text that stands out without being alarming. Bone. |
| [`Active`](#active) | Something switched on or available. Crimson. |
| [`ActiveSelected`](#activeselected) | The cursor row of the focused pane. Ink on ash, so it is never read as a failure. |
| [`Arlecchino`](#arlecchino) | The framework's own colors, on whatever background the terminal has. It is what a palette starts from, and [`ThemePalette.Basic`](../arlecchino.rendering.colors/ThemePalette.md#basic) is the way back to the sixteen plain colors. |
| [`Basic`](#basic) | The terminal's own sixteen colors, with nothing exact behind them. It was the default before 2.0. |
| [`Default`](#default) | Ordinary text. The terminal's own foreground and background. |
| [`Error`](#error) | Failures and validation messages. Bone on crimson. |
| [`Header`](#header) | Screen titles. Bold crimson. |
| [`Info`](#info) | Borders and structural lines. Ash. |
| [`Input`](#input) | The editable part of a text field. Ink on bone. |
| [`Muted`](#muted) | Secondary text such as hints and footers. Ash. |
| [`Selected`](#selected) | The cursor row of an unfocused pane. Bone on the hairline gray. |
| [`TableHeader`](#tableheader) | Column headers. Bold bone. |
| [`Warning`](#warning) | Something worth noticing. Ink on amber. |

## Constructors in detail

### `ThemePalette()` {#themepalette}

```csharp
public ThemePalette();
```

## Properties in detail

### `Accent` {#accent}

```csharp
public TermColor Accent { get; init; }
```

Text that stands out without being alarming. Bone.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Active` {#active}

```csharp
public TermColor Active { get; init; }
```

Something switched on or available. Crimson.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `ActiveSelected` {#activeselected}

```csharp
public TermColor ActiveSelected { get; init; }
```

The cursor row of the focused pane. Ink on ash, so it is never read as a failure.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Arlecchino` {#arlecchino}

```csharp
public static ThemePalette Arlecchino { get; }
```

The framework's own colors, on whatever background the terminal has. It is what a palette starts from, and [`ThemePalette.Basic`](../arlecchino.rendering.colors/ThemePalette.md#basic) is the way back to the sixteen plain colors.

**Type** [`ThemePalette`](../arlecchino.rendering.colors/ThemePalette.md)

### `Basic` {#basic}

```csharp
public static ThemePalette Basic { get; }
```

The terminal's own sixteen colors, with nothing exact behind them. It was the default before 2.0.

**Type** [`ThemePalette`](../arlecchino.rendering.colors/ThemePalette.md)

### `Default` {#default}

```csharp
public TermColor Default { get; init; }
```

Ordinary text. The terminal's own foreground and background.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Error` {#error}

```csharp
public TermColor Error { get; init; }
```

Failures and validation messages. Bone on crimson.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Header` {#header}

```csharp
public TermColor Header { get; init; }
```

Screen titles. Bold crimson.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Info` {#info}

```csharp
public TermColor Info { get; init; }
```

Borders and structural lines. Ash.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Input` {#input}

```csharp
public TermColor Input { get; init; }
```

The editable part of a text field. Ink on bone.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Muted` {#muted}

```csharp
public TermColor Muted { get; init; }
```

Secondary text such as hints and footers. Ash.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Selected` {#selected}

```csharp
public TermColor Selected { get; init; }
```

The cursor row of an unfocused pane. Bone on the hairline gray.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `TableHeader` {#tableheader}

```csharp
public TermColor TableHeader { get; init; }
```

Column headers. Bold bone.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Warning` {#warning}

```csharp
public TermColor Warning { get; init; }
```

Something worth noticing. Ink on amber.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

