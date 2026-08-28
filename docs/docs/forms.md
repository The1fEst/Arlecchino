---
title: Forms
sidebar_label: Forms
description: Form and Field&lt;T&gt; — turning a store of atoms into editable rows, each opening the modal that matches its type.
---

# Forms

A view is the form; `Form` is the part that turns [atoms](atoms.md) into editable rows, each opening
the [modal](modals.md) that matches its type.

```csharp
private readonly Surface _surface;
private readonly Form _form;
private readonly ActionField _apply;
private readonly IDisposable _complete;

public SettingsView(
    Surface surface,
    SettingsStore settings,
    ArlecchinoState state,
    ArlecchinoOptions options)
{
    _surface = surface;

    _apply = Fields.Action(() => Loc(LocString.Apply), Apply);

    _form = new Form(state, options)
    {
        Fields =
        [
            Fields.Text(() => Loc(LocString.Profile), settings.Profile)
                .WithHelp(() => Loc(LocString.ProfileHelp)),
            Fields.Secret(() => Loc(LocString.Passphrase), settings.Passphrase),
            Fields.Choice(() => Loc(LocString.Theme), ["dark", "light"], settings.Theme),
            Fields.Slider(() => Loc(LocString.Volume), settings.Volume)
                .WithItems(() => [.. Enumerable.Range(0, 101)]),
            Fields.Toggle(() => Loc(LocString.Fullscreen), settings.Fullscreen)
                .WithTemplate(() => settings.Fullscreen.Value ? Yes : No),
            Fields.Path(() => Loc(LocString.Folder), settings.Folder, ViewKind.Settings,
                pickFolder: true),
            _apply,
        ],
    };

    _complete = settings.IsComplete.Subscribe(() => _apply.IsEnabled = settings.IsComplete.Value);
}

public void Draw() => _form.Draw(_surface.Content);
public ViewRoute Handle(KeyPress key) => _form.Handle(key).Route;
public ViewRoute HandleMouse(MouseEvent mouse) => _form.HandleMouse(mouse).Route;
```

`settings` is the application's own [store](stores.md) of atoms — `Atom<string> Profile`,
`Atom<decimal> Volume` and so on — not anything the framework defines. `state` and `options` are
`ArlecchinoState` and `ArlecchinoOptions` from the container: the form needs the first to open modals
and the second for the keymap and the wording.

## What it looks like

Rendered as `label = value`, labels padded to the longest, the help of the selected field on the line
under it, actions as `> Label`:

```
  Profile    = empty
    shown in the title bar
  Passphrase = ••••••
  Theme      = dark
  Volume     = 60

  > Apply
```

The help line exists only when the selected field actually has help, so a form of fields without any
is a solid column of rows rather than a column with gaps in it.

## The fields

| Factory | Opens |
|---|---|
| `Fields.Text`, `Fields.Secret` | [Text modal](modals.md#text), masked for secrets |
| `Fields.Number`, `Fields.Slider` | [Number and slider modals](modals.md#numbers) |
| `Fields.Toggle` | [Toggle modal](modals.md#toggle) |
| `Fields.Choice`, `Fields.MultiChoice` | [Choice and multi-choice modals](modals.md#choice) |
| `Fields.Date`, `Fields.Time`, `Fields.Color` | [Segment editors and the color picker](modals.md#dates-times-and-colors) |
| `Fields.Path` | The [file picker](file-picker.md); returns its route so the view navigates |
| `Fields.Action` | Nothing — runs your delegate and returns a route |

Each takes a label and the atom it edits, and nothing else. What the row also wants is added on
afterwards, one call per thing:

| Call | What it says |
|---|---|
| `WithHelp(() => …)` | The line under the row while it is selected |
| `WithValidation(value => …)` | Whether a value is allowed; refusing one shows `ArlecchinoStrings.NotAnAllowedValue` |
| `WithItems(() => …)` | What the value may be — the options of a choice, and for a number or a slider the ends of its range |
| `WithTemplate(() => …)` | How the value reads on the row, where the value's own `ToString` is not what to show |
| `WithLabel(() => …)` | Replaces the label |

Labels, help and templates are delegates, not strings, so a form follows the current
[language](localization.md) without being rebuilt.

### Where a path field starts

`Fields.Path` opens the picker at the path the field already holds. While the field is still empty
there is nothing to open at and the picker lands on the drives, so `WithStart` says where it should
start instead:

```csharp
Fields.Path(() => "Save folder", settings.Folder, ViewKind.Settings, pickFolder: true)
    .WithStart(() => state.PickerLastFolder);
```

A field that already has a value opens there and ignores the start; a value that no longer exists on
disk lands on the drives as before. It is a delegate rather than a string so that "wherever the
user was last time" is answered when the picker opens rather than when the form is built —
`ArlecchinoState.PickerLastFolder`, which the [picker](file-picker.md) fills in as it closes, is
exactly that answer.

## Keys and clicks

Movement, `Confirm` and `Erase` come from the [keymap](keyboard.md#the-keymap); `Erase` resets a field
to its empty value. Clicking a row selects it, clicking the selected row opens it, and the wheel moves
the selection.

`Form` implements [`IArlecchinoFocusable`](focus.md), so a screen that is a form beside something else
puts both in a ring and stops routing keys by hand.

## Enabled and disabled

`IsEnabled` is a plain `bool` on the field, and a row draws itself muted while it is false. A button
that follows the state of the form is subscribed to it — usually to a
[`Computed<bool>`](atoms.md#derived-values), which is what makes it follow rather than be asked:

```csharp
var apply = Fields.Action(() => "Apply", Apply);

_complete = settings.IsComplete.Subscribe(() => apply.IsEnabled = settings.IsComplete.Value);
```

## Why atoms rather than properties

Because fields read and write atoms, two things come free:

- an edit made through a modal is already undoable when the atom is a `HistoryAtom<T>`;
- a value changed from **outside** the form — by a command, a background load, another screen —
  redraws it without anyone telling it to.

A form over plain properties would need both wired by hand.
