---
title: TextInputMode
sidebar_label: TextInputMode
---

# TextInputMode enum

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

How a key press becomes a character, which decides what happens on a non-latin layout.

```csharp
public enum TextInputMode : byte
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `LatinOnly` | `0` | ASCII is taken as typed; anything else falls back to the physical key, so filters and shortcuts keep working on a Cyrillic layout without switching it. |
| `Native` | `1` | Any non-control character is taken as typed. |

