---
title: What's new in 2026.8.2
sidebar_label: What's new in 2026.8.2
description: Editing a line of text moves out of the modals into a package of its own — one contract, a selection held with Shift, and Tab that finishes the half-typed word.
---

# What's new in 2026.8.2

A release about the line being typed into. Editing lived in the modals, where it was written for the
dialogs; it is now `Arlecchino.Editing`, which anything typed into can carry — a command line, a filter
row, a field in a form. [Migrating to 2026.8.2](migrating-to-2026.8.2.md) is the one break, and the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202682) is the full record.

## One contract for every line

`ITextEntry` is the text and where the caret is, and that is the whole of what a widget has to hold for
the editing to work on it:

```csharp
public sealed class Filter : ITextEntry
{
    public string Text { get; set; } = "";
    public int Caret { get; set; }
}
```

`TextEditing` does the rest — the keys, the words, the clipboard — so a screen that wants a line of its
own writes two properties rather than a text editor. See [Editing](editing.md).

## A selection, with Shift held

`SelectLeft`, `SelectRight`, `SelectWordLeft`, `SelectWordRight` and the rest hold a selection the way
every other text field on the machine does, and copy, cut and paste act on it. The caret also moves by
word the way the letters read rather than by whitespace alone: a dot stops it, so `report.2026.md` is
three steps and not one.

## Tab finishes the word

`TextCompleter` hangs on any line. The first press fills in what is common to every candidate, the next
steps through them, and what is offered comes from an `ISuggestsWords` of your own — asynchronous, and
cancelled when the question changes, so a completer can read a folder over a network without the frame
waiting for it.

## A line drawn the same way everywhere

`EntryRow.Draw` writes the text, the selection behind it and the symbol the caret stands on the way the
framework's own fields are written, and answers with the column the caret landed on. A line longer than
its width scrolls inside it and the caret stays on screen.

Alongside it, a screen now says when it is being typed into — `IArlecchinoView.IsTyping` — so keys a
field edits by are not read as keys that move the screen underneath.
