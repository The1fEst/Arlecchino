---
title: TerminalInputReader
sidebar_label: TerminalInputReader
---

# TerminalInputReader class

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino`

Turns what the terminal reports into keys and mouse events. Terminals send arrows, function keys and mouse reports as escape sequences, so an escape has to be read together with what follows it. Anything that turns out not to be a sequence is replayed key by key, which is what makes a plain Escape work even though it starts the same way. The rest of a sequence does not always arrive with its escape — over ssh or a busy terminal it can land a few milliseconds later — so the reader waits a short while for it. That wait is also what a lone Escape costs, which is the trade every terminal editor makes.

```csharp
public sealed class TerminalInputReader
```

## Constructors

| Member | Summary |
|---|---|
| [`TerminalInputReader(IArlecchinoTerminal, InputRouter, ArlecchinoOptions)`](#terminalinputreader-iarlecchinoterminal-inputrouter-arlecchinooptions) | Creates the reader. Everything it reads is routed as it is read, which is what a caller driving the reader itself wants — inside the framework it is built with a queue instead, so that the thread reading the terminal never touches what the frame loop is drawing. |

## Methods

| Member | Summary |
|---|---|
| [`Read(ConsoleKeyInfo)`](#read-consolekeyinfo) | Handles one key press, reading further keys itself when it looks like the start of a sequence. |
| [`ReadPending()`](#readpending) | Reads everything waiting and returns, without blocking for more. Mouse events are drained too, since a terminal that reports them outside the key stream would otherwise pile them up. |

## Constructors in detail

### `TerminalInputReader(IArlecchinoTerminal, InputRouter, ArlecchinoOptions)` {#terminalinputreader-iarlecchinoterminal-inputrouter-arlecchinooptions}

```csharp
public TerminalInputReader(IArlecchinoTerminal terminal, InputRouter router, ArlecchinoOptions options);
```

Creates the reader. Everything it reads is routed as it is read, which is what a caller driving the reader itself wants — inside the framework it is built with a queue instead, so that the thread reading the terminal never touches what the frame loop is drawing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `terminal` | [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md) | Where key presses come from. |
| `router` | [`InputRouter`](../arlecchino/InputRouter.md) | Where the result is sent. |
| `options` | [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md) | Supplies how long to wait for the rest of a sequence. |

## Methods in detail

### `Read(ConsoleKeyInfo)` {#read-consolekeyinfo}

```csharp
public void Read(ConsoleKeyInfo key);
```

Handles one key press, reading further keys itself when it looks like the start of a sequence.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key that was read. |

### `ReadPending()` {#readpending}

```csharp
public void ReadPending();
```

Reads everything waiting and returns, without blocking for more. Mouse events are drained too, since a terminal that reports them outside the key stream would otherwise pile them up.

