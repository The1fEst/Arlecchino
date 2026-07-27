---
title: Showcase
sidebar_label: Showcase
description: The three sample applications that ship in the repository, what each one demonstrates, and how to render any of their screens as a single frame.
---

# Showcase

Three applications ship in the repository. They are the readable version of everything on these pages:
each one is built on the public API and nothing else.

## Arlecchino.Packages

A dependency review for a .NET solution, and the largest of the three. It runs `dotnet list package`
four times — the graph, newer versions, advisories, deprecations — merges the reports into one
catalogue, and shows what came out across four screens.

```bash
dotnet run --project samples/Arlecchino.Packages
```

![Every package in the solution, coloured by what is wrong with it](/img/screenshots/inventory.png)

With no arguments it reads the fixture solution kept beside it — three projects wired to packages that
are outdated, vulnerable, deprecated and resolved at more than one version, so every screen has
something to show without reaching for a real repository. `--solution <path>` points it at one of
yours.

| Screen | What it shows | Uses |
|---|---|---|
| Inventory | A filtered table behind tabs | [`Table`](table.md), [`Tabs`](tabs.md), [text modal](modals.md#text) |
| Package | One package, its advisories and every project that pulls it in | [Regions](layout.md#regions), [`ListBox`](lists.md) |
| Projects | The dependency tree beside a per-project table | [`Tree`](tree.md), [`FocusRing`](focus.md) |
| Upgrade | A form that writes `dotnet add package` commands and can run them | [`Form`](forms.md), [atoms](atoms.md), every [modal](modals.md) |

![The vulnerable tab](/img/screenshots/vulnerable.png)

![The dependency tree beside a per-project table](/img/screenshots/projects.png)

![The upgrade form and the commands it would run](/img/screenshots/upgrade.png)

It runs **without the output line**, so the bottom row belongs to the screen's own
[status bar](status-bar.md), and `:h` turns the hints box off and on — the palette command flips
`options.ShowHints` at runtime.

The four screens render headlessly as `--frame inventory 120x30`, `--frame package 120x26`,
`--frame projects 120x30` and `--frame upgrade 120x24`.

## Arlecchino.Processes

A small application that does real work rather than showing off widgets. It lists the processes on the
machine in a sortable [table](table.md), reads them on a background thread through an
[`AsyncAtom`](async-atoms.md) with a [spinner](status-bar.md#spinner) while it loads, filters them from
a text modal, and opens a details screen for the selected row.

```bash
dotnet run --project samples/Arlecchino.Processes
```

`r` re-reads the list, `m` and `n` sort, `f` filters, `Enter` opens the details. All four are
[view commands](commands.md#commands-of-a-view), so they appear in the palette and in the hints box
without being written down twice.

It renders headlessly too — `--frame processes 110x26` or `--frame details 90x18`.

## Arlecchino.Sample

The gallery: a default view, an about view, a settings form, the widget page, a command palette and
the file picker.

```bash
dotnet run --project samples/Arlecchino.Sample
```

![The command palette over the inventory](/img/screenshots/palette.png)

![The file picker asking for another solution](/img/screenshots/picker.png)

The frame goes to stdout as ANSI text; the view name is `default`, `about`, `picker`, `widgets`, or one
of `password`, `number`, `slider`, `toggle`, `multi`, `date`, `time`, `color` to render the matching
modal over the default view:

```bash
dotnet run --project samples/Arlecchino.Sample -- --frame widgets 100x24
```

## Rendering any of them as a frame

Every sample takes `--frame <view> <width>x<height>` and writes one composed frame to stdout. That is
the fastest way to look at a layout, and it is three lines of wiring in an application of your own —
see [Rendering](rendering.md#rendering-without-a-terminal).

![The keys screen](/img/screenshots/help.png)

![The log overlay](/img/screenshots/log.png)

## Built something?

If you have an application built on Arlecchino,
[an issue](https://github.com/The1fEst/Arlecchino/issues) is the place to say so.
