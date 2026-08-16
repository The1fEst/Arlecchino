---
title: "EntryKeys"
sidebar_label: "EntryKeys"
---

# EntryKeys class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

Every key a line of text answers to, in the order they are read: the clipboard, the selection, the caret, then rubbing out. Whatever is typed into is offered these, so a filter is edited the way a field is.

```csharp
public static class EntryKeys
```

## Methods

| Member | Summary |
|---|---|
| [`Clipped(ITextEntry, ArlecchinoKeymap, Action<string>, KeyPress)`](#clipped-itextentry-arlecchinokeymap-action-string-keypress) | Copying and cutting. Copying takes the selection where there is one and the whole line where there is not; cutting takes the selection alone, since cutting a line nothing is selected on means nothing. |
| [`Handled(ITextEntry, ArlecchinoKeymap, Action<string>, KeyPress)`](#handled-itextentry-arlecchinokeymap-action-string-keypress) | Does what the key says to the line. |

## Methods in detail

### `Clipped(ITextEntry, ArlecchinoKeymap, Action<string>, KeyPress)` {#clipped-itextentry-arlecchinokeymap-action-string-keypress}

```csharp
public static bool Clipped(
    ITextEntry entry,
    ArlecchinoKeymap keymap,
    Action<string> copy,
    KeyPress key);
```

Copying and cutting. Copying takes the selection where there is one and the whole line where there is not; cutting takes the selection alone, since cutting a line nothing is selected on means nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | The keys the application obeys. |
| `copy` | `Action<T>`&lt;`string`&gt; | Puts text on the clipboard. |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that arrived. |

**Returns** `bool` — `true` when the key was one of these and has been dealt with.

### `Handled(ITextEntry, ArlecchinoKeymap, Action<string>, KeyPress)` {#handled-itextentry-arlecchinokeymap-action-string-keypress}

```csharp
public static bool Handled(
    ITextEntry entry,
    ArlecchinoKeymap keymap,
    Action<string> copy,
    KeyPress key);
```

Does what the key says to the line.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being edited. |
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | The keys the application obeys. |
| `copy` | `Action<T>`&lt;`string`&gt; | Puts text on the clipboard, for when the line is copied or cut. |
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that arrived. |

**Returns** `bool` — `true` when the key was one of these and has been dealt with.

