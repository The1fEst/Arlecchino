---
title: Arlecchino.Input
sidebar_label: Arlecchino.Input
sidebar_position: 0
---

# Arlecchino.Input

## Classes

| Type | Summary |
|---|---|
| [`KeyText`](KeyText.md) | Turns a key press into the character it should type. Take it as a constructor parameter instead of reading `KeyPress.Character` yourself — that is what keeps filters and shortcuts working on a non-latin layout. |
| [`TerminalInputReader`](TerminalInputReader.md) | Turns what the terminal reports into keys and mouse events, reading an escape together with what follows it. Anything that turns out not to be a sequence is replayed key by key, after a short wait. |

## Structs

| Type | Summary |
|---|---|
| [`KeyBinding`](KeyBinding.md) | A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. It is added to with [`KeyBinding.AddAlternative`](../arlecchino.input/KeyBinding.md#addalternative-consolekey-keymodifiers) and turned into a chord with [`KeyBinding.ThenKey`](../arlecchino.input/KeyBinding.md#thenkey-consolekey-keymodifiers). |
| [`KeyPress`](KeyPress.md) | One key press, as the framework hands it to a view. It is `ConsoleKeyInfo` with room for Command, which that type has nowhere to put. |
| [`KeyStroke`](KeyStroke.md) | One key and the modifiers held with it, which is the smallest thing a binding can be made of. A [`KeyBinding`](../arlecchino.input/KeyBinding.md) is one of these plus its alternatives and its finishing key. |
| [`MouseEvent`](MouseEvent.md) | A mouse report from the terminal. Coordinates are frame cells — the same ones [`Surface.WriteAt`](../arlecchino.rendering/Surface.md#writeat-int-int-string-iarlecchinocolor) and [`SurfaceRegion.Contains`](../arlecchino.rendering/SurfaceRegion.md#contains-int-int) use, so hit-testing is comparing numbers. |

## Enums

| Type | Summary |
|---|---|
| [`KeyModifiers`](KeyModifiers.md) | Modifiers held with a key. The three the console knows about keep the values `ConsoleModifiers` gives them, so the two agree bit by bit; [`KeyModifiers.Super`](../arlecchino.input/KeyModifiers.md) is the one the console has no room for — Command on a Mac, the Windows key elsewhere. |
| [`MouseAction`](MouseAction.md) | What the mouse did. |
| [`MouseButton`](MouseButton.md) | Which button an event belongs to. |
| [`TextInputMode`](TextInputMode.md) | How a key press becomes a character, which decides what happens on a non-latin layout. |

