[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Theming

Every drawing call takes an `IArlecchinoColor`. `Theme` is the static accessor views read from, and
`ThemePalette` is the object behind it.

## Roles

| Role | Default |
|---|---|
| `Default` | terminal default foreground and background |
| `Header` | crimson, bold |
| `TableHeader` | bone, bold |
| `Accent` | bone |
| `Info` | ash — used for box borders |
| `Muted` | ash — used for footers and hints |
| `Input` | ink on bone — the text-modal input line |
| `Selected` | bone on the hairline grey |
| `Active` | crimson |
| `ActiveSelected` | ink on ash |
| `Warning` | ink on amber — the output line when it carries text |
| `Error` | bone on crimson — modal validation messages |

Views should pick a role, not a colour: swapping the palette then restyles the whole app, chrome
included.

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

`Theme.Palette` and `TerminalCapabilities.Color` are **process-wide** on purpose: that is what lets a
view write `Theme.Header` with nothing plumbed through to it. The price is that one process hosts one
look — two hosts side by side share the palette and the colour level, and the last one built wins. A
test that changes either of them shares the change with whatever else is running, which is why the
[test host](packages-and-building.md) pins the colour level as it builds.

`ThemePalette` properties are `init`-only and each has a default, so a partial palette is a valid one.
`Theme.Palette` is assigned when `ArlecchinoOptions` is resolved from the container, which is why
`Theme.Header` works from a view without any plumbing. Assigning `Theme.Palette` directly also works
when there is no container at all.

### The framework's own palette

The defaults above are the harlequin mask in colours — crimson `#C9382B`, bone `#EDE6D9`, ink
`#141317` and the hairline `#2E2B33` of the [brand assets](../assets/README.md). An application gets
them without asking; `ThemePalette.Arlecchino` is the same thing under a name, for saying so out loud:

```csharp
builder.Services
    .AddArlecchino()
    .UseTheme(ThemePalette.Arlecchino);
```

The background is left to the terminal everywhere except the two cursor rows, which have to paint
behind their text to be a selection at all. That is what makes it sit on a light terminal as readily
as on a dark one — it colours the writing, not the screen.

The sixteen plain colours that were the default before 2.0 are still there as `ThemePalette.Basic` —
`UseTheme(ThemePalette.Basic)` is the whole of the way back to magenta titles and a green cursor row.

Every entry names an exact colour *and* a palette colour, so a terminal without 24-bit shows the
fallback its author picked rather than whatever the nearest-colour arithmetic lands on. Crimson falls
back to `BrightRed`, bone to `BrightWhite`, the hairline to `BrightBlack`.

Crimson is spent on one thing at a time: titles, `Active`, and `Error`. The cursor row is deliberately
**not** crimson — a selection that looks like a failure is a screen you have to read twice:

| Role | Colour | Reads as |
|---|---|---|
| `ActiveSelected` | ink on ash `#8A8189` | Where the cursor is |
| `Selected` | bone on hairline `#2E2B33` | Where it was, in the pane without focus |
| `Warning` | ink on amber `#D08A2C` | Worth noticing — deprecated, drifted |
| `Error` | bone on crimson `#C9382B` | Something is wrong |

## TermColor

```csharp
public sealed class TermColor : IArlecchinoColor
{
    public TerminalColor Foreground { get; init; } = TerminalColor.Default;
    public TerminalColor Background { get; init; } = TerminalColor.Default;
    public Rgb? ExactForeground { get; init; }
    public Rgb? ExactBackground { get; init; }
    public TextStyle Style { get; init; } = TextStyle.None;
}
```

The two `Exact` colours are drawn where the terminal can do 24-bit, and the palette colours beside
them are what a terminal without it gets. Setting both is how a palette says a brand colour and still
degrades to something its author chose:

```csharp
Header = new TermColor
{
    Foreground = TerminalColor.BrightRed,
    ExactForeground = new Rgb(0xC9, 0x38, 0x2B),
    Style = TextStyle.Bold,
};
```

`TerminalColor` is the sixteen-colour ANSI set — `Default`, the eight base colours, and their
`Bright` counterparts. `TextStyle` is a `[Flags]` enum of `Bold`, `Italic`, `Underline` and `Dim`.

## 24-bit colour

`IArlecchinoColor` is a single member — `string Ansi { get; }` — so a style is anything that can produce an
escape sequence. `RgbTermColor` is the second implementation that ships:

```csharp
_surface.WriteAt(row, column, "████", new RgbTermColor { Foreground = new Rgb(63, 169, 245) });
```

`Rgb` is a `(Red, Green, Blue)` record struct with `Hex`, `TryParseHex`, and `FromHsl` / `ToHsl`
conversions — it is what the [colour modal](modals-and-state.md) edits and hands back. `Foreground`
and `Background` are both optional, so a swatch is a background with no foreground.

Palette roles stay on the sixteen-colour set: they have to look right on a terminal the application
does not control. Reach for `RgbTermColor` where an exact colour is the point — a swatch, a chart, a
syntax highlighter — and keep chrome on `Theme`.

Both implementations cache the escape sequence, and the frame writer compares styles by reference, so
hold on to a style instance rather than building one per cell.

## What the terminal can actually do

`TerminalCapabilities.Color` decides how styles are emitted, and is detected once at startup:

| Level | When | Effect |
|---|---|---|
| `TrueColor` | `COLORTERM` is `truecolor` or `24bit`, or `WT_SESSION` is set | `RgbTermColor` emits 24-bit sequences |
| `Palette` | anything else | `RgbTermColor` falls back to the nearest of the sixteen colours |
| `None` | `NO_COLOR` is set, `TERM=dumb`, or the Windows console refused virtual terminal mode | no style sequence is emitted at all, including the per-line reset |

Set it yourself to override the guess:

```csharp
TerminalCapabilities.Color = ColorSupport.Palette;
```

On Windows `SystemTerminal` turns on `ENABLE_VIRTUAL_TERMINAL_PROCESSING` when it starts. If the
console refuses — an old `conhost`, say — colour drops to `None` and the alternate-screen sequences
are not written either, so the application degrades to plain text instead of spraying escape codes.

`TerminalCapabilities.NearestPaletteColor(rgb)` is the same conversion the fallback uses, available
for your own rendering.
