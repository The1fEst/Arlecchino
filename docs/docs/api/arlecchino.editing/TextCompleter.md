---
title: "TextCompleter"
sidebar_label: "TextCompleter"
---

# TextCompleter class

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

Finishing the word being typed, hung on any line of text. The first press fills in what every candidate agrees on, later presses step through them, and typing on leaves the offer behind.

```csharp
public sealed class TextCompleter
```

## Constructors

| Member | Summary |
|---|---|
| [`TextCompleter(ITextEntry, ISuggestsWords, ICutsWords, ArlecchinoKeymap)`](#textcompleter-itextentry-isuggestswords-icutswords-arlecchinokeymap) | Hangs completion on a line. |

## Properties

| Member | Summary |
|---|---|
| [`ChosenIndex`](#chosenindex) | Which of [`TextCompleter.Words`](../arlecchino.editing/TextCompleter.md#words) is on the line now, or `-1` while the line holds only as much as they all agree on and none of them in particular. |
| [`Words`](#words) | What the last press found, for an application that draws them. It is empty while nothing has been offered and empties itself once the line has been typed into again. |

## Methods

| Member | Summary |
|---|---|
| [`Complete(bool)`](#complete-bool) | Finishes the word being typed. Where the words are known already, this steps through them instead of asking for them again. |
| [`Forget()`](#forget) | Drops what was offered and gives up whatever is still being asked for. A line that is closed or wiped calls it; a line that is only typed into need not, since an offer outlives no edit. |
| [`Handle(KeyPress)`](#handle-keypress) | Finishes the word, or steps to the next of what was offered for it. Any other key is left alone and this can be asked before the editing keys are. |

## Constructors in detail

### `TextCompleter(ITextEntry, ISuggestsWords, ICutsWords, ArlecchinoKeymap)` {#textcompleter-itextentry-isuggestswords-icutswords-arlecchinokeymap}

```csharp
public TextCompleter(
    ITextEntry entry,
    ISuggestsWords words,
    ICutsWords cuts,
    ArlecchinoKeymap keymap);
```

Hangs completion on a line.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `entry` | [`ITextEntry`](../arlecchino.editing/ITextEntry.md) | The line being typed into. |
| `words` | [`ISuggestsWords`](../arlecchino.editing/ISuggestsWords.md) | Where the words come from. |
| `cuts` | [`ICutsWords`](../arlecchino.editing/ICutsWords.md) | Which part of the line is the word being finished. |
| `keymap` | [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md) | The keys the application obeys, which [`TextCompleter.Handle`](../arlecchino.editing/TextCompleter.md#handle-keypress) reads by. |

## Properties in detail

### `ChosenIndex` {#chosenindex}

```csharp
public int ChosenIndex { get; }
```

Which of [`TextCompleter.Words`](../arlecchino.editing/TextCompleter.md#words) is on the line now, or `-1` while the line holds only as much as they all agree on and none of them in particular.

**Type** `int`

### `Words` {#words}

```csharp
public IReadOnlyList<string> Words { get; }
```

What the last press found, for an application that draws them. It is empty while nothing has been offered and empties itself once the line has been typed into again.

**Type** `IReadOnlyList<T>`&lt;`string`&gt;

## Methods in detail

### `Complete(bool)` {#complete-bool}

```csharp
public void Complete(bool forward);
```

Finishes the word being typed. Where the words are known already, this steps through them instead of asking for them again.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `forward` | `bool` | `true` to step to the next of them, `false` to the one before. |

### `Forget()` {#forget}

```csharp
public void Forget();
```

Drops what was offered and gives up whatever is still being asked for. A line that is closed or wiped calls it; a line that is only typed into need not, since an offer outlives no edit.

### `Handle(KeyPress)` {#handle-keypress}

```csharp
public bool Handle(KeyPress key);
```

Finishes the word, or steps to the next of what was offered for it. Any other key is left alone and this can be asked before the editing keys are.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that arrived. |

**Returns** `bool` — `true` when the key was one of these and has been dealt with.

## Example

```csharp
var completer = new TextCompleter(entry, new WordList(() => Commands), new SpaceWords(), keymap);

if (completer.Handle(key))
{
return true;
}

```

