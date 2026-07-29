---
title: Overview
sidebar_label: Overview
sidebar_position: 0
slug: /
description: Arlecchino is a terminal UI framework for .NET. Views are plain classes, navigation keeps a history, and everything is wired through Microsoft.Extensions.DependencyInjection.
---

# Arlecchino documentation

Arlecchino is a terminal UI framework for .NET. A view is a plain class, navigation keeps a history,
and every part of the machinery is a service in `Microsoft.Extensions.DependencyInjection`.

```bash
dotnet add package Arlecchino
```

New here? [Getting started](getting-started.md) is the smallest application that runs, and
[Lexicon](lexicon.md) is every term these pages use — including the ones this framework deliberately
does not have.

## Start here

| Page | What it covers |
|---|---|
| [Getting started](getting-started.md) | Installing the package, the smallest app that runs, the first view |
| [Tutorial: your first app](tutorial-todo.md) | A todo list from an empty project: a store of atoms, a list, two modals and a status bar |
| [Showcase](showcase.md) | The applications built on the framework and what each one demonstrates |
| [Lexicon](lexicon.md) | Every term, in one place |

## The application

| Page | What it covers |
|---|---|
| [Hosting and options](hosting-and-options.md) | `AddArlecchino`, every option, the builder API, running without the hosted service |
| [The frame loop](frame-loop.md) | When a frame is drawn, which thread draws it, and how work gets back onto it |
| [Views and navigation](views-and-navigation.md) | `IArlecchinoView`, `ViewRoute`, the navigator, history, view registration |
| [Source generator](source-generator.md) | How `ViewKind`, the factories and the registrations are emitted, MSBuild switches |

## Drawing

| Page | What it covers |
|---|---|
| [Rendering](rendering.md) | `Surface`: what a frame costs, geometry, headless rendering |
| [Layout](layout.md) | The flow cursor, absolute calls, regions and clipping |
| [Text and width](text.md) | Why measurement is in columns, and the `TextWidth` calls that do it |
| [Colours](colours.md) | `TermColor`, `RgbTermColor`, and what the terminal can actually show |
| [Theming](theming.md) | `Theme`, `ThemePalette`, and the framework's own palette |

## Input

| Page | What it covers |
|---|---|
| [Keyboard](keyboard.md) | How a key travels, the keymap, the keys screen, layouts, paste and copy |
| [Commands](commands.md) | `IArlecchinoCommand`, `ViewCommand`, the palette, the conflict check |
| [Mouse](mouse.md) | `MouseEvent`, hit-testing, and why Windows is different |
| [Focus](focus.md) | `FocusRing` and `IArlecchinoFocusable` |
| [ANSI and the terminal](ansi.md) | What goes out, what comes in, and `IArlecchinoTerminal` |

## State

| Page | What it covers |
|---|---|
| [Atoms](atoms.md) | `TrackedAtom` and `LocalAtom`, computed values, undo |
| [Stores](stores.md) | A class of atoms that registers itself |
| [Async atoms](async-atoms.md) | Loading in the background, and tying work to a screen |
| [Forms](forms.md) | `Form` and `Field` |
| [Application state](state.md) | `ArlecchinoState`, the output line, notifications |
| [Modals](modals.md) | Every dialog that ships, plus stacking and validation |

## Widgets

| Page | What it covers |
|---|---|
| [Widgets overview](widgets.md) | The two interfaces, and writing one of your own |
| [ListBox](lists.md) | A scrolling, selectable, clickable list |
| [Table](table.md) | Columns that size themselves, and sorting |
| [Tree](tree.md) | A hierarchy that fills its children in on demand |
| [Tabs](tabs.md) | A strip of titles across a pane |
| [Scrolling](scrolling.md) | `ScrollPane`, `ScrollWindow` and `ScrollBar` |
| [TextView](text-view.md) | A block of text, wrapped and cached |
| [Status bar and indicators](status-bar.md) | `StatusBar`, `ProgressBar`, `Spinner` |
| [Charts](charts.md) | `Sparkline`, `BarChart`, `Gauge` |
| [File picker](file-picker.md) | Requesting a path, the places sidebar, filters and keys |

## Guides

| Page | What it covers |
|---|---|
| [Localization](localization.md) | `ArlecchinoStrings` and why no user-visible text is hardcoded |
| [Diagnostics](diagnostics.md) | The log overlay, notifications, and the report to attach to a bug |
| [Testing](testing.md) | `ArlecchinoTestHost`, `FakeTerminal`, `FrameText` |
| [Packages and building](packages-and-building.md) | What ships in which package, versioning, CI, benchmarks |

## Releases

| Page | What it covers |
|---|---|
| [What's new in 2.0](whats-new-in-2.0.md) | The three breaking changes, and what came with them |
| [Migrating to 2.0](migrating-to-2.0.md) | The edits an application written against `1.x` needs |
| [API reference](api/index.md) | Every public type, generated from the assemblies |

## Where things live

| Assembly | Namespaces | Contents |
|---|---|---|
| `Arlecchino.Core` | `Arlecchino`, `Arlecchino.Rendering`, `Arlecchino.Input`, `Arlecchino.Atoms` | `Surface`, `Theme`, `TermColor`, `KeyText`, `IArlecchinoTerminal`, the atoms — the renderer, no DI |
| `Arlecchino` | `Arlecchino.Hosting`, `Arlecchino.Navigation`, `Arlecchino.Commands`, `Arlecchino.Modals`, `Arlecchino.State`, `Arlecchino.Views`, `Arlecchino.Forms`, `Arlecchino.Focus`, `Arlecchino.Widgets`, `Arlecchino.Diagnostics` | views, navigation, modals, commands, forms, widgets, hosting, the file picker |
| `Arlecchino.Testing` | `Arlecchino.Testing` | `ArlecchinoTestHost`, `FakeTerminal`, `FrameText` — the headless host for tests |
| `Arlecchino.Generators` | — | the incremental generator, shipped inside the `Arlecchino` package |

What changed between versions is in the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md).
