---
title: ThemePalette
sidebar_label: ThemePalette
---

# ThemePalette class

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

The colours behind the roles in [`Theme`](../arlecchino.rendering/Theme.md). Every role has a default, so a palette that overrides two of them is a valid palette — and what it does not override is the framework's own colours, described on [`ThemePalette.Arlecchino`](../arlecchino.rendering/ThemePalette.md#arlecchino).

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
| [`Arlecchino`](#arlecchino) | The framework's own colours — the crimson, bone and ink of the harlequin mask. The background stays whatever the terminal has, so it sits on a light theme as readily as a dark one; only the two cursor rows paint behind their text, because a selection has to be visible. Each entry carries an exact colour and a palette colour behind it, so a terminal without 24-bit draws the nearest thing the author picked rather than the nearest thing arithmetic found. This is what a palette starts from, so `new ThemePalette()` is already it; the property is here to name it, and [`ThemePalette.Basic`](../arlecchino.rendering/ThemePalette.md#basic) is the way back to the sixteen plain colours. |
| [`Basic`](#basic) | The terminal's own sixteen colours, with nothing exact behind them: magenta titles, blue column headers, cyan borders, a green cursor row. This was the default before 2.0, and `UseTheme(ThemePalette.Basic)` is how an application that liked it keeps it. |
| [`Default`](#default) | Ordinary text. The terminal's own foreground and background. |
| [`Error`](#error) | Failures and validation messages. Bone on crimson. |
| [`Header`](#header) | Screen titles. Bold crimson. |
| [`Info`](#info) | Borders and structural lines. Ash. |
| [`Input`](#input) | The editable part of a text field. Ink on bone. |
| [`Muted`](#muted) | Secondary text such as hints and footers. Ash. |
| [`Selected`](#selected) | The cursor row of an unfocused pane. Bone on the hairline grey. |
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

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Active` {#active}

```csharp
public TermColor Active { get; init; }
```

Something switched on or available. Crimson.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `ActiveSelected` {#activeselected}

```csharp
public TermColor ActiveSelected { get; init; }
```

The cursor row of the focused pane. Ink on ash, so it is never read as a failure.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Arlecchino` {#arlecchino}

```csharp
public static ThemePalette Arlecchino { get; }
```

The framework's own colours — the crimson, bone and ink of the harlequin mask. The background stays whatever the terminal has, so it sits on a light theme as readily as a dark one; only the two cursor rows paint behind their text, because a selection has to be visible. Each entry carries an exact colour and a palette colour behind it, so a terminal without 24-bit draws the nearest thing the author picked rather than the nearest thing arithmetic found. This is what a palette starts from, so `new ThemePalette()` is already it; the property is here to name it, and [`ThemePalette.Basic`](../arlecchino.rendering/ThemePalette.md#basic) is the way back to the sixteen plain colours.

**Type** [`ThemePalette`](../arlecchino.rendering/ThemePalette.md)

### `Basic` {#basic}

```csharp
public static ThemePalette Basic { get; }
```

The terminal's own sixteen colours, with nothing exact behind them: magenta titles, blue column headers, cyan borders, a green cursor row. This was the default before 2.0, and `UseTheme(ThemePalette.Basic)` is how an application that liked it keeps it.

**Type** [`ThemePalette`](../arlecchino.rendering/ThemePalette.md)

### `Default` {#default}

```csharp
public TermColor Default { get; init; }
```

Ordinary text. The terminal's own foreground and background.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Error` {#error}

```csharp
public TermColor Error { get; init; }
```

Failures and validation messages. Bone on crimson.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Header` {#header}

```csharp
public TermColor Header { get; init; }
```

Screen titles. Bold crimson.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Info` {#info}

```csharp
public TermColor Info { get; init; }
```

Borders and structural lines. Ash.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Input` {#input}

```csharp
public TermColor Input { get; init; }
```

The editable part of a text field. Ink on bone.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Muted` {#muted}

```csharp
public TermColor Muted { get; init; }
```

Secondary text such as hints and footers. Ash.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Selected` {#selected}

```csharp
public TermColor Selected { get; init; }
```

The cursor row of an unfocused pane. Bone on the hairline grey.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `TableHeader` {#tableheader}

```csharp
public TermColor TableHeader { get; init; }
```

Column headers. Bold bone.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

### `Warning` {#warning}

```csharp
public TermColor Warning { get; init; }
```

Something worth noticing. Ink on amber.

**Type** [`TermColor`](../arlecchino.rendering/TermColor.md)

