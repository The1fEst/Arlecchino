---
title: ColorSupport
sidebar_label: ColorSupport
---

# ColorSupport enum

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

How much colour the terminal can show. Detected once at startup by [`TerminalCapabilities.DetectColor`](../arlecchino.rendering/TerminalCapabilities.md#detectcolor) and used by every style when it builds its escape sequence.

```csharp
public enum ColorSupport : byte
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `None` | `0` | No colour at all: styles emit nothing, not even the per-line reset. Chosen for `NO_COLOR`, `TERM=dumb`, or a Windows console that refused virtual terminal mode. |
| `Palette` | `1` | The sixteen ANSI colours; a [`Rgb`](../arlecchino.rendering/Rgb.md) is mapped to the nearest of them. |
| `TrueColor` | `2` | Full 24-bit colour, so [`RgbTermColor`](../arlecchino.rendering/RgbTermColor.md) emits exact values. |

