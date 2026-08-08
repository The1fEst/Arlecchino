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
| [`TerminalInputReader`](TerminalInputReader.md) | Turns what the terminal reports into keys and mouse events. Terminals send arrows, function keys and mouse reports as escape sequences, so an escape has to be read together with what follows it. Anything that turns out not to be a sequence is replayed key by key, which is what makes a plain Escape work even though it starts the same way. The rest of a sequence does not always arrive with its escape — over ssh or a busy terminal it can land a few milliseconds later — so the reader waits a short while for it. That wait is also what a lone Escape costs, which is the trade every terminal editor makes. |

## Structs

| Type | Summary |
|---|---|
| [`KeyBinding`](KeyBinding.md) | A key plus the exact modifiers that must be held with it, so `Ctrl+S` never fires on a bare `S`. Every key the framework reacts to is one of these, which is what makes them rebindable. A binding starts as the one combination it is named after and is added to from there: [`KeyBinding.AddAlternative`](../arlecchino.input/KeyBinding.md#addalternative-consolekey-keymodifiers) for the combinations a platform disagrees about, and [`KeyBinding.ThenKey`](../arlecchino.input/KeyBinding.md#thenkey-consolekey-keymodifiers) for a second keystroke, which turns the binding into a chord. |
| [`KeyPress`](KeyPress.md) | One key press, as the framework hands it to a view. It is `ConsoleKeyInfo` with room for one more modifier. That type stores Shift, Alt and Control as three booleans and has nowhere to put Command, so a terminal reporting Command had to be either misread as an unmodified key or dropped. Everything a view is given goes through here instead. |
| [`KeyStroke`](KeyStroke.md) | One key and the modifiers held with it — the smallest thing a binding can be made of. A [`KeyBinding`](../arlecchino.input/KeyBinding.md) is one of these, plus the alternatives that mean the same, plus the key that finishes it when the binding is a chord. |
| [`MouseEvent`](MouseEvent.md) | A mouse report from the terminal. Coordinates are frame cells — the same ones [`Surface.WriteAt`](../arlecchino.rendering/Surface.md#writeat-int-int-string-iarlecchinocolor) and [`SurfaceRegion.Contains`](../arlecchino.rendering/SurfaceRegion.md#contains-int-int) use, so hit-testing is comparing numbers. |

## Enums

| Type | Summary |
|---|---|
| [`KeyModifiers`](KeyModifiers.md) | Modifiers held with a key. The three the console knows about keep the values `ConsoleModifiers` gives them, so the two agree bit by bit; [`KeyModifiers.Super`](../arlecchino.input/KeyModifiers.md) is the one the console has no room for — Command on a Mac, the Windows key elsewhere. |
| [`MouseAction`](MouseAction.md) | What the mouse did. |
| [`MouseButton`](MouseButton.md) | Which button an event belongs to. |
| [`TextInputMode`](TextInputMode.md) | How a key press becomes a character, which decides what happens on a non-latin layout. |

