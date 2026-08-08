---
title: Modals
sidebar_label: Modals
description: Every dialog that ships — text, password, number, slider, toggle, choice, multi-choice, date, time, color, message and multi-line — plus stacking, validation and the mouse.
---

# Modals

`Modal` is an abstract class carrying only a `Title`; each kind of input is its own type. The
`Request*` helpers on [`ArlecchinoState`](state.md) cover the simple cases, and assigning
`ArlecchinoState.Modal` directly gives access to every property.

## The types

| Type | Input | Result |
|---|---|---|
| `TextModal` | A single line of text; optionally masked, or checked as an email or a link | `Action<string>` |
| `TextAreaModal` | Several lines, edited in place; `Enter` breaks the line | `Action<string>` |
| `MessageModal` | Nothing — something to read and dismiss | `Action?` on close |
| `NumberModal` | Digits, plus `↑↓` / `PgUp` `PgDn` stepping within `Minimum`..`Maximum` | `Action<decimal>` |
| `SliderModal` | A track adjusted with `←→`, no typing | `Action<decimal>` |
| `ToggleModal` | Yes / No | `Action<bool>` |
| `ChoiceModal` | One option out of a filterable list | `Action<string>` |
| `MultiChoiceModal` | Any number of options, marked with `Space` | `Action<IReadOnlyList<string>>` |
| `DateModal` | A `yyyy-mm-dd` field edited per segment | `Action<DateOnly>` |
| `TimeModal` | An `hh:mm` field edited per segment | `Action<TimeOnly>` |
| `ColorModal` | Hue / saturation / lightness sliders under a live swatch | `Action<Rgb>` |
| `CommandModal` | The [command palette](commands.md#the-command-palette), opened by the input router | — |
| Your own | [Whatever the application draws and reads for itself](#a-dialog-of-your-own) | Whatever it decides |

If you are coming from HTML inputs: `text`, `password`, `email` and `url` are all `TextModal`, `number`
is `NumberModal`, `range` is `SliderModal`, `checkbox` is `ToggleModal` for one flag and
`MultiChoiceModal` for a set, `radio` is `ChoiceModal`, `color` is `ColorModal`, `date` and `time` are
their own modals, and `file` is the [file picker](file-picker.md).

## The shape they share

Shared behavior lives in interfaces rather than being repeated per type:

| Interface | Members | Implemented by |
|---|---|---|
| `IAffixedModal` | `Prefix`, `Suffix` | `TextModal`, `NumericModal` |
| `ITextEntryModal` | `Text`, `Message`, `Masked`, `AcceptsCharacter(char)` | `TextModal`, `NumberModal` |
| `IBoundedModal` | `Minimum`, `Maximum`, `Step`, `LargeStep`, `Add(delta)` | `NumberModal`, `SliderModal` |

`NumericModal` is the abstract base of `NumberModal` and `SliderModal`, holding `Step`, `LargeStep`,
`Decimals`, the affixes and the `FormatNumber` / `Display` formatting. `OptionListModal` is the base of
`ChoiceModal` and `MultiChoiceModal`, holding `Options`, `Filter`, `Index` and `MatchingOptions()`.

The framework renders and routes keys through those interfaces, so a text entry behaves the same
whether it is free text or a number.

## Text

```csharp
private readonly ArlecchinoState _state;

_state.RequestText(
    title: "Rename",
    initial: current,
    validate: value => value.Length == 0 ? "name must not be empty" : null,
    onSubmit: value => Rename(value));
```

The validator runs on `Enter` and returns an error message or `null`. A non-null result keeps the modal
open and shows the message in `Error` style. `Esc` cancels without calling `onSubmit`.

Once a message is showing it follows the field: every edit re-runs the check, and the message goes away
the moment the input is actually valid instead of on the next keystroke. Nothing is reported before the
first `Enter`, so a field never complains about being half-typed. The same holds for the number field.

### Editing a line

Editing is a real line, not an append-only buffer. The caret is drawn as `▏` where the next character
will go, and every text field — including the number field — takes the same keys:

| Key | Does |
|---|---|
| `←` / `→` | Move the caret one symbol |
| `Ctrl+←` / `Ctrl+→` | Move it a word at a time |
| `Home` / `End` | Jump to either end |
| `Backspace` / `Delete` | Remove the symbol before or after the caret |
| `Ctrl+Backspace` | Remove the word before the caret |
| `Ctrl+U` | Remove everything before the caret |

All of these come from the [keymap](keyboard.md#the-keymap), so they are rebindable. `TextEditing`
holds the logic and works on any `ITextEntryModal`, which is what keeps the number field's editing
identical to the text field's.

A value longer than the terminal scrolls inside the field: the box never grows past the frame, `…`
marks the side that continues, and the caret is always on screen — it sits near the middle while the
text is scrolled and at the end once it is not. A masked field draws one dot per symbol, so the dots
line up with what backspace will remove.

Everything moves and deletes by **symbols**, not `char` values: an emoji, a family ZWJ sequence or a
letter with a combining mark goes in one press instead of leaving half a surrogate pair behind. The
boundary helpers are public — see [Text and width](text.md#walking) — for fields you write yourself.
Assigning `Text` puts the caret at the end, which is why stepping a number with `↑` leaves the caret
after the new value.

### Password, email and link

Three flavors of the same modal:

```csharp
_state.RequestPassword("Passphrase", value => Unlock(value));
_state.RequestEmail("Email", current, value => SetEmail(value));
_state.RequestUrl("Homepage", "https://", value => SetHomepage(value));
```

`Masked = true` renders every character as `•` — the text itself is still handed to `OnSubmit`
untouched. `Format` is `TextFormat.Free`, `Email` or `Url`, and the built-in check runs on `Enter`
**before** your own `Validate`, reporting `must be an email address` or `must be a http or https link`.
A link is accepted when it parses as an absolute `http` or `https` URI; an address needs exactly one
`@`, no spaces, and a dotted domain. Anything stricter belongs in `Validate`.

## Numbers

```csharp
_state.RequestNumber("Weight", initial: 1200, minimum: 0, maximum: 5000, value => SetWeight(value));
```

Or with everything spelled out:

```csharp
_state.Modal = new NumberModal
{
    Title = "Price",
    Text = "12.50",
    Minimum = 0,
    Maximum = 500,
    Step = 5,
    LargeStep = 50,
    Decimals = 2,
    Prefix = "$ ",
    Validate = value => value % 5 != 0 ? "must be a multiple of 5" : null,
    OnSubmit = value => SetPrice(value),
};
```

`↑` `↓` step by `Step`, `PgUp` `PgDn` by `LargeStep`, and both clamp to the bounds. Typing is restricted
to digits — a decimal separator is accepted only when `Decimals > 0`, a minus sign only when `Minimum`
is negative, and both `.` and `,` parse. `Enter` reports `must be a number` for unparsable input and
`must be between …` when out of bounds before `Validate` ever runs; the values in that message are
formatted with the affixes.

### Slider

```csharp
_state.Modal = new SliderModal
{
    Title = "Volume",
    Value = 60,
    Step = 5,
    Suffix = " %",
    OnSubmit = value => SetVolume(value),
};
```

Renders a 24-cell track with the value beside it. `←→` step, `PgUp` `PgDn` jump by `LargeStep`,
`Home` `End` go to the ends, `Enter` confirms. `Minimum` defaults to `0` and `Maximum` to `100`, so a
percentage needs neither.

## Toggle

```csharp
_state.RequestToggle("Fullscreen", current, value => SetFullscreen(value));
```

Two chips, the active one highlighted. `←→`, `Tab` and `Space` switch, `Enter` confirms, `Esc` cancels.
The labels come from `ArlecchinoStrings.Yes` / `No`.

## Choice

```csharp
_state.RequestChoice("Theme", ["dark", "light"], picked => Apply(picked), current: "dark");
```

`↑` `↓` move, `Enter` picks, `Esc` cancels. Typing filters the list case-insensitively and `Backspace`
shortens the filter; the selection resets to the top on every edit. `current` decides which option
starts selected. An empty result set renders as `nothing matches` and `Enter` does nothing.

The list scrolls when it does not fit: up to 12 rows, fewer on a short terminal, centered on the
selection.

### Multi-choice

```csharp
_state.RequestMultiChoice(
    "Columns",
    ["Name", "Date Modified", "Size", "Kind"],
    selected: ["Name", "Size"],
    picked => SetColumns(picked));
```

The same list, with `[×]` / `[ ]` in front of every row and the count in the title. `Space` marks the
row under the cursor, `Enter` confirms and hands back the marked options **in the order of `Options`**,
not in the order they were marked. Filtering works as in a choice modal, and marks survive a filter
change.

## Dates, times and colors

```csharp
_state.RequestDate("Release date", DateOnly.FromDateTime(DateTime.Today), value => SetDate(value));
_state.RequestTime("Start at", new TimeOnly(9, 41), value => SetTime(value));
```

Both are segment editors: `yyyy-mm-dd` and `hh:mm`, with the segment under the cursor highlighted.
`←→` (or `Tab`) move between segments, `↑↓` change the one under the cursor — a month step keeps the
day valid, an hour step wraps around midnight — and typing digits fills the segment left to right,
jumping to the next one when it is full. `Backspace` discards a half-typed segment, `Enter` commits
whatever is typed and submits, `Esc` cancels.

`DateModal` also takes `Minimum` and `Maximum`; stepping and typing both clamp to that window.

:::tip[A segment editor of your own]

Both derive from `SegmentedModal`. A fixed-width field of your own — a version number, an id — needs
`SegmentCount`, `Separator`, `SegmentTexts()`, `SegmentLength` and `ApplyTypedValue`, and gets the
cursor, typing and rendering behavior for free.

:::

### Color

```csharp
_state.RequestColor("Accent color", new Rgb(63, 169, 245), value => SetAccent(value.Hex));
```

A swatch of the current color with its hex code, and under it three sliders — hue `0..359`, saturation
and lightness `0..100`. `↑↓` (or `Tab`) pick the channel, `←→` adjust by `Step`, `PgUp` `PgDn` by
`LargeStep`, `Home` `End` go to the ends of the channel, hue wraps around. `Enter` hands back an `Rgb`.

The swatch is drawn with a 24-bit ANSI color, so it shows the actual color rather than the nearest of
the sixteen — see [Colors](colors.md). Channels are integers, so a color that arrives from `Rgb` and
comes back out may shift by a unit or two through the HSL round trip.

## Message and confirmation

Two dialogs that take no value. `RequestMessage` is something to read — a result, a warning, an
explanation of what failed — wrapped to half the frame and dismissed with either closing key:

```csharp
_state.RequestMessage("Saved", "The profile was written to disk.");
```

`RequestConfirmation` asks before something happens, with **No** selected to begin with, so a stray
`Enter` cancels rather than deletes. The callback runs only on yes:

```csharp
_state.RequestConfirmation("Delete the profile?", () => Delete(profile));
```

## Several lines of text

```csharp
_state.RequestTextArea(
    "Release notes",
    current,
    text => Save(text),
    validate: static text => text.Length < 10 ? "at least ten characters" : null,
    visibleRows: 12);
```

`Enter` starts a new line here, which is why confirming is a key of its own: the `Submit` binding,
`Ctrl+Enter` by default. `Esc` still cancels.

The caret is a row plus a position inside it, and everything moves and deletes by symbols rather than
`char` values. `Backspace` at the start of a line joins it onto the one above, `Delete` at the end pulls
the next one up, the arrows walk across line ends, `PgUp`/`PgDn` jump a page and `Home`/`End` go to the
ends of the line. The text scrolls to keep the caret in view, a long line shifts sideways for the same
reason, and a pasted block keeps its line breaks. The validator runs on submit and its message is drawn
under the text while the dialog stays open.

## Stacking

`CloseModal()` drops the modal on top. The framework already closes it on submit, pick and cancel, so
calling it yourself is only needed to dismiss one from the outside.

`PushModal(modal)` opens one over whatever is already open — the usual case is a callback that has to
ask a follow-up question:

```csharp
_state.RequestText("Name", current, null, name =>
    _state.PushModal(new ToggleModal
    {
        Title = $"Save {name}?",
        Value = true,
        OnSubmit = save => { if (save) Rename(name); },
    }));
```

Keys only ever reach the top one, and closing it uncovers the one underneath with its text intact.
Assigning `State.Modal` still replaces the whole stack, and `CloseAllModals()` clears it. Every open
modal is drawn, each offset a row down and three columns right of the one below, so the stack is
visible rather than hidden behind the top box. `State.Modals` is the stack itself, bottom first.

Modals draw last, on top of the view, and suppress the hints box while open. All chrome text — hints,
`nothing matches`, `Yes` / `No`, the filter prefix, the validation messages — comes from
[`ArlecchinoStrings`](localization.md).

## A dialog of your own

The dialogs above know what a number looks like and what a choice looks like. An application with a
look of its own wants neither, and the answer is not a second slot beside `Modal` — two things that
both take every key will disagree about which of them has it. Derive from `Modal` instead: the kinds
the framework brings are nothing more than its first few subclasses, and one you write is the next.

```csharp
public sealed class ConfirmModal : Modal
{
    public required string Question { get; init; }

    public required Action<bool> OnAnswer { get; init; }

    public override void Draw(ModalFrame frame)
    {
        var box = frame.Screen.Rows(frame.Height / 3, 3);

        box.Fill(Theme.Selected);
        box.WriteLine(0, Question, Theme.Header, Align.Center);
        box.WriteLine(2, "y / n", Theme.Muted, Align.Center);

        Box = box;
    }

    public override void Handle(ModalFrame frame, KeyPress key)
    {
        if (key.Key is not (ConsoleKey.Y or ConsoleKey.N))
        {
            return;
        }

        frame.Close();
        OnAnswer(key.Key == ConsoleKey.Y);
    }
}

state.Modal = new ConfirmModal { Title = "Careful", Question = "Delete it?", OnAnswer = Delete };
```

`Handle` gets every key while it is on top — including `Esc`, so closing is the dialog's job — and
`HandleMouse` is there to override when clicks matter. Everything else — where it sits in the stack,
that it draws last, that the view behind it keeps running — is the framework's, exactly as for the
dialogs that ship.

### What a dialog is handed

A dialog is a value: an application writes `new ConfirmModal { … }` and hands it over, so there is no
constructor to give it a keymap or a clipboard. It is given them when it is asked to do something,
through `ModalFrame`:

| Member | What it is for |
|---|---|
| `Screen`, `Width`, `Height` | The whole frame, so the dialog decides its own size and place |
| `Strings`, `Keymap`, `Keys` | The [words](localization.md) and the [keys](keyboard.md) this application was started with |
| `Close()` | Closes the dialog on top, which while it is being handled is this one |
| `Copy(text)` | Puts text on the clipboard |
| `Centered(width, height)` | A box of that size in the middle, never off the edge |
| `Box(title, body, footer)` | The titled box every dialog that ships is drawn through |
| `Depth` | How many dialogs are already open underneath |

Set `Box` to whatever region you drew in: it is what tells a click on the dialog from a click outside.
Drawing through `frame.Box` sets it for you and gives the same border, the same title in the top edge
and the same hints under a rule as the rest — which is what makes a dialog you wrote read as part of
the same application:

```csharp
public override void Draw(ModalFrame frame) =>
    (Box, _) = frame.Box(
        Title,
        [[new Piece(Question, Theme.Default)]],
        "y confirms · n cancels");
```

## Mouse

Once [mouse reporting](mouse.md) is on, the modals answer it themselves:

| Modal | Click | Wheel |
|---|---|---|
| Choice | Selects the row; a second click on the selected row picks it | Moves the selection |
| Multi-choice | Selects the row; a second click marks or unmarks it | Moves the selection |
| Slider | Jumps to the position on the track, dragging keeps updating | — |
| Toggle | Picks the chip under the cursor | — |
| Color | Selects the channel and sets it from the position on its track | — |
| Command palette | Runs the command on that row | — |

Clicks outside the box change nothing — a modal is not dismissed by clicking away, because a stray
click should not discard what was typed. Each modal remembers where it was drawn (`Box`, `Rows`,
`Track`, the chips and channel tracks), so hit-testing is comparing coordinates rather than guessing.

The file picker follows the same rule: a click selects a row, a second click opens it, a click in the
places sidebar goes to that place and moves focus there, and the wheel scrolls the list.
