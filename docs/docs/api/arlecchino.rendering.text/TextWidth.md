---
title: "TextWidth"
sidebar_label: "TextWidth"
---

# TextWidth class

**Namespace:** `Arlecchino.Rendering.Text` &middot; **Assembly:** `Arlecchino.Core`

Measures text the way a terminal shows it, in columns rather than in `char` values. Use these instead of `string.Length`, `PadRight` and slicing wherever the result lands on screen.

```csharp
public static class TextWidth
```

## Methods

| Member | Summary |
|---|---|
| [`CountClusters(string)`](#countclusters-string) | How many symbols the text is made of, as the user would count them. |
| [`NextClusterEnd(string, int)`](#nextclusterend-string-int) | Where the symbol at an index ends. This is what a forward delete has to remove. |
| [`NextClusterLength(string, int)`](#nextclusterlength-string-int) | How many `char` values the next symbol occupies, starting at an index. |
| [`Of(string)`](#of-string) | How many columns the text occupies. |
| [`OfCluster(ReadOnlySpan<char>)`](#ofcluster-readonlyspan-char) | Width of a single grapheme cluster — one symbol as the user sees it. |
| [`OfRune(Rune)`](#ofrune-rune) | Width of a single code point, before combining marks are taken into account. |
| [`PadLeft(string, int)`](#padleft-string-int) | Pads the text with spaces on the left, which right-aligns it in that width. |
| [`PadRight(string, int)`](#padright-string-int) | Pads the text with spaces on the right until it fills the given column width. |
| [`PreviousClusterStart(string, int)`](#previousclusterstart-string-int) | Where the symbol before an index starts. This is what a backspace has to remove: deleting one `char` would cut an emoji or a letter with a combining mark in half. |
| [`SnapToCluster(string, int)`](#snaptocluster-string-int) | Pulls a position back to the start of the symbol it lands in, so an index that came from somewhere else never points into the middle of one. |
| [`Truncate(string, int)`](#truncate-string-int) | Cuts the text down to a column width on a symbol boundary, so a wide character or a surrogate pair is never split in half. |
| [`TruncateStart(string, int)`](#truncatestart-string-int) | Cuts the text down to a column width from the other end, keeping the tail rather than the head. That is what a field scrolled to the right shows. |
| [`Wrap(string, int)`](#wrap-string-int) | Breaks text into lines that fit a column width, at spaces where there is one and mid-word only for a word wider than the space. Line breaks already in the text are kept. |

## Methods in detail

### `CountClusters(string)` {#countclusters-string}

```csharp
public static int CountClusters(string text);
```

How many symbols the text is made of, as the user would count them.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to count. |

**Returns** `int` — The number of grapheme clusters.

### `NextClusterEnd(string, int)` {#nextclusterend-string-int}

```csharp
public static int NextClusterEnd(string text, int index);
```

Where the symbol at an index ends. This is what a forward delete has to remove.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text being walked. |
| `index` | `int` | Position to look forward from. |

**Returns** `int` — The next boundary, or the length of the text at its end.

### `NextClusterLength(string, int)` {#nextclusterlength-string-int}

```csharp
public static int NextClusterLength(string text, int index);
```

How many `char` values the next symbol occupies, starting at an index.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text being walked. |
| `index` | `int` | Where the symbol starts. |

**Returns** `int` — Length of the cluster in `char` values.

### `Of(string)` {#of-string}

```csharp
public static int Of(string text);
```

How many columns the text occupies.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to measure. |

**Returns** `int` — Width in terminal columns.

### `OfCluster(ReadOnlySpan<char>)` {#ofcluster-readonlyspan-char}

```csharp
public static int OfCluster(ReadOnlySpan<char> cluster);
```

Width of a single grapheme cluster — one symbol as the user sees it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `cluster` | `ReadOnlySpan<T>`&lt;`char`&gt; | The cluster, as returned by [`TextWidth.NextClusterLength`](../arlecchino.rendering.text/TextWidth.md#nextclusterlength-string-int). |

**Returns** `int` — 0, 1 or 2 columns.

### `OfRune(Rune)` {#ofrune-rune}

```csharp
public static int OfRune(Rune rune);
```

Width of a single code point, before combining marks are taken into account.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `rune` | `Rune` | The code point to measure. |

**Returns** `int` — 0 for marks and control characters, 2 for wide ranges, 1 otherwise.

### `PadLeft(string, int)` {#padleft-string-int}

```csharp
public static string PadLeft(string text, int width);
```

Pads the text with spaces on the left, which right-aligns it in that width.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to pad. |
| `width` | `int` | Columns to fill. |

**Returns** `string` — The padded text, unchanged when it is already that wide.

### `PadRight(string, int)` {#padright-string-int}

```csharp
public static string PadRight(string text, int width);
```

Pads the text with spaces on the right until it fills the given column width.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to pad. |
| `width` | `int` | Columns to fill. |

**Returns** `string` — The padded text, unchanged when it is already that wide.

### `PreviousClusterStart(string, int)` {#previousclusterstart-string-int}

```csharp
public static int PreviousClusterStart(string text, int index);
```

Where the symbol before an index starts. This is what a backspace has to remove: deleting one `char` would cut an emoji or a letter with a combining mark in half.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text being walked. |
| `index` | `int` | Position to look back from. |

**Returns** `int` — The boundary before the index, or `0` at the start of the text.

### `SnapToCluster(string, int)` {#snaptocluster-string-int}

```csharp
public static int SnapToCluster(string text, int index);
```

Pulls a position back to the start of the symbol it lands in, so an index that came from somewhere else never points into the middle of one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text being walked. |
| `index` | `int` | Position to snap. |

**Returns** `int` — The boundary at or before the index.

### `Truncate(string, int)` {#truncate-string-int}

```csharp
public static string Truncate(string text, int maxWidth);
```

Cuts the text down to a column width on a symbol boundary, so a wide character or a surrogate pair is never split in half.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to cut. |
| `maxWidth` | `int` | Columns available. |

**Returns** `string` — The longest prefix that fits.

### `TruncateStart(string, int)` {#truncatestart-string-int}

```csharp
public static string TruncateStart(string text, int maxWidth);
```

Cuts the text down to a column width from the other end, keeping the tail rather than the head. That is what a field scrolled to the right shows.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to cut. |
| `maxWidth` | `int` | Columns available. |

**Returns** `string` — The longest suffix that fits.

### `Wrap(string, int)` {#wrap-string-int}

```csharp
public static List<string> Wrap(string text, int width);
```

Breaks text into lines that fit a column width, at spaces where there is one and mid-word only for a word wider than the space. Line breaks already in the text are kept.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The text to break up. |
| `width` | `int` | Columns available; anything below one is treated as one. |

**Returns** `List<T>`&lt;`string`&gt; — The lines, in order.

