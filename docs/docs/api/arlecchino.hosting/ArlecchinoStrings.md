---
title: "ArlecchinoStrings"
sidebar_label: "ArlecchinoStrings"
---

# ArlecchinoStrings class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Every piece of text the framework itself draws, as delegates with English defaults. They are called on the frames that need them, so pointing them elsewhere switches language with nothing to rebuild.

```csharp
public sealed class ArlecchinoStrings
```

## Constructors

| Member | Summary |
|---|---|
| [`ArlecchinoStrings()`](#arlecchinostrings) |  |

## Properties

| Member | Summary |
|---|---|
| [`ColorHue`](#colorhue) | Label of the hue channel. |
| [`ColorLightness`](#colorlightness) | Label of the lightness channel. |
| [`ColorSaturation`](#colorsaturation) | Label of the saturation channel. |
| [`CommandPaletteTitle`](#commandpalettetitle) | Title of the command palette. |
| [`CommandUnknown`](#commandunknown) | Shown when a key pressed in the palette belongs to no command. |
| [`Empty`](#empty) | Stands in for a value that has not been set, in forms and fields. |
| [`FilePicker`](#filepicker) | Text of the file picker, which has enough of its own to be grouped separately. |
| [`Filter`](#filter) | What the filter line above a list is called. What has been typed is drawn after it by the list itself, since it carries a caret and a selection of its own. |
| [`FormEdit`](#formedit) | Hint for opening the field under the cursor. |
| [`FormMove`](#formmove) | Hint for moving between form fields. |
| [`FormReset`](#formreset) | Hint for clearing the field under the cursor. |
| [`HelpClose`](#helpclose) | Line under the title, saying how to leave. |
| [`HelpCommandsSection`](#helpcommandssection) | Heading over the application's own commands. |
| [`HelpFrameworkSection`](#helpframeworksection) | Heading over the keys the framework itself answers to. |
| [`HelpKeys`](#helpkeys) | What each key the framework answers to does, in the order the screen lists them. |
| [`HelpNoCommands`](#helpnocommands) | Shown in place of the command list when the application registered none. |
| [`HelpScreenSection`](#helpscreensection) | Heading over the commands of the screen the help was opened from, given its route. |
| [`HelpTitle`](#helptitle) | Title of the screen listing every key. |
| [`HintCommands`](#hintcommands) | What the hints box calls the command palette. The framework adds the line itself, beside the key that opens it, whenever there is at least one command to show. |
| [`KeysTitle`](#keystitle) | Title of the hints box. |
| [`ListPosition`](#listposition) | Where the cursor is in a list that does not fit on screen. The position is one-based, since it is read by a person rather than an index. |
| [`LogEmpty`](#logempty) | Shown in the log overlay while nothing has been logged. |
| [`LogHints`](#loghints) | The key line under the log overlay. |
| [`LogTitle`](#logtitle) | Title of the log overlay, with how many lines are held. |
| [`LogWithoutProviders`](#logwithoutproviders) | Shown in the log overlay instead, when the host has no logging provider at all. The overlay shows what a provider writes to the console, so without one there is nothing for it to ever show. |
| [`ModalChoiceHints`](#modalchoicehints) | Footer of a single-choice list. |
| [`ModalColorHints`](#modalcolorhints) | Footer of the color picker. |
| [`ModalCommandHints`](#modalcommandhints) | Footer of the command palette. |
| [`ModalDateHints`](#modaldatehints) | Footer of a date field. |
| [`ModalMessageHints`](#modalmessagehints) | The key line under a dialog that only has something to say. |
| [`ModalMultiChoiceHints`](#modalmultichoicehints) | Footer of a multi-choice list. |
| [`ModalNotificationHints`](#modalnotificationhints) | The key line under an opened notification that has something to do about it. |
| [`ModalNumberHints`](#modalnumberhints) | Footer of a number field. |
| [`ModalSliderHints`](#modalsliderhints) | Footer of a slider. |
| [`ModalTextAreaHints`](#modaltextareahints) | The key line under the multi-line text dialog. |
| [`ModalTextHints`](#modaltexthints) | Footer of a text field. |
| [`ModalTimeHints`](#modaltimehints) | Footer of a time field. |
| [`ModalToggleHints`](#modaltogglehints) | Footer of a yes/no dialog. |
| [`No`](#no) | The negative chip of a yes/no dialog. |
| [`NotANumber`](#notanumber) | Shown when a number field holds something that will not parse. |
| [`NotAUrl`](#notaurl) | Shown when a link field holds something that is not an http address. |
| [`NotAnEmail`](#notanemail) | Shown when an email field holds something that is not an address. |
| [`NothingMatches`](#nothingmatches) | Shown in place of a list when the filter matches nothing. |
| [`NotificationsClear`](#notificationsclear) | Hint for throwing the list away. |
| [`NotificationsClose`](#notificationsclose) | Hint for leaving the screen. |
| [`NotificationsCount`](#notificationscount) | Line under that title, saying how many are held. |
| [`NotificationsEmpty`](#notificationsempty) | Shown when nothing has been said lately. |
| [`NotificationsOpen`](#notificationsopen) | Hint for reading one notification in full. |
| [`NotificationsTitle`](#notificationstitle) | Title of the notifications screen. |
| [`OutOfRange`](#outofrange) | Shown when a number is outside its bounds. The values arrive already formatted, affixes included. |
| [`SelectedCount`](#selectedcount) | How many options are marked, shown in the title of a multi-choice list. |
| [`TerminalMinimum`](#terminalminimum) | Introduces the required size in that notice. |
| [`TerminalSize`](#terminalsize) | Formats a window size, used for both the current and the required one. |
| [`TerminalTooSmall`](#terminaltoosmall) | Headline of the notice that replaces the view in a too-small window. |
| [`ViewFailed`](#viewfailed) | Shown on the output line when a view or a callback throws. The application keeps running, so this is what the user sees instead of a crash. |
| [`Yes`](#yes) | The affirmative chip of a yes/no dialog. |

## Constructors in detail

### `ArlecchinoStrings()` {#arlecchinostrings}

```csharp
public ArlecchinoStrings();
```

## Properties in detail

### `ColorHue` {#colorhue}

```csharp
public Func<string> ColorHue { get; set; }
```

Label of the hue channel.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ColorLightness` {#colorlightness}

```csharp
public Func<string> ColorLightness { get; set; }
```

Label of the lightness channel.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ColorSaturation` {#colorsaturation}

```csharp
public Func<string> ColorSaturation { get; set; }
```

Label of the saturation channel.

**Type** `Func<TResult>`&lt;`string`&gt;

### `CommandPaletteTitle` {#commandpalettetitle}

```csharp
public Func<string> CommandPaletteTitle { get; set; }
```

Title of the command palette.

**Type** `Func<TResult>`&lt;`string`&gt;

### `CommandUnknown` {#commandunknown}

```csharp
public Func<string, string> CommandUnknown { get; set; }
```

Shown when a key pressed in the palette belongs to no command.

**Type** `Func<T, TResult>`&lt;`string`, `string`&gt;

### `Empty` {#empty}

```csharp
public Func<string> Empty { get; set; }
```

Stands in for a value that has not been set, in forms and fields.

**Type** `Func<TResult>`&lt;`string`&gt;

### `FilePicker` {#filepicker}

```csharp
public ArlecchinoStrings+FilePickerStrings FilePicker { get; set; }
```

Text of the file picker, which has enough of its own to be grouped separately.

**Type** [`FilePickerStrings`](../arlecchino.hosting/FilePickerStrings.md)

### `Filter` {#filter}

```csharp
public Func<string> Filter { get; set; }
```

What the filter line above a list is called. What has been typed is drawn after it by the list itself, since it carries a caret and a selection of its own.

**Type** `Func<TResult>`&lt;`string`&gt;

### `FormEdit` {#formedit}

```csharp
public Func<string> FormEdit { get; set; }
```

Hint for opening the field under the cursor.

**Type** `Func<TResult>`&lt;`string`&gt;

### `FormMove` {#formmove}

```csharp
public Func<string> FormMove { get; set; }
```

Hint for moving between form fields.

**Type** `Func<TResult>`&lt;`string`&gt;

### `FormReset` {#formreset}

```csharp
public Func<string> FormReset { get; set; }
```

Hint for clearing the field under the cursor.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HelpClose` {#helpclose}

```csharp
public Func<string> HelpClose { get; set; }
```

Line under the title, saying how to leave.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HelpCommandsSection` {#helpcommandssection}

```csharp
public Func<string> HelpCommandsSection { get; set; }
```

Heading over the application's own commands.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HelpFrameworkSection` {#helpframeworksection}

```csharp
public Func<string> HelpFrameworkSection { get; set; }
```

Heading over the keys the framework itself answers to.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HelpKeys` {#helpkeys}

```csharp
public Func<ArlecchinoKeymap, IReadOnlyList<ValueTuple<KeyBinding, string>>> HelpKeys { get; set; }
```

What each key the framework answers to does, in the order the screen lists them.

**Type** `Func<T, TResult>`&lt;[`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md), `IReadOnlyList<T>`&lt;`ValueTuple<T1, T2>`&lt;[`KeyBinding`](../arlecchino.input/KeyBinding.md), `string`&gt;&gt;&gt;

### `HelpNoCommands` {#helpnocommands}

```csharp
public Func<string> HelpNoCommands { get; set; }
```

Shown in place of the command list when the application registered none.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HelpScreenSection` {#helpscreensection}

```csharp
public Func<string, string> HelpScreenSection { get; set; }
```

Heading over the commands of the screen the help was opened from, given its route.

**Type** `Func<T, TResult>`&lt;`string`, `string`&gt;

### `HelpTitle` {#helptitle}

```csharp
public Func<string> HelpTitle { get; set; }
```

Title of the screen listing every key.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintCommands` {#hintcommands}

```csharp
public Func<string> HintCommands { get; set; }
```

What the hints box calls the command palette. The framework adds the line itself, beside the key that opens it, whenever there is at least one command to show.

**Type** `Func<TResult>`&lt;`string`&gt;

### `KeysTitle` {#keystitle}

```csharp
public Func<string> KeysTitle { get; set; }
```

Title of the hints box.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ListPosition` {#listposition}

```csharp
public Func<int, int, string> ListPosition { get; set; }
```

Where the cursor is in a list that does not fit on screen. The position is one-based, since it is read by a person rather than an index.

**Type** `Func<T1, T2, TResult>`&lt;`int`, `int`, `string`&gt;

### `LogEmpty` {#logempty}

```csharp
public Func<string> LogEmpty { get; set; }
```

Shown in the log overlay while nothing has been logged.

**Type** `Func<TResult>`&lt;`string`&gt;

### `LogHints` {#loghints}

```csharp
public Func<string> LogHints { get; set; }
```

The key line under the log overlay.

**Type** `Func<TResult>`&lt;`string`&gt;

### `LogTitle` {#logtitle}

```csharp
public Func<int, string> LogTitle { get; set; }
```

Title of the log overlay, with how many lines are held.

**Type** `Func<T, TResult>`&lt;`int`, `string`&gt;

### `LogWithoutProviders` {#logwithoutproviders}

```csharp
public Func<string> LogWithoutProviders { get; set; }
```

Shown in the log overlay instead, when the host has no logging provider at all. The overlay shows what a provider writes to the console, so without one there is nothing for it to ever show.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalChoiceHints` {#modalchoicehints}

```csharp
public Func<string> ModalChoiceHints { get; set; }
```

Footer of a single-choice list.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalColorHints` {#modalcolorhints}

```csharp
public Func<string> ModalColorHints { get; set; }
```

Footer of the color picker.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalCommandHints` {#modalcommandhints}

```csharp
public Func<string> ModalCommandHints { get; set; }
```

Footer of the command palette.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalDateHints` {#modaldatehints}

```csharp
public Func<string> ModalDateHints { get; set; }
```

Footer of a date field.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalMessageHints` {#modalmessagehints}

```csharp
public Func<string> ModalMessageHints { get; set; }
```

The key line under a dialog that only has something to say.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalMultiChoiceHints` {#modalmultichoicehints}

```csharp
public Func<string> ModalMultiChoiceHints { get; set; }
```

Footer of a multi-choice list.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalNotificationHints` {#modalnotificationhints}

```csharp
public Func<string> ModalNotificationHints { get; set; }
```

The key line under an opened notification that has something to do about it.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalNumberHints` {#modalnumberhints}

```csharp
public Func<string> ModalNumberHints { get; set; }
```

Footer of a number field.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalSliderHints` {#modalsliderhints}

```csharp
public Func<string> ModalSliderHints { get; set; }
```

Footer of a slider.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalTextAreaHints` {#modaltextareahints}

```csharp
public Func<string> ModalTextAreaHints { get; set; }
```

The key line under the multi-line text dialog.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalTextHints` {#modaltexthints}

```csharp
public Func<string> ModalTextHints { get; set; }
```

Footer of a text field.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalTimeHints` {#modaltimehints}

```csharp
public Func<string> ModalTimeHints { get; set; }
```

Footer of a time field.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ModalToggleHints` {#modaltogglehints}

```csharp
public Func<string> ModalToggleHints { get; set; }
```

Footer of a yes/no dialog.

**Type** `Func<TResult>`&lt;`string`&gt;

### `No` {#no}

```csharp
public Func<string> No { get; set; }
```

The negative chip of a yes/no dialog.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NotANumber` {#notanumber}

```csharp
public Func<string> NotANumber { get; set; }
```

Shown when a number field holds something that will not parse.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NotAUrl` {#notaurl}

```csharp
public Func<string> NotAUrl { get; set; }
```

Shown when a link field holds something that is not an http address.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NotAnEmail` {#notanemail}

```csharp
public Func<string> NotAnEmail { get; set; }
```

Shown when an email field holds something that is not an address.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NothingMatches` {#nothingmatches}

```csharp
public Func<string> NothingMatches { get; set; }
```

Shown in place of a list when the filter matches nothing.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NotificationsClear` {#notificationsclear}

```csharp
public Func<string> NotificationsClear { get; set; }
```

Hint for throwing the list away.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NotificationsClose` {#notificationsclose}

```csharp
public Func<string> NotificationsClose { get; set; }
```

Hint for leaving the screen.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NotificationsCount` {#notificationscount}

```csharp
public Func<int, string> NotificationsCount { get; set; }
```

Line under that title, saying how many are held.

**Type** `Func<T, TResult>`&lt;`int`, `string`&gt;

### `NotificationsEmpty` {#notificationsempty}

```csharp
public Func<string> NotificationsEmpty { get; set; }
```

Shown when nothing has been said lately.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NotificationsOpen` {#notificationsopen}

```csharp
public Func<string> NotificationsOpen { get; set; }
```

Hint for reading one notification in full.

**Type** `Func<TResult>`&lt;`string`&gt;

### `NotificationsTitle` {#notificationstitle}

```csharp
public Func<string> NotificationsTitle { get; set; }
```

Title of the notifications screen.

**Type** `Func<TResult>`&lt;`string`&gt;

### `OutOfRange` {#outofrange}

```csharp
public Func<string, string, string> OutOfRange { get; set; }
```

Shown when a number is outside its bounds. The values arrive already formatted, affixes included.

**Type** `Func<T1, T2, TResult>`&lt;`string`, `string`, `string`&gt;

### `SelectedCount` {#selectedcount}

```csharp
public Func<int, string> SelectedCount { get; set; }
```

How many options are marked, shown in the title of a multi-choice list.

**Type** `Func<T, TResult>`&lt;`int`, `string`&gt;

### `TerminalMinimum` {#terminalminimum}

```csharp
public Func<string> TerminalMinimum { get; set; }
```

Introduces the required size in that notice.

**Type** `Func<TResult>`&lt;`string`&gt;

### `TerminalSize` {#terminalsize}

```csharp
public Func<int, int, string> TerminalSize { get; set; }
```

Formats a window size, used for both the current and the required one.

**Type** `Func<T1, T2, TResult>`&lt;`int`, `int`, `string`&gt;

### `TerminalTooSmall` {#terminaltoosmall}

```csharp
public Func<string> TerminalTooSmall { get; set; }
```

Headline of the notice that replaces the view in a too-small window.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ViewFailed` {#viewfailed}

```csharp
public Func<string, string> ViewFailed { get; set; }
```

Shown on the output line when a view or a callback throws. The application keeps running, so this is what the user sees instead of a crash.

**Type** `Func<T, TResult>`&lt;`string`, `string`&gt;

### `Yes` {#yes}

```csharp
public Func<string> Yes { get; set; }
```

The affirmative chip of a yes/no dialog.

**Type** `Func<TResult>`&lt;`string`&gt;

