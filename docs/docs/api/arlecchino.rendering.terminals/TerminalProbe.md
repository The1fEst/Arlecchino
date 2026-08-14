---
title: "TerminalProbe"
sidebar_label: "TerminalProbe"
---

# TerminalProbe class

**Namespace:** `Arlecchino.Rendering.Terminals` &middot; **Assembly:** `Arlecchino.Core`

Asks the terminal what it can do, once, before the application starts reading keys. The questions end with the one every terminal answers, so its reply is the signal that no other reply is coming.

```csharp
public static class TerminalProbe
```

## Methods

| Member | Summary |
|---|---|
| [`Ask(IArlecchinoTerminal, TimeSpan)`](#ask-iarlecchinoterminal-timespan) | Asks, waits no longer than it is told, and installs what came back into [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md) and [`Glyphs`](../arlecchino.rendering.text/Glyphs.md), leaving unanswered questions alone. Call it before the mouse and paste modes go on. |

## Methods in detail

### `Ask(IArlecchinoTerminal, TimeSpan)` {#ask-iarlecchinoterminal-timespan}

```csharp
public static bool Ask(IArlecchinoTerminal terminal, TimeSpan within);
```

Asks, waits no longer than it is told, and installs what came back into [`TerminalCapabilities`](../arlecchino.rendering.terminals/TerminalCapabilities.md) and [`Glyphs`](../arlecchino.rendering.text/Glyphs.md), leaving unanswered questions alone. Call it before the mouse and paste modes go on.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `terminal` | [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md) | The terminal to ask. |
| `within` | `TimeSpan` | How long to wait for the last answer before giving up. |

**Returns** `bool` — `true` when the terminal said anything at all. What it said is in [`TerminalCapabilities.Sixel`](../arlecchino.rendering.terminals/TerminalCapabilities.md#sixel), [`TerminalCapabilities.Kitty`](../arlecchino.rendering.terminals/TerminalCapabilities.md#kitty) and [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth); a `false` here is how you tell a terminal that cannot draw pictures from one that never replied.

