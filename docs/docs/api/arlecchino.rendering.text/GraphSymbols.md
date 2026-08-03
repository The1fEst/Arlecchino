---
title: GraphSymbols
sidebar_label: GraphSymbols
---

# GraphSymbols enum

**Namespace:** `Arlecchino.Rendering.Text` &middot; **Assembly:** `Arlecchino.Core`

Which characters a graph is drawn with. The choice is about the font the terminal was given rather than about taste: the denser the symbols, the more of them a font has to carry.

```csharp
public enum GraphSymbols
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `Braille` | `0` | Braille dots, four levels and two samples to a cell — the densest, and what a graph looks best in. Needs a font carrying the Braille Patterns block, or a terminal that falls back to one that does; Windows Terminal does, the classic console host does not. |
| `Blocks` | `1` | Quadrant blocks, two levels and two samples to a cell — half the height of braille, and in nearly every monospace font there is. |
| `Tty` | `2` | Shaded blocks, three levels and one sample to a cell. The plainest of the three, for a console whose font carries little more than ASCII. |

