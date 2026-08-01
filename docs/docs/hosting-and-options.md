---
title: Hosting and options
sidebar_label: Hosting and options
description: AddArlecchino, every option, the builder API, and running without the hosted service.
---

# Hosting and options

## AddArlecchino

```csharp
builder.Services.AddArlecchino(options =>
{
    options.MinimumWidth = 60;
    options.MinimumHeight = 16;
});
```

One call registers everything and returns a `ArlecchinoBuilder` for the rest of the setup. The services
it puts in the container are singletons:

| Service | Role |
|---|---|
| `ArlecchinoOptions` | The configured options; resolving it also installs the theme palette |
| `IArlecchinoTerminal` | `SystemTerminal` unless replaced |
| `Surface` | The renderer, with padding taken from the options |
| `KeyText` | Character resolution for the configured input mode |
| `ArlecchinoState` | Output line, modal, file picker request |
| `Repaint` | The "this frame is stale" signal the render loop waits on |
| `Navigator`, `ViewResolver`, `IArlecchinoViewFactory` | Routing and view construction |
| `CommandRegistry` | Registered commands |
| `Screen` | Frame composition |
| `InputRouter` | Key dispatch |
| `ArlecchinoKeymap`, `ArlecchinoStrings` | The keymap and the wording, for widgets and stores built by the container |
| `Ticker` | Work on a clock, run between frames |
| `Notifications` | What the application has said lately, behind the output row |
| `TimeProvider` | Where the ticker and the notifications read the time; a test host replaces it |

`IArlecchinoTerminal` is registered with `TryAdd`, so registering your own before `AddArlecchino` also wins.

## Options

| Option | Default | Effect |
|---|---|---|
| `TargetFramesPerSecond` | `60` | Frame rate of the render loop |
| `MinimumWidth` / `MinimumHeight` | `100` / `30` | Below this the frame is replaced by a size notice |
| `HorizontalPadding` / `VerticalPadding` | `2` / `1` | Gutters applied by the surface |
| `UseAlternateScreen` | `true` | Enter the alternate screen buffer and hide the cursor while running |
| `ShowHints` | `true` | Draw the `Keys` box from the current view's `Hints()` |
| `ShowOutputLine` | `true` | Draw `ArlecchinoState.Output` on the last row |
| `CommandPaletteKey` | `':'` | Key that opens the palette |
| `TextInput` | `Native` | How typed characters are resolved |
| `MouseInput` | `false` | Report clicks, drags and the wheel to views |
| `BracketedPaste` | `true` | Pasted text arrives as one block instead of a burst of keys |
| `EscapeTimeout` | `25 ms` | How long the reader waits for the rest of an escape sequence |
| `Keymap` | `new ArlecchinoKeymap()` | Keys the framework itself reacts to |
| `Theme` | `ThemePalette.Arlecchino` | Colour roles |
| `GraphSymbols` | `Braille` | Which characters charts are drawn with |
| `ImageProtocol` | `Auto` | How a `Picture` reaches the terminal |
| `AskTerminal` | `true` | Ask the terminal what it can draw before the first frame |
| `TerminalAnswer` | `120 ms` | How long to wait for that answer before giving up on it |
| `CellWidth` / `CellHeight` | `10` / `20` | Pixels a cell is taken to be when the terminal will not say |
| `Strings` | `new ArlecchinoStrings()` | User-visible text |
| `StartRoute` | `ViewRoute.None` | Route shown on the first frame |
| `InputPollInterval` | `8 ms` | Sleep between key polls when the input queue is empty |
| `NotificationTimeout` | `5 s` | How long a message holds the output row |
| `NotificationLifetime` | `10 min` | How long it stays readable on the notifications screen |

## Builder API

| Call | Effect |
|---|---|
| `AddView<T>(route)` | Registers a view resolved through the container |
| `AddView(route, factory)` | Registers a view built by your own factory delegate |
| `AddViewFactory<T>()` | Adds an `IArlecchinoViewFactory` — this is what `AddGeneratedViews()` does |
| `AddStore<T>()` | Registers one store by hand — singleton, or scoped when it implements `IArlecchinoScopedStore` |
| `AddGeneratedStores()` | Generated: registers every `IArlecchinoStore` in the project, singleton or scoped — see [Source generator](source-generator.md#stores) |
| `AddGeneratedCommands()` | Generated: registers every `IArlecchinoCommand` in the project as a singleton — see [Source generator](source-generator.md#commands) |
| `AddGeneratedWidgets()` | Generated: registers every `IArlecchinoWidget` of the project as a singleton — see [Source generator](source-generator.md#widgets) |
| `AddWidget<T>()` | Registers one widget by hand as a singleton; an alternative to `AddGeneratedWidgets()`, not a layer on top |
| `AddCommand<T>()` | Registers one `IArlecchinoCommand` by hand; an alternative to `AddGeneratedCommands()`, not a layer on top |
| `AddStartup<T>()` | Registers an `IArlecchinoStartup` |
| `StartAt(route)` | Sets `StartRoute`; also takes a plain string |
| `UseTextInput(mode)`, `UseKeysByPosition()` | Keyboard layout handling |
| `UseKeymap(keymap)` | Replaces the key bindings |
| `UseNotifications(key, timeout, lifetime)` | Turns the output row on, sets both timeouts and the key that opens the notifications screen |
| `WithoutNotifications()` | Leaves the output row off |
| `UseMouse()` | Turns on mouse reporting |
| `UseTheme(palette)` | Replaces the colour palette |
| `UseStrings(strings)` | Replaces user-visible text |
| `UseTerminal<T>()` | Replaces `IArlecchinoTerminal` |
| `WithoutHostedService()` | Drops the render loop, leaving the services |
| `Services`, `Options` | The underlying collection and options, for anything not covered above |

## Startup routes

`StartAt` is a constant. When the first route depends on runtime state — a missing config file sending
the user to a setup view, say — implement `IArlecchinoStartup`:

```csharp
public sealed class ChooseStartView : IArlecchinoStartup
{
    private readonly Settings _settings;

    public ChooseStartView(Settings settings) => _settings = settings;

    public ViewRoute Start() => _settings.Exists ? ViewKind.Default : ViewKind.Setup;
}
```

Register with `.AddStartup<ChooseStartView>()`. Every startup runs when the hosted service begins, in
registration order, each one applied to the navigator.

Every [`ArlecchinoAsyncStore`](stores.md#a-store-that-loads-itself) is started at the same moment,
with the token cancelled when the host stops — started, not awaited: the first frame is drawn while
they load, and each store says where it got to.

## The two loops

The hosted service runs two of them at once. One reads the terminal — a blocking, timing-sensitive
job, because the rest of an escape sequence arrives a few milliseconds after its `Esc`. The other
draws at the configured rate, and only when a frame is owed.

They do not share state: the reader queues what it read, and the frame loop drains the queue at the
top of every turn, before the ticker and before drawing. [The frame loop](frame-loop.md) is that side
in full, `Ticker` included.

## Failures and shutdown

A terminal application that dies mid-frame leaves the user in the alternate screen with a hidden
cursor and no prompt, so the hosted service treats that as its job:

- `Ctrl+C` is intercepted (`Console.CancelKeyPress`) and turned into `IHostApplicationLifetime.StopApplication`,
  so the normal shutdown path runs instead of the process being torn down.
- The terminal is restored on every exit — normal stop, cancellation, an unhandled error in the loop,
  `ProcessExit`, or `AppDomain.UnhandledException`.
- An exception thrown by a view's `Draw` is logged through `ILogger` and reported on the output line
  via `ArlecchinoStrings.ViewFailed`; the frame still renders and the application keeps running.
- The same applies to `Handle` and to modal callbacks: `InputRouter` catches, logs and reports rather
  than letting one bad key kill the process.
- POSIX signals are answered too. `SIGTERM` and `SIGHUP` give the screen back before the process goes,
  `SIGTSTP` (`Ctrl+Z`) restores the terminal *before* the shell suspends the process, and `SIGCONT`
  puts the modes back and repaints from scratch when it is resumed. On Windows only `SIGTERM` exists,
  and signals the platform does not have are skipped rather than throwing.

`Screen.RedrawEverything()` is what the resume path uses, and it is public for the same reason: when
something outside the framework has written over the screen, the next frame has to be a full paint
rather than a difference against a picture that is no longer there.

`AddArlecchino` calls `AddLogging()`, so `ILogger` is always resolvable — and registers a logger
provider of its own, because a console logger would write into the middle of a frame. Where those
lines go, and what to attach to a bug report, is on [Diagnostics](diagnostics.md).

## Running without the hosted service

`WithoutHostedService()` leaves every service in place but removes the loop, which is how a single
frame is rendered headlessly — for screenshots, layout checks or tests:

```csharp
var services = new ServiceCollection();
services.AddArlecchino().AddGeneratedViews().AddGeneratedStores().WithoutHostedService();
services.AddSingleton<IHostApplicationLifetime, NullLifetime>();

using var provider = services.BuildServiceProvider();

provider.GetRequiredService<Surface>().SetFixedSize(130, 30);
provider.GetRequiredService<Navigator>().Apply(ViewKind.Default);
provider.GetRequiredService<Screen>().DrawOnce();
```

`SetFixedSize` pins the frame so nothing asks the real terminal for its size, and `DrawOnce` composes
exactly one frame to stdout. `IHostApplicationLifetime` only needs a stand-in when your commands take
it. The sample wires this up behind `--frame` — see [Getting started](getting-started.md).
