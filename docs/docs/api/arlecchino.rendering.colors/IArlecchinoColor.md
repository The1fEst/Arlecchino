---
title: "IArlecchinoColor"
sidebar_label: "IArlecchinoColor"
---

# IArlecchinoColor interface

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

Anything that can style a cell. The frame writer only ever asks for [`IArlecchinoColor.Ansi`](../arlecchino.rendering.colors/IArlecchinoColor.md#ansi) and compares styles by reference, so hold on to instances instead of building one per cell.

```csharp
public interface IArlecchinoColor
```

**Implemented by** [`RgbTermColor`](../arlecchino.rendering.colors/RgbTermColor.md), [`TermColor`](../arlecchino.rendering.colors/TermColor.md)

## Properties

| Member | Summary |
|---|---|
| [`Ansi`](#ansi) | The escape sequence that switches the terminal to this style, or an empty string when color is turned off. Implementations are expected to build it once and cache it. |

## Properties in detail

### `Ansi` {#ansi}

```csharp
public string Ansi { get; }
```

The escape sequence that switches the terminal to this style, or an empty string when color is turned off. Implementations are expected to build it once and cache it.

**Type** `string`

