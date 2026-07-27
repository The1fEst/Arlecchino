---
title: TerminalColor
sidebar_label: TerminalColor
---

# TerminalColor enum

**Namespace:** `Arlecchino.Rendering` &middot; **Assembly:** `Arlecchino.Core`

The sixteen ANSI colours plus the terminal's own default. Exact shades belong to the terminal theme, which is why chrome should pick a role from [`Theme`](../arlecchino.rendering/Theme.md) rather than a colour here.

```csharp
public enum TerminalColor : byte
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `Default` | `0` | Whatever the terminal uses when no colour is set. |
| `Black` | `1` | Black. |
| `Red` | `2` | Red. |
| `Green` | `3` | Green. |
| `Yellow` | `4` | Yellow. |
| `Blue` | `5` | Blue. |
| `Magenta` | `6` | Magenta. |
| `Cyan` | `7` | Cyan. |
| `White` | `8` | White. |
| `BrightBlack` | `9` | Bright black, usually rendered as grey. |
| `BrightRed` | `10` | Bright red. |
| `BrightGreen` | `11` | Bright green. |
| `BrightYellow` | `12` | Bright yellow. |
| `BrightBlue` | `13` | Bright blue. |
| `BrightMagenta` | `14` | Bright magenta. |
| `BrightCyan` | `15` | Bright cyan. |
| `BrightWhite` | `16` | Bright white. |

