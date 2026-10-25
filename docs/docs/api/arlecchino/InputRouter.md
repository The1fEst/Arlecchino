---
title: "InputRouter"
sidebar_label: "InputRouter"
---

# InputRouter class

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino`

Decides who gets a key or a mouse event, in order: an open dialog, the palette key, the view's commands, the commands available everywhere, then the view. A handler that throws is reported on the output line.

```csharp
public class InputRouter
```

## Methods

| Member | Summary |
|---|---|
| [`ProcessKey(KeyPress)`](#processkey-keypress) | Routes one key press and asks for a frame, whether anything took it. |
| [`ProcessMouse(MouseEvent)`](#processmouse-mouseevent) | Routes one mouse event and asks for a frame. |
| [`ProcessPaste(string)`](#processpaste-string) | Routes a block of pasted text and asks for a frame. It goes wherever typing would, but as one edit rather than one per character. |

## Methods in detail

### `ProcessKey(KeyPress)` {#processkey-keypress}

```csharp
public void ProcessKey(KeyPress key);
```

Routes one key press and asks for a frame, whether anything took it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

### `ProcessMouse(MouseEvent)` {#processmouse-mouseevent}

```csharp
public void ProcessMouse(MouseEvent mouse);
```

Routes one mouse event and asks for a frame.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event that arrived. |

### `ProcessPaste(string)` {#processpaste-string}

```csharp
public void ProcessPaste(string text);
```

Routes a block of pasted text and asks for a frame. It goes wherever typing would, but as one edit rather than one per character.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was pasted. |

