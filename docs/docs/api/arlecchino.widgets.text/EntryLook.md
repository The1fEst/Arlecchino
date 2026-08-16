---
title: "EntryLook"
sidebar_label: "EntryLook"
---

# EntryLook struct

**Namespace:** `Arlecchino.Widgets.Text` &middot; **Assembly:** `Arlecchino`

How a line being typed into is written: the text itself, the part of it that is selected, and the one symbol the caret stands on, which is written the other way round.

```csharp
public readonly struct EntryLook : IEquatable<EntryLook>
```

**Implements** `IEquatable<T>`&lt;[`EntryLook`](../arlecchino.widgets.text/EntryLook.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`EntryLook(IArlecchinoColor, IArlecchinoColor, IArlecchinoColor)`](#entrylook-iarlecchinocolor-iarlecchinocolor-iarlecchinocolor) | How a line being typed into is written: the text itself, the part of it that is selected, and the one symbol the caret stands on, which is written the other way round. |

## Properties

| Member | Summary |
|---|---|
| [`Caret`](#caret) | What the symbol under the caret is written in. |
| [`Selected`](#selected) | What the selected part of it is written in. |
| [`Text`](#text) | What the line is written in. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out IArlecchinoColor, out IArlecchinoColor, out IArlecchinoColor)`](#deconstruct-out-iarlecchinocolor-out-iarlecchinocolor-out-iarlecchinocolor) |  |

## Constructors in detail

### `EntryLook(IArlecchinoColor, IArlecchinoColor, IArlecchinoColor)` {#entrylook-iarlecchinocolor-iarlecchinocolor-iarlecchinocolor}

```csharp
public EntryLook(IArlecchinoColor Text, IArlecchinoColor Selected, IArlecchinoColor Caret);
```

How a line being typed into is written: the text itself, the part of it that is selected, and the one symbol the caret stands on, which is written the other way round.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Text` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | What the line is written in. |
| `Selected` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | What the selected part of it is written in. |
| `Caret` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) | What the symbol under the caret is written in. |

## Properties in detail

### `Caret` {#caret}

```csharp
public IArlecchinoColor Caret { get; init; }
```

What the symbol under the caret is written in.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Selected` {#selected}

```csharp
public IArlecchinoColor Selected { get; init; }
```

What the selected part of it is written in.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

### `Text` {#text}

```csharp
public IArlecchinoColor Text { get; init; }
```

What the line is written in.

**Type** [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)

## Methods in detail

### `Deconstruct(out IArlecchinoColor, out IArlecchinoColor, out IArlecchinoColor)` {#deconstruct-out-iarlecchinocolor-out-iarlecchinocolor-out-iarlecchinocolor}

```csharp
public void Deconstruct(
    out IArlecchinoColor Text,
    out IArlecchinoColor Selected,
    out IArlecchinoColor Caret);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Text` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) |  |
| `Selected` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) |  |
| `Caret` | [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md) |  |

