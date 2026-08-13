![Arlecchino](https://raw.githubusercontent.com/The1fEst/Arlecchino/master/assets/arlecchino-banner.png)

[![NuGet](https://img.shields.io/nuget/v/Arlecchino.Testing?logo=nuget&label=Arlecchino.Testing&color=C9382B&labelColor=141317)](https://www.nuget.org/packages/Arlecchino.Testing)
[![Downloads](https://img.shields.io/nuget/dt/Arlecchino.Testing?color=C9382B&labelColor=141317)](https://www.nuget.org/packages/Arlecchino.Testing)
[![Build](https://img.shields.io/github/actions/workflow/status/The1fEst/Arlecchino/build.yml?branch=master&logo=github&labelColor=141317)](https://github.com/The1fEst/Arlecchino/actions/workflows/build.yml)
![Target frameworks](https://img.shields.io/badge/net8.0%20%7C%20net10.0-512BD4?logo=dotnet&logoColor=white&labelColor=141317)
[![MIT](https://img.shields.io/badge/license-MIT-EDE6D9?labelColor=141317)](https://github.com/The1fEst/Arlecchino/blob/master/LICENSE)

The headless host for applications built on the
[Arlecchino](https://www.nuget.org/packages/Arlecchino) terminal UI framework. It builds the whole
application — container, views, navigation, modals, commands — against a terminal in memory, and
draws a frame when a test asks for one. There is nothing to wait for and nothing to race against.

## Quick start

```
dotnet add package Arlecchino.Testing
```

```csharp
using var app = new ArlecchinoTestHost(configure: arlecchino =>
    arlecchino.AddGeneratedViews().StartAt(ViewKind.Default));

Assert.Contains("hello", app.Frame(), StringComparison.Ordinal);

app.Press(ConsoleKey.A);

Assert.Equal(ViewKind.About, app.Navigator.CurrentRoute);
```

The `configure` delegate is the same `ArlecchinoBuilder` the real application configures, so a test
starts at any route, registers a fake in place of a service, or takes the application's own
registration wholesale. The size of the terminal is `new ArlecchinoTestHost(width: 120, height: 40)`,
and every test framework works — the host is a plain disposable class.

## What a test can reach

- **The frame as text.** `Frame()` is the screen as a string; `FrameLines()`, `FrameContains` and
  `FrameLineContaining` ask about it without picking the string apart, and `Styles()` lists the ANSI
  sequences the frame carried, so a test can assert on colour without matching escape codes.
- **Input.** `Press` with `shift`, `alt` and `control`, `Type` for text, `Click` and `Scroll` for
  the mouse, and `ReadFromTerminal` to feed a raw escape sequence the way a terminal would.
- **Time that does not pass on its own.** `Advance(TimeSpan.FromSeconds(1))` moves the clock rather
  than sleeping, and the next frame shows what fell due — timeouts, notification lifetimes and work
  on a timer are tested in microseconds.
- **The application itself.** `Services`, `Navigator`, `State`, `History` and `Options` are the
  live objects, so a test can undo, inspect a store or drive navigation directly.
- **The terminal.** `Terminal` is a `FakeTerminal`: `Written` is everything sent to it, `Copied` is
  what the application put on the clipboard, and its size can be changed mid-test to check a resize.
- **`FrameText`** — `WithoutStyles`, `Lines`, `BoxWidth` and the regexes behind them, for assertions
  written against the plain text of a frame.

## Packages

| Package | Contents |
|---|---|
| [`Arlecchino.Core`](https://www.nuget.org/packages/Arlecchino.Core) | `Surface`, `Theme`, `TermColor`, `KeyText`, `IArlecchinoTerminal` — the renderer, no DI — and atoms with their undo history |
| [`Arlecchino`](https://www.nuget.org/packages/Arlecchino) | views, navigation, modals, commands, hosting, DI, async stores, and the generator |
| [`Arlecchino.Pictures`](https://www.nuget.org/packages/Arlecchino.Pictures) | PNG, JPEG, BMP, Netpbm, QOI and Targa read into pixels |
| [`Arlecchino.Testing`](https://www.nuget.org/packages/Arlecchino.Testing) | this one — the headless host applications write their tests against |

They ship together and always carry the same version. This package belongs in the test project
only.

## Links

[Documentation](https://the1fest.github.io/Arlecchino.Docs/) ·
[Testing](https://the1fest.github.io/Arlecchino.Docs/docs/testing) ·
[Changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md) ·
[Source and issues](https://github.com/The1fEst/Arlecchino)

MIT.
