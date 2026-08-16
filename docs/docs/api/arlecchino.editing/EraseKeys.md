---
title: "EraseKeys"
sidebar_label: "EraseKeys"
---

# EraseKeys class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

The keys that rub text out of a line: a symbol, a word, or everything back to the start. Any of them takes the selection instead while there is one.

```csharp
public static class EraseKeys
```

## Methods

| Member | Summary |
|---|---|
| [`Erased(ITextEntry, ArlecchinoKeymap, KeyPress)`](#erased-itextentry-arlecchinokeymap-keypress) | Rubs out what the key says. |

## Methods in detail

### `Erased(ITextEntry, ArlecchinoKeymap, KeyPress)` {#erased-itextentry-arlecchinokeymap-keypress}

```csharp
public static bool Erased(ITextEntry entry, ArlecchinoKeymap keymap, KeyPress key);
```

Rubs out what the key says.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | The keys the application obeys. |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that arrived. |

**Returns** `bool` — `true` when the key was one of these and has been dealt with.

