---
title: Lines of text
sidebar_label: Lines of text
description: ITextEntry, TextEditing, the selection, completion and drawing a line being typed into.
---

# Lines of text

Everything typed into — a modal, a filter, the file picker, a command line an application draws
itself — is the same thing: a string, a caret in it, and an anchor the selection is measured from.
`Arlecchino.Editing` is that contract and the work done on it, so a line an application writes takes
the keys the framework's own fields take and behaves the same way.

## The contract

```csharp
public interface ITextEntry
{
    string Text { get; set; }
    int Caret { get; set; }
    int Anchor { get; set; }
}
```

`Caret` is a position in the string, not an index into `char` values: everything moves and deletes by
symbols, so an emoji or a family sequence goes in one press. `Anchor` is where a selection started;
it equals `Caret` when nothing is selected.

`TextEntry` implements it and holds nothing else, for a caller with no line of its own:

```csharp
var line = new TextEntry { Text = "arlecchino" };

TextEditing.MoveToEnd(line);
TextEditing.EraseWord(line);
```

## What can be done to one

`TextEditing` is the whole of it, static and working on any `ITextEntry`:

| Call | Does |
|---|---|
| `Insert(entry, character)` | Types a character over the selection, if there is one |
| `Backspace(entry)` / `Delete(entry)` | Removes the symbol before or after the caret |
| `EraseWord(entry)` / `EraseToStart(entry)` | Removes the word before the caret, or everything before it |
| `MoveCaret(entry, delta)` / `MoveWord(entry, direction)` | Moves by symbol or by word |
| `MoveToStart(entry)` / `MoveToEnd(entry)` | Moves to either end |
| `SelectCaret(entry, delta)` / `SelectWord(entry, direction)` | The same moves, dragging the selection |
| `SelectToStart(entry)` / `SelectToEnd(entry)` / `SelectAll(entry)` | Selects to an end, or all of it |
| `Selection(entry)` / `Selected(entry)` | Where the selection is, and what is in it |
| `EraseSelection(entry)` | Takes the selection out; `false` when there was none |

A word ends at a space, at punctuation and at a dot, so `report.2026.md` is crossed in steps; an
underscore is part of the word, so `read_me` is one.

## Answering keys

Three helpers match a key against the [keymap](keyboard.md#the-keymap) and say whether they took it,
which is a line's whole key handling:

```csharp
public bool Handle(KeyPress key)
{
    if (SelectKeys.Handled(_line, _keymap, key) ||
        CaretKeys.Moved(_line, _keymap, key) ||
        EraseKeys.Erased(_line, _keymap, key))
    {
        return true;
    }

    if (KeyText.For(_options.TextInput).Resolve(key) is { } typed)
    {
        TextEditing.Insert(_line, typed);

        return true;
    }

    return false;
}
```

`SelectKeys` covers the arrows, the word keys, `Home` and `End` with Shift held, `Ctrl+A` and the
cut; `CaretKeys` the same moves without it; `EraseKeys` the rub-outs. Order them as above: the
selecting keys are the moving keys with a modifier, so they have to be asked first.

## Finishing the word

`TextCompleter` hangs completion on a line. The first press fills in what every candidate agrees on,
later presses step through them, and typing on leaves the offer behind:

```csharp
var completer = new TextCompleter(entry, new WordList(() => Commands), new SpaceWords(), keymap);

if (completer.Handle(key))
{
    return true;
}
```

Two small interfaces decide what is being finished:

| Interface | Ready-made | Answers |
|---|---|---|
| `ISuggestsWords` | `WordList` | Which words could finish this one, asynchronously |
| `ICutsWords` | `SpaceWords`, `WholeLine` | Which part of the line is the word being finished |

`SpaceWords` cuts at spaces, for a command line; `WholeLine` treats the line as one word, for a path
or a name. What either hands back is a `CompletionAsk` — the line, where the word starts and how long
it is, with `Word`, `Before` and `After` read off it. `Words` lists what was offered and `Chosen`
says which one is filled in, for a caller that draws the candidates; `Forget` drops the offer.

Suggesting is asynchronous and cancelled when the question changes, so a completer can read a folder
over a network without the frame waiting for it.

## Drawing one

`EntryRow.Draw` writes a line the way the framework's own fields are written: the text, the selection
behind it, and the symbol the caret stands on drawn the other way round.

```csharp
EntryRow.Draw(region, row: 0, column: 2, width: 40, entry, new EntryLook(
    Theme.Input,
    Theme.Selected,
    Theme.Caret));
```

It answers the column the caret landed on. A line longer than the width scrolls inside it, and the
caret stays on screen. `EntryRuns.Of` breaks the same line into runs and hands each to a callback,
for a caller that writes them itself.

## What a screen owes

A screen holding a line says so, so that keys a field edits by are not read as keys that move the
application:

```csharp
public bool IsTyping => _filter.IsOpen;
```

`IArlecchinoView.IsTyping` is `false` by default, and `Navigator.CurrentIsTyping` is that answer for
whatever screen is open.
