---
title: Showcase
sidebar_label: Showcase
description: The applications built on Arlecchino, what each one demonstrates, and how to render any of their screens as a single frame.
---

# Showcase

Three applications are the readable version of everything on these pages: each one is built on the
public API and nothing else. Two ship beside the framework; the largest has a repository of its own.

## Arlecchino.Commander

A Midnight Commander, and the largest of the three. Two panels, the function keys where they have
always been, and the same panel over a local disk, an SFTP server or an FTP one. It lives in
[its own repository](https://github.com/The1fEst/Arlecchino.Commander) and takes the framework from
NuGet, the way an application of yours would.

```bash
dotnet run --project src/Arlecchino.Commander -- C:\some\folder C:\another
```

![Two panels over a local disk](/img/screenshots/panels.png)

Each panel is a [widget](widgets.md) of its own over one folder. `Space` marks a row, and what is
marked is counted at the foot of the panel:

![Three files marked, counted at the foot of the panel](/img/screenshots/marks.png)

Either panel sorts by name, size or date, narrows down to a filter, and reads a file without leaving
the panels:

![The right panel sorted by size](/img/screenshots/sorted.png)

![The panel filtered by name](/img/screenshots/filter.png)

![A file read without leaving the panels](/img/screenshots/viewer.png)

| Part | What it shows | Uses |
|---|---|---|
| The panels | Two widgets in one layout, either of which may hold the focus | [widgets](widgets.md), [`PaneTree`](layout.md), [`FocusRing`](focus.md) |
| The tabs | A band that says what is open and takes a click on it | [mouse](mouse.md), [atoms](atoms.md) |
| The menu | Sections of a menu offered as lists | [modals](modals.md#a-dialog-of-your-own), [view commands](commands.md) |
| Copy, move, delete | Questions asked first, then the work off the drawing thread | [modals](modals.md), [rendering](rendering.md) |
| Work in flight | A bar, what is being worked on now, and a key that stops it | [status bar](status-bar.md), [notifications](state.md#notifications) |
| Servers | A panel over SFTP or FTP, and a screen that runs commands over SSH | [stores](stores.md), [`Form`](forms.md), [async atoms](async-atoms.md) |

`F9` opens the menu, and what can be done to what is marked is one list under it:

![The menu, opened by F9](/img/screenshots/menu.png)

![What can be done to what is marked](/img/screenshots/file-menu.png)

Copying and deleting ask first, with the negative answer selected, so a stray `Enter` cancels:

![Copying asks where to](/img/screenshots/copy.png)

![Deleting asks first, with no selected](/img/screenshots/delete.png)

Work of any size runs in the background with a bar and `Alt+Esc` to stop it, and reports itself as a
[notification](state.md#notifications) that opens in full and turns into what came of it:

![A copy running in the background, with a bar and Alt+Esc to stop](/img/screenshots/progress.png)

![The same copy opened in full, with Stop offered](/img/screenshots/notification.png)

![The same entry once the copy is over](/img/screenshots/done.png)

Each tab holds two panels of its own, so a second pair of folders is a tab away rather than a place to
navigate back to. The band along the top says what each one is connected to and answers to the mouse —
a click shows a tab, its `×` closes it, the `+` at the end opens another. `Alt+T`, `Alt+W`, `Alt+PgDn`
and `Alt+PgUp` do the same from the keyboard, and `F2` lists them all. More tabs than the band can hold
shortens the names first and then scrolls, which is worth stealing: shortening alone ends in a row of
stubs that name nothing, and scrolling alone hides tabs that two fewer letters would have fitted.

A panel connects by a `Host` entry from `~/.ssh/config`, browses the server as it browses a disk, and
the same credentials run a command on it:

![Hosts read from ~/.ssh/config](/img/screenshots/hosts.png)

![A panel browsing a server over SFTP](/img/screenshots/server.png)

![A command run on that server](/img/screenshots/ssh.png)

It runs **without the output line**, so the bottom row belongs to the screen's own
[status bar](status-bar.md), and every screen renders headlessly — `--frame 132x26`, with `--keys` to
play keys first and `--connect` to open a panel on a server.

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

The frame goes to stdout as ANSI text; the view name is `default`, `about`, `picker`, `widgets`, or one
of `password`, `number`, `slider`, `toggle`, `multi`, `date`, `time`, `color` to render the matching
modal over the default view:

```bash
dotnet run --project samples/Arlecchino.Sample -- --frame widgets 100x24
```

## Rendering any of them as a frame

Every one of them takes `--frame` and writes a composed frame to stdout — the samples beside the
framework name a view, the commander names a size. That is the fastest way to look at a layout, and it
is three lines of wiring in an application of your own — see
[Rendering](rendering.md#rendering-without-a-terminal).

The command palette and the keys screen are the framework's own, and appear in every one of them:

![The command palette](/img/screenshots/palette.png)

![The keys screen](/img/screenshots/help.png)

## Built something?

If you have an application built on Arlecchino,
[an issue](https://github.com/The1fEst/Arlecchino/issues) is the place to say so.
