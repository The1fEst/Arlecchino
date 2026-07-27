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
| [`ArlecchinoStrings`](ArlecchinoStrings.md) | Every piece of text the framework itself draws. All of it is delegates with English defaults, so an application points them at its own resolver and switches language process-wide without the framework knowing that languages exist. They are called on the frames that need them, so nothing has to be rebuilt when the language changes. |
| [`FilePickerStrings`](FilePickerStrings.md) | Text of the file picker: labels, column headers, and the three formatters. |
| [`Ticker`](Ticker.md) | Work on a clock, run on the frame loop. A terminal application redraws when something asks it to, so anything that changes on its own — a spinner, a clock, a list that refreshes itself, a message that fades — needs someone to say when. That someone is this: schedule an action and it runs between frames, on the same thread as drawing and input, with a repaint asked for afterwards. Every schedule returns the handle that cancels it. Hand it to [`ViewLifetime.Track`](../arlecchino.navigation/ViewLifetime.md#track-t-t) and the work stops when the screen goes away. Missed time is not made up for: an action runs at most once per pass, so a loop that was held up — a window that came back from being minimised, a long operation, a debugger — resumes with a single run rather than firing everything it slept through. |

## Interfaces

| Type | Summary |
|---|---|
| [`IArlecchinoStartup`](IArlecchinoStartup.md) | Work to do once before the first frame. Several may be registered; they run in registration order, and the last route that is not [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) decides where the application opens. |

