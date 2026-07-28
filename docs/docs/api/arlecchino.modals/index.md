---
title: Arlecchino.Modals
sidebar_label: Arlecchino.Modals
sidebar_position: 0
---

# Arlecchino.Modals

## Classes

| Type | Summary |
|---|---|
| [`ChoiceModal`](ChoiceModal.md) | One option out of a filterable list. |
| [`ColorModal`](ColorModal.md) | A colour picked on three sliders. Hue, saturation and lightness are edited rather than the raw channels because they are what people reach for; the result is converted to [`Rgb`](../arlecchino.rendering/Rgb.md) on the way out. Both directions round to whole units, so feeding a colour back in can shift it by one. |
| [`CommandModal`](CommandModal.md) | The list of what can be pressed right now. It is a reminder rather than a menu: the keys keep working while it is open, so a command runs from its own key instead of from a selection. |
| [`DateModal`](DateModal.md) | A calendar date, edited as year, month and day. The value is kept inside the bounds and inside the calendar at every step, so a day that the new month does not have is pulled back to its last day rather than rejected. |
| [`MessageModal`](MessageModal.md) | Something the user only has to read: a result, a warning, an explanation of what just failed. It takes no input beyond the key that closes it, which is what separates it from every other dialog here. |
| [`Modal`](Modal.md) | A dialog waiting for an answer. Assign one to `ArlecchinoState.Modal` — while it is open it takes every key, draws over the view and suppresses the hints box. |
| [`MultiChoiceModal`](MultiChoiceModal.md) | Any number of options out of a filterable list. Marks survive a change of filter. |
| [`NotificationModal`](NotificationModal.md) | One notification, read in full. The output row and the notifications screen have one line each to give a message, which is not enough for the errors a copy collected or the output of a command — opening the entry shows the whole of it, and offers whatever the entry said could be done about it. The notifications screen opens this itself, so an application only fills in [`Notification.Detail`](../arlecchino.diagnostics/Notification.md#detail) and [`Notification.Actions`](../arlecchino.diagnostics/Notification.md#actions) when it raises the entry. |
| [`NumberModal`](NumberModal.md) | A number that can be both typed and stepped. Bounds are checked before your validator runs, and the message reports them with affixes, so the user sees the same form they are editing. |
| [`NumericModal`](NumericModal.md) | What the number field and the slider have in common: stepping, precision and affixes. |
| [`OptionListModal`](OptionListModal.md) | What the single- and multi-choice dialogs share: the options, the typed filter and the cursor. |
| [`SegmentedModal`](SegmentedModal.md) | A value edited as a row of fixed-width number segments, the way dates and times are. Digits are collected per segment and only applied once the segment fills up, so a half-typed segment never produces a nonsensical intermediate value. |
| [`SliderModal`](SliderModal.md) | A value inside a range, adjusted by arrows or by dragging the track. There is nothing to type, so the value is always valid and the dialog never reports an error. |
| [`TextAreaModal`](TextAreaModal.md) | Several lines of text, edited in place: a description, a commit message, a snippet of configuration. `Enter` starts a new line here rather than accepting the dialog, so confirming is a key of its own — the `Submit` binding. The caret is a row and a position inside that row, and every move and edit goes by symbols rather than `char` values, so emoji and combining marks survive a backspace. |
| [`TextEditing`](TextEditing.md) | Editing a line of text: where the caret goes and what each edit does to it. Kept apart from the fields themselves so the text field, the number field and anything added later behave identically, and so the behaviour can be tested without a terminal. Editing never touches the validation message — that is the router's job, which re-checks the field and clears the message only once the input is actually valid. |
| [`TextModal`](TextModal.md) | A line of text: free text, a secret, an email or a link. The built-in format check runs before [`TextModal.Validate`](../arlecchino.modals/TextModal.md#validate), so your validator only sees input that already looks right. |
| [`TimeModal`](TimeModal.md) | A time of day, edited as hours and minutes on a 24-hour clock. Everything wraps: stepping past midnight comes back around instead of stopping. |
| [`ToggleModal`](ToggleModal.md) | A yes-or-no answer, flipped with the arrows or picked by clicking one of the two chips. |

## Interfaces

| Type | Summary |
|---|---|
| [`IAffixedModal`](IAffixedModal.md) | A field that shows something around its value — a currency sign, a unit. Affixes are decoration only: the callback still receives the bare value. |
| [`IBoundedModal`](IBoundedModal.md) | A value that moves in steps between two ends. Shared by the number field and the slider, so the stepping keys work the same in both. |
| [`ITextEntryModal`](ITextEntryModal.md) | A field that is typed into. Shared by the text field and the number field, which is why both behave the same way when it comes to editing and error messages. |

## Enums

| Type | Summary |
|---|---|
| [`ColorChannel`](ColorChannel.md) | One of the three sliders in the colour dialog. |
| [`TextFormat`](TextFormat.md) | A built-in check a text field runs before your own validator, so common mistakes are caught with a translated message instead of a hand-written regex. |

