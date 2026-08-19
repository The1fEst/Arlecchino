---
title: "FakeTerminal"
sidebar_label: "FakeTerminal"
---

# FakeTerminal class

**Namespace:** `Arlecchino.Testing` &middot; **Assembly:** `Arlecchino.Testing`

A terminal that keeps everything in memory: keys queued in, output collected as text, and the size a test sets. The input queues are concurrent, so keys can be delivered late as a real terminal delivers them.

```csharp
public sealed class FakeTerminal : IArlecchinoTerminal, IChecksFrames
```

**Implements** [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md), `IChecksFrames`

## Constructors

| Member | Summary |
|---|---|
| [`FakeTerminal(int, int)`](#faketerminal-int-int) | Creates the terminal at a fixed size. |

## Properties

| Member | Summary |
|---|---|
| [`AreControlKeysTaken`](#arecontrolkeystaken) | Whether the application borrowed Ctrl+C and has not handed it back. |
| [`CopiedText`](#copiedtext) | The last text copied, or `null` when nothing has been. |
| [`Height`](#height) | Rows. Assigning simulates a resize. |
| [`IsFullScreen`](#isfullscreen) | Whether the application took over the screen and has not given it back. |
| [`IsMouseEnabled`](#ismouseenabled) | Whether the application asked for the mouse. |
| [`IsPasteEnabled`](#ispasteenabled) | Whether the application asked for bracketed paste. |
| [`KeyAvailable`](#keyavailable) | Whether any queued key is still waiting. |
| [`MouseAvailable`](#mouseavailable) | Whether any queued mouse event is still waiting. |
| [`Screen`](#screen) | What the screen holds, rather than the cursor jumps and short runs [`FakeTerminal.WrittenText`](../arlecchino.testing/FakeTerminal.md#writtentext) collected to get it there. It survives [`FakeTerminal.Clear`](../arlecchino.testing/FakeTerminal.md#clear), as a real screen does. |
| [`Width`](#width) | Columns. Assigning simulates a resize. |
| [`WrittenText`](#writtentext) | Everything written so far, escape sequences included. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Throws away what has been written, so the next assertion sees one frame rather than all of them. |
| [`CopyToClipboard(string)`](#copytoclipboard-string) | Keeps what was copied instead of reaching a real clipboard. |
| [`DisableMouse()`](#disablemouse) | Records that the mouse was released. |
| [`DisablePaste()`](#disablepaste) | Records that bracketed paste was turned off. |
| [`EnableMouse()`](#enablemouse) | Records that the mouse was asked for. |
| [`EnablePaste()`](#enablepaste) | Records that bracketed paste was asked for. |
| [`Enqueue(KeyPress)`](#enqueue-keypress) | Queues a key press to be read. |
| [`EnqueueMouse(MouseEvent)`](#enqueuemouse-mouseevent) | Queues a mouse event to be read, the way a console that reports the mouse outside the key stream delivers one. |
| [`EnqueueText(string)`](#enqueuetext-string) | Queues text one character at a time, as a terminal reports it, naming the key wherever `ReadKey` names one. An escape and the letter after it stay two presses. |
| [`EnterFullScreen()`](#enterfullscreen) | Records that the screen was taken over. |
| [`GiveBackControlKeys()`](#givebackcontrolkeys) | Records that Ctrl+C was handed back to the terminal. |
| [`LeaveFullScreen()`](#leavefullscreen) | Records that the screen was given back, which is what a test checks after a crash. |
| [`ReadKey()`](#readkey) | Takes the next queued key, or nothing when the queue has run dry. |
| [`ReadMouse()`](#readmouse) | Takes the next queued mouse event, or nothing when the queue has run dry. |
| [`TakeControlKeys()`](#takecontrolkeys) | Records that Ctrl+C was borrowed, so it arrives as a key rather than as a signal. |
| [`Unread(KeyPress)`](#unread-keypress) | Puts a key back, so the next read returns it. |
| [`Write(string)`](#write-string) | Collects output instead of showing it, and applies it to [`FakeTerminal.Screen`](../arlecchino.testing/FakeTerminal.md#screen). |

## Constructors in detail

### `FakeTerminal(int, int)` {#faketerminal-int-int}

```csharp
public FakeTerminal(int width, int height);
```

Creates the terminal at a fixed size.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `width` | `int` | Columns. |
| `height` | `int` | Rows. |

## Properties in detail

### `AreControlKeysTaken` {#arecontrolkeystaken}

```csharp
public bool AreControlKeysTaken { get; }
```

Whether the application borrowed Ctrl+C and has not handed it back.

**Type** `bool`

### `CopiedText` {#copiedtext}

```csharp
public string? CopiedText { get; }
```

The last text copied, or `null` when nothing has been.

**Type** `string`

### `Height` {#height}

```csharp
public int Height { get; set; }
```

Rows. Assigning simulates a resize.

**Type** `int`

### `IsFullScreen` {#isfullscreen}

```csharp
public bool IsFullScreen { get; }
```

Whether the application took over the screen and has not given it back.

**Type** `bool`

### `IsMouseEnabled` {#ismouseenabled}

```csharp
public bool IsMouseEnabled { get; }
```

Whether the application asked for the mouse.

**Type** `bool`

### `IsPasteEnabled` {#ispasteenabled}

```csharp
public bool IsPasteEnabled { get; }
```

Whether the application asked for bracketed paste.

**Type** `bool`

### `KeyAvailable` {#keyavailable}

```csharp
public bool KeyAvailable { get; }
```

Whether any queued key is still waiting.

**Type** `bool`

### `MouseAvailable` {#mouseavailable}

```csharp
public bool MouseAvailable { get; }
```

Whether any queued mouse event is still waiting.

**Type** `bool`

### `Screen` {#screen}

```csharp
public ScreenGrid Screen { get; }
```

What the screen holds, rather than the cursor jumps and short runs [`FakeTerminal.WrittenText`](../arlecchino.testing/FakeTerminal.md#writtentext) collected to get it there. It survives [`FakeTerminal.Clear`](../arlecchino.testing/FakeTerminal.md#clear), as a real screen does.

**Type** [`ScreenGrid`](../arlecchino.testing/ScreenGrid.md)

### `Width` {#width}

```csharp
public int Width { get; set; }
```

Columns. Assigning simulates a resize.

**Type** `int`

### `WrittenText` {#writtentext}

```csharp
public string WrittenText { get; }
```

Everything written so far, escape sequences included.

**Type** `string`

## Methods in detail

### `Clear()` {#clear}

```csharp
public void Clear();
```

Throws away what has been written, so the next assertion sees one frame rather than all of them.

### `CopyToClipboard(string)` {#copytoclipboard-string}

```csharp
public void CopyToClipboard(string text);
```

Keeps what was copied instead of reaching a real clipboard.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was copied. |

### `DisableMouse()` {#disablemouse}

```csharp
public void DisableMouse();
```

Records that the mouse was released.

### `DisablePaste()` {#disablepaste}

```csharp
public void DisablePaste();
```

Records that bracketed paste was turned off.

### `EnableMouse()` {#enablemouse}

```csharp
public void EnableMouse();
```

Records that the mouse was asked for.

### `EnablePaste()` {#enablepaste}

```csharp
public void EnablePaste();
```

Records that bracketed paste was asked for.

### `Enqueue(KeyPress)` {#enqueue-keypress}

```csharp
public void Enqueue(KeyPress key);
```

Queues a key press to be read.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key press. |

### `EnqueueMouse(MouseEvent)` {#enqueuemouse-mouseevent}

```csharp
public void EnqueueMouse(MouseEvent mouse);
```

Queues a mouse event to be read, the way a console that reports the mouse outside the key stream delivers one.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event. |

### `EnqueueText(string)` {#enqueuetext-string}

```csharp
public void EnqueueText(string text);
```

Queues text one character at a time, as a terminal reports it, naming the key wherever `ReadKey` names one. An escape and the letter after it stay two presses.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The characters to queue. |

### `EnterFullScreen()` {#enterfullscreen}

```csharp
public void EnterFullScreen();
```

Records that the screen was taken over.

### `GiveBackControlKeys()` {#givebackcontrolkeys}

```csharp
public void GiveBackControlKeys();
```

Records that Ctrl+C was handed back to the terminal.

### `LeaveFullScreen()` {#leavefullscreen}

```csharp
public void LeaveFullScreen();
```

Records that the screen was given back, which is what a test checks after a crash.

### `ReadKey()` {#readkey}

```csharp
public KeyPress ReadKey();
```

Takes the next queued key, or nothing when the queue has run dry.

**Returns** [`KeyPress`](../arlecchino.input/KeyPress.md) — The key press.

### `ReadMouse()` {#readmouse}

```csharp
public MouseEvent ReadMouse();
```

Takes the next queued mouse event, or nothing when the queue has run dry.

**Returns** [`MouseEvent`](../arlecchino.input/MouseEvent.md) — The event.

### `TakeControlKeys()` {#takecontrolkeys}

```csharp
public void TakeControlKeys();
```

Records that Ctrl+C was borrowed, so it arrives as a key rather than as a signal.

### `Unread(KeyPress)` {#unread-keypress}

```csharp
public void Unread(KeyPress key);
```

Puts a key back, so the next read returns it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key to put back. |

### `Write(string)` {#write-string}

```csharp
public void Write(string text);
```

Collects output instead of showing it, and applies it to [`FakeTerminal.Screen`](../arlecchino.testing/FakeTerminal.md#screen).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was written. |

