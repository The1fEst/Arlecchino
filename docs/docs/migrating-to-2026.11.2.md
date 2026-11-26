---
title: Migrating to 2026.11.2
sidebar_label: Migrating to 2026.11.2
description: Moving an application to net10.0 or net11.0 and a .NET 10 SDK, and what CS9057 or a missing AddGeneratedViews means.
---

# Migrating to 2026.11.2

One break, and the restore or the compiler names it: the packages target `net10.0` and `net11.0`, and the
source generator needs the compiler of a .NET 10 SDK or newer. An application already on one of those
has nothing to do.

| What changed | What to do |
|---|---|
| `net8.0` and `net9.0` applications cannot reference the packages | Set `<TargetFramework>` to `net10.0` or `net11.0` |
| The generator is skipped by the compiler of an older SDK | Build with the .NET 10 SDK or newer |

## The target framework

An application on `net8.0` or `net9.0` fails to restore with `NU1202`, since the packages carry no
library for it. Change the project file:

```xml
<TargetFramework>net10.0</TargetFramework>
```

`net10.0` is the long-term support release and `net11.0` the current one; either works.

## The SDK

The generator is built against Roslyn 5.0.0, which ships in the .NET 10.0.100 SDK. A compiler older than
that does not load it and reports `CS9057`, and nothing is generated, so what follows is a list of
errors about what the generator would have written:

```text
error CS1061: ... does not contain a definition for 'AddGeneratedViews'
```

`AddGeneratedViews`, `AddGeneratedStores`, `AddGeneratedWidgets` and the generated routes are missing
together. `dotnet --version` says which SDK builds the project; a `global.json` that pins an older one
is the usual reason it is not the one installed. Use a .NET 10 SDK or newer, and the errors go away.
