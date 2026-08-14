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
| [`TextAreaModal`](TextAreaModal.md) | Several lines of text, edited in place, where `Enter` starts a new line and the `Submit` binding confirms. Every move and edit goes by symbols, so emoji and combining marks survive a backspace. |
| [`TextEditing`](TextEditing.md) | Editing a line of text: where the caret goes and what each edit does to it, kept apart from the fields so every one of them behaves alike. The validation message is the router's to clear, not this one's. |
| [`TextModal`](TextModal.md) | A line of text: free text, a secret, an email or a link. The built-in format check runs before [`TextModal.Validate`](../arlecchino.modals.asking/TextModal.md#validate), so your validator only sees input that already looks right. |

## Interfaces

| Type | Summary |
|---|---|
| [`IAffixedModal`](IAffixedModal.md) | A field that shows something around its value — a currency sign, a unit. Affixes are decoration only: the callback still receives the bare value. |
| [`ITextEntryModal`](ITextEntryModal.md) | A field that is typed into, shared by the text field and the number field so both edit and complain alike. |

## Enums

| Type | Summary |
|---|---|
| [`TextFormat`](TextFormat.md) | A built-in check a text field runs before your own validator, so common mistakes are caught with a translated message instead of a handwritten regex. |

