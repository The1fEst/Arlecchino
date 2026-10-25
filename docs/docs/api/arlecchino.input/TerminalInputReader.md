---
title: "TerminalInputReader"
sidebar_label: "TerminalInputReader"
---

# TerminalInputReader class

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino`

Turns what the terminal reports into keys and mouse events, reading an escape together with what follows it. Anything that turns out not to be a sequence is replayed key by key, after a short wait.

```csharp
public sealed class TerminalInputReader
```

## Constructors

| Member | Summary |
|---|---|
| [`TerminalInputReader(IArlecchinoTerminal, InputRouter, ArlecchinoOptions)`](#terminalinputreader-iarlecchinoterminal-inputrouter-arlecchinooptions) | Creates the reader, routing everything as it is read. Inside the framework it is built with a queue instead, so the reading thread never touches what the frame loop draws. |

## Methods

| Member | Summary |
|---|---|
| [`Read(KeyPress)`](#read-keypress) | Handles one key press, reading further keys itself where it looks like the start of a sequence. An escape followed by another escape is `Alt+Escape`, which the runtime does not fold back together. |
| [`ReadPending()`](#readpending) | Reads everything waiting and returns, without blocking for more. Mouse events are drained too, since a terminal that reports them outside the key stream would otherwise pile them up. |

## Constructors in detail

### `TerminalInputReader(IArlecchinoTerminal, InputRouter, ArlecchinoOptions)` {#terminalinputreader-iarlecchinoterminal-inputrouter-arlecchinooptions}

```csharp
public TerminalInputReader(
    IArlecchinoTerminal terminal,
    InputRouter router,
    ArlecchinoOptions options);
```

Creates the reader, routing everything as it is read. Inside the framework it is built with a queue instead, so the reading thread never touches what the frame loop draws.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `terminal` | [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md) | Where key presses come from. |
| `router` | [`InputRouter`](../arlecchino/InputRouter.md) | Where the result is sent. |
| `options` | [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md) | Supplies how long to wait for the rest of a sequence. |

## Methods in detail

### `Read(KeyPress)` {#read-keypress}

```csharp
public void Read(KeyPress key);
```

Handles one key press, reading further keys itself where it looks like the start of a sequence. An escape followed by another escape is `Alt+Escape`, which the runtime does not fold back together.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was read. |

### `ReadPending()` {#readpending}

```csharp
public void ReadPending();
```

Reads everything waiting and returns, without blocking for more. Mouse events are drained too, since a terminal that reports them outside the key stream would otherwise pile them up.

