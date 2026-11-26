---
title: What's new in 2026.11.2
sidebar_label: What's new in 2026.11.2
description: The packages target net10.0 and net11.0 and no longer net8.0; the source generator needs the compiler of a .NET 10 SDK or newer.
---

# What's new in 2026.11.2

A release about the .NET versions the packages run on. They target `net10.0` and `net11.0`, which are the
supported releases, and `net8.0` is gone. The one break is for an application that is not on .NET 10 or
newer, or is built with an older SDK; [Migrating to 2026.11.2](migrating-to-2026.11.2.md) is what to do
about it. The [changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#2026112) is the
full record.

## Target frameworks

`Arlecchino.Core`, `Arlecchino`, `Arlecchino.Atoms.History`, `Arlecchino.Pictures` and
`Arlecchino.Testing` carry `lib/net10.0` and `lib/net11.0`. `net10.0` is the long-term support release and
`net11.0` the current one, and the suite runs on both. An application on `net8.0` or `net9.0` can no
longer reference the packages.

`LogBuffer` and `HistoryFile` lock on `System.Threading.Lock`.

## The generator needs a newer compiler

The source generator is built against Roslyn 5.0.0, the compiler of the .NET 10.0.100 SDK. A generator
runs inside the compiler the application is built with, so an older SDK skips it with `CS9057` and nothing
is generated. See [Packages and building](packages-and-building.md) for why the version is pinned there.
