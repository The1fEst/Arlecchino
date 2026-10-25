---
title: "TextEntry"
sidebar_label: "TextEntry"
---

# TextEntry class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

A line of text held on its own, for whatever is typed into but is not a dialog: a filter in a view, a line an application draws for itself. What each key does to it is [`TextEditing`](../arlecchino.editing/TextEditing.md)'s.

```csharp
public sealed class TextEntry : ITextEntry
```

**Implements** [`ITextEntry`](../arlecchino.editing/ITextEntry.md)

## Constructors

| Member | Summary |
|---|---|
| [`TextEntry()`](#textentry) |  |

## Properties

| Member | Summary |
|---|---|
| [`Anchor`](#anchor) |  |
| [`Caret`](#caret) |  |
| [`Text`](#text) |  |

## Constructors in detail

### `TextEntry()` {#textentry}

```csharp
public TextEntry();
```

## Properties in detail

### `Anchor` {#anchor}

```csharp
public int Anchor { get; set; }
```

**Type** `int`

### `Caret` {#caret}

```csharp
public int Caret { get; set; }
```

**Type** `int`

### `Text` {#text}

```csharp
public string Text { get; set; }
```

**Type** `string`

