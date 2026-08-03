---
title: Arlecchino.Modals.Choosing
sidebar_label: Arlecchino.Modals.Choosing
sidebar_position: 0
---

# Arlecchino.Modals.Choosing

## Classes

| Type | Summary |
|---|---|
| [`ChoiceModal`](ChoiceModal.md) | One option out of a filterable list. |
| [`CommandModal`](CommandModal.md) | The list of what can be pressed right now. It is a reminder rather than a menu: the keys keep working while it is open, so a command runs from its own key instead of from a selection. What a key or a row means is not this dialog's to know — the commands it lists belong to the view and to the application, and both are found through the container. Whoever opens it says what to do with a press, which is why the palette is opened by the framework rather than by an application writing `new CommandModal`. |
| [`MultiChoiceModal`](MultiChoiceModal.md) | Any number of options out of a filterable list. Marks survive a change of filter. |
| [`OptionListModal`](OptionListModal.md) | What the single- and multi-choice dialogs share: the options, the typed filter and the cursor. |

