---
title: What's new in 2026.8.3
sidebar_label: What's new in 2026.8.3
description: Work that waits can be handed to the drawing thread whole, and what the terminal answers about itself stops being typed into the line.
---

# What's new in 2026.8.3

A short release: one thing added, one thing fixed, and nothing to migrate. The
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202683) is the full record.

## Work that waits, handed over whole

`FrameThread.Post` takes work that waits. It starts on the drawing thread and every `await` inside it
comes back there, so a piece of work that reads a folder, then writes what it read into a widget, is one
thing handed over rather than two:

```csharp
FrameThread.Post(async () =>
{
    var entries = await Listing.ReadAsync(folder);

    _list.Items = entries;
});
```

What it throws reaches the frame loop the way a posted action's failure does, before an `await` or after
one. See [Waiting without leaving the drawing thread](frame-loop.md#waiting-without-leaving-the-drawing-thread).

## The terminal's own answer stays out of the line

The probe asks the terminal what it can do, and the reply arrives as text through the same input the
keyboard uses. A reply that arrived while something was being typed into used to land in the field:
`[?65;4;6;18;22;52c` typed itself into a filter row. The reader now recognizes the answer for what it is
and drops it, which is what tells sixel, the color behind the text and the size of a cell apart.
