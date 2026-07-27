---
title: InputRouter
sidebar_label: InputRouter
---

# InputRouter class

**Namespace:** `Arlecchino` &middot; **Assembly:** `Arlecchino`

Decides who gets a key or a mouse event. The order is what keeps the application predictable: an open dialog takes everything, then the palette key, then the view's own commands, then commands available everywhere, and only then the view itself. A handler that throws is reported on the output line rather than allowed to stop the loop.

```csharp
public class InputRouter
```

## Methods

| Member | Summary |
|---|---|
| [`ProcessKey(ConsoleKeyInfo)`](#processkey-consolekeyinfo) | Routes one key press and asks for a frame, whether or not anything took it. |
| [`ProcessMouse(MouseEvent)`](#processmouse-mouseevent) | Routes one mouse event and asks for a frame. |
| [`ProcessPaste(string)`](#processpaste-string) | Routes a block of pasted text and asks for a frame. It goes wherever typing would, but as one edit rather than one per character. |

## Methods in detail

### `ProcessKey(ConsoleKeyInfo)` {#processkey-consolekeyinfo}

```csharp
public void ProcessKey(ConsoleKeyInfo key);
```

Routes one key press and asks for a frame, whether or not anything took it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key that was pressed. |

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

