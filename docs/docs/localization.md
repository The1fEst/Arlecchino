---
title: Localization
sidebar_label: Localization
description: The localization generator that gives every string a name, ArlecchinoStrings for the framework's own chrome, and why no user-visible text is hardcoded.
---

# Localization

There are two halves to this. The application's own text gets a name from the
[localization generator](#text-with-a-name); the framework's chrome is translated through
[`ArlecchinoStrings`](#the-frameworks-own-words). Both exist for the same reason, which is that a
sentence typed twice is a sentence that will one day disagree with itself.

## Text with a name

Text written in the place it is drawn gets written twice — the same sentence in a dialog and in the log
line that follows it — and the day one of them is reworded the two quietly disagree. Put the text in a
TOML file instead, and a generator turns it into a name the compiler checks. Translation comes free of
the same machinery, but one language is reason enough to do it.

```toml
# Localization/Localization.toml
[localization]
language = "en"

[strings]
Copy = "Copy"
CopyManyTitle = "Copy {0} items"
Overwrite = "{0} already exists"
```

Hand the folder to the compiler, and nothing else:

```xml
<ItemGroup>
  <AdditionalFiles Include="Localization\*.toml" />
</ItemGroup>
```

What comes out is an enum with one name per entry and a static `Localization` class that resolves it.
A `using static` at the top of a file is all a call site needs:

```csharp
using static MyApp.Localization;

Title = sources.Count == 1 ? Loc(LocString.Copy) : Loc(LocString.CopyManyTitle, sources.Count);
```

`Loc(key)` is the text; `Loc(key, arguments)` fills in its `{0}` placeholders through
`string.Format`. Each entry carries the default text in its XML doc, so hovering `LocString.Copy` in an
editor shows what it says without opening the file.

### Translations

Every other TOML file in the folder is a translation of the default, named by its own `language`:

```toml
# Localization/Localization.ru.toml
[localization]
language = "ru"

[strings]
Copy = "Копировать"
CopyManyTitle = "Копировать {0} объектов"
```

`Localization.Language` decides which is drawn and starts at the closest match to the machine's own
`CurrentUICulture` — `ru-RU` finds `ru` — falling back to the default when there is nothing near. Set
it at runtime and the next frame is in the new language; nothing is rebuilt, because resolving is a
`switch` over a closed set rather than a dictionary anyone has to reload.

| The generator says | When |
|---|---|
| `ARL021` error | A file could not be read |
| `ARL022` error | No file claims to be the default |
| `ARL023` error | A translation has a string the default does not, so nothing would ever ask for it |
| `ARL024` info | A translation is missing a string, and the default is drawn there instead |

A missing string is information rather than an error on purpose: a half-finished translation should
show English where it has nothing to say, not stop the build or leave a hole on the screen.

### Naming a key

A [view command](commands.md#commands-of-a-view) takes a `Func<string>` for its label, because the
label is read every frame — that is what lets changing language change the screen. It cannot take a
`LocString`: there is no such type until an application is compiled, since the enum is written out of
that application's own file. So the generator writes the shorthand beside the enum, where both are in
scope:

```csharp
public IReadOnlyList<ViewCommand> Commands() =>
[
    Bind.To(new(ConsoleKey.F5), LocString.Copy, _files.Copy),
    Bind.Going(new(ConsoleKey.F3), LocString.View, _files.Read),
    Bind.When(new(ConsoleKey.Escape, ConsoleModifiers.Alt), LocString.Stop,
        () => _work.IsBusy, _work.Cancel),
];
```

`To` stays on the screen, `Going` returns a route, and `When` adds an `IsEnabled`. All three are the
same `Func<string>` underneath.

### Where it lands

The enum, the resolver and `Bind` go in `RootNamespace`, or `Localization` when that is empty.
`ArlecchinoLocalizationFolder` moves the folder it reads (`Localization` by default) and
`ArlecchinoLocalizationLanguage` says which language is the default (`en`).

## The framework's own words

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
| `HintCommands` | `commands` — the palette line the hints box adds by itself |
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
