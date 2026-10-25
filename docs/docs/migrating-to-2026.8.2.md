---
title: Migrating to 2026.8.2
sidebar_label: Migrating to 2026.8.2
description: One break — editing a line of text moved out of the modals into Arlecchino.Editing — and what the move brought with it.
---

# Migrating to 2026.8.2

One break, and the compiler names every place it touches. Editing a line of text lived in
`Arlecchino.Modals.Asking`, where it was written for the dialogs; it is now `Arlecchino.Editing`,
which anything typed into can carry.

| What changed | What to do |
|---|---|
| `Arlecchino.Modals.Asking.ITextEntryModal` | `Arlecchino.Editing.ITextEntry` |
| `Arlecchino.Modals.Asking.TextEditing` | `Arlecchino.Editing.TextEditing` |
| A public method threw `ArgumentNullException` on a null argument | Nothing, unless you relied on the throw |

Everything else in this release is added rather than changed: a selection, completion, and a line
drawn the way the framework draws its own.

## The move

The calls are the same and take the same arguments, so a caller changes the `using` and the interface
it implements:

```csharp
// 2026.8.1
using Arlecchino.Modals.Asking;

public sealed class Filter : ITextEntryModal
{
    public string Text { get; set; } = "";
    public int Caret { get; set; }
}

// 2026.8.2
using Arlecchino.Editing;

public sealed class Filter : ITextEntry
{
    public string Text { get; set; } = "";
    public int Caret { get; set; }
    public int Anchor { get; set; }
}
```

`Anchor` is the one new member: it is where a selection started and equals `Caret` when nothing is
selected. A line that never selects can leave it alone — it is a property the editing calls keep in
step by themselves.

The modals implement the new contract, so a caller that reached one through the old interface reaches
it through the new one. `TextEntry` is a ready-made holder for a line with nothing else on it.

## What came with the move

Nothing here has to be adopted; it is what a line gets by asking for it. See
[Lines of text](editing.md) for all of it.

- **A selection.** `SelectKeys.Handled` answers the arrows, the word keys, `Home` and `End` with
  Shift held, `Ctrl+A` and the cut. `TextEditing.Selection`, `Selected` and `EraseSelection` are the
  calls under it, and typing over a selection replaces it.
- **Completion.** `TextCompleter` finishes the word being typed from an `ISuggestsWords`, cutting the
  line with an `ICutsWords` — `SpaceWords` for a command line, `WholeLine` for a path.
- **Drawing.** `EntryRow.Draw` writes the text, the selection and the symbol the caret stands on;
  `EntryLook` is the three colors, and `Theme.Caret` is the new one.
- **`IArlecchinoView.IsTyping`.** A screen holding a line says so, and the keys a field edits by stop
  being read as keys that move the application.

## Arguments are no longer checked for null

Public methods dropped their `ArgumentNullException.ThrowIfNull` guards: the nullable annotations
already refuse a null at the call, and the check was a second answer to a question the compiler had
answered. Passing `null!` now faults where the value is used rather than where it arrived. Nothing
that compiles without warnings is affected.

## The caret moves by word the way names read

A dot now stops the caret, so `report.2026.md` is crossed in steps rather than in one, and an
underscore does not, so `read_me` stays one word. It is a change to what `Ctrl+←` and `Ctrl+→` do
inside a field rather than to any API.
