---
title: "TextStyle"
sidebar_label: "TextStyle"
---

# TextStyle enum

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

Text attributes a style carries on top of its colors. Combine them with `|`; a terminal that does not support one simply ignores it.

```csharp
[Flags]
public enum TextStyle : byte
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `None` | `0` | No attributes. |
| `Bold` | `1` | Bold, which some terminals render as a brighter color instead. |
| `Italic` | `2` | Italic, the least widely supported of the four. |
| `Underline` | `4` | Underlined. |
| `Dim` | `8` | Dim, the opposite of [`TextStyle.Bold`](../arlecchino.rendering.colors/TextStyle.md). |

