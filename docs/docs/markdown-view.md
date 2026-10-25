---
title: Markdown view
sidebar_label: MarkdownView
description: MarkdownView — a Markdown document drawn as what its marks say, wrapped to the pane, with the pictures it names drawn where it names them.
---

# MarkdownView

A Markdown document to read: headings, lists, quotes, code blocks, tables and links drawn as what they
are rather than as the marks that say so. It wraps to the width it is given and scrolls the way
[`TextView`](text-view.md) does, being a [`ScrollPane`](scrolling.md) inside as well.

```csharp
_readme = new MarkdownView(options.Keymap) { Text = File.ReadAllText(path) };

_readme.Draw(region);
```

| Member | Meaning |
|---|---|
| `Text` | The document, as it is written in the file |
| `Styles` | What each part of it is written in — see [Colors](#colors) |
| `Pictures` | Where a picture comes from — see [Pictures](#pictures) |
| `PictureRows` | How many rows a picture may take. `16` by default |
| `Offset` | The first row shown |
| `LineCount` | How many rows the document takes once laid out |
| `IsFocused` | Set by the [focus ring](focus.md) |

## What it reads

| Written | Drawn |
|---|---|
| `# Title` to `###### Title`, and a row of `=` under a line | A heading; the first level is ruled off underneath |
| `- item`, `* item`, `+ item` | A bullet, by how deep the item is rather than by which mark was used |
| `1. item` | The number it was given |
| `- [ ] item`, `- [x] item` | An empty box and a ticked one |
| `> said` | The text with a bar down every row it takes |
| ` ```csharp ` fences, and `~~~` | A code block on a band of its own, drawn as it stands |
| `` `span` `` | A code span |
| `**loud**`, `*slant*` | Bold and italic |
| `[label](url)`, `<https://…>` | The label, and where it goes written after it |
| `\| a \| b \|` with a row of dashes under it | A table padded to its columns |
| `---`, `***`, `___` | A rule across the pane |
| `&rarr;`, `&nbsp;`, `&#8594;` | The symbol the entity stands for |
| `<p>`, `<sub>`, `<br>` and the rest of the markup | Dropped, with the words around it kept |

A paragraph the file broke over several lines is one paragraph again: that break belonged to the editor
it was written in and not to the pane it is read in. An indented block is prose rather than code, since
a document indents the continuation of a list item far more often than it means four spaces as a fence.
A reference link — `[label][name]` — is left as it was written.

## Not wrapped

A table and a code block are the two things a line may not be broken in, since a line broken in either
says something it did not say. A row of one that runs past the edge is cut, and marked as cut with a
`›` in its last column, so a reader can tell it from a row that ended.

## Colors

`MarkdownStyles` says what each part is written in. Every one of them is `null` to begin with, which
leaves it to the [theme](theming.md) — an application that has said what its colors are has already
said most of this:

```csharp
_readme.Styles = new MarkdownStyles
{
    Code = Paint(Coral, Chip),                 // a background of its own is worth giving this one
    Link = Paint(Sea, null, TextStyle.Underline),
};
```

| Style | What it writes | Left alone |
|---|---|---|
| `Heading` | The first two levels of heading | `Theme.Header` |
| `Subheading` | The four below them | `Theme.Accent` |
| `Strong`, `Emphasis` | What was put between two stars, and between one | The ordinary text, in bold and in italic |
| `Code` | A code span, and every line of a block | `Theme.Info` |
| `Marker` | Bullets, the numbers of a list, the bar of a quote | `Theme.Active` |
| `Quote` | What a quote says | `Theme.Secondary` |
| `Link`, `Url` | What a link says, and where it goes | The ordinary text underlined, and `Theme.Secondary` |
| `Rule` | Rules, and the lines between the columns of a table | `Theme.Info` |
| `TableHeader` | The first row of a table | `Theme.TableHeader` |

A style with a background of its own is worth giving `Code`: a block is painted across the pane with
it, which is what sets the block apart from the prose around it.

## Pictures

A document names its pictures — `![alt](chart.svg)`, or an `<img>` left in it as markup — and the
application says what those names are. Reading one belongs to the application because only the
application knows what `tables/recoil.svg` is beside, and whether that is a disk or a server:

```csharp
_readme.Pictures = address => _illustrations.Of(address);
```

The delegate is asked while a frame is being drawn, so it answers at once: the picture it already
holds, or `null` for one that is not there. An application that reads pictures from a disk or a server
starts the read on the first asking and answers `null` until it lands — the frame it lands for draws
it. [`Arlecchino.Pictures`](pictures.md) is what turns the bytes into the pixels a
[`Picture`](pictures.md) shows.

A picture is taken out of the line it was written in and given rows of its own, no more than
`PictureRows` of them, and two pictures written into one line share that band side by side — a before
and an after being what a line of two is usually for. Until a picture arrives, or for good where
nothing can draw it, those rows say what it stands for: its alt text, or its address where it has none.

:::note

A picture is drawn only where the whole of it is in the pane. What reaches the terminal as a graphics
protocol is placed at a cell rather than written into the frame, which no pane can clip — so half a
picture would be drawn over whatever the pane sits inside of.

:::

## Keys

`↑↓` move a row, `PgUp`/`PgDn` ten of them, `Home`/`End` go to the ends, and the wheel scrolls while
the pointer is over it — all from the [keymap](keyboard.md#the-keymap), through the pane inside.

## Reading rather than editing

`MarkdownView` shows a document; it does not edit one. A file to be changed is
[text](text-view.md), and the marks in it are what the writer typed.
