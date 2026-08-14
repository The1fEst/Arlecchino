---
title: Arlecchino.Hosting
sidebar_label: Arlecchino.Hosting
sidebar_position: 0
---

# Arlecchino.Hosting

## Classes

| Type | Summary |
|---|---|
| [`ArlecchinoBuilder`](ArlecchinoBuilder.md) | Configures an application while its services are being registered. Every method returns the builder, so a whole application is described in one chain at startup. |
| [`ArlecchinoKeymap`](ArlecchinoKeymap.md) | Every key the framework itself reacts to, in one place. Replace the whole map through `UseKeymap`, or one binding at a time with `with`; each binding also relabels itself in the hints box and the palette. |
| [`ArlecchinoOptions`](ArlecchinoOptions.md) | Everything the framework can be told about an application. Configure it in the `AddArlecchino` callback; most of it also has a builder call. |
| [`ArlecchinoServiceCollectionExtensions`](ArlecchinoServiceCollectionExtensions.md) | Registers Arlecchino with the host's container. |
| [`ArlecchinoStrings`](ArlecchinoStrings.md) | Every piece of text the framework itself draws, as delegates with English defaults. They are called on the frames that need them, so pointing them elsewhere switches language with nothing to rebuild. |
| [`FilePickerStrings`](FilePickerStrings.md) | Text of the file picker: labels, column headers, and the three formatters. |
| [`Handover`](Handover.md) | Lends the terminal to a full-screen program of its own — an editor, a pager, a shell — and takes it back afterward. It runs on the drawing thread and blocks it, so no frame lands on top of the other program. |
| [`Ticker`](Ticker.md) | Work on a clock, run between frames on the thread that draws, with a repaint asked for afterward. Missed time is not made up for: an action runs at most once per pass. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoStartup`](IArlecchinoStartup.md) | Work to do once before the first frame. Several may be registered; they run in registration order, and the last route that is not [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) decides where the application opens. |

## Enums

| Type | Summary |
|---|---|
| [`HintsShown`](HintsShown.md) | When the framework draws its own box of keys in the corner. |

