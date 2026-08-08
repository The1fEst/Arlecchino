---
title: "SystemTerminal"
sidebar_label: "SystemTerminal"
---

# SystemTerminal class

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino.Core`

The real console. Registered by default and replaceable through `UseTerminal<T>()`. On Windows it turns on virtual terminal output at startup and turns off virtual terminal input, because that flag stops `Console.ReadKey` from delivering keys at all.

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
| [`KeyAvailable`](#keyavailable) | Whether a key is waiting. Always false when input is redirected. With the Windows mouse on, the answer comes from the console's own event queue rather than from `Console`, because the two cannot both consume it. |
| [`MouseAvailable`](#mouseavailable) | Whether a mouse event is waiting. Only ever true while the Windows mouse is on. |
| [`Width`](#width) | Window width, or a fixed width when output is redirected. |

## Methods

| Member | Summary |
|---|---|
| [`CopyToClipboard(string)`](#copytoclipboard-string) | Copies through the terminal itself, encoded as base64. This is the only way to reach the clipboard of the machine the user is actually at when the application runs over a remote session; terminals that have it switched off silently drop it. |
| [`DisableMouse()`](#disablemouse) | Stops mouse reporting and gives the console back the mode it had. |
| [`DisablePaste()`](#disablepaste) | Turns bracketed paste off again. |
| [`EnableMouse()`](#enablemouse) | Starts reporting presses, releases, drags and the wheel. Elsewhere, that means SGR reports mixed into the key stream; on Windows the console is read record by record instead, because the flag that delivers SGR reports there also silences the keyboard. Quick-edit mode is turned off while this is on, since otherwise the console eats clicks as text selection. |
| [`EnablePaste()`](#enablepaste) | Turns on bracketed paste. Terminals that do not know the mode ignore it. |
| [`EnterFullScreen()`](#enterfullscreen) | Switches to the alternate screen and hides the cursor. The keyboard protocol is not asked for, though it is what would make `Ctrl+Enter` a key at all. Asking moves the function keys from `SS3 P` to `CSI P`, and the escape sequences are read by `Console.ReadKey` before this library sees a byte of them — which reads `CSI P` as F4, `CSI Q` as F5 and `CSI S` as F7. Measured, not deduced: kitty on a Mac, where the terminal's own description is inside the application bundle rather than in the system database, so the runtime falls back to one that spells those keys differently. Trading four working function keys for one new combination is not a trade. Reading the bytes ourselves would settle it, and until then the protocol stays unasked for. |
| [`LeaveFullScreen()`](#leavefullscreen) | Returns to the normal screen and makes the cursor visible again. |
| [`ReadKey()`](#readkey) | Takes the next key without echoing it. |
| [`ReadMouse()`](#readmouse) | Takes the next mouse event read from the console's event queue. |
| [`Unread(KeyPress)`](#unread-keypress) | Puts a key back, so the next read returns it. |
| [`Write(string)`](#write-string) | Writes a composed frame. |

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

Whether a key is waiting. Always false when input is redirected. With the Windows mouse on, the answer comes from the console's own event queue rather than from `Console`, because the two cannot both consume it.

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

Copies through the terminal itself, encoded as base64. This is the only way to reach the clipboard of the machine the user is actually at when the application runs over a remote session; terminals that have it switched off silently drop it.

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

Starts reporting presses, releases, drags and the wheel. Elsewhere, that means SGR reports mixed into the key stream; on Windows the console is read record by record instead, because the flag that delivers SGR reports there also silences the keyboard. Quick-edit mode is turned off while this is on, since otherwise the console eats clicks as text selection.

### `EnablePaste()` {#enablepaste}

```csharp
public void EnablePaste();
```

Turns on bracketed paste. Terminals that do not know the mode ignore it.

### `EnterFullScreen()` {#enterfullscreen}

```csharp
public void EnterFullScreen();
```

Switches to the alternate screen and hides the cursor. The keyboard protocol is not asked for, though it is what would make `Ctrl+Enter` a key at all. Asking moves the function keys from `SS3 P` to `CSI P`, and the escape sequences are read by `Console.ReadKey` before this library sees a byte of them — which reads `CSI P` as F4, `CSI Q` as F5 and `CSI S` as F7. Measured, not deduced: kitty on a Mac, where the terminal's own description is inside the application bundle rather than in the system database, so the runtime falls back to one that spells those keys differently. Trading four working function keys for one new combination is not a trade. Reading the bytes ourselves would settle it, and until then the protocol stays unasked for.

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

Writes a composed frame.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | Text with escape sequences already embedded. |

