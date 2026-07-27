---
title: File picker
sidebar_label: File picker
description: Requesting a path, the places sidebar, filters and keys.
---

# File picker

A file browser that ships with the framework: a places sidebar on the left, a
Name / Date Modified / Size / Kind table on the right, a toolbar with history arrows and a filter
field, and a status row with the item count and a key legend.

It is registered by `AddArlecchino` under the route `Routes.FilePicker`, so nothing has to be wired up.

## Asking for a path

```csharp
_state.FilePicker = new FilePickerRequest(
    Title: "Pick a folder",
    PickFolder: true,
    InitialPath: Environment.CurrentDirectory,
    ReturnView: ViewKind.Default,
    OnPicked: path => _state.Output = $"picked: {path}");

return Routes.FilePicker;
```

Fill `ArlecchinoState.FilePicker`, then navigate to `Routes.FilePicker` — the view reads the request in its
constructor. Navigating there without a request opens a folder picker rooted at the drive list that
returns to `ViewRoute.None`.

| Member | Meaning |
|---|---|
| `Title` | Shown in the toolbar |
| `PickFolder` | `true` picks directories, `false` picks files |
| `InitialPath` | Starting folder; a path that does not exist starts at the drive list |
| `ReturnView` | Route navigated to after a pick or a cancel |
| `OnPicked` | Called with the chosen path before returning |
| `Places` | Extra sidebar entries, listed above the standard favorites |
| `FileFilter` | Predicate over the full path; files that fail it are not listed |

```csharp
_state.FilePicker = new FilePickerRequest("Pick a save", PickFolder: false, start, ViewKind.Default, Load)
{
    Places = [new FilePickerPlace("Game saves", savesPath, "▪")],
    FileFilter = static path => path.EndsWith(".sav", StringComparison.OrdinalIgnoreCase),
};
```

The request is cleared on pick and on cancel, and `ArlecchinoState.PickerLastFolder` is set to the folder the
picker ended in — pass it as the next `InitialPath` to resume where the user left off.

## Sidebar

Above the standard entries come the request's own `Places`. Then Favorites — Desktop, Documents,
Downloads, Pictures, Music, Videos, each skipped when the folder does not exist — and Locations: the
home folder, `This computer`, and every ready drive. Section headers are not selectable.

## Keys

| Key | In the list | In the sidebar |
|---|---|---|
| `↑` `↓` | Move the selection | Move between places |
| `PgUp` `PgDn` `Home` `End` | Jump ten rows / to the ends | — |
| `→` | Enter the selected folder | Focus the list |
| `←` | Go to the parent folder, or focus the sidebar when already at the drive list | — |
| `Enter` | Open a folder; pick a file when picking files | Go to the place and focus the list |
| `Ctrl+Enter` | Pick the current folder when picking folders | — |
| `Tab` | Switch panes | Switch panes |
| `Backspace` | Shorten the filter, or go up when it is empty | — |
| any character | Appends to the filter | — |
| `Esc` | Cancel and return to `ReturnView` | Cancel |

Typed characters go through `KeyText`, so filtering works on a non-latin layout — see
[Keyboard](keyboard.md).

## Text

Every label, column header, size, date and kind string is a delegate on
`ArlecchinoStrings.FilePicker`, including the defaults that format `12.3 MB`, `Today at 9:41` and
`ZIP archive`. See [Localization](localization.md).
