---
title: What's new in 2026.10.1
sidebar_label: What's new in 2026.10.1
description: A folder picked with Enter in a Unix terminal, and a terminal window closed without a core dump.
---

# What's new in 2026.10.1

Two fixes, both found in a terminal on Linux and macOS. Nothing breaks. The
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#2026101) is the full record.

## Picking a folder with `Enter`

A Unix terminal sends `Ctrl+Enter` as the same `\r` it sends for `Enter`. Picking a folder was bound to
`Ctrl+Enter` alone, so `Enter` opened the folder under the cursor instead of picking anything. While
folders are being picked, `Enter` now picks the folder being listed, whatever is under the cursor and
whatever the filter narrows the listing to, so an empty folder can be picked too. `Ctrl+Enter` still picks.

Walking into a folder is `→` or a second click, and `←` goes back up with the cursor on the folder that was
left. In the drives, where no folder is being listed, `Enter` opens the drive under the cursor. The keys are
in the [file picker](file-picker.md) page.

## Closing the terminal window

Closing the terminal window used to end an application with a core dump. A terminal that hangs up sends
`SIGHUP` and then `SIGCONT`, and the shell in front of the application sends a `SIGHUP` of its own. Each
handler went to give the screen back, or to take it again, on a terminal that answers every write with
`EIO`. The `IOException` that came out was thrown on the thread pool, where nothing catches it, and the
runtime aborted the process.

A terminal that refuses the write is now walked past, and the hangup ends the process the way it was always
going to.
