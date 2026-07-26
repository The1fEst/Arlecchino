[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Theming

Every drawing call takes an `ITermColor`. `Theme` is the static accessor views read from, and
`ThemePalette` is the object behind it.

## Roles

| Role | Default |
|---|---|
| `Default` | terminal default foreground and background |
| `Header` | bright magenta, bold |
| `TableHeader` | bright blue, bold |
| `Accent` | bright white |
| `Info` | cyan — used for box borders |
| `Muted` | bright black — used for footers and hints |
| `Input` | black on blue — the text-modal input line |
| `Selected` | bright black background |
| `Active` | green |
| `ActiveSelected` | black on green |
| `Warning` | black on yellow — the output line when it carries text |
| `Error` | black on red — modal validation messages |

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

`ThemePalette` properties are `init`-only and each has a default, so a partial palette is a valid one.
`Theme.Palette` is assigned when `ArlecchinoOptions` is resolved from the container, which is why
`Theme.Header` works from a view without any plumbing. Assigning `Theme.Palette` directly also works
when there is no container at all.

## TermColor

```csharp
public sealed class TermColor : ITermColor
{
    public TerminalColor Foreground { get; init; } = TerminalColor.Default;
    public TerminalColor Background { get; init; } = TerminalColor.Default;
    public TextStyle Style { get; init; } = TextStyle.None;
}
```

`TerminalColor` is the sixteen-colour ANSI set — `Default`, the eight base colours, and their
`Bright` counterparts. `TextStyle` is a `[Flags]` enum of `Bold`, `Italic`, `Underline` and `Dim`.

## 24-bit colour

`ITermColor` is a single member — `string Ansi { get; }` — so a style is anything that can produce an
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
