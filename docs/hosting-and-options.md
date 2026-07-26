[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

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
| `ITerminal` | `SystemTerminal` unless replaced |
| `Surface` | The renderer, with padding taken from the options |
| `KeyText` | Character resolution for the configured input mode |
| `TuiState` | Output line, modal, file picker request |
| `Repaint` | The "this frame is stale" signal the render loop waits on |
| `UiDispatcher` | Queue for handing results back from background work; runs them between frames |
| `Navigator`, `ViewResolver`, `ViewRegistrations`, `IViewFactory` | Routing and view construction |
| `CommandRegistry` | Registered commands |
| `Screen` | Frame composition |
| `InputRouter` | Key dispatch |
| `ArlecchinoHostedService` | The hosted service owning the render and input loops |

`ITerminal` is registered with `TryAdd`, so registering your own before `AddArlecchino` also wins.

## Options

| Option | Default | Effect |
|---|---|---|
| `TargetFramesPerSecond` | `60` | Frame rate of the render loop |
| `MinimumWidth` / `MinimumHeight` | `100` / `30` | Below this the frame is replaced by a size notice |
| `HorizontalPadding` / `VerticalPadding` | `2` / `1` | Gutters applied by the surface |
| `UseAlternateScreen` | `true` | Enter the alternate screen buffer and hide the cursor while running |
| `ShowHints` | `true` | Draw the `Keys` box from the current view's `Hints()` |
| `ShowOutputLine` | `true` | Draw `TuiState.Output` on the last row |
| `CommandPaletteKey` | `':'` | Key that opens the palette |
| `TextInput` | `LatinOnly` | How typed characters are resolved |
| `MouseInput` | `false` | Report clicks, drags and the wheel to views |
| `BracketedPaste` | `true` | Pasted text arrives as one block instead of a burst of keys |
| `EscapeTimeout` | `25 ms` | How long the reader waits for the rest of an escape sequence |
| `Keymap` | `new ArlecchinoKeymap()` | Keys the framework itself reacts to |
| `Theme` | `new ThemePalette()` | Colour roles |
| `Strings` | `new ArlecchinoStrings()` | User-visible text |
| `StartRoute` | `ViewRoute.None` | Route shown on the first frame |
| `InputPollInterval` | `8 ms` | Sleep between key polls when the input queue is empty |

## Builder API

| Call | Effect |
|---|---|
| `AddView<T>(route)` | Registers a view resolved through the container |
| `AddView(route, factory)` | Registers a view built by your own factory delegate |
| `AddViewFactory<T>()` | Adds an `IViewFactory` — this is what `AddGeneratedViews()` does |
| `AddGeneratedStores()` | Generated: registers every `IArlecchinoStore` in the project, singleton or scoped — see [Source generator](source-generator.md#stores) |
| `AddGeneratedCommands()` | Generated: registers every `IArlecchinoCommand` in the project as a singleton — see [Source generator](source-generator.md#commands) |
| `AddGeneratedWidgets()` | Generated: registers every `IArlecchinoWidget` of the project as a singleton — see [Source generator](source-generator.md#widgets) |
| `AddWidget<T>()` | Registers one widget by hand as a singleton; an alternative to `AddGeneratedWidgets()`, not a layer on top |
| `AddCommand<T>()` | Registers one `IArlecchinoCommand` by hand; an alternative to `AddGeneratedCommands()`, not a layer on top |
| `AddStartup<T>()` | Registers an `IArlecchinoStartup` |
| `StartAt(route)` | Sets `StartRoute`; also takes a plain string |
| `UseTextInput(mode)`, `UseLatinOnlyInput()`, `UseNativeInput()` | Keyboard layout handling |
| `UseKeymap(keymap)` | Replaces the key bindings |
| `UseMouse()` | Turns on mouse reporting |
| `UseTheme(palette)` | Replaces the colour palette |
| `UseStrings(strings)` | Replaces user-visible text |
| `UseTerminal<T>()` | Replaces `ITerminal` |
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

`AddArlecchino` calls `AddLogging()`, so `ILogger` is always resolvable.

## The log overlay

A console logger cannot work here: its lines land in the middle of the frame. `AddArlecchino` therefore
registers `ArlecchinoLoggerProvider`, which keeps the last 200 lines in a `LogBuffer` in memory, and
`Ctrl+L` (the `ToggleLog` binding) shows them over the bottom half of the screen:

```
╭─ Log (7) ───────────────────────────────────────────╮
│ 14:02:11 fail Screen: The view at route Mods failed │
│ 14:02:11 warn CommandConflicts: Ctrl+S is claimed…  │
│ ↑↓ scroll · End latest · Backspace clear · Esc close │
╰─────────────────────────────────────────────────────╯
```

Warnings and errors are coloured, the newest line is at the bottom, and `↑`/`↓` scroll back through
the buffer while `End` pins it to the newest again. Only those keys are taken while the overlay is
open, so the screen underneath keeps working — it is something to read, not a mode to get stuck in.

This is where the failures above end up: a view that throws while drawing reports one line on the
output line and the whole story here. `LogBuffer.Capacity` sets how much is kept; add your own file
or Seq provider the usual way, and drop any provider that writes to standard output
(`builder.Logging.ClearProviders()` removes this one too, overlay included).

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
