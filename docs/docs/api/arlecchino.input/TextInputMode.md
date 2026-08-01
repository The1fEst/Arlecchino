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
| `ByPosition` | `0` | The character is taken from where the key sits on the keyboard rather than from what the layout makes of it, so a filter or a shortcut reads the same on a Cyrillic or a Greek layout as it does on a US one — at the cost of not being able to type that language at all. |
| `Native` | `1` | Any non-control character is taken as typed, so any language can be typed. |

