---
title: "IArlecchinoTerminal"
sidebar_label: "IArlecchinoTerminal"
---

# IArlecchinoTerminal interface

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino.Core`

Everything the framework needs from a console. Replace it with `UseTerminal<T>()` to drive a test harness or a remote session; `SystemTerminal` is the real one.

```csharp
public interface IArlecchinoTerminal
```

**Implemented by** [`SystemTerminal`](../arlecchino/SystemTerminal.md), [`FakeTerminal`](../arlecchino.testing/FakeTerminal.md)

## Properties

| Member | Summary |
|---|---|
| [`Height`](#height) | Height of the window in rows. |
| [`KeyAvailable`](#keyavailable) | Whether [`IArlecchinoTerminal.ReadKey`](../arlecchino/IArlecchinoTerminal.md#readkey) can return immediately. |
| [`MouseAvailable`](#mouseavailable) | Whether [`IArlecchinoTerminal.ReadMouse`](../arlecchino/IArlecchinoTerminal.md#readmouse) can return immediately. Only terminals that deliver the mouse outside the key stream — the Windows console — ever report `true`; elsewhere mouse reports arrive as escape sequences among the keys. |
| [`Width`](#width) | Width of the window in cells. |

## Methods

| Member | Summary |
|---|---|
| [`CopyToClipboard(string)`](#copytoclipboard-string) | Puts text on the clipboard of whatever is showing the terminal, which is the local machine even over a remote session. Terminals may refuse it, and none report back. |
| [`DisableMouse()`](#disablemouse) | Stops reporting mouse events. |
| [`DisablePaste()`](#disablepaste) | Stops the paste markers. |
| [`EnableMouse()`](#enablemouse) | Starts reporting mouse events, if the platform supports it. |
| [`EnablePaste()`](#enablepaste) | Asks the terminal to wrap pasted text in markers, so a paste arrives as one block instead of looking like someone typing very fast. |
| [`EnterFullScreen()`](#enterfullscreen) | Switches to the alternate screen and hides the cursor. |
| [`LeaveFullScreen()`](#leavefullscreen) | Returns to the normal screen and shows the cursor again. |
| [`ReadKey()`](#readkey) | Takes the next key, blocking until one arrives. |
| [`ReadMouse()`](#readmouse) | Takes the next mouse event. Only call it while [`IArlecchinoTerminal.MouseAvailable`](../arlecchino/IArlecchinoTerminal.md#mouseavailable) is true. |
| [`Unread(KeyPress)`](#unread-keypress) | Puts a key back, so the next [`IArlecchinoTerminal.ReadKey`](../arlecchino/IArlecchinoTerminal.md#readkey) returns it. It is for code that had to read a key to find out it did not want it. |
| [`Write(string)`](#write-string) | Writes composed output. A frame arrives as one call. |

## Properties in detail

### `Height` {#height}

```csharp
public int Height { get; }
```

Height of the window in rows.

**Type** `int`

### `KeyAvailable` {#keyavailable}

```csharp
public bool KeyAvailable { get; }
```

Whether [`IArlecchinoTerminal.ReadKey`](../arlecchino/IArlecchinoTerminal.md#readkey) can return immediately.

**Type** `bool`

### `MouseAvailable` {#mouseavailable}

```csharp
public bool MouseAvailable { get; }
```

Whether [`IArlecchinoTerminal.ReadMouse`](../arlecchino/IArlecchinoTerminal.md#readmouse) can return immediately. Only terminals that deliver the mouse outside the key stream — the Windows console — ever report `true`; elsewhere mouse reports arrive as escape sequences among the keys.

**Type** `bool`

### `Width` {#width}

```csharp
public int Width { get; }
```

Width of the window in cells.

**Type** `int`

## Methods in detail

### `CopyToClipboard(string)` {#copytoclipboard-string}

```csharp
public void CopyToClipboard(string text);
```

Puts text on the clipboard of whatever is showing the terminal, which is the local machine even over a remote session. Terminals may refuse it, and none report back.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to copy. |

### `DisableMouse()` {#disablemouse}

```csharp
public void DisableMouse();
```

Stops reporting mouse events.

### `DisablePaste()` {#disablepaste}

```csharp
public void DisablePaste();
```

Stops the paste markers.

### `EnableMouse()` {#enablemouse}

```csharp
public void EnableMouse();
```

Starts reporting mouse events, if the platform supports it.

### `EnablePaste()` {#enablepaste}

```csharp
public void EnablePaste();
```

Asks the terminal to wrap pasted text in markers, so a paste arrives as one block instead of looking like someone typing very fast.

### `EnterFullScreen()` {#enterfullscreen}

```csharp
public void EnterFullScreen();
```

Switches to the alternate screen and hides the cursor.

### `LeaveFullScreen()` {#leavefullscreen}

```csharp
public void LeaveFullScreen();
```

Returns to the normal screen and shows the cursor again.

### `ReadKey()` {#readkey}

```csharp
public KeyPress ReadKey();
```

Takes the next key, blocking until one arrives.

**Returns** [`KeyPress`](../arlecchino.input/KeyPress.md) — The key that was pressed.

### `ReadMouse()` {#readmouse}

```csharp
public MouseEvent ReadMouse();
```

Takes the next mouse event. Only call it while [`IArlecchinoTerminal.MouseAvailable`](../arlecchino/IArlecchinoTerminal.md#mouseavailable) is true.

**Returns** [`MouseEvent`](../arlecchino.input/MouseEvent.md) — What the mouse did, in frame cells.

### `Unread(KeyPress)` {#unread-keypress}

```csharp
public void Unread(KeyPress key);
```

Puts a key back, so the next [`IArlecchinoTerminal.ReadKey`](../arlecchino/IArlecchinoTerminal.md#readkey) returns it. It is for code that had to read a key to find out it did not want it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key to put back. |

### `Write(string)` {#write-string}

```csharp
public void Write(string text);
```

Writes composed output. A frame arrives as one call.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | Text with escape sequences already embedded. |

