---
title: Lexicon
sidebar_label: Lexicon
description: Every term these pages use, in one place — and the words this framework deliberately does not have.
---

# Lexicon

The words used throughout the documentation, with the page that explains each one.

## Drawing

| Term | Means |
|---|---|
| **Cell** | One position in the grid. Holds a whole grapheme cluster and one style — not a `char`. See [Text and width](text.md) |
| **Frame** | One complete picture of the screen, composed in memory and written in one call. See [Rendering](rendering.md) |
| **Surface** | The cell grid a frame is composed in. See [Rendering](rendering.md) |
| **Region** | A rectangle on the surface with its own coordinates and its own clipping. See [Layout](layout.md) |
| **Flow cursor** | The row the next `AppendLine` writes to. See [Layout](layout.md#flow-layout) |
| **Clip** | A scope confining every write to a rectangle, whatever coordinates the writing code uses. See [Layout](layout.md#clipping-a-whole-stretch-of-drawing) |
| **Column** | The unit width is measured in. A wide symbol is two. See [Text and width](text.md) |
| **Role** | A named entry in the palette — `Header`, `Muted`, `Error`. Views pick roles, not colours. See [Theming](theming.md) |
| **Palette** | The object behind the roles. See [Theming](theming.md) |

## The application

| Term | Means |
|---|---|
| **View** | A screen. A plain class implementing `IArlecchinoView`. See [Views and navigation](views-and-navigation.md) |
| **Route** | The name of a screen — a `ViewRoute`, which is a string wearing a type. See [Views and navigation](views-and-navigation.md) |
| **`ViewKind`** | The generated class holding one route per view, so routes read like an enum. See [Source generator](source-generator.md) |
| **Navigator** | What shows a route and keeps the history. See [Views and navigation](views-and-navigation.md) |
| **View scope** | The DI scope one screen lives in. See [Views and navigation](views-and-navigation.md) |
| **Frame loop** | The thread that drains input, runs the ticker and draws. See [The frame loop](frame-loop.md) |
| **Drawing thread** | The one thread allowed to touch a view, a widget, an atom or the surface. See [The frame loop](frame-loop.md#which-thread-draws) |
| **Repaint request** | The "this frame is stale" flag the loop waits on. See [The frame loop](frame-loop.md#frames-are-drawn-on-request) |
| **Ticker** | Scheduled work, run between frames on the drawing thread. See [The frame loop](frame-loop.md#work-on-a-clock) |

## Input

| Term | Means |
|---|---|
| **Binding** | A key plus its exact modifiers, and optionally a second combination. See [Keyboard](keyboard.md#keybinding) |
| **Keymap** | Every key the framework itself reacts to, in one object. See [Keyboard](keyboard.md#the-keymap) |
| **Command** | A key, a label and something to run — visible to the palette, the hints box and the keys screen. See [Commands](commands.md) |
| **View command** | The same, belonging to one screen. See [Commands](commands.md#commands-of-a-view) |
| **Palette** (the other one) | The modal listing commands, opened with `:`. See [Commands](commands.md#the-command-palette) |
| **Hints box** | The box in the bottom-right corner listing a screen's keys. See [Views and navigation](views-and-navigation.md) |
| **Keys screen** | `F1` — everything the application answers to. See [Keyboard](keyboard.md#the-keys-screen) |
| **Focus ring** | The cycle of focusable elements inside one view. See [Focus](focus.md) |

## State

| Term | Means |
|---|---|
| **Atom** | One observable cell of state that asks for a repaint when it changes. See [Atoms](atoms.md) |
| **Tracked / local** | Whether an atom's edits enter the undo history. See [Atoms](atoms.md) |
| **Computed** | A derived value that tracks whatever it read. See [Atoms](atoms.md#derived-values) |
| **Store** | A class of atoms that registers itself. See [Stores](stores.md) |
| **Async atom** | A load in progress, with its status as an atom. See [Async atoms](async-atoms.md) |
| **Lifetime** | The scoped object that cancels a screen's work when it goes away. See [Async atoms](async-atoms.md#tying-work-to-the-screen) |
| **Application state** | `ArlecchinoState` — the output line, the modal stack, the picker request. See [Application state](state.md) |
| **Output line** | The last row of the frame, and the newest notification. See [Application state](state.md#the-output-line) |
| **Notification** | Something the application said, kept after the row has cleared. See [Diagnostics](diagnostics.md#notifications) |

## Pieces

| Term | Means |
|---|---|
| **Widget** | Something that draws into a region and hands back what is left. See [Widgets](widgets.md) |
| **Interactive widget** | A widget that also takes the focus. See [Widgets](widgets.md) |
| **Modal** | A dialog that takes every key while it is open. See [Modals](modals.md) |
| **Modal stack** | Several open at once, each drawn offset from the one below. See [Modals](modals.md#stacking) |
| **Form** | Atoms rendered as editable rows, each opening the modal that matches its type. See [Forms](forms.md) |
| **Field** | One row of a form. See [Forms](forms.md#the-fields) |

## Words this framework does not have

Worth saying plainly, because they are the first thing a reader coming from another toolkit looks for:

| Not here | Instead |
|---|---|
| **Component tree** | A view draws. There is no tree of nested objects and no reconciliation |
| **Layout engine** | A view places things itself, with [flow calls, absolute calls or regions](layout.md) |
| **Container widget** | Widgets do not nest — a composite lays its parts out and routes to them by hand |
| **Data binding** | [Atoms](atoms.md) notify, and a frame reads them fresh |
| **Style sheet** | A [palette](theming.md) of roles |
| **Window** | One frame, one screen. A [modal](modals.md) is drawn over it, not beside it |
| **Event bubbling** | A key is resolved in [one documented order](keyboard.md#how-a-key-travels) |
