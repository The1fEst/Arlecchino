---
title: "KeyModifiers"
sidebar_label: "KeyModifiers"
---

# KeyModifiers enum

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

Modifiers held with a key. The three the console knows about keep the values `ConsoleModifiers` gives them, so the two agree bit by bit; [`KeyModifiers.Super`](../arlecchino.input/KeyModifiers.md) is the one the console has no room for — Command on a Mac, the Windows key elsewhere.

```csharp
[Flags]
public enum KeyModifiers
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `None` | `0` | Nothing held. |
| `Alt` | `1` | Alt, or Option on a Mac. |
| `Shift` | `2` | Shift. |
| `Control` | `4` | Control. |
| `Super` | `8` | Command on a Mac, the Windows key elsewhere. The Windows console never reports it, since the system takes the key before an application sees it. |

