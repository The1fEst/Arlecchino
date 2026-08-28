---
title: "ArlecchinoHost"
sidebar_label: "ArlecchinoHost"
---

# ArlecchinoHost class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Builds the host a terminal application runs on.

```csharp
public static class ArlecchinoHost
```

## Methods

| Member | Summary |
|---|---|
| [`CreateBuilder(string[])`](#createbuilder-string) | Builds the host, with settings from the environment and the command line and a log through the console. It watches no folder for changes, which costs seconds in a large one. |

## Methods in detail

### `CreateBuilder(string[])` {#createbuilder-string}

```csharp
public static HostApplicationBuilder CreateBuilder(string[]? args = null);
```

Builds the host, with settings from the environment and the command line and a log through the console. It watches no folder for changes, which costs seconds in a large one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `args` | `string`\[\] | What the application was started with, or `null` when it takes none. |

**Returns** `HostApplicationBuilder` — The builder, for registering the application on with `AddArlecchino`.

