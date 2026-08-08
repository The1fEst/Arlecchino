---
title: Arlecchino.Rendering.Terminals
sidebar_label: Arlecchino.Rendering.Terminals
sidebar_position: 0
---

# Arlecchino.Rendering.Terminals

## Classes

| Type | Summary |
|---|---|
| [`TerminalCapabilities`](TerminalCapabilities.md) | What the terminal can actually show. Detected once at startup and consulted by every style when it builds its escape sequence; assign [`TerminalCapabilities.Color`](../arlecchino.rendering.terminals/TerminalCapabilities.md#color) to override the guess. |
| [`TerminalProbe`](TerminalProbe.md) | Asks the terminal what it can do, once, before the application starts reading keys. Everything here rests on one arrangement: the questions go out in an order that ends with the one every terminal answers — primary device attributes — so the reply to it is the signal that no other reply is coming. Without that fence there is nothing to wait for but a guess at how long a terminal takes to stay silent. The fence stops the waiting, not the reading: once it has arrived, whatever is already buffered is taken too, and only a lull or a keystroke ends it. A terminal that answers out of the order it was asked would otherwise have its last answer cut off, and nothing in any specification says it must answer in order. A terminal that answers nothing costs the deadline and leaves every setting as it was, which is the behavior an application already had to live with. Whatever was read is then handed straight back, because without the fence there is no telling an answer from a keystroke. With the fence, there is: answers are escape sequences, so what was read outside every sequence was typed while the terminal was being asked, and `TerminalReply` picks it out to be handed back. Judging the whole read by its shape does not work. On Windows the console layer eats the kitty query's reply and leaves the last character of it behind, so the first thing a terminal says can be a lone backslash. Treating that as something a person typed threw away every answer behind it. |

## Enums

| Type | Summary |
|---|---|
| [`ImageProtocol`](ImageProtocol.md) | How a picture reaches the terminal. Like [`GraphSymbols`](../arlecchino.rendering.text/GraphSymbols.md), this is a question of what the terminal can do rather than of taste. |

