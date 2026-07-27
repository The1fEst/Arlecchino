---
title: Localization
sidebar_label: Localization
description: ArlecchinoStrings and why no user-visible text is hardcoded.
---

# Localization

The framework never hardcodes user-visible text at a call site. Every string it draws is a delegate on
`ArlecchinoStrings` with an English default, so an application can translate all of the chrome — and
switch languages at runtime — without the framework knowing that languages exist.

```csharp
builder.Services
    .AddArlecchino()
    .UseStrings(new ArlecchinoStrings
    {
        KeysTitle = () => Loc(LocString.Keys),
        Filter = filter => Loc(LocString.Filter, filter),
        FilePicker = new ArlecchinoStrings.FilePickerStrings
        {
            ColumnName = () => Loc(LocString.Name),
        },
    });
```

Delegates are called on every frame that needs them, so pointing them at a resolver that reads the
current language is enough — nothing has to be rebuilt when the language changes. Every property has a
default, so a partial override is a valid `ArlecchinoStrings`.

## Chrome

| Property | Default |
|---|---|
| `KeysTitle` | `Keys` — title of the hints box |
| `CommandPaletteTitle` | `Commands` |
| `CommandUnknown(key)` | `unknown command: {key}` |
| `ModalTextHints` | `Enter — confirm   Esc — cancel` |
| `ModalChoiceHints` | `↑↓ — move   Enter — pick   Esc — cancel` |
| `ModalMultiChoiceHints` | `↑↓ — move   Space — mark   Enter — confirm   Esc — cancel` |
| `ModalNumberHints` | `↑↓ — step   PgUp/PgDn — jump   Enter — confirm   Esc — cancel` |
| `ModalSliderHints` | `←→ — adjust   Home/End — ends   Enter — confirm   Esc — cancel` |
| `ModalToggleHints` | `←→ — switch   Enter — confirm   Esc — cancel` |
| `ModalCommandHints` | `press a key   Esc — cancel` |
| `Yes` / `No` | `Yes` / `No` — the toggle chips |
| `ModalDateHints` / `ModalTimeHints` | `←→ — field   ↑↓ — change   digits — type   Enter — confirm   Esc — cancel` |
| `ModalColorHints` | `↑↓ — channel   ←→ — adjust   Enter — pick   Esc — cancel` |
| `ColorHue` / `ColorSaturation` / `ColorLightness` | `Hue` / `Saturation` / `Lightness` |
| `NotANumber` | `must be a number` |
| `NotAnEmail` | `must be an email address` |
| `NotAUrl` | `must be a http or https link` |
| `OutOfRange(minimum, maximum)` | `must be between {minimum} and {maximum}` |
| `SelectedCount(count)` | `1 selected` / `{count} selected` — shown in the multi-choice title |
| `Filter(text)` | `Filter: {text}` |
| `ListPosition(position, total)` | `3/40` — beside the scroll bar of a list that does not fit |
| `NothingMatches` | `nothing matches` |
| `Empty` | `empty` |
| `FormMove` / `FormEdit` / `FormReset` | `move` / `edit` / `reset` — the verbs in the [form](forms.md) legend |
| `ModalTextAreaHints` | `Enter — new line   Ctrl+Enter — confirm   Esc — cancel` |
| `ModalMessageHints` | `Enter — close   Esc — close` |
| `NotificationsTitle` / `NotificationsCount(count)` / `NotificationsEmpty` | Title, count and empty text of the [notifications screen](diagnostics.md#notifications) |
| `NotificationsClear` / `NotificationsClose` | `clear` / `back` — its hint line |
| `HelpTitle` / `HelpFrameworkSection` / `HelpScreenSection` / `HelpCommandsSection` / `HelpNoCommands` / `HelpClose` | The [keys screen](keyboard.md) |
| `HelpKeys(keymap)` | Every key the framework answers to, paired with what it does — the one place to translate the descriptions |
| `LogTitle(count)` | `Log ({count})` — title of the [log overlay](hosting-and-options.md) |
| `LogHints` | `↑↓ scroll · End latest · Backspace clear · Esc close` |
| `LogEmpty` | `nothing logged yet` |
| `ViewFailed(message)` | `error: {message}` — shown when a view or a callback throws |
| `TerminalTooSmall` | `Terminal window is too small` |
| `TerminalSize(width, height)` | `{width} x {height}` |
| `TerminalNeeded` | `needed at least` |

## File picker

`ArlecchinoStrings.FilePicker` is a nested `FilePickerStrings` covering the
[file picker](file-picker.md): `Title`, `FolderMode` / `FileMode`, `Drives`, `Favorites`,
`Locations`, `Search`, the column headers `ColumnName` / `ColumnDateModified` / `ColumnSize` /
`ColumnKind`, `ItemCount(count)`, the legend entries `HintMove`, `HintOpen`, `HintUp`, `HintPlaces`,
`HintOpenFolder`, `HintOpenFolderOrPickFile`, `HintFilter`, `HintPickCurrentFolder`, `HintCancel`,
and three formatters:

| Formatter | Default behaviour |
|---|---|
| `KindOf(extension)` | Maps an extension to a human name — `ZIP archive`, `Source file`, `PDF document`, falling back to `XYZ file` |
| `DateModified(value)` | `Today at 9:41`, `Yesterday at 9:41`, otherwise `7 Jul 2026 at 9:41` |
| `Size(bytes)` | `40 B`, `12.3 MB`, `--` for a negative length |

`KindFolder` and `KindVolume` name the two non-file kinds.

## The rule for contributors

Adding anything the user can read means adding a field to `ArlecchinoStrings` and calling it — a literal
at the call site is a bug, because an application has no way to reach it. The same rule keeps
application domain types out of the framework: a modal validator is a `Func<string, string?>`, not
somebody's value object.

The rule is enforced rather than trusted. A test replaces every delegate on `ArlecchinoStrings` —
found by reflection, so nothing can be forgotten — with a marker, draws the main screen, the keys
screen, the notification list and two modals, and fails if a single word of the framework's English
survives anywhere in those frames. A hardcoded literal shows up as a failing test the day it is
written, and a string added to `ArlecchinoStrings` but never documented fails a second test that
compares this page with the type itself.
