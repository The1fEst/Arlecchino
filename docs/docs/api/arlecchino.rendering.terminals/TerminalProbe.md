---
title: "TerminalProbe"
sidebar_label: "TerminalProbe"
---

# TerminalProbe class

**Namespace:** `Arlecchino.Rendering.Terminals` &middot; **Assembly:** `Arlecchino.Core`

Asks the terminal what it can do, once, before the application starts reading keys. Everything here rests on one arrangement: the questions go out in an order that ends with the one every terminal answers — primary device attributes — so the reply to it is the signal that no other reply is coming. Without that fence there is nothing to wait for but a guess at how long a terminal takes to stay silent. The fence stops the waiting, not the reading: once it has arrived, whatever is already buffered is taken too, and only a lull or a keystroke ends it. A terminal that answers out of the order it was asked would otherwise have its last answer cut off, and nothing in any specification says it must answer in order. A terminal that answers nothing costs the deadline and leaves every setting as it was, which is the behavior an application already had to live with. Whatever was read is then handed straight back, because without the fence there is no telling an answer from a keystroke. With the fence, there is: answers are escape sequences, so what was read outside every sequence was typed while the terminal was being asked, and `TerminalReply` picks it out to be handed back. Judging the whole read by its shape does not work. On Windows the console layer eats the kitty query's reply and leaves the last character of it behind, so the first thing a terminal says can be a lone backslash. Treating that as something a person typed threw away every answer behind it.

```csharp
public static class TerminalProbe
```

## Methods

| Member | Summary |
|---|---|
| [`Ask(IArlecchinoTerminal, TimeSpan)`](#ask-iarlecchinoterminal-timespan) | Asks, waits no longer than it is told, and installs what came back into [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md) and [`Glyphs`](../arlecchino.rendering.text/Glyphs.md). Anything the terminal did not answer is left alone. Call it before the mouse and paste modes go on, so their reports cannot arrive among the answers. |

## Methods in detail

### `Ask(IArlecchinoTerminal, TimeSpan)` {#ask-iarlecchinoterminal-timespan}

```csharp
public static bool Ask(IArlecchinoTerminal terminal, TimeSpan within);
```

Asks, waits no longer than it is told, and installs what came back into [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md) and [`Glyphs`](../arlecchino.rendering.text/Glyphs.md). Anything the terminal did not answer is left alone. Call it before the mouse and paste modes go on, so their reports cannot arrive among the answers.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `terminal` | [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md) | The terminal to ask. |
| `within` | `TimeSpan` | How long to wait for the last answer before giving up. |

**Returns** `bool` — `true` when the terminal said anything at all. What it said is in [`TerminalCapabilities.Sixel`](../arlecchino.rendering.terminals/TerminalCapabilities.md#sixel), [`TerminalCapabilities.Kitty`](../arlecchino.rendering.terminals/TerminalCapabilities.md#kitty) and [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth); a `false` here is how you tell a terminal that cannot draw pictures from one that never replied.

