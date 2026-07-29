---
title: ArlecchinoOptions
sidebar_label: ArlecchinoOptions
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
| [`BracketedPaste`](#bracketedpaste) | Whether pasted text arrives as one block. On by default: without it a paste reads as a burst of key presses, and a long one can trip validation or a shortcut halfway through. |
| [`CommandPaletteKey`](#commandpalettekey) | Character that opens the command palette. A character rather than a binding, so it survives a layout where the key sits elsewhere. |
| [`EscapeTimeout`](#escapetimeout) | How long to wait for the rest of an escape sequence before deciding there is none. Arrows and function keys arrive as several characters, and over a slow link they do not always arrive together; this is also the delay a lone `Esc` costs, so keep it short. |
| [`GraphSymbols`](#graphsymbols) | What graphs are drawn with. Installed into [`Glyphs.Graph`](../arlecchino.rendering/Glyphs.md#graph) on resolve, and settable afterwards, so an application can offer the choice in its own settings. |
| [`HorizontalPadding`](#horizontalpadding) | Cells kept free on the left and right of the content area. |
| [`InputPollInterval`](#inputpollinterval) | How long the input loop sleeps when no key is waiting. |
| [`Keymap`](#keymap) | Keys the framework itself reacts to. |
| [`MinimumHeight`](#minimumheight) | Below this height the view is replaced by a "make the window bigger" notice. |
| [`MinimumWidth`](#minimumwidth) | Below this width the view is replaced by a "make the window bigger" notice. |
| [`MouseInput`](#mouseinput) | Whether to report mouse events. Off by default, because with it on the terminal stops handling selection itself and copying text with the mouse no longer works the way the user expects. |
| [`NotificationLifetime`](#notificationlifetime) | How long a notification stays in the list behind the output row — long enough to go and read what went past while the screen was busy. |
| [`NotificationTimeout`](#notificationtimeout) | How long a notification stays on the output row. Once it is up the row goes quiet, and the message is only in the notifications screen. |
| [`ShowHints`](#showhints) | Whether to draw the hints box in the bottom-right corner. |
| [`ShowOutputLine`](#showoutputline) | Whether to keep the last row for `ArlecchinoState.Output`. |
| [`StartRoute`](#startroute) | Route shown on the first frame. For a start that depends on state, use a startup. |
| [`Strings`](#strings) | Every piece of text the framework draws. |
| [`TargetFramesPerSecond`](#targetframespersecond) | How often the loop may draw. Frames are only composed when something asked for one. |
| [`TextInput`](#textinput) | How a key press becomes a character on a non-latin layout. |
| [`Theme`](#theme) | Colours behind the roles. Installed into [`Theme`](../arlecchino.rendering/Theme.md) on resolve. |
| [`UseAlternateScreen`](#usealternatescreen) | Whether to run on the alternate screen, which leaves the user's scrollback untouched on exit. |
| [`VerticalPadding`](#verticalpadding) | Rows kept free above and below the content area. |

## Constructors in detail

### `ArlecchinoOptions()` {#arlecchinooptions}

```csharp
public ArlecchinoOptions();
```

## Properties in detail

### `BracketedPaste` {#bracketedpaste}

```csharp
public bool BracketedPaste { get; set; }
```

Whether pasted text arrives as one block. On by default: without it a paste reads as a burst of key presses, and a long one can trip validation or a shortcut halfway through.

**Type** `bool`

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

How long to wait for the rest of an escape sequence before deciding there is none. Arrows and function keys arrive as several characters, and over a slow link they do not always arrive together; this is also the delay a lone `Esc` costs, so keep it short.

**Type** `TimeSpan`

### `GraphSymbols` {#graphsymbols}

```csharp
public GraphSymbols GraphSymbols { get; set; }
```

What graphs are drawn with. Installed into [`Glyphs.Graph`](../arlecchino.rendering/Glyphs.md#graph) on resolve, and settable afterwards, so an application can offer the choice in its own settings.

**Type** [`GraphSymbols`](../arlecchino.rendering/GraphSymbols.md)

### `HorizontalPadding` {#horizontalpadding}

```csharp
public int HorizontalPadding { get; set; }
```

Cells kept free on the left and right of the content area.

**Type** `int`

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

### `ShowHints` {#showhints}

```csharp
public bool ShowHints { get; set; }
```

Whether to draw the hints box in the bottom-right corner.

**Type** `bool`

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

### `TextInput` {#textinput}

```csharp
public TextInputMode TextInput { get; set; }
```

How a key press becomes a character on a non-latin layout.

**Type** [`TextInputMode`](../arlecchino.input/TextInputMode.md)

### `Theme` {#theme}

```csharp
public ThemePalette Theme { get; set; }
```

Colours behind the roles. Installed into [`Theme`](../arlecchino.rendering/Theme.md) on resolve.

**Type** [`ThemePalette`](../arlecchino.rendering/ThemePalette.md)

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

