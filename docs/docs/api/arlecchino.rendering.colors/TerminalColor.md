---
title: "TerminalColor"
sidebar_label: "TerminalColor"
---

# TerminalColor enum

**Namespace:** `Arlecchino.Rendering.Colors` &middot; **Assembly:** `Arlecchino.Core`

The sixteen ANSI colors plus the terminal's own default. Exact shades belong to the terminal theme, which is why chrome should pick a role from [`Theme`](../arlecchino.rendering.colors/Theme.md) rather than a color here.

```csharp
public enum TerminalColor : byte
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `Default` | `0` | Whatever the terminal uses when no color is set. |
| `Black` | `1` | Black. |
| `Red` | `2` | Red. |
| `Green` | `3` | Green. |
| `Yellow` | `4` | Yellow. |
| `Blue` | `5` | Blue. |
| `Magenta` | `6` | Magenta. |
| `Cyan` | `7` | Cyan. |
| `White` | `8` | White. |
| `BrightBlack` | `9` | Bright black, usually rendered as gray. |
| `BrightRed` | `10` | Bright red. |
| `BrightGreen` | `11` | Bright green. |
| `BrightYellow` | `12` | Bright yellow. |
| `BrightBlue` | `13` | Bright blue. |
| `BrightMagenta` | `14` | Bright magenta. |
| `BrightCyan` | `15` | Bright cyan. |
| `BrightWhite` | `16` | Bright white. |

