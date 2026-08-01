---
title: ArlecchinoState
sidebar_label: ArlecchinoState
---

# ArlecchinoState class

**Namespace:** `Arlecchino.State` &middot; **Assembly:** `Arlecchino`

State that outlives a single screen: the output line, the dialog that is open, and a pending file picker request. Derive from it to hang application state that every screen reads. A frame reads all of it, so all of it is written on the drawing thread — the `Request…` methods included, since each of them opens a dialog. Anything arriving on a timer, a task or a socket hands the change over with [`FrameThread.Post`](../arlecchino/FrameThread.md#post-action), which runs it just before the next frame; only [`ArlecchinoState.Invalidate`](../arlecchino.state/ArlecchinoState.md#invalidate) may be called from anywhere. The stack of dialogs is a [`LocalAtomsList`](../arlecchino.atoms/LocalAtomsList-1.md), so opening or closing one asks for a frame by itself. It is outside the undo history: stepping back through what was typed should not reopen a dialog that was answered.

```csharp
public class ArlecchinoState
```

## Constructors

| Member | Summary |
|---|---|
| [`ArlecchinoState(Repaint, Notifications)`](#arlecchinostate-repaint-notifications) | Creates the state. |

## Properties

| Member | Summary |
|---|---|
| [`FilePicker`](#filepicker) | What the file picker should show. Fill it in, then navigate to `Routes.FilePicker`; it is cleared when the picker finishes either way. Written on the drawing thread, as [`ArlecchinoState.Modal`](../arlecchino.state/ArlecchinoState.md#modal) is. |
| [`Modal`](#modal) | The dialog on top, or `null` when none is open. It takes every key while it is there. Assigning replaces whatever was open, however deep it was stacked; use [`ArlecchinoState.PushModal`](../arlecchino.state/ArlecchinoState.md#pushmodal-modal) to open one over another instead. Opened on the drawing thread: a dialog that appeared halfway through a frame would be drawn into a surface that has already been measured without it. Hand it over with [`FrameThread.Post`](../arlecchino/FrameThread.md#post-action) from anywhere else. |
| [`Modals`](#modals) | Every open dialog, bottom first. Drawing goes through this so the ones underneath stay visible behind the top one. A live view of the stack rather than a copy, and read-only all the way down: a widget handed it once draws whatever is open on every later frame, and there is no cast that gets a caller back to the list underneath. |
| [`Notifications`](#notifications) | What the application has said lately, and the screen behind the output row. |
| [`Output`](#output) | The status line at the bottom of the frame. Writing to it raises a notification, so the line clears itself after `ArlecchinoOptions.NotificationTimeout` and the message stays readable afterwards on the notifications screen. An empty string clears the row at once. |
| [`PickerLastFolder`](#pickerlastfolder) | Folder the picker ended in. Pass it as the next starting path to resume where the user left off. Written on the drawing thread, as [`ArlecchinoState.Modal`](../arlecchino.state/ArlecchinoState.md#modal) is. |

## Methods

| Member | Summary |
|---|---|
| [`CloseAllModals()`](#closeallmodals) | Closes every open dialog at once, however deep they are stacked. |
| [`CloseModal()`](#closemodal) | Closes the dialog on top, uncovering whatever it was opened over. Submitting, picking and cancelling already do this, so it is only needed to dismiss one from the outside. |
| [`Invalidate()`](#invalidate) | Asks for a repaint. Needed only for changes the framework cannot see — a field mutated from outside, or data that arrived on a timer. |
| [`PushModal(Modal)`](#pushmodal-modal) | Opens a dialog over whatever is already open, which is how a callback asks a follow-up question without losing what the user was in the middle of. Closing it uncovers the one underneath. |
| [`RequestChoice(string, IReadOnlyList<string>, Action<string>, string)`](#requestchoice-string-ireadonlylist-string-action-string-string) | Asks for one option out of a list that can be filtered by typing. |
| [`RequestColor(string, Rgb, Action<Rgb>)`](#requestcolor-string-rgb-action-rgb) | Asks for a colour with a swatch and three sliders. Channels are whole numbers, so a colour that goes in can come back shifted by a unit or two. |
| [`RequestConfirmation(string, Action)`](#requestconfirmation-string-action) | Asks a question that has to be confirmed before something happens. The negative answer starts selected, so a stray `Enter` cancels rather than deletes. |
| [`RequestDate(string, DateOnly, Action<DateOnly>)`](#requestdate-string-dateonly-action-dateonly) | Asks for a date, edited one segment at a time. |
| [`RequestEmail(string, string, Action<string>)`](#requestemail-string-string-action-string) | Asks for an email address, checked before the dialog will close. |
| [`RequestMessage(string, string, Action)`](#requestmessage-string-string-action) | Shows a message with nothing to fill in; any of the closing keys dismisses it. |
| [`RequestMultiChoice(string, IReadOnlyList<string>, IReadOnlyList<string>, Action<IReadOnlyList<string>>)`](#requestmultichoice-string-ireadonlylist-string-ireadonlylist-string-action-ireadonlylist-string) | Asks for any number of options. Marks survive filtering, and the result comes back in the order of `options` rather than the order they were marked. |
| [`RequestNumber(string, decimal, decimal, decimal, Action<decimal>)`](#requestnumber-string-decimal-decimal-decimal-action-decimal) | Asks for a number within bounds. Typing is restricted to digits, and stepping keys clamp to the range. |
| [`RequestPassword(string, Action<string>)`](#requestpassword-string-action-string) | Asks for a secret. The field shows dots, but the text handed to the callback is untouched. |
| [`RequestSlider(string, decimal, decimal, decimal, Action<decimal>)`](#requestslider-string-decimal-decimal-decimal-action-decimal) | Asks for a number on a track, adjusted with the arrows rather than typed. |
| [`RequestText(string, string, Func<string, string>, Action<string>)`](#requesttext-string-string-func-string-string-action-string) | Asks for a line of text. |
| [`RequestTextArea(string, string, Action<string>, Func<string, string>, int)`](#requesttextarea-string-string-action-string-func-string-string-int) | Asks for several lines of text. `Enter` starts a new line, so the text is confirmed with the `Submit` key — `Ctrl+Enter` unless the keymap says otherwise. |
| [`RequestTime(string, TimeOnly, Action<TimeOnly>)`](#requesttime-string-timeonly-action-timeonly) | Asks for a time of day, edited one segment at a time. |
| [`RequestToggle(string, bool, Action<bool>)`](#requesttoggle-string-bool-action-bool) | Asks a yes-or-no question. |
| [`RequestUrl(string, string, Action<string>)`](#requesturl-string-string-action-string) | Asks for an http or https link, checked before the dialog will close. |

## Constructors in detail

### `ArlecchinoState(Repaint, Notifications)` {#arlecchinostate-repaint-notifications}

```csharp
public ArlecchinoState(Repaint repaint, Notifications notifications);
```

Creates the state.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `repaint` | [`Repaint`](../arlecchino/Repaint.md) | Signal raised whenever any of this changes. |
| `notifications` | [`Notifications`](../arlecchino.diagnostics/Notifications.md) | Holds the output row and the notifications screen behind it. |

## Properties in detail

### `FilePicker` {#filepicker}

```csharp
public FilePickerRequest? FilePicker { get; set; }
```

What the file picker should show. Fill it in, then navigate to `Routes.FilePicker`; it is cleared when the picker finishes either way. Written on the drawing thread, as [`ArlecchinoState.Modal`](../arlecchino.state/ArlecchinoState.md#modal) is.

**Type** [`FilePickerRequest`](../arlecchino.state/FilePickerRequest.md)

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called from off the drawing thread. |

### `Modal` {#modal}

```csharp
public Modal? Modal { get; set; }
```

The dialog on top, or `null` when none is open. It takes every key while it is there. Assigning replaces whatever was open, however deep it was stacked; use [`ArlecchinoState.PushModal`](../arlecchino.state/ArlecchinoState.md#pushmodal-modal) to open one over another instead. Opened on the drawing thread: a dialog that appeared halfway through a frame would be drawn into a surface that has already been measured without it. Hand it over with [`FrameThread.Post`](../arlecchino/FrameThread.md#post-action) from anywhere else.

**Type** [`Modal`](../arlecchino.modals/Modal.md)

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called from off the drawing thread. |

### `Modals` {#modals}

```csharp
public IReadOnlyList<Modal> Modals { get; }
```

Every open dialog, bottom first. Drawing goes through this so the ones underneath stay visible behind the top one. A live view of the stack rather than a copy, and read-only all the way down: a widget handed it once draws whatever is open on every later frame, and there is no cast that gets a caller back to the list underneath.

**Type** `IReadOnlyList<T>`&lt;[`Modal`](../arlecchino.modals/Modal.md)&gt;

### `Notifications` {#notifications}

```csharp
public Notifications Notifications { get; }
```

What the application has said lately, and the screen behind the output row.

**Type** [`Notifications`](../arlecchino.diagnostics/Notifications.md)

### `Output` {#output}

```csharp
public string Output { get; set; }
```

The status line at the bottom of the frame. Writing to it raises a notification, so the line clears itself after `ArlecchinoOptions.NotificationTimeout` and the message stays readable afterwards on the notifications screen. An empty string clears the row at once.

**Type** `string`

### `PickerLastFolder` {#pickerlastfolder}

```csharp
public string PickerLastFolder { get; set; }
```

Folder the picker ended in. Pass it as the next starting path to resume where the user left off. Written on the drawing thread, as [`ArlecchinoState.Modal`](../arlecchino.state/ArlecchinoState.md#modal) is.

**Type** `string`

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called from off the drawing thread. |

## Methods in detail

### `CloseAllModals()` {#closeallmodals}

```csharp
public void CloseAllModals();
```

Closes every open dialog at once, however deep they are stacked.

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called from off the drawing thread. |

### `CloseModal()` {#closemodal}

```csharp
public void CloseModal();
```

Closes the dialog on top, uncovering whatever it was opened over. Submitting, picking and cancelling already do this, so it is only needed to dismiss one from the outside.

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called from off the drawing thread. |

### `Invalidate()` {#invalidate}

```csharp
public void Invalidate();
```

Asks for a repaint. Needed only for changes the framework cannot see — a field mutated from outside, or data that arrived on a timer.

### `PushModal(Modal)` {#pushmodal-modal}

```csharp
public void PushModal(Modal modal);
```

Opens a dialog over whatever is already open, which is how a callback asks a follow-up question without losing what the user was in the middle of. Closing it uncovers the one underneath.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `modal` | [`Modal`](../arlecchino.modals/Modal.md) | The dialog to open. |

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called from off the drawing thread. |

### `RequestChoice(string, IReadOnlyList<string>, Action<string>, string)` {#requestchoice-string-ireadonlylist-string-action-string-string}

```csharp
public void RequestChoice(
    string title,
    IReadOnlyList<string> options,
    Action<string> onPicked,
    string current = "");
```

Asks for one option out of a list that can be filtered by typing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `options` | `IReadOnlyList<T>`&lt;`string`&gt; | What to choose from. |
| `onPicked` | `Action<T>`&lt;`string`&gt; | Called with the chosen option. |
| `current` | `string` | Option to start on; the first one when it is not in the list. |

### `RequestColor(string, Rgb, Action<Rgb>)` {#requestcolor-string-rgb-action-rgb}

```csharp
public void RequestColor(string title, Rgb initial, Action<Rgb> onPicked);
```

Asks for a colour with a swatch and three sliders. Channels are whole numbers, so a colour that goes in can come back shifted by a unit or two.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | [`Rgb`](../arlecchino.rendering/Rgb.md) | Colour the sliders start on. |
| `onPicked` | `Action<T>`&lt;[`Rgb`](../arlecchino.rendering/Rgb.md)&gt; | Called with the chosen colour. |

### `RequestConfirmation(string, Action)` {#requestconfirmation-string-action}

```csharp
public void RequestConfirmation(string title, Action onConfirmed);
```

Asks a question that has to be confirmed before something happens. The negative answer starts selected, so a stray `Enter` cancels rather than deletes.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | The question. |
| `onConfirmed` | `Action` | Called only when the answer was yes. |

### `RequestDate(string, DateOnly, Action<DateOnly>)` {#requestdate-string-dateonly-action-dateonly}

```csharp
public void RequestDate(string title, DateOnly initial, Action<DateOnly> onSubmit);
```

Asks for a date, edited one segment at a time.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | `DateOnly` | Date the field starts on. |
| `onSubmit` | `Action<T>`&lt;`DateOnly`&gt; | Called with the chosen date. |

### `RequestEmail(string, string, Action<string>)` {#requestemail-string-string-action-string}

```csharp
public void RequestEmail(string title, string initial, Action<string> onSubmit);
```

Asks for an email address, checked before the dialog will close.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | `string` | Text the field starts with. |
| `onSubmit` | `Action<T>`&lt;`string`&gt; | Called with the accepted address. |

### `RequestMessage(string, string, Action)` {#requestmessage-string-string-action}

```csharp
public void RequestMessage(string title, string text, Action? onClosed = null);
```

Shows a message with nothing to fill in; any of the closing keys dismisses it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `text` | `string` | What to say. Long text wraps inside the box. |
| `onClosed` | `Action` | Called once it is dismissed. |

### `RequestMultiChoice(string, IReadOnlyList<string>, IReadOnlyList<string>, Action<IReadOnlyList<string>>)` {#requestmultichoice-string-ireadonlylist-string-ireadonlylist-string-action-ireadonlylist-string}

```csharp
public void RequestMultiChoice(
    string title,
    IReadOnlyList<string> options,
    IReadOnlyList<string> selected,
    Action<IReadOnlyList<string>> onSubmit);
```

Asks for any number of options. Marks survive filtering, and the result comes back in the order of `options` rather than the order they were marked.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `options` | `IReadOnlyList<T>`&lt;`string`&gt; | What to choose from. |
| `selected` | `IReadOnlyList<T>`&lt;`string`&gt; | Options marked to begin with. |
| `onSubmit` | `Action<T>`&lt;`IReadOnlyList<T>`&lt;`string`&gt;&gt; | Called with everything marked. |

### `RequestNumber(string, decimal, decimal, decimal, Action<decimal>)` {#requestnumber-string-decimal-decimal-decimal-action-decimal}

```csharp
public void RequestNumber(
    string title,
    decimal initial,
    decimal minimum,
    decimal maximum,
    Action<decimal> onSubmit);
```

Asks for a number within bounds. Typing is restricted to digits, and stepping keys clamp to the range.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | `decimal` | Value the field starts on, clamped to the range. |
| `minimum` | `decimal` | Lowest value allowed. |
| `maximum` | `decimal` | Highest value allowed. |
| `onSubmit` | `Action<T>`&lt;`decimal`&gt; | Called with the accepted number. |

### `RequestPassword(string, Action<string>)` {#requestpassword-string-action-string}

```csharp
public void RequestPassword(string title, Action<string> onSubmit);
```

Asks for a secret. The field shows dots, but the text handed to the callback is untouched.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `onSubmit` | `Action<T>`&lt;`string`&gt; | Called with what was typed. |

### `RequestSlider(string, decimal, decimal, decimal, Action<decimal>)` {#requestslider-string-decimal-decimal-decimal-action-decimal}

```csharp
public void RequestSlider(
    string title,
    decimal initial,
    decimal minimum,
    decimal maximum,
    Action<decimal> onSubmit);
```

Asks for a number on a track, adjusted with the arrows rather than typed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | `decimal` | Value the slider starts on, clamped to the range. |
| `minimum` | `decimal` | Left end of the track. |
| `maximum` | `decimal` | Right end of the track. |
| `onSubmit` | `Action<T>`&lt;`decimal`&gt; | Called with the chosen value. |

### `RequestText(string, string, Func<string, string>, Action<string>)` {#requesttext-string-string-func-string-string-action-string}

```csharp
public void RequestText(
    string title,
    string initial,
    Func<string, string?>? validate,
    Action<string> onSubmit);
```

Asks for a line of text.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | `string` | Text the field starts with. |
| `validate` | `Func<T, TResult>`&lt;`string`, `string`&gt; | Checked on confirm; return a message to keep the dialog open, or `null` to accept. |
| `onSubmit` | `Action<T>`&lt;`string`&gt; | Called with the accepted text. |

### `RequestTextArea(string, string, Action<string>, Func<string, string>, int)` {#requesttextarea-string-string-action-string-func-string-string-int}

```csharp
public void RequestTextArea(
    string title,
    string initial,
    Action<string> onSubmit,
    Func<string, string?>? validate = null,
    int visibleRows = 8);
```

Asks for several lines of text. `Enter` starts a new line, so the text is confirmed with the `Submit` key — `Ctrl+Enter` unless the keymap says otherwise.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | `string` | Text the editor starts with. |
| `onSubmit` | `Action<T>`&lt;`string`&gt; | Called with the accepted text. |
| `validate` | `Func<T, TResult>`&lt;`string`, `string`&gt; | Checked on submit; return a message to keep the dialog open, or `null` to accept. |
| `visibleRows` | `int` | How many rows to show before the text starts scrolling. |

### `RequestTime(string, TimeOnly, Action<TimeOnly>)` {#requesttime-string-timeonly-action-timeonly}

```csharp
public void RequestTime(string title, TimeOnly initial, Action<TimeOnly> onSubmit);
```

Asks for a time of day, edited one segment at a time.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | `TimeOnly` | Time the field starts on. |
| `onSubmit` | `Action<T>`&lt;`TimeOnly`&gt; | Called with the chosen time. |

### `RequestToggle(string, bool, Action<bool>)` {#requesttoggle-string-bool-action-bool}

```csharp
public void RequestToggle(string title, bool initial, Action<bool> onSubmit);
```

Asks a yes-or-no question.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | The question. |
| `initial` | `bool` | Which chip starts selected. |
| `onSubmit` | `Action<T>`&lt;`bool`&gt; | Called with the answer. |

### `RequestUrl(string, string, Action<string>)` {#requesturl-string-string-action-string}

```csharp
public void RequestUrl(string title, string initial, Action<string> onSubmit);
```

Asks for an http or https link, checked before the dialog will close.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `title` | `string` | Title of the dialog. |
| `initial` | `string` | Text the field starts with, often just the scheme. |
| `onSubmit` | `Action<T>`&lt;`string`&gt; | Called with the accepted link. |

