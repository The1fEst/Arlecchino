---
title: "CaretKeys"
sidebar_label: "CaretKeys"
---

# CaretKeys class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

The keys that take the caret about a field of one line, by a symbol, by a word or to either end. Any of them drops whatever was selected, which is what tells them from the keys with Shift held.

```csharp
public static class CaretKeys
```

## Methods

| Member | Summary |
|---|---|
| [`Moved(ITextEntry, ArlecchinoKeymap, KeyPress)`](#moved-itextentry-arlecchinokeymap-keypress) | Moves the caret where the key says, leaving the text as it is. |

## Methods in detail

### `Moved(ITextEntry, ArlecchinoKeymap, KeyPress)` {#moved-itextentry-arlecchinokeymap-keypress}

```csharp
public static bool Moved(ITextEntry entry, ArlecchinoKeymap keymap, KeyPress key);
```

Moves the caret where the key says, leaving the text as it is.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | The keys the application obeys. |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that arrived. |

**Returns** `bool` — `true` when the key was one of these and has been dealt with.

