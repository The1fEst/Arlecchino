---
title: Forms
sidebar_label: Forms
description: Form and Field — turning a store of atoms into editable rows, each opening the modal that matches its type.
---

# Forms

A view is the form; `Form` is the part that turns [atoms](atoms.md) into editable rows, each opening
the [modal](modals.md) that matches its type.

```csharp
private readonly Surface _surface;
private readonly Form _form;

public SettingsView(
    Surface surface,
    SettingsStore settings,
    ArlecchinoState state,
    ArlecchinoOptions options)
{
    _surface = surface;

    _form = new Form(state, options)
    {
        Fields =
        [
            Field.Text(() => Loc(LocString.Profile), settings.Profile,
                help: () => Loc(LocString.ProfileHelp)),
            Field.Secret(() => Loc(LocString.Passphrase), settings.Passphrase),
            Field.Choice(() => Loc(LocString.Theme), ["dark", "light"], settings.Theme),
            Field.Slider(() => Loc(LocString.Volume), settings.Volume, 0, 100),
            Field.Toggle(() => Loc(LocString.Fullscreen), settings.Fullscreen,
                value => value ? Yes : No),
            Field.Path(() => Loc(LocString.Folder), settings.Folder, ViewKind.Settings,
                pickFolder: true),
            Field.Action(() => Loc(LocString.Apply), Apply,
                enabled: () => settings.IsComplete.Value),
        ],
    };
}

public void Draw() => _form.Draw(_surface.Content);
public ViewRoute Handle(ConsoleKeyInfo key) => _form.Handle(key).Route;
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
| `Field.Text`, `Field.Secret` | [Text modal](modals.md#text), masked for secrets |
| `Field.Number`, `Field.Slider` | [Number and slider modals](modals.md#numbers) |
| `Field.Toggle` | [Toggle modal](modals.md#toggle) |
| `Field.Choice`, `Field.MultiChoice` | [Choice and multi-choice modals](modals.md#choice) |
| `Field.Date`, `Field.Time`, `Field.Color` | [Segment editors and the colour picker](modals.md#dates-times-and-colours) |
| `Field.Path`, `Field.PathFrom` | The [file picker](file-picker.md); returns its route so the view navigates |
| `Field.Action` | Nothing — runs your delegate and returns a route |

Labels and help are delegates, not strings, so a form follows the current
[language](localization.md) without being rebuilt.

### Where a path field starts

`Field.Path` opens the picker at the path the field already holds. While the field is still empty
there is nothing to open at and the picker lands on the drives, so `Field.PathFrom` says where it
should start instead:

```csharp
Field.PathFrom(() => "Save folder", settings.Folder, ViewKind.Settings, pickFolder: true,
    start: () => state.PickerLastFolder);
```

A field that already has a value opens there and ignores `start`; a value that no longer exists on
disk lands on the drives as before. `start` is a delegate rather than a string so that "wherever the
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

`Field.Action` takes an `enabled` predicate — usually a [`Computed<bool>`](atoms.md#derived-values) —
and draws itself muted while it is false:

```csharp
Field.Action(() => "Apply", Apply, enabled: () => settings.IsComplete.Value)
```

## Why atoms rather than properties

Because fields read and write atoms, two things come free:

- an edit made through a modal is already undoable when the atom is `TrackedAtom<T>`;
- a value changed from **outside** the form — by a command, a background load, another screen —
  redraws it without anyone telling it to.

A form over plain properties would need both wired by hand.
