---
title: Theming
sidebar_label: Theming
description: Theme and ThemePalette — the twelve roles a view draws with, the framework's own palette, and how to swap it.
---

# Theming

Every drawing call takes an `IArlecchinoColor`. `Theme` is the static accessor views read from, and
`ThemePalette` is the object behind it. Views should pick a **role**, not a color: swapping the
palette then restyles the whole application, chrome included.

## Roles

| Role | Default | Used for |
|---|---|---|
| `Default` | terminal default foreground and background | Ordinary text |
| `Header` | crimson, bold | Screen titles |
| `TableHeader` | bone, bold | Column headers |
| `Accent` | bone | Text that stands out without being alarming |
| `Info` | ash | Box borders and structural lines |
| `Muted` | ash | Hints, footers, secondary text |
| `Input` | ink on bone | The editable part of a text field |
| `Caret` | black on white | The symbol the caret stands on in a [line of text](editing.md) |
| `Selected` | bone on hairline | The cursor row of an unfocused pane, and what is selected in a line |
| `Active` | crimson | Something switched on or available |
| `ActiveSelected` | ink on ash | The cursor row of the focused pane |
| `Warning` | ink on amber | The output line when it carries text |
| `Error` | bone on crimson | Modal validation messages |

## Swapping the palette

```csharp
builder.Services
    .AddArlecchino()
    .UseTheme(new ThemePalette
    {
        Header = new TermColor { Foreground = TerminalColor.BrightCyan, Style = TextStyle.Bold },
        Selected = new TermColor { Background = TerminalColor.Blue },
    });
```

`ThemePalette` properties are `init`-only and each has a default, so a partial palette is a valid one.
`Theme.Palette` is assigned when `ArlecchinoOptions` is resolved from the container, which is why
`Theme.Header` works from a view without anything plumbed through to it. Assigning `Theme.Palette`
directly also works when there is no container at all.

:::warning[The palette is process-wide]

`Theme.Palette` and `TerminalCapabilities.Color` are process-wide **on purpose**: that is what lets a
view write `Theme.Header` with nothing passed to it. The price is that one process hosts one look —
two hosts side by side share the palette and the color level, and the last one built wins. A test
that changes either of them shares the change with whatever else is running, which is why the
[test host](testing.md) pins the color level as it builds.

:::

## The framework's own palette

The defaults are the harlequin mask in colors — crimson `#C9382B`, bone `#EDE6D9`, ink `#141317` and
the hairline `#2E2B33` of the brand assets. An application gets them without asking;
`ThemePalette.Arlecchino` is the same thing under a name, for saying so out loud:

```csharp
builder.Services
    .AddArlecchino()
    .UseTheme(ThemePalette.Arlecchino);
```

The background is left to the terminal everywhere except the two cursor rows, which have to paint
behind their text to be a selection at all. That is what makes it sit on a light terminal as readily
as on a dark one — it colors the writing, not the screen.

Every entry names an exact color *and* a palette color, so a terminal without 24-bit shows the
fallback its author picked rather than whatever the nearest-color arithmetic lands on. Crimson falls
back to `BrightRed`, bone to `BrightWhite`, the hairline to `BrightBlack`. How that is expressed is on
[Colors](colors.md).

Crimson is spent on one thing at a time: titles, `Active`, and `Error`. The cursor row is deliberately
**not** crimson — a selection that looks like a failure is a screen you have to read twice:

| Role | Color | Reads as |
|---|---|---|
| `ActiveSelected` | ink on ash `#8A8189` | Where the cursor is |
| `Selected` | bone on hairline `#2E2B33` | Where it was, in the pane without focus |
| `Warning` | ink on amber `#D08A2C` | Worth noticing — deprecated, drifted |
| `Error` | bone on crimson `#C9382B` | Something is wrong |

## The sixteen plain colors

What was the default before 2.0 is still there as `ThemePalette.Basic` — magenta titles, blue column
headers, cyan borders, a green cursor row, and nothing exact behind any of them:

```csharp
builder.Services.AddArlecchino().UseTheme(ThemePalette.Basic);
```

That is the whole of the way back. See [Migrating to 2.0](migrating-to-2.0.md).

## Writing a palette of your own

Start from the defaults and override what matters. Two rules keep a palette usable:

- **Say both colors.** An `Exact` color with a palette color behind it degrades the way you chose.
- **Keep the background out of it** except where a row has to read as selected. A palette that paints
  the screen fights every terminal it did not expect.

```csharp
public static class Solarized
{
    public static ThemePalette Palette { get; } = new()
    {
        Header = new TermColor
        {
            Foreground = TerminalColor.BrightYellow,
            ExactForeground = new Rgb(0xB5, 0x89, 0x00),
            Style = TextStyle.Bold,
        },
        Info = new TermColor
        {
            Foreground = TerminalColor.BrightBlack,
            ExactForeground = new Rgb(0x58, 0x6E, 0x75),
        },
    };
}
```
