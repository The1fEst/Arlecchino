---
title: ArlecchinoTestHost
sidebar_label: ArlecchinoTestHost
---

# ArlecchinoTestHost class

**Namespace:** `Arlecchino.Testing` &middot; **Assembly:** `Arlecchino.Testing`

A whole application wired up for a test: real services, a terminal in memory, and no loop running in the background. Frames are drawn when asked for rather than on a timer, so a test presses keys and then looks at what would be on screen, with nothing to wait for and nothing to race against.

```csharp
public sealed class ArlecchinoTestHost : IDisposable
```

**Implements** `IDisposable`

## Constructors

| Member | Summary |
|---|---|
| [`ArlecchinoTestHost(int, int, Action<ArlecchinoBuilder>)`](#arlecchinotesthost-int-int-action-arlecchinobuilder) | Builds the application. The minimum size is dropped to one cell, so a test can work in a window far smaller than a real one without hitting the too-small notice. Colour is fixed at [`ColorSupport.TrueColor`](../arlecchino.rendering/ColorSupport.md) so that frames do not change with the environment the test runs in — assign [`TerminalCapabilities.Color`](../arlecchino.rendering/TerminalCapabilities.md#color) afterwards to test another level. |

## Properties

| Member | Summary |
|---|---|
| [`Clock`](#clock) | The clock scheduled work runs on, moved by [`ArlecchinoTestHost.Advance`](../arlecchino.testing/ArlecchinoTestHost.md#advance-timespan). |
| [`History`](#history) | Undo history. It is resolved as the host is built, so edits are recorded from the start. |
| [`Navigator`](#navigator) | Navigation, for checking or forcing which view is current. |
| [`Options`](#options) | The settings, for changing them after the application is built. |
| [`Repaint`](#repaint) | The repaint flag, for checking that something actually asked for a frame. |
| [`Screen`](#screen) | What is on screen after every frame drawn so far. [`ArlecchinoTestHost.FrameLines`](../arlecchino.testing/ArlecchinoTestHost.md#framelines) reads the last frame as it was written, which is the whole picture only while frames are written whole; this is the picture itself, diffed frames and all. |
| [`Services`](#services) | The container, for reaching whatever the test registered. |
| [`State`](#state) | The shared state, for opening dialogs or reading the output line. |
| [`Surface`](#surface) | The cell grid, for tests that draw into it directly. |
| [`Terminal`](#terminal) | The terminal being drawn to, for asserting on raw output or resizing mid-test. |

## Methods

| Member | Summary |
|---|---|
| [`Advance(TimeSpan)`](#advance-timespan) | Moves the clock forward and runs whatever fell due, exactly as the frame loop would. The frame is not drawn by this — ask for one afterwards. |
| [`Click(int, int, MouseButton)`](#click-int-int-mousebutton) | Clicks a cell, in the terminal's own coordinates. |
| [`Dispose()`](#dispose) | Disposes the container and everything in it, and drops work still posted to the frame. |
| [`DrainInput()`](#draininput) | Routes whatever the reader has queued, which is what the frame loop does before it draws. [`ArlecchinoTestHost.ReadFromTerminal`](../arlecchino.testing/ArlecchinoTestHost.md#readfromterminal-string) and [`ArlecchinoTestHost.Frame`](../arlecchino.testing/ArlecchinoTestHost.md#frame) do it for you; call it yourself after driving `TerminalInputReader` directly, since the reader queues rather than routes. |
| [`Frame()`](#frame) | Draws a frame the way a running application does — as the difference from the last one — and returns what is on screen afterwards, styling and all stripped away. The frame written and the screen returned are not the same thing, and that is the point: an idle frame writes nothing at all, and a frame that changed one cell writes one cell. Reading the screen is what lets a test assert on the whole picture regardless. |
| [`FrameContains(string)`](#framecontains-string) | Whether a frame holds some text anywhere. Text split across rows will not be found. |
| [`FrameLineContaining(string)`](#framelinecontaining-string) | The first row holding some text, which is how a test reads what was drawn beside a label. |
| [`FrameLines()`](#framelines) | Draws a frame and returns the rows on screen afterwards. |
| [`Press(ConsoleKey, bool, bool, bool)`](#press-consolekey-bool-bool-bool) | Presses a key, routed exactly as a real one would be. |
| [`ReadFromTerminal(string)`](#readfromterminal-string) | Feeds raw characters through the reader that recognises escape sequences. This is the way to test what a real terminal sends for arrows, function keys and mouse reports. |
| [`Scroll(int, int, bool)`](#scroll-int-int-bool) | Turns the wheel over a cell. |
| [`Send(ConsoleKeyInfo)`](#send-consolekeyinfo) | Routes a key exactly as the terminal reported it, character and all. [`ArlecchinoTestHost.Press`](../arlecchino.testing/ArlecchinoTestHost.md#press-consolekey-bool-bool-bool) and [`ArlecchinoTestHost.Type`](../arlecchino.testing/ArlecchinoTestHost.md#type-string) cover what a test writes by hand; this is for one played back from a [`SessionTape`](../arlecchino.testing/SessionTape.md), where the character and the key both matter. |
| [`Send(MouseEvent)`](#send-mouseevent) | Routes a mouse event exactly as the terminal reported it. |
| [`SendPaste(string)`](#sendpaste-string) | Pastes a block of text, as bracketed paste delivers it. |
| [`Styles()`](#styles) | Draws a frame whole and returns the colour sequences in it, in order. Whole rather than diffed on purpose: a diffed frame only restates the styles of the cells it rewrites, so the sequences in it are the ones that changed rather than the ones the frame is drawn in. |
| [`Type(string)`](#type-string) | Types text one character at a time. The presses carry a character but no key, which is what a terminal reports for ordinary typing. |

## Constructors in detail

### `ArlecchinoTestHost(int, int, Action<ArlecchinoBuilder>)` {#arlecchinotesthost-int-int-action-arlecchinobuilder}

```csharp
public ArlecchinoTestHost(
    int width = 80,
    int height = 24,
    Action<ArlecchinoBuilder>? configure = null);
```

Builds the application. The minimum size is dropped to one cell, so a test can work in a window far smaller than a real one without hitting the too-small notice. Colour is fixed at [`ColorSupport.TrueColor`](../arlecchino.rendering/ColorSupport.md) so that frames do not change with the environment the test runs in — assign [`TerminalCapabilities.Color`](../arlecchino.rendering/TerminalCapabilities.md#color) afterwards to test another level.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `width` | `int` | Columns of the fake terminal. |
| `height` | `int` | Rows of the fake terminal. |
| `configure` | `Action<T>`&lt;[`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md)&gt; | Registers the views, commands and services under test. |

## Properties in detail

### `Clock` {#clock}

```csharp
public TestClock Clock { get; }
```

The clock scheduled work runs on, moved by [`ArlecchinoTestHost.Advance`](../arlecchino.testing/ArlecchinoTestHost.md#advance-timespan).

**Type** [`TestClock`](../arlecchino.testing/TestClock.md)

### `History` {#history}

```csharp
public AtomHistory History { get; }
```

Undo history. It is resolved as the host is built, so edits are recorded from the start.

**Type** [`AtomHistory`](../arlecchino.atoms/AtomHistory.md)

### `Navigator` {#navigator}

```csharp
public Navigator Navigator { get; }
```

Navigation, for checking or forcing which view is current.

**Type** [`Navigator`](../arlecchino.navigation/Navigator.md)

### `Options` {#options}

```csharp
public ArlecchinoOptions Options { get; }
```

The settings, for changing them after the application is built.

**Type** [`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md)

### `Repaint` {#repaint}

```csharp
public Repaint Repaint { get; }
```

The repaint flag, for checking that something actually asked for a frame.

**Type** [`Repaint`](../arlecchino/Repaint.md)

### `Screen` {#screen}

```csharp
public ScreenGrid Screen { get; }
```

What is on screen after every frame drawn so far. [`ArlecchinoTestHost.FrameLines`](../arlecchino.testing/ArlecchinoTestHost.md#framelines) reads the last frame as it was written, which is the whole picture only while frames are written whole; this is the picture itself, diffed frames and all.

**Type** [`ScreenGrid`](../arlecchino.testing/ScreenGrid.md)

### `Services` {#services}

```csharp
public IServiceProvider Services { get; }
```

The container, for reaching whatever the test registered.

**Type** `IServiceProvider`

### `State` {#state}

```csharp
public ArlecchinoState State { get; }
```

The shared state, for opening dialogs or reading the output line.

**Type** [`ArlecchinoState`](../arlecchino.state/ArlecchinoState.md)

### `Surface` {#surface}

```csharp
public Surface Surface { get; }
```

The cell grid, for tests that draw into it directly.

**Type** [`Surface`](../arlecchino.rendering/Surface.md)

### `Terminal` {#terminal}

```csharp
public FakeTerminal Terminal { get; }
```

The terminal being drawn to, for asserting on raw output or resizing mid-test.

**Type** [`FakeTerminal`](../arlecchino.testing/FakeTerminal.md)

## Methods in detail

### `Advance(TimeSpan)` {#advance-timespan}

```csharp
public void Advance(TimeSpan amount);
```

Moves the clock forward and runs whatever fell due, exactly as the frame loop would. The frame is not drawn by this — ask for one afterwards.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `amount` | `TimeSpan` | How far to move the clock. |

### `Click(int, int, MouseButton)` {#click-int-int-mousebutton}

```csharp
public void Click(int row, int column, MouseButton button = Left);
```

Clicks a cell, in the terminal's own coordinates.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row, counted from the top of the terminal. |
| `column` | `int` | Column, counted from its left edge. |
| `button` | [`MouseButton`](../arlecchino.input/MouseButton.md) | Which button was pressed. |

### `Dispose()` {#dispose}

```csharp
public void Dispose();
```

Disposes the container and everything in it, and drops work still posted to the frame.

### `DrainInput()` {#draininput}

```csharp
public void DrainInput();
```

Routes whatever the reader has queued, which is what the frame loop does before it draws. [`ArlecchinoTestHost.ReadFromTerminal`](../arlecchino.testing/ArlecchinoTestHost.md#readfromterminal-string) and [`ArlecchinoTestHost.Frame`](../arlecchino.testing/ArlecchinoTestHost.md#frame) do it for you; call it yourself after driving `TerminalInputReader` directly, since the reader queues rather than routes.

### `Frame()` {#frame}

```csharp
public string Frame();
```

Draws a frame the way a running application does — as the difference from the last one — and returns what is on screen afterwards, styling and all stripped away. The frame written and the screen returned are not the same thing, and that is the point: an idle frame writes nothing at all, and a frame that changed one cell writes one cell. Reading the screen is what lets a test assert on the whole picture regardless.

**Returns** `string` — The screen.

### `FrameContains(string)` {#framecontains-string}

```csharp
public bool FrameContains(string text);
```

Whether a frame holds some text anywhere. Text split across rows will not be found.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to look for. |

**Returns** `bool` — `true` when it is there.

### `FrameLineContaining(string)` {#framelinecontaining-string}

```csharp
public string FrameLineContaining(string text);
```

The first row holding some text, which is how a test reads what was drawn beside a label.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to look for. |

**Returns** `string` — The whole row, or an empty string when no row holds it.

### `FrameLines()` {#framelines}

```csharp
public string[] FrameLines();
```

Draws a frame and returns the rows on screen afterwards.

**Returns** `string`\[\] — One string per row.

### `Press(ConsoleKey, bool, bool, bool)` {#press-consolekey-bool-bool-bool}

```csharp
public void Press(ConsoleKey key, bool shift = false, bool alt = false, bool control = false);
```

Presses a key, routed exactly as a real one would be.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKey` | The key. |
| `shift` | `bool` | Whether Shift was held. |
| `alt` | `bool` | Whether Alt was held. |
| `control` | `bool` | Whether Ctrl was held. |

### `ReadFromTerminal(string)` {#readfromterminal-string}

```csharp
public void ReadFromTerminal(string sequence);
```

Feeds raw characters through the reader that recognises escape sequences. This is the way to test what a real terminal sends for arrows, function keys and mouse reports.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `sequence` | `string` | The characters, escapes included. |

### `Scroll(int, int, bool)` {#scroll-int-int-bool}

```csharp
public void Scroll(int row, int column, bool down);
```

Turns the wheel over a cell.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `row` | `int` | Row the pointer is over. |
| `column` | `int` | Column the pointer is over. |
| `down` | `bool` | Whether the wheel turned down. |

### `Send(ConsoleKeyInfo)` {#send-consolekeyinfo}

```csharp
public void Send(ConsoleKeyInfo key);
```

Routes a key exactly as the terminal reported it, character and all. [`ArlecchinoTestHost.Press`](../arlecchino.testing/ArlecchinoTestHost.md#press-consolekey-bool-bool-bool) and [`ArlecchinoTestHost.Type`](../arlecchino.testing/ArlecchinoTestHost.md#type-string) cover what a test writes by hand; this is for one played back from a [`SessionTape`](../arlecchino.testing/SessionTape.md), where the character and the key both matter.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key as the terminal reported it. |

### `Send(MouseEvent)` {#send-mouseevent}

```csharp
public void Send(MouseEvent mouse);
```

Routes a mouse event exactly as the terminal reported it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mouse` | [`MouseEvent`](../arlecchino.input/MouseEvent.md) | The event. |

### `SendPaste(string)` {#sendpaste-string}

```csharp
public void SendPaste(string text);
```

Pastes a block of text, as bracketed paste delivers it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What was pasted. |

### `Styles()` {#styles}

```csharp
public IReadOnlyList<string> Styles();
```

Draws a frame whole and returns the colour sequences in it, in order. Whole rather than diffed on purpose: a diffed frame only restates the styles of the cells it rewrites, so the sequences in it are the ones that changed rather than the ones the frame is drawn in.

**Returns** `IReadOnlyList<T>`&lt;`string`&gt; — The sequences as they appeared.

### `Type(string)` {#type-string}

```csharp
public void Type(string text);
```

Types text one character at a time. The presses carry a character but no key, which is what a terminal reports for ordinary typing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | What to type. |

