---
title: Arlecchino.Editing
sidebar_label: Arlecchino.Editing
sidebar_position: 0
---

# Arlecchino.Editing

## Classes

| Type | Summary |
|---|---|
| [`CaretKeys`](CaretKeys.md) | The keys that take the caret about a field of one line, by a symbol, by a word or to either end. Any of them drops whatever was selected, which is what tells them from the keys with Shift held. |
| [`EntryKeys`](EntryKeys.md) | Every key a line of text answers to, in the order they are read: the clipboard, the selection, the caret, then rubbing out. Whatever is typed into is offered these, so a filter is edited the way a field is. |
| [`EraseKeys`](EraseKeys.md) | The keys that rub text out of a line: a symbol, a word, or everything back to the start. Any of them takes the selection instead while there is one. |
| [`PastedText`](PastedText.md) | What a block of pasted text comes to where it lands. A line of one row takes the first line of it, since what was on the clipboard does not turn one row into several. |
| [`SelectKeys`](SelectKeys.md) | The keys that take the selection about rather than the caret, which are the moving keys with Shift held. They are read before the moving keys. |
| [`SpaceWords`](SpaceWords.md) | Words told apart by the spaces between them, which is how a line of anything typed reads: the word being finished is what stands between the last space and the caret. |
| [`TextCompleter`](TextCompleter.md) | Finishing the word being typed, hung on any line of text. The first press fills in what every candidate agrees on, later presses step through them, and typing on leaves the offer behind. |
| [`TextEditing`](TextEditing.md) | Editing a line of text: where the caret goes and what each edit does to it, apart from whatever holds the line. A symbol is a grapheme cluster rather than a `char`, so an emoji is rubbed out whole. |
| [`TextEntry`](TextEntry.md) | A line of text held on its own, for whatever is typed into but is not a dialog: a filter in a view, a line an application draws for itself. What each key does to it is [`TextEditing`](../arlecchino.editing/TextEditing.md)'s. |
| [`WholeLine`](WholeLine.md) | Everything up to the caret as one word, for a field that holds one thing: a path, a host, a name. Nothing in it divides it, spaces included, since a name is allowed to have them. |
| [`WordList`](WordList.md) | Words an application already holds: the names of its commands, the hosts it knows. They are read through a delegate, so the list is whatever it is when the word is being finished. |

## Structs

| Type | Summary |
|---|---|
| [`CompletionAsk`](CompletionAsk.md) | The half-typed word something is being asked to finish, and the line it stands in. The line goes with it because what a word could turn into depends on what stands in front of it. |

## Interfaces

| Type | Summary |
|---|---|
| [`ICutsWords`](ICutsWords.md) | Which part of a line is the word being finished. A line of shell reaches back to the last space, where a field holding one path is all one word. |
| [`ISuggestsWords`](ISuggestsWords.md) | Where the words a half-typed one could turn into come from. It is asked rather than read, since the answer can be a folder on the far side of a network. |
| [`ITextEntry`](ITextEntry.md) | A line of text being typed into, which is all the editing needs to know about whatever holds it. A dialog field is one; so is a line an application draws for itself. |

