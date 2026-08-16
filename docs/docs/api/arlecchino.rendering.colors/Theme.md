---
title: "Theme"
sidebar_label: "Theme"
---

# Theme class

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

The palette in use, reachable from anywhere that draws. Views pick a role here rather than a color, so swapping [`Theme.Palette`](../arlecchino.rendering.colors/Theme.md#palette) restyles the whole application, chrome included.

```csharp
public static class Theme
```

## Properties

| Member | Summary |
|---|---|
| [`Accent`](#accent) | Text that should stand out from ordinary text. |
| [`Active`](#active) | Something switched on, such as an enabled action. |
| [`ActiveSelected`](#activeselected) | The row under the cursor in the focused pane. |
| [`Caret`](#caret) | The symbol the caret stands on, written the other way round rather than beside. |
| [`Default`](#default) | Ordinary text on the terminal's own background. |
| [`Error`](#error) | Validation messages and failures. |
| [`Header`](#header) | Screen titles. |
| [`Info`](#info) | Box borders and other structural lines. |
| [`Input`](#input) | The editable part of a text field. |
| [`Muted`](#muted) | Secondary text: hints, footers, disabled rows. |
| [`Palette`](#palette) | The colors behind the roles, process-wide, so two hosts in one process share one palette. It is swapped on the drawing thread and asks for a frame itself. |
| [`Selected`](#selected) | The row under the cursor while its pane is not focused. |
| [`TableHeader`](#tableheader) | Column headers of a table. |
| [`Warning`](#warning) | Something the user should notice, such as the output line. |

## Properties in detail

### `Accent` {#accent}

```csharp
public static TermColor Accent { get; }
```

Text that should stand out from ordinary text.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Active` {#active}

```csharp
public static TermColor Active { get; }
```

Something switched on, such as an enabled action.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `ActiveSelected` {#activeselected}

```csharp
public static TermColor ActiveSelected { get; }
```

The row under the cursor in the focused pane.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Caret` {#caret}

```csharp
public static TermColor Caret { get; }
```

The symbol the caret stands on, written the other way round rather than beside.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Default` {#default}

```csharp
public static TermColor Default { get; }
```

Ordinary text on the terminal's own background.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Error` {#error}

```csharp
public static TermColor Error { get; }
```

Validation messages and failures.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Header` {#header}

```csharp
public static TermColor Header { get; }
```

Screen titles.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Info` {#info}

```csharp
public static TermColor Info { get; }
```

Box borders and other structural lines.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Input` {#input}

```csharp
public static TermColor Input { get; }
```

The editable part of a text field.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Muted` {#muted}

```csharp
public static TermColor Muted { get; }
```

Secondary text: hints, footers, disabled rows.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Palette` {#palette}

```csharp
public static ThemePalette Palette { get; set; }
```

The colors behind the roles, process-wide, so two hosts in one process share one palette. It is swapped on the drawing thread and asks for a frame itself.

**Type** [`ThemePalette`](../arlecchino.rendering.colors/ThemePalette.md)

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Assigned from off the drawing thread. |

### `Selected` {#selected}

```csharp
public static TermColor Selected { get; }
```

The row under the cursor while its pane is not focused.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `TableHeader` {#tableheader}

```csharp
public static TermColor TableHeader { get; }
```

Column headers of a table.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

### `Warning` {#warning}

```csharp
public static TermColor Warning { get; }
```

Something the user should notice, such as the output line.

**Type** [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

