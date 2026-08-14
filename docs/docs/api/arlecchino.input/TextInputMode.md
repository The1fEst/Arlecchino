---
title: "TextInputMode"
sidebar_label: "TextInputMode"
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
| `ByPosition` | `0` | The character is taken from where the key sits rather than from what the layout makes of it, so a shortcut reads the same on every layout and that language cannot be typed at all. |
| `Native` | `1` | Any non-control character is taken as typed, so any language can be typed. |

