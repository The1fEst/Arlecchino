---
title: "SystemTerminal"
sidebar_label: "SystemTerminal"
---

# SystemTerminal class

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino.Core`

The real console, registered by default and replaceable through `UseTerminal<T>()`. On Windows it turns virtual terminal output on, virtual terminal input off, and borrows `Ctrl+C` while it has the terminal.

```csharp
public sealed class SystemTerminal : IArlecchinoTerminal
```

**Implements** [`IArlecchinoTerminal`](../arlecchino/IArlecchinoTerminal.md)

## Constructors

| Member | Summary |
|---|---|
| [`SystemTerminal()`](#systemterminal) | Prepares the console: UTF-8 output, hidden cursor, and escape sequences where the platform allows them. A console that refuses them drops color to [`ColorSupport.None`](../arlecchino.rendering.colors/ColorSupport.md). |

## Properties

| Member | Summary |
|---|---|
| [`Height`](#height) | Window height, or a fixed height when output is redirected. |
| [`KeyAvailable`](#keyavailable) | Whether a key is waiting, and always false when input is redirected. With the Windows mouse on, the answer comes from the console's own event queue. |
| [`MouseAvailable`](#mouseavailable) | Whether a mouse event is waiting. Only ever true while the Windows mouse is on. |
| [`Width`](#width) | Window width, or a fixed width when output is redirected. |

## Methods

| Member | Summary |
|---|---|
| [`CopyToClipboard(string)`](#copytoclipboard-string) | Copies through the terminal as base64, the only way to the clipboard of whoever is watching a remote session, and then through `ClipboardPrograms`, which a silent terminal is not. |
| [`DisableMouse()`](#disablemouse) | Stops mouse reporting and gives the console back the mode it had. |
| [`DisablePaste()`](#disablepaste) | Turns bracketed paste off again. |
| [`EnableMouse()`](#enablemouse) | Starts reporting presses, releases, drags and the wheel: as SGR reports in the key stream, or record by record on Windows. Quick-edit mode is turned off while this is on. |
| [`EnablePaste()`](#enablepaste) | Turns on bracketed paste. Terminals that do not know the mode ignore it. |
| [`EnterFullScreen()`](#enterfullscreen) | Switches to the alternate screen and hides the cursor. The keyboard protocol is left unasked for, since asking it moves the function keys onto sequences the runtime reads as other keys. |
| [`GiveBackControlKeys()`](#givebackcontrolkeys) | Gives `Ctrl+C` back to the console, which the shell and any program the terminal is lent to expect to have it. Doing so twice, or without having taken it, changes nothing. |
| [`LeaveFullScreen()`](#leavefullscreen) | Returns to the normal screen and makes the cursor visible again. |
| [`ReadKey()`](#readkey) | Takes the next key without echoing it. |
| [`ReadMouse()`](#readmouse) | Takes the next mouse event read from the console's event queue. |
| [`TakeControlKeys()`](#takecontrolkeys) | Takes `Ctrl+C` off the Windows console, which raises it as a signal — and raises the same signal for `Ctrl+Shift+C`, since both type the same character. Whether it was the console's is remembered, so it is given back as found. |
| [`Unread(KeyPress)`](#unread-keypress) | Puts a key back, so the next read returns it. |
| [`Write(string)`](#write-string) | Writes a composed frame, and everything else this terminal says, through the one writer it was given — or the console's own, when it was given none. |

## Constructors in detail

### `SystemTerminal()` {#systemterminal}

```csharp
public SystemTerminal();
```

Prepares the console: UTF-8 output, hidden cursor, and escape sequences where the platform allows them. A console that refuses them drops color to [`ColorSupport.None`](../arlecchino.rendering.colors/ColorSupport.md).

## Properties in detail

### `Height` {#height}

```csharp
public int Height { get; }
```

Window height, or a fixed height when output is redirected.

**Type** `int`

### `KeyAvailable` {#keyavailable}

```csharp
public bool KeyAvailable { get; }
```

Whether a key is waiting, and always false when input is redirected. With the Windows mouse on, the answer comes from the console's own event queue.

**Type** `bool`

### `MouseAvailable` {#mouseavailable}

```csharp
public bool MouseAvailable { get; }
```

Whether a mouse event is waiting. Only ever true while the Windows mouse is on.

**Type** `bool`

### `Width` {#width}

```csharp
public int Width { get; }
```

Window width, or a fixed width when output is redirected.

**Type** `int`

## Methods in detail

### `CopyToClipboard(string)` {#copytoclipboard-string}

```csharp
public void CopyToClipboard(string text);
```

Copies through the terminal as base64, the only way to the clipboard of whoever is watching a remote session, and then through `ClipboardPrograms`, which a silent terminal is not.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to copy. |

### `DisableMouse()` {#disablemouse}

```csharp
public void DisableMouse();
```

Stops mouse reporting and gives the console back the mode it had.

### `DisablePaste()` {#disablepaste}

```csharp
public void DisablePaste();
```

Turns bracketed paste off again.

### `EnableMouse()` {#enablemouse}

```csharp
public void EnableMouse();
```

Starts reporting presses, releases, drags and the wheel: as SGR reports in the key stream, or record by record on Windows. Quick-edit mode is turned off while this is on.

### `EnablePaste()` {#enablepaste}

```csharp
public void EnablePaste();
```

Turns on bracketed paste. Terminals that do not know the mode ignore it.

### `EnterFullScreen()` {#enterfullscreen}

```csharp
public void EnterFullScreen();
```

Switches to the alternate screen and hides the cursor. The keyboard protocol is left unasked for, since asking it moves the function keys onto sequences the runtime reads as other keys.

### `GiveBackControlKeys()` {#givebackcontrolkeys}

```csharp
public void GiveBackControlKeys();
```

Gives `Ctrl+C` back to the console, which the shell and any program the terminal is lent to expect to have it. Doing so twice, or without having taken it, changes nothing.

### `LeaveFullScreen()` {#leavefullscreen}

```csharp
public void LeaveFullScreen();
```

Returns to the normal screen and makes the cursor visible again.

### `ReadKey()` {#readkey}

```csharp
public KeyPress ReadKey();
```

Takes the next key without echoing it.

**Returns** [`KeyPress`](../arlecchino.input/KeyPress.md) — The key that was pressed.

### `ReadMouse()` {#readmouse}

```csharp
public MouseEvent ReadMouse();
```

Takes the next mouse event read from the console's event queue.

**Returns** [`MouseEvent`](../arlecchino.input/MouseEvent.md) — What the mouse did, in frame cells.

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | The mouse is not being read on this platform. |

### `TakeControlKeys()` {#takecontrolkeys}

```csharp
public void TakeControlKeys();
```

Takes `Ctrl+C` off the Windows console, which raises it as a signal — and raises the same signal for `Ctrl+Shift+C`, since both type the same character. Whether it was the console's is remembered, so it is given back as found.

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

Writes a composed frame, and everything else this terminal says, through the one writer it was given — or the console's own, when it was given none.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | Text with escape sequences already embedded. |

