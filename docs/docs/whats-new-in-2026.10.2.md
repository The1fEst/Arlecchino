---
title: What's new in 2026.10.2
sidebar_label: What's new in 2026.10.2
description: The file picker lists the drives anyone means to browse, a long drive path is cut from the start, and text can be clipped to a column with an ellipsis at either end.
---

# What's new in 2026.10.2

A release about the drives the file picker offers, and the two text calls that came out of cutting their
names to fit. Nothing breaks. The
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#2026102) is the full record.

## The drives anyone means to browse

On Linux the picker listed every mount the system has. That took in the kernel's own file systems, such as
`/proc`, `/sys`, `/dev` and the cgroup and tracing file systems, and the folders a distribution puts on
partitions or subvolumes of their own. Those are now left out, and so is anything mounted under `/proc`,
`/sys`, `/dev`, `/run`, `/boot`, `/efi`, `/tmp`, `/var`, `/usr`, `/opt`, `/srv`, `/root`, `/swap`,
`/.snapshots` or `/home` itself. A drive udisks mounts under `/run/media` is still offered, and on macOS the
volumes under `/System/Volumes` are left out the same way.

A mount that cannot be read is skipped rather than emptying the list. The root was listed as an empty row,
since its name was what is left of `/` once the separator is trimmed; it is now listed as `/`.

## A long drive path is cut from the start

A drive whose path runs past the sidebar is cut at its start rather than its end. The end of a path is what
tells `…/Disks/Data` from `…/Disks/Trash`, and both used to read `/home/user/Disks/…`.

## `TextWidth.Clip` and `TextWidth.ClipStart`

`Clip` cuts text to a column width the way `Truncate` does, and ends it in an ellipsis when anything was left
out. `ClipStart` is the same from the other end, opening with the ellipsis, so the part of a path that tells
it from its neighbors is the part kept. The file picker uses them for its labels and columns, and an
application that fits text to a column can use them too. The calls are on the [text and width](text.md) page.
