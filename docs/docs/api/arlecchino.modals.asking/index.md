---
title: Arlecchino.Modals.Asking
sidebar_label: Arlecchino.Modals.Asking
sidebar_position: 0
---

# Arlecchino.Modals.Asking

## Classes

| Type | Summary |
|---|---|
| [`NumberModal`](NumberModal.md) | A number that can be both typed and stepped. Bounds are checked before your validator runs, and the message reports them with affixes, so the user sees the same form they are editing. |
| [`NumericModal`](NumericModal.md) | What the number field and the slider have in common: stepping, precision and affixes. |
| [`TextAreaModal`](TextAreaModal.md) | Several lines of text, edited in place: a description, a commit message, a snippet of configuration. `Enter` starts a new line here rather than accepting the dialog, so confirming is a key of its own — the `Submit` binding. The caret is a row and a position inside that row, and every move and edit goes by symbols rather than `char` values, so emoji and combining marks survive a backspace. |
| [`TextEditing`](TextEditing.md) | Editing a line of text: where the caret goes and what each edit does to it. Kept apart from the fields themselves so the text field, the number field and anything added later behave identically, and so the behavior can be tested without a terminal. Editing never touches the validation message — that is the router's job, which re-checks the field and clears the message only once the input is actually valid. |
| [`TextModal`](TextModal.md) | A line of text: free text, a secret, an email or a link. The built-in format check runs before [`TextModal.Validate`](../arlecchino.modals.asking/TextModal.md#validate), so your validator only sees input that already looks right. |

## Interfaces

| Type | Summary |
|---|---|
| [`IAffixedModal`](IAffixedModal.md) | A field that shows something around its value — a currency sign, a unit. Affixes are decoration only: the callback still receives the bare value. |
| [`ITextEntryModal`](ITextEntryModal.md) | A field that is typed into. Shared by the text field and the number field, which is why both behave the same way when it comes to editing and error messages. |

## Enums

| Type | Summary |
|---|---|
| [`TextFormat`](TextFormat.md) | A built-in check a text field runs before your own validator, so common mistakes are caught with a translated message instead of a handwritten regex. |

