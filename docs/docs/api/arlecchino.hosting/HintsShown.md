---
title: "HintsShown"
sidebar_label: "HintsShown"
---

# HintsShown enum

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

When the framework draws its own box of keys in the corner.

```csharp
public enum HintsShown
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `Always` | `0` | On every frame, listing whatever can be pressed where the cursor is. |
| `WhileWaiting` | `1` | Only while a chord is half typed, listing the keys that would finish it. |
| `Never` | `2` | Never. The application draws the keys itself, chords included. |

