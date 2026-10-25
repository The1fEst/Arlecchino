---
title: "Align"
sidebar_label: "Align"
---

# Align enum

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

Where text or a block sits inside the space it is drawn into. Horizontal and vertical flags combine, so `Align.Right | Align.Bottom` anchors to a corner.

```csharp
[Flags]
public enum Align : byte
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `Left` | `1` | Against the left edge of the content area. |
| `Center` | `2` | Centered horizontally in the content area. |
| `Right` | `4` | Against the right edge of the content area. |
| `Top` | `8` | Against the top edge. Only block and region calls honor the vertical flags. |
| `Middle` | `16` | Centered vertically. |
| `Bottom` | `32` | Against the bottom edge. |

