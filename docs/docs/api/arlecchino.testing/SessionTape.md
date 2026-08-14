---
title: "SessionTape"
sidebar_label: "SessionTape"
---

# SessionTape class

**Namespace:** `Arlecchino.Testing` &middot; **Assembly:** `Arlecchino.Testing`

A session written down: the events that go in, the waits between them, and where a frame is worth looking at. A tape is written by hand rather than recorded.

```csharp
var frames = new SessionTape()
.Type(":")
.Shot()
.Type("copy")
.Wait(200)
.Shot()
.Play(host);

Assert.Contains("Copy files", frames[^1], StringComparison.Ordinal);

```

```csharp
public sealed class SessionTape
```

## Constructors

| Member | Summary |
|---|---|
| [`SessionTape()`](#sessiontape) | Starts an empty tape, for one written by hand. |
| [`SessionTape(TimeProvider)`](#sessiontape-timeprovider) | Starts an empty tape that measures the gaps between events itself, for one captured from an application as it runs. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many steps are on the tape. |

## Methods

| Member | Summary |
|---|---|
| [`Click(int, int, MouseButton)`](#click-int-int-mousebutton) | Writes down a click. |
| [`Key(ConsoleKey, KeyModifiers)`](#key-consolekey-keymodifiers) | Writes down a key press as the terminal would report it. |
| [`Paste(string)`](#paste-string) | Writes down a paste. |
| [`Play(ArlecchinoTestHost)`](#play-arlecchinotesthost) | Plays the tape into a host, waiting what it waited and doing what it did, and hands back a frame for every mark on it. |
| [`Read(string)`](#read-string) | Reads a tape back from what [`SessionTape.ToString`](../arlecchino.testing/SessionTape.md#tostring) wrote. |
| [`RecordKey(KeyPress)`](#recordkey-keypress) | Writes down a key exactly as a terminal reports one — character and key together — for a test that drives the tape from events it built itself rather than from the members above. |
| [`RecordMouse(MouseEvent)`](#recordmouse-mouseevent) | Writes down a mouse event exactly as a terminal reports one. |
| [`Scroll(int, int, bool)`](#scroll-int-int-bool) | Writes down a turn of the wheel. |
| [`Shot()`](#shot) | Marks that a frame is worth looking at here. Playing the tape hands one back for every mark, so a tape says not only what happened but where to look. |
| [`ToString()`](#tostring) | The tape as text, one step to a line, ready to be written to a file. |
| [`Type(string)`](#type-string) | Writes down text typed one character at a time. |
| [`Wait(int)`](#wait-int) | Writes down a wait, which is what makes timeouts and work on a clock replayable. |

## Constructors in detail

### `SessionTape()` {#sessiontape}

```csharp
public SessionTape();
```

Starts an empty tape, for one written by hand.

### `SessionTape(TimeProvider)` {#sessiontape-timeprovider}

```csharp
public SessionTape(TimeProvider clock);
```

Starts an empty tape that measures the gaps between events itself, for one captured from an application as it runs.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `clock` | `TimeProvider` | Where the gaps are measured from. It is read as `GetUtcNow` rather than as a timestamp, because that is the face the application itself lives by — a tape measured off the high-frequency timer cannot be replayed against a clock a test moves by hand. |

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many steps are on the tape.

**Type** `int`

## Methods in detail

### `Click(int, int, MouseButton)` {#click-int-int-mousebutton}

```csharp
public SessionTape Click(int row, int column, MouseButton button = Left);
```

Writes down a click.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row, counted from the top of the terminal. |
| `column` | `int` | Column, counted from its left edge. |
| `button` | [`MouseButton`](../arlecchino.input/MouseButton.md) | Which button. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

### `Key(ConsoleKey, KeyModifiers)` {#key-consolekey-keymodifiers}

```csharp
public SessionTape Key(ConsoleKey key, KeyModifiers modifiers = None);
```

Writes down a key press as the terminal would report it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKey` | The key. |
| `modifiers` | [`KeyModifiers`](../arlecchino.input/KeyModifiers.md) | What was held with it. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

### `Paste(string)` {#paste-string}

```csharp
public SessionTape Paste(string text);
```

Writes down a paste.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was pasted. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

### `Play(ArlecchinoTestHost)` {#play-arlecchinotesthost}

```csharp
public List<string> Play(ArlecchinoTestHost host);
```

Plays the tape into a host, waiting what it waited and doing what it did, and hands back a frame for every mark on it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `host` | [`ArlecchinoTestHost`](../arlecchino.testing/ArlecchinoTestHost.md) | The application to play into. |

**Returns** `List<T>`&lt;`string`&gt; — One frame per mark, in order.

### `Read(string)` {#read-string}

```csharp
public static SessionTape Read(string text);
```

Reads a tape back from what [`SessionTape.ToString`](../arlecchino.testing/SessionTape.md#tostring) wrote.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The tape as text. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape.

### `RecordKey(KeyPress)` {#recordkey-keypress}

```csharp
public SessionTape RecordKey(KeyPress key);
```

Writes down a key exactly as a terminal reports one — character and key together — for a test that drives the tape from events it built itself rather than from the members above.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

### `RecordMouse(MouseEvent)` {#recordmouse-mouseevent}

```csharp
public SessionTape RecordMouse(MouseEvent mouse);
```

Writes down a mouse event exactly as a terminal reports one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

### `Scroll(int, int, bool)` {#scroll-int-int-bool}

```csharp
public SessionTape Scroll(int row, int column, bool down);
```

Writes down a turn of the wheel.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row the pointer was over. |
| `column` | `int` | Column the pointer was over. |
| `down` | `bool` | Whether it turned down. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

### `Shot()` {#shot}

```csharp
public SessionTape Shot();
```

Marks that a frame is worth looking at here. Playing the tape hands one back for every mark, so a tape says not only what happened but where to look.

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

The tape as text, one step to a line, ready to be written to a file.

**Returns** `string` — The tape.

### `Type(string)` {#type-string}

```csharp
public SessionTape Type(string text);
```

Writes down text typed one character at a time.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was typed. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

### `Wait(int)` {#wait-int}

```csharp
public SessionTape Wait(int milliseconds);
```

Writes down a wait, which is what makes timeouts and work on a clock replayable.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `milliseconds` | `int` | How long was waited. |

**Returns** [`SessionTape`](../arlecchino.testing/SessionTape.md) — The tape, so steps chain.

