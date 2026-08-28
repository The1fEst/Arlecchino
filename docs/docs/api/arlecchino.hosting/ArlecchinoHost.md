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
| [`CreateBuilder(string[])`](#createbuilder-string) | Builds a host that reads its settings from the environment and the command line, and writes its log through the console the application has taken over. |

## Methods in detail

### `CreateBuilder(string[])` {#createbuilder-string}

```csharp
public static HostApplicationBuilder CreateBuilder(string[]? args = null);
```

Builds a host that reads its settings from the environment and the command line, and writes its log through the console the application has taken over.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `args` | `string`\[\] | What the application was started with, or `null` when it takes none. |

**Returns** `HostApplicationBuilder` — The builder, for registering the application on with `AddArlecchino`.

This is `CreateApplicationBuilder` without the part of it that watches the file system. The host's own defaults reload configuration from the folder the process was started in, and a folder watched that way is watched along with everything under it. A terminal application is started wherever its user happens to be, so started from a home directory that is one watch per folder in it — hundreds of thousands of them, and seconds before the first frame appears.



Settings files are left to the application, which knows where its own belong and whether they are worth watching. Anything read from one is found next to the application rather than next to the user.

