---
title: "ArlecchinoOptions"
sidebar_label: "ArlecchinoOptions"
---

# ArlecchinoOptions class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Everything the framework can be told about an application. Configure it in the `AddArlecchino` callback; most of it also has a builder call.

```csharp
public sealed class ArlecchinoOptions
```

## Constructors

| Member | Summary |
|---|---|
| [`ArlecchinoOptions()`](#arlecchinooptions) |  |

## Properties

| Member | Summary |
|---|---|
| [`AskTerminal`](#askterminal) | Whether to ask the terminal what it can do as the application starts. It costs at most [`ArlecchinoOptions.TerminalAnswer`](../arlecchino.hosting/ArlecchinoOptions.md#terminalanswer), and only on a terminal that stays silent. |
| [`BracketedPaste`](#bracketedpaste) | Whether pasted text arrives as one block. On by default: without it a paste reads as a burst of key presses, and a long one can trip validation or a shortcut halfway through. |
| [`CellHeight`](#cellheight) | How many pixels tall a cell is taken to be, installed into [`Glyphs.CellHeight`](../arlecchino.rendering.text/Glyphs.md#cellheight) on resolve. See [`ArlecchinoOptions.CellWidth`](../arlecchino.hosting/ArlecchinoOptions.md#cellwidth). |
| [`CellWidth`](#cellwidth) | How many pixels wide a cell is taken to be, installed into [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth) on resolve. Only [`ImageProtocol.Sixel`](../arlecchino.rendering.terminals/ImageProtocol.md) reads it, since sixel is measured in pixels. |
| [`CommandPaletteKey`](#commandpalettekey) | Character that opens the command palette. A character rather than a binding, so it survives a layout where the key sits elsewhere. |
| [`EscapeTimeout`](#escapetimeout) | How long to wait for the rest of an escape sequence before deciding there is none. It is also the delay a lone `Esc` costs, so keep it short. |
| [`GraphSymbols`](#graphsymbols) | What graphs are drawn with. Installed into [`Glyphs.Graph`](../arlecchino.rendering.text/Glyphs.md#graph) on resolve, and settable afterward, so an application can offer the choice in its own settings. |
| [`Hints`](#hints) | When to draw the box of keys in the bottom-right corner. |
| [`HorizontalPadding`](#horizontalpadding) | Cells kept free on the left and right of the content area. |
| [`ImageProtocol`](#imageprotocol) | How pictures reach the terminal, installed into [`Glyphs.Picture`](../arlecchino.rendering.text/Glyphs.md#picture) on resolve and settable afterward. [`ImageProtocol.Auto`](../arlecchino.rendering.terminals/ImageProtocol.md) asks the terminal rather than guessing. |
| [`InputPollInterval`](#inputpollinterval) | How long the input loop sleeps when no key is waiting. |
| [`Keymap`](#keymap) | Keys the framework itself reacts to. |
| [`MinimumHeight`](#minimumheight) | Below this height the view is replaced by a "make the window bigger" notice. |
| [`MinimumWidth`](#minimumwidth) | Below this width the view is replaced by a "make the window bigger" notice. |
| [`MouseInput`](#mouseinput) | Whether to report mouse events. Off by default, because with it on the terminal stops handling selection itself and copying text with the mouse no longer works the way the user expects. |
| [`NotificationLifetime`](#notificationlifetime) | How long a notification stays in the list behind the output row — long enough to go and read what went past while the screen was busy. |
| [`NotificationTimeout`](#notificationtimeout) | How long a notification stays on the output row. Once it is up the row goes quiet, and the message is only in the notifications screen. |
| [`ShowOutputLine`](#showoutputline) | Whether to keep the last row for `ArlecchinoState.Output`. |
| [`StartRoute`](#startroute) | Route shown on the first frame. For a start that depends on state, use a startup. |
| [`Strings`](#strings) | Every piece of text the framework draws. |
| [`TargetFramesPerSecond`](#targetframespersecond) | How often the loop may draw. Frames are only composed when something asked for one. |
| [`TerminalAnswer`](#terminalanswer) | How long to wait for the terminal to finish answering. See [`ArlecchinoOptions.AskTerminal`](../arlecchino.hosting/ArlecchinoOptions.md#askterminal). |
| [`TextInput`](#textinput) | How a key press becomes a character on a non-latin layout. Whatever the terminal reports is taken by default, so any language can be typed without the application asking for it; [`ArlecchinoBuilder.UseKeysByPosition`](../arlecchino.hosting/ArlecchinoBuilder.md#usekeysbyposition) trades that for keys that always read the same. |
| [`Theme`](#theme) | Colors behind the roles. Installed into [`ArlecchinoOptions.Theme`](../arlecchino.hosting/ArlecchinoOptions.md#theme) on resolve. |
| [`UseAlternateScreen`](#usealternatescreen) | Whether to run on the alternate screen, which leaves the user's scrollback untouched on exit. |
| [`VerticalPadding`](#verticalpadding) | Rows kept free above and below the content area. |

## Constructors in detail

### `ArlecchinoOptions()` {#arlecchinooptions}

```csharp
public ArlecchinoOptions();
```

## Properties in detail

### `AskTerminal` {#askterminal}

```csharp
public bool AskTerminal { get; set; }
```

Whether to ask the terminal what it can do as the application starts. It costs at most [`ArlecchinoOptions.TerminalAnswer`](../arlecchino.hosting/ArlecchinoOptions.md#terminalanswer), and only on a terminal that stays silent.

**Type** `bool`

### `BracketedPaste` {#bracketedpaste}

```csharp
public bool BracketedPaste { get; set; }
```

Whether pasted text arrives as one block. On by default: without it a paste reads as a burst of key presses, and a long one can trip validation or a shortcut halfway through.

**Type** `bool`

### `CellHeight` {#cellheight}

```csharp
public int CellHeight { get; set; }
```

How many pixels tall a cell is taken to be, installed into [`Glyphs.CellHeight`](../arlecchino.rendering.text/Glyphs.md#cellheight) on resolve. See [`ArlecchinoOptions.CellWidth`](../arlecchino.hosting/ArlecchinoOptions.md#cellwidth).

**Type** `int`

### `CellWidth` {#cellwidth}

```csharp
public int CellWidth { get; set; }
```

How many pixels wide a cell is taken to be, installed into [`Glyphs.CellWidth`](../arlecchino.rendering.text/Glyphs.md#cellwidth) on resolve. Only [`ImageProtocol.Sixel`](../arlecchino.rendering.terminals/ImageProtocol.md) reads it, since sixel is measured in pixels.

**Type** `int`

### `CommandPaletteKey` {#commandpalettekey}

```csharp
public char CommandPaletteKey { get; set; }
```

Character that opens the command palette. A character rather than a binding, so it survives a layout where the key sits elsewhere.

**Type** `char`

### `EscapeTimeout` {#escapetimeout}

```csharp
public TimeSpan EscapeTimeout { get; set; }
```

How long to wait for the rest of an escape sequence before deciding there is none. It is also the delay a lone `Esc` costs, so keep it short.

**Type** `TimeSpan`

### `GraphSymbols` {#graphsymbols}

```csharp
public GraphSymbols GraphSymbols { get; set; }
```

What graphs are drawn with. Installed into [`Glyphs.Graph`](../arlecchino.rendering.text/Glyphs.md#graph) on resolve, and settable afterward, so an application can offer the choice in its own settings.

**Type** [`GraphSymbols`](../arlecchino.rendering.text/GraphSymbols.md)

### `Hints` {#hints}

```csharp
public HintsShown Hints { get; set; }
```

When to draw the box of keys in the bottom-right corner.

**Type** [`HintsShown`](../arlecchino.hosting/HintsShown.md)

### `HorizontalPadding` {#horizontalpadding}

```csharp
public int HorizontalPadding { get; set; }
```

Cells kept free on the left and right of the content area.

**Type** `int`

### `ImageProtocol` {#imageprotocol}

```csharp
public ImageProtocol ImageProtocol { get; set; }
```

How pictures reach the terminal, installed into [`Glyphs.Picture`](../arlecchino.rendering.text/Glyphs.md#picture) on resolve and settable afterward. [`ImageProtocol.Auto`](../arlecchino.rendering.terminals/ImageProtocol.md) asks the terminal rather than guessing.

**Type** [`ImageProtocol`](../arlecchino.rendering.terminals/ImageProtocol.md)

### `InputPollInterval` {#inputpollinterval}

```csharp
public TimeSpan InputPollInterval { get; set; }
```

How long the input loop sleeps when no key is waiting.

**Type** `TimeSpan`

### `Keymap` {#keymap}

```csharp
public ArlecchinoKeymap Keymap { get; set; }
```

Keys the framework itself reacts to.

**Type** [`ArlecchinoKeymap`](../arlecchino.hosting/ArlecchinoKeymap.md)

### `MinimumHeight` {#minimumheight}

```csharp
public int MinimumHeight { get; set; }
```

Below this height the view is replaced by a "make the window bigger" notice.

**Type** `int`

### `MinimumWidth` {#minimumwidth}

```csharp
public int MinimumWidth { get; set; }
```

Below this width the view is replaced by a "make the window bigger" notice.

**Type** `int`

### `MouseInput` {#mouseinput}

```csharp
public bool MouseInput { get; set; }
```

Whether to report mouse events. Off by default, because with it on the terminal stops handling selection itself and copying text with the mouse no longer works the way the user expects.

**Type** `bool`

### `NotificationLifetime` {#notificationlifetime}

```csharp
public TimeSpan NotificationLifetime { get; set; }
```

How long a notification stays in the list behind the output row — long enough to go and read what went past while the screen was busy.

**Type** `TimeSpan`

### `NotificationTimeout` {#notificationtimeout}

```csharp
public TimeSpan NotificationTimeout { get; set; }
```

How long a notification stays on the output row. Once it is up the row goes quiet, and the message is only in the notifications screen.

**Type** `TimeSpan`

### `ShowOutputLine` {#showoutputline}

```csharp
public bool ShowOutputLine { get; set; }
```

Whether to keep the last row for `ArlecchinoState.Output`.

**Type** `bool`

### `StartRoute` {#startroute}

```csharp
public ViewRoute StartRoute { get; set; }
```

Route shown on the first frame. For a start that depends on state, use a startup.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

### `Strings` {#strings}

```csharp
public ArlecchinoStrings Strings { get; set; }
```

Every piece of text the framework draws.

**Type** [`ArlecchinoStrings`](../arlecchino.hosting/ArlecchinoStrings.md)

### `TargetFramesPerSecond` {#targetframespersecond}

```csharp
public int TargetFramesPerSecond { get; set; }
```

How often the loop may draw. Frames are only composed when something asked for one.

**Type** `int`

### `TerminalAnswer` {#terminalanswer}

```csharp
public TimeSpan TerminalAnswer { get; set; }
```

How long to wait for the terminal to finish answering. See [`ArlecchinoOptions.AskTerminal`](../arlecchino.hosting/ArlecchinoOptions.md#askterminal).

**Type** `TimeSpan`

### `TextInput` {#textinput}

```csharp
public TextInputMode TextInput { get; set; }
```

How a key press becomes a character on a non-latin layout. Whatever the terminal reports is taken by default, so any language can be typed without the application asking for it; [`ArlecchinoBuilder.UseKeysByPosition`](../arlecchino.hosting/ArlecchinoBuilder.md#usekeysbyposition) trades that for keys that always read the same.

**Type** [`TextInputMode`](../arlecchino.input/TextInputMode.md)

### `Theme` {#theme}

```csharp
public ThemePalette Theme { get; set; }
```

Colors behind the roles. Installed into [`ArlecchinoOptions.Theme`](../arlecchino.hosting/ArlecchinoOptions.md#theme) on resolve.

**Type** [`ThemePalette`](../arlecchino.rendering.colors/ThemePalette.md)

### `UseAlternateScreen` {#usealternatescreen}

```csharp
public bool UseAlternateScreen { get; set; }
```

Whether to run on the alternate screen, which leaves the user's scrollback untouched on exit.

**Type** `bool`

### `VerticalPadding` {#verticalpadding}

```csharp
public int VerticalPadding { get; set; }
```

Rows kept free above and below the content area.

**Type** `int`

