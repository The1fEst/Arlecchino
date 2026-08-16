---
title: "SelectKeys"
sidebar_label: "SelectKeys"
---

# SelectKeys class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

The keys that take the selection about rather than the caret, which are the moving keys with Shift held. They are read before the moving keys.

```csharp
public static class SelectKeys
```

## Methods

| Member | Summary |
|---|---|
| [`Handled(ITextEntry, ArlecchinoKeymap, KeyPress)`](#handled-itextentry-arlecchinokeymap-keypress) | Takes the selection where the key says, leaving the text as it is. |

## Methods in detail

### `Handled(ITextEntry, ArlecchinoKeymap, KeyPress)` {#handled-itextentry-arlecchinokeymap-keypress}

```csharp
public static bool Handled(ITextEntry entry, ArlecchinoKeymap keymap, KeyPress key);
```

Takes the selection where the key says, leaving the text as it is.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | The keys the application obeys. |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that arrived. |

**Returns** `bool` — `true` when the key was one of these and has been dealt with.

