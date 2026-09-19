---
title: What's new in 2026.9.2
sidebar_label: What's new in 2026.9.2
description: A Windows console is put on UTF-8 before anything takes hold of its writer, so a frame reaches it as the symbols it was drawn from.
---

# What's new in 2026.9.2

A release about one line of startup, and about how late is too late to write it. Nothing breaks, and an
application carrying the workaround by hand can drop it. The
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202692) is the full record.

## A box with rounded corners, rather than `?◆◆◆◆?`

A Windows console comes up on the national code page, and the runtime builds a writer around whatever
page is in force when that writer is first asked for. `SystemTerminal` changed the page — but it is
built out of the container, which is well into startup, and by then `AddArlecchino` had taken the
console over and kept the writer every frame is drawn through. That writer holds the encoder it was
born with whatever the page becomes afterwards, so frames went out through the national page while the
console itself had moved to UTF-8. A symbol the page cannot spell arrived as `?`; one it can arrived as
the single byte it spells it with, which is not UTF-8 and draws as the replacement character.

The page is now set from a module initializer generated into the application, which runs before its
entry point does — before the host builder, before `AddArlecchino`, before anything an application
could write for itself. There is nothing to add to `Program.cs`, and

```csharp
Console.OutputEncoding = new UTF8Encoding(false);
```

carried as a workaround can go.

## `SystemTerminal.UseUtf8()`

The same thing as a method, for an application that builds the terminal by hand rather than through
`AddArlecchino`. An application that takes the package the usual way never has to call it: the
generated module initializer already has.
