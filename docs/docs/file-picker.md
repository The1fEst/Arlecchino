---
title: File picker
sidebar_label: File picker
description: Requesting a path, the places sidebar, filters and keys.
---

# File picker

A file browser that ships with the framework: a places sidebar on the left, a
Name / Date Modified / Size / Kind table on the right, a toolbar carrying the folder, the title and a
filter field, and a status row with the item count and a key legend.

It is registered by `AddArlecchino` under the route `Routes.FilePicker`, so nothing has to be wired up.

## Asking for a path

```csharp
private readonly ArlecchinoState _state;

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
| `InitialPath` | Where browsing starts: a folder is opened, a **file** is opened in the folder that holds it and starts under the cursor, and a path that no longer exists starts at the drive list |
| `ReturnView` | Route navigated to after a pick or a cancel |
| `OnPicked` | Called with the chosen path before returning |
| `Places` | Extra sidebar entries, listed above the standard favorites |
| `FileFilter` | Predicate over the full path; files that fail it are not listed |

```csharp
_state.FilePicker = new FilePickerRequest(
    "Pick a save",
    PickFolder: false,
    start,
    ViewKind.Default,
    Load)
{
    Places = [new FilePickerPlace("Game saves", savesPath, "▪")],
    FileFilter = static path => path.EndsWith(".sav", StringComparison.OrdinalIgnoreCase),
};
```

The request is cleared on pick and on cancel. Picking also sets `ArlecchinoState.PickerLastFolder` to the
folder the picker ended in — pass it as the next `InitialPath` to resume where the user left off.
Cancelling leaves it holding whatever the last pick did.

## Sidebar

Favorites comes first, and the request's own `Places` are the top of it, above Desktop, Documents,
Downloads, Pictures, Music and Videos — each of those skipped when the folder does not exist. Then
Locations: the home folder, `This computer`, and the drives under it. Section headers are not selectable.

## Drives

`This computer` lists the drives anyone means to browse: the ones that are mounted and ready, without the
file systems the kernel keeps for itself, such as `proc`, `sysfs` and `tmpfs`. On Linux a mount under
`/proc`, `/sys`, `/dev`, `/run`, `/boot`, `/efi`, `/tmp`, `/var`, `/usr`, `/opt`, `/srv`, `/root`, `/swap`
or `/.snapshots` is left out, and so is `/home` itself; a drive udisks mounts under `/run/media` is kept.
On macOS the volumes under `/System/Volumes` are left out. The root is listed as `/`, and a mount that
cannot be read is skipped.

In the sidebar, a drive whose path is longer than the sidebar is cut from the start rather than the end,
so the end of the path, which tells two drives apart, stays in view.

## Keys

| Key | In the list | In the sidebar |
|---|---|---|
| `↑` `↓` | Move the selection | Move between places |
| `PgUp` `PgDn` `Home` `End` | Jump ten rows / to the ends | — |
| `→` | Enter the selected folder | Focus the list |
| `←` | Go to the parent folder, or focus the sidebar when already at the drive list | — |
| `Enter` | Open the row under the cursor, or pick a file when picking files. When picking folders, pick the folder being listed, whatever is under the cursor; in the drives, open the drive under the cursor | Go to the place and focus the list |
| `Ctrl+Enter` | Pick the folder being listed when picking folders | Pick the folder being listed when picking folders |
| `Tab` | Switch panes | Switch panes |
| `Backspace` | Shorten the filter, or go up when it is empty | — |
| any character | Appends to the filter | — |
| `Esc` | Cancel and return to `ReturnView` | Cancel |

A Unix terminal sends `Ctrl+Enter` as `Enter`, which is why `Enter` picks the folder being listed when folders
are picked, and an empty folder can be picked that way too. Walking into a folder is `→` or a second click on
its row: with the mouse, the first click selects a row and the second opens it.

Typed characters go through `KeyText`, so filtering works on a non-latin layout — see
[Keyboard](keyboard.md).

## Text

Every label, column header, size, date and kind string is a delegate on
`ArlecchinoStrings.FilePicker`, including the defaults that format `12.3 MB`, `Today at 9:41` and
`ZIP archive`. See [Localization](localization.md).
