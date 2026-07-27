---
title: TextFormat
sidebar_label: TextFormat
---

# TextFormat enum

**Namespace:** `Arlecchino.Modals` &middot; **Assembly:** `Arlecchino`

A built-in check a text field runs before your own validator, so common mistakes are caught with a translated message instead of a hand-written regex.

```csharp
public enum TextFormat : byte
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `Free` | `0` | Anything goes. |
| `Email` | `1` | One `@`, no spaces, and a dotted domain. |
| `Url` | `2` | An absolute `http` or `https` address. |

