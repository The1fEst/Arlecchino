---
title: "LoadStatus"
sidebar_label: "LoadStatus"
---

# LoadStatus enum

**Namespace:** `Arlecchino.Atoms` &middot; **Assembly:** `Arlecchino`

Where a background load has got to.

```csharp
public enum LoadStatus : byte
```

## Fields

| Name | Value | Summary |
|---|---:|---|
| `Idle` | `0` | Nothing has been loaded yet. |
| `Loading` | `1` | A load is running. |
| `Loaded` | `2` | The last load finished and its result is in place. |
| `Failed` | `3` | The last load threw; see the error. |

