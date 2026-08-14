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
| [`TerminalProbe`](TerminalProbe.md) | Asks the terminal what it can do, once, before the application starts reading keys. The questions end with the one every terminal answers, so its reply is the signal that no other reply is coming. |

## Enums

| Type | Summary |
|---|---|
| [`ImageProtocol`](ImageProtocol.md) | How a picture reaches the terminal. Like [`GraphSymbols`](../arlecchino.rendering.text/GraphSymbols.md), this is a question of what the terminal can do rather than of taste. |

