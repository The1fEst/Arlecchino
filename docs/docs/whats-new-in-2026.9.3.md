---
title: What's new in 2026.9.3
sidebar_label: What's new in 2026.9.3
description: A Markdown document drawn as what its marks say, the pictures it names drawn where it names them, a reader for the flat SVG a chart is written as, and shortcuts reached by the key the terminal names.
---

# What's new in 2026.9.3

A release about a file that was written to be read. A document has been drawable as text all along,
which meant its hashes, its stars and its bars on screen — the marks a reader was meant to be spared.
Beside that, the shortcuts stopped working out which key was pressed and started asking the terminal,
which is the one break — see [Migrating to 2026.9.3](migrating-to-2026.9.3.md). The
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md#202693) is the full record.

## `MarkdownView`

Headings, lists, quotes, code blocks, tables and links, drawn as what they are rather than as the marks
that say so. It wraps to the width it is given and scrolls the way [`TextView`](text-view.md) does:

```csharp
_readme = new MarkdownView(options.Keymap) { Text = File.ReadAllText(path) };

_readme.Draw(region);
```

A paragraph the file broke over several lines is one paragraph again, that break having belonged to the
editor it was written in rather than to the pane it is read in. A table is padded to its columns by
what a cell comes to on screen rather than by what it says in the file — `**no**` is two columns wide
and six in the file. A quote carries its bar down every row it takes, and the rest of an item of a list
is set under its first word rather than under its bullet.

What a document writes as an entity for want of the symbol — `&rarr;`, `&nbsp;`, `&#8594;` — is drawn
as the symbol, and stays as it was written inside a code span. Markup left in the file for a browser is
dropped with the words around it kept. A table and a code block are the two things never wrapped, since
a line broken in either says what it did not: a row of one that runs past the edge is marked as cut.

The whole of it is on the [MarkdownView](markdown-view.md) page.

## Pictures in a document

A document names its pictures, and the application says what those names are — only the application
knows what `tables/recoil.svg` is beside, and whether that is a disk or a server:

```csharp
_readme.Pictures = address => _illustrations.Of(address);
```

The delegate is asked while a frame is being drawn, so it answers at once: the picture it holds, or
`null` for one that is not there yet. A picture is taken out of the line it was written in and given
rows of its own — `PictureRows` of them at most — and two written into one line share that band side by
side, a before and an after being what a line of two is usually for. Until one arrives, those rows say
what it stands for.

`MarkdownStyles` is what each part of a document is written in. Every one of them is `null` to begin
with, which leaves it to the [theme](theming.md); what is worth setting is what a palette has no role
for, such as the ground behind a block of code.

## SVG, the flat kind

[`Arlecchino.Pictures`](pictures.md) reads the SVG a chart or a diagram is written as: rectangles,
lines, runs of lines, polygons, circles and ellipses, dashed or solid, in colors named or written as
digits. It is not an SVG renderer and does not pretend to be one — a file carrying a path, text of its
own, a transform, a gradient or a style sheet is refused whole rather than drawn as a picture missing
the half that mattered, and a caller handed nothing shows the file as the text it is.

Being drawn from shapes rather than from pixels, it is read at whatever size the caller has a use for:
a picture asked for larger than the file says comes out sharper rather than enlarged.

## The terminal names the key

`UseKeysByPosition()` worked the position out from the key the terminal named, and every terminal but
the Windows console names no key at all for a letter that is not Latin — it sends the character and
nothing else. So the setting was a no-op where it was most wanted: with a Cyrillic layout switched
on, a command on a letter was unreachable, and a chord was unreachable along with the `g` it hangs
off.

It now asks the terminal to name the keys it sends. A terminal that answers writes the key of the
base layout beside the character, so the framework is told where the key sits rather than working it
out — and it is told for Greek, Arabic and Hebrew as readily as for Cyrillic, with no list of
alphabets kept anywhere:

```csharp
builder.TryUseKeysByPosition();   // g is pressed as п, and п still types п
```

The name says trying because it is a request. Terminals answer where they speak the kitty keyboard
protocol — kitty, foot, Ghostty, WezTerm, Alacritty, rio, contour and iTerm2 among them, with tmux
passing it through under `extended-keys on`. Where one does not, the shortcuts stay where the layout
puts them, exactly as before the request was made. The Windows console names its keys without being
asked.

Typing is untouched, as before: a field takes the text the terminal reports, in whatever language it
was typed. The rename is the one break of this release — see
[Migrating to 2026.9.3](migrating-to-2026.9.3.md). `IArlecchinoTerminal` grew `EnableKeyNames` and
`DisableKeyNames`, both with a body that does nothing, so a terminal of your own needs no change.
