---
title: What's new in 2026.11.1
sidebar_label: What's new in 2026.11.1
description: An application with translations starts in the user's language, also under InvariantGlobalization and NativeAOT; a shortcut on a character typed with Shift answers on Windows; the sample speaks English, Russian and German.
---

# What's new in 2026.11.1

A release about the language an application starts in. One that ships several translations now starts in the
one the user's system is set to, and the sample shows it. The one break is a call written `Nearest(null)`,
which no longer compiles, and a shortcut on `:` or `!` now answers on Windows. The
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#2026111) is the full record.

## Starting in the system language

`Localization.Language` started at the nearest match to `CultureInfo.CurrentUICulture`. That is empty under
`InvariantGlobalization`, so every such application started in the default language whatever the system was
set to. It now starts at `Localization.SystemLanguage`, the language there is text for that is nearest the
ones the operating system reports, in order of preference. `ru-RU` finds `ru`, and when nothing matches the
default is drawn. `CultureInfo` is not consulted, so this also holds in a NativeAOT build.

`SystemLanguages.GetPreferred()` reads those languages when asked, as tags:

- **Windows** asks for the user's preferred UI languages.
- **Linux, macOS and the BSDs** read the gettext environment: `LANGUAGE`, then the first of `LC_ALL`,
  `LC_MESSAGES` and `LANG` that is set.

An application that offers `auto` in its settings assigns `SystemLanguage` instead of naming a language:

```csharp
Localization.Language = setting switch
{
    "auto" => Localization.SystemLanguage,
    _ => setting,
};
```

The rules for the match, and what the generator writes, are on the [localization](localization.md#translations)
page.

## `Nearest` takes a list

`Localization.Nearest` also takes a list of cultures, for an application that keeps its own idea of what the
user prefers. A bare `null` fits both overloads, so `Localization.Nearest(null)` no longer compiles. Say which
one is meant:

```csharp
Localization.Nearest((string?)null);
```

## Shift and a position on Windows

A shortcut on a character typed with Shift, such as `:` or `!`, did not answer on Windows when keys are read
by position. The console drops the Shift that only typed the character, and the position was read after it
went, so Shift and the key under `;` stood for `;` rather than `:`. The position is now read before the Shift
is dropped. How a key is named is on the [keyboard](keyboard.md) page.

## The sample

`samples/Arlecchino.Sample` is in English, Russian and German. Its settings form has a `Language` choice of
`auto`, `en`, `ru` and `de`, and a line above it shows the languages the system reported. See
[Getting started](getting-started.md#running-the-samples) for running it.
