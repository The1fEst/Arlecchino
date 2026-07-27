---
title: FakeTerminal
sidebar_label: FakeTerminal
---

# FakeTerminal class

**Namespace:** `Arlecchino.Testing` &middot; **Assembly:** `Arlecchino.Testing`

A terminal that keeps everything in memory: keys are queued in, output is collected as text, and the size is whatever a test sets it to. Nothing is written anywhere, so tests can run side by side and assert on what would have been drawn. The input queues are concurrent, so a test can deliver keys late — the way a real terminal splits an escape sequence across two reads.

```csharp
public sealed class FakeTerminal : IArlecchinoTerminal
```

**Implements** [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md)

## Constructors

| Member | Summary |
|---|---|
| [`FakeTerminal(int, int)`](#faketerminal-int-int) | Creates the terminal at a fixed size. |

## Properties

| Member | Summary |
|---|---|
| [`Copied`](#copied) | The last text copied, or `null` when nothing has been. |
| [`Height`](#height) | Rows. Assigning simulates a resize. |
| [`IsFullScreen`](#isfullscreen) | Whether the application took over the screen and has not given it back. |
| [`IsMouseEnabled`](#ismouseenabled) | Whether the application asked for the mouse. |
| [`IsPasteEnabled`](#ispasteenabled) | Whether the application asked for bracketed paste. |
| [`KeyAvailable`](#keyavailable) | Whether any queued key is still waiting. |
| [`MouseAvailable`](#mouseavailable) | Whether any queued mouse event is still waiting. |
| [`Width`](#width) | Columns. Assigning simulates a resize. |
| [`Written`](#written) | Everything written so far, escape sequences included. |

## Methods

| Member | Summary |
|---|---|
| [`Clear()`](#clear) | Throws away what has been written, so the next assertion sees one frame rather than all of them. |
| [`CopyToClipboard(string)`](#copytoclipboard-string) | Keeps what was copied instead of reaching a real clipboard. |
| [`DisableMouse()`](#disablemouse) | Records that the mouse was released. |
| [`DisablePaste()`](#disablepaste) | Records that bracketed paste was turned off. |
| [`EnableMouse()`](#enablemouse) | Records that the mouse was asked for. |
| [`EnablePaste()`](#enablepaste) | Records that bracketed paste was asked for. |
| [`Enqueue(ConsoleKeyInfo)`](#enqueue-consolekeyinfo) | Queues a key press to be read. |
| [`EnqueueMouse(MouseEvent)`](#enqueuemouse-mouseevent) | Queues a mouse event to be read, the way a console that reports the mouse outside the key stream delivers one. |
| [`EnqueueText(string)`](#enqueuetext-string) | Queues text one character at a time, as a terminal reports it. Escapes are marked as such, so whole escape sequences can be fed in as a plain string. |
| [`EnterFullScreen()`](#enterfullscreen) | Records that the screen was taken over. |
| [`LeaveFullScreen()`](#leavefullscreen) | Records that the screen was given back, which is what a test checks after a crash. |
| [`ReadKey()`](#readkey) | Takes the next queued key, or nothing when the queue has run dry. |
| [`ReadMouse()`](#readmouse) | Takes the next queued mouse event, or nothing when the queue has run dry. |
| [`Write(string)`](#write-string) | Collects output instead of showing it. |

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

### `Copied` {#copied}

```csharp
public string? Copied { get; }
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

### `Width` {#width}

```csharp
public int Width { get; set; }
```

Columns. Assigning simulates a resize.

**Type** `int`

### `Written` {#written}

```csharp
public string Written { get; }
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

### `Enqueue(ConsoleKeyInfo)` {#enqueue-consolekeyinfo}

```csharp
public void Enqueue(ConsoleKeyInfo key);
```

Queues a key press to be read.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key press. |

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

Queues text one character at a time, as a terminal reports it. Escapes are marked as such, so whole escape sequences can be fed in as a plain string.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The characters to queue. |

### `EnterFullScreen()` {#enterfullscreen}

```csharp
public void EnterFullScreen();
```

Records that the screen was taken over.

### `LeaveFullScreen()` {#leavefullscreen}

```csharp
public void LeaveFullScreen();
```

Records that the screen was given back, which is what a test checks after a crash.

### `ReadKey()` {#readkey}

```csharp
public ConsoleKeyInfo ReadKey();
```

Takes the next queued key, or nothing when the queue has run dry.

**Returns** `ConsoleKeyInfo` — The key press.

### `ReadMouse()` {#readmouse}

```csharp
public MouseEvent ReadMouse();
```

Takes the next queued mouse event, or nothing when the queue has run dry.

**Returns** [`MouseEvent`](../arlecchino.input/MouseEvent.md) — The event.

### `Write(string)` {#write-string}

```csharp
public void Write(string text);
```

Collects output instead of showing it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was written. |

