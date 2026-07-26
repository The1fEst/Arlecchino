# Changelog

Notable changes to the `Arlecchino`, `Arlecchino.Core` and `Arlecchino.Testing` packages. The three ship
together and always carry the same version.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html). Up to `1.0.0` a breaking change only
bumped the minor, which is why the `0.x` entries below are full of them; from `1.0.0` on, breaking
the public API means a new major. See [Versioning](docs/packages-and-building.md).

## 1.0.0

The first stable release: the public surface is what it is going to be, and from here a breaking
change means a `2.0`. This release is the API review that made that possible, and the features it was
waiting for — everything under **Changed** is breaking, and it is the last release that intends to be.

### Added

- **The packages target `net8.0` as well as `net10.0`.** An application on the long-term support
  release can use them now; the two libraries are built from the same source, which is why `LogBuffer`
  locks on a plain object instead of `System.Threading.Lock`. The suite runs on both frameworks.
- **Work on a clock.** `Ticker` schedules an action `Every(interval)` or `After(delay)`, runs it
  between frames on the drawing thread and asks for a repaint afterwards; the handle it returns
  cancels the work, so `ViewLifetime.Track` ties it to a screen. No thread of its own — the frame loop
  calls it, and `ArlecchinoTestHost.Advance(...)` moves a `TestClock` instead, so a test never waits.
- **Message and confirmation dialogs.** `RequestMessage` shows something to read, wrapped and
  dismissed with either closing key. `RequestConfirmation` asks first with **No** preselected and runs
  the callback only on yes.
- **The output row times out, and keeps a history.** Writing `ArlecchinoState.Output` raises a
  notification: the row shows it for `NotificationTimeout` and then goes quiet, while the message stays
  readable for `NotificationLifetime` on a screen of its own — `Ctrl+N` or a click on the row opens
  `Routes.Notifications`, where `Backspace` clears the list. `UseNotifications(key, timeout, lifetime)`
  configures all of it, `WithoutNotifications()` turns the row off.
- **A keys screen.** `F1` opens `Routes.Help`: every key the framework answers to with what it does,
  then the commands of the screen it was opened from, then the application's commands. The middle
  section is the point — a view's `Commands()` are the keys that work only there, which is what
  somebody pressing `F1` is usually after; a screen with none gets no section rather than an empty
  heading. The descriptions come from `ArlecchinoStrings.HelpKeys` and the heading from
  `HelpScreenSection`, so they translate like everything else.
- **`ScrollPane`**, a window onto content taller than its space, and **`Surface.Clip`** underneath it:
  a scope that confines every write to a rectangle whatever coordinates the caller uses, so content
  drawn at an offset cannot land on a neighbour.
- **`TextView`** for reading a block of text — wrapped, scrolled, reflowed when the width changes —
  and **`TextWidth.Wrap`** behind it, public for layout code of your own.
- **`TextAreaModal` and `RequestTextArea`** for editing several lines: `Enter` breaks the line, the new
  `Submit` binding (`Ctrl+Enter`) confirms, the caret moves by symbols across line ends, pasted blocks
  keep their breaks, and the validator's message is drawn under the text. `Copy` takes the whole text
  to the clipboard.
- **The notification list is bounded.** `Notifications.Capacity` (200) caps it however young the
  messages are, so reporting in a loop no longer grows it without limit.
- **A binding can carry two combinations.** `KeyBinding` gained `AlsoKey` and `AlsoModifiers`, so
  `Copy` answers to both `Ctrl+Insert` and `Ctrl+Shift+C` — the two habits for the same action. Pasting
  needs nothing here: the terminal turns `Ctrl+Shift+V` into a bracketed paste, which already arrives
  as one block.

- `AddStore<T>()`, so a store can be registered by hand as views, commands and widgets already could —
  scoped when the type implements `IArlecchinoScopedStore`, singleton otherwise.

### Changed

- **`Arlecchino.State` is laid out by subject.** It split three ways: atoms and stores to
  `Arlecchino.Atoms`, every modal to `Arlecchino.Modals`, and `ArlecchinoState` with the file-picker
  request left where they were. `TerminalInputReader` moved to `Arlecchino.Input`.
- **`TuiState` is `ArlecchinoState`** — the last name carrying the old prefix.
- **The atom vocabulary is finished.** `IReadableState<T>` is `IReadableAtom<T>`, `StateHistory` is
  `AtomHistory`, `AsyncState<T>` is `AsyncAtom<T>`, and `IStateEdit` is `IAtomEdit`.
- **Every contract an application implements carries the package name**: `IViewFactory`,
  `ITerminal`, `IFocusable` and `ITermColor` are now `IArlecchinoViewFactory`,
  `IArlecchinoTerminal`, `IArlecchinoFocusable` and `IArlecchinoColor`. Interfaces only the framework
  implements — `IReadableAtom<T>`, `IAffixedModal` and the rest — deliberately keep the short name.
- **Diagnostics are `ARL001`–`ARL007`**, not `TSR*`.
- **`Style` means one thing.** The per-item delegate on `ListBox<T>`, `Table<T>` and `Tree<T>` is
  `ItemStyle`; `Style` stays the single colour on `ProgressBar`, `StatusBar` and `Spinner`.
- **`Form` has one input surface.** `Handle` and `HandleMouse` return a `FocusResult` like every other
  widget, instead of a `ViewRoute` from the public method and a `FocusResult` from an explicit
  implementation. A view hosting a form returns `_form.Handle(key).Route`.

### Fixed

- A view, store or command without a public constructor is now left out of the generated code instead
  of being registered anyway. The generator reported it (`ARL002`, `ARL005`, `ARL006`) and then emitted
  a `new` of it regardless, so the diagnostic arrived alongside a compiler error in generated code
  rather than in place of one. Widgets already behaved this way; the other three now match.

### Removed

- Implementation details are no longer public: `ArlecchinoHostedService`, `RegisteredViewFactory`,
  `ViewRegistrations`, `CommandConflicts`, `LogOverlay`, `ArlecchinoLoggerProvider`, `FilePickerView`,
  `EscapeSequenceParser`, `AtomChanges` and `AtomTracking`. The constructors of `Navigator`, `Screen`
  and `InputRouter` went with them — the container builds those, an application resolves them.

### Documentation

- The packages carry an icon, and the README opens with the banner. The brand assets live in
  `assets/`: the harlequin mask as an icon on its plate, transparent, and as a single-colour glyph
  that inherits `currentColor`, plus the banner and the social card — SVG throughout, with the raster
  sizes rendered beside them.
- [Rendering](docs/rendering.md) ends with the terminals the framework has actually run in and what
  each one showed — plain `xterm-256color`, `COLORTERM=truecolor`, `NO_COLOR`, `TERM=dumb`, tmux,
  macOS on Arm — and, just as usefully, the ones it has not: conhost without virtual terminal support,
  Terminal.app, PuTTY, kitty and friends.
- `Theme.Palette` and `TerminalCapabilities.Color` are documented as process-wide, which is what they
  have always been: one look per process, last host built wins, and a test that changes either shares
  the change with everything else running.

### Performance

- **Reading and writing an atom no longer allocates.** A read used to hand `AtomTracking` a delegate
  so that an enclosing `Computed` could discover the dependency, and built one whether or not anything
  was collecting — 64 bytes on every read, on a path frames take constantly. The read now asks first.
  Subscribers are held in an array replaced on subscribe instead of a list copied on every write, so a
  write no longer allocates either; a listener that unsubscribes while being notified still runs to
  completion, and one that subscribes there hears the next write rather than the current one. Reading
  a cached `Computed` went from 3.0 ns and 64 B to 0.5 ns and nothing, writing an atom twenty things
  listen to from 18 ns and 184 B to 8.3 ns and nothing, and re-running a `Computed` allocates 304 B
  rather than 480. The benchmarks that found this are in the repository.

### Tests

- `SystemTerminal` is covered. Until now every test went through `FakeTerminal`, so the escape
  sequences an application actually sends — the alternate screen, bracketed paste, SGR mouse
  reporting, `OSC 52` copying — were never executed by anything. The suite now asserts the bytes, and
  the platform split with them: away from Windows the mouse is asked for with sequences, on Windows
  nothing reaches the output because the console is read record by record instead.
- Translating a Windows console record into a key or a mouse event moved out of the P/Invoke wrapper
  into a type of its own, which the suite drives directly on either platform: presses, releases,
  drags, a wheel in both directions, held buttons that must not report twice, and the modifiers each
  event carries.
- The file picker's `Places` are tested. Shortcuts an application puts in the sidebar had no test at
  all: that they are listed, that they come before the folders the framework offers, that one without
  an icon gets the default, and that clicking one browses to it.
- Benchmarks cover what the earlier ones left out: a key through the router, a click, a pasted block,
  writing atoms watched and unwatched, a computed value read cached and invalidated, undo and redo,
  and `TextWidth.Wrap`. They are what found the allocation above.

### Continuous integration

- The build fails on a ReSharper inspection as well as on a compiler warning: `jb inspectcode` runs
  against `.editorconfig` and annotates what it finds. That covers the rules the compiler has no say
  in — a redundant type in an argument, an `if` worth inverting, a member that should be static.
- CI builds a console application against the freshly packed `.nupkg` files, with views, a store, a
  widget and a command in it, registered both by the generator and by hand. Three bugs this cycle only
  showed up that way, and none of them were visible from a build of the repository itself. The project
  is generated outside the checkout, so it is shaped by the packages rather than by our build props.
- The AOT claim is tested rather than asserted. `IsAotCompatible` only turns on an analyzer, so CI now
  publishes the sample with `PublishAot`, runs the native binary and fails unless it draws a frame —
  the failure mode being an application that compiles clean, publishes clean and then shows an empty
  screen because the trimmer took a registration with it. The probe is `-p:AotProbe=true` on the
  sample; the binary is about 5 MB and needs no runtime installed.
- Coverage is measured on every run and the build fails when it drops: 85% of lines, 70% of branches.
  The figures per assembly land in the run summary, so a change that adds code without tests is
  visible before it is merged rather than after.
- CodeQL analyses the C# on every push and once a week, and Dependabot proposes dependency and action
  updates monthly, grouped so they arrive as a few pull requests rather than many.
- Every benchmark is executed on each run as a dry job. Measurements from a shared runner mean
  nothing, but a benchmark that no longer compiles or has started throwing is caught the day it
  breaks; `benchmarks.yml` runs them properly on demand and writes the tables into the run summary.

## 0.9.0

### Fixed

- A form left a blank row under the selected field even when that field had no help, so moving
  through fields dragged a hole along with the selection. The help line is drawn only when there is
  help to show.

### Changed

- **Breaking.** `Form.ReserveHelpRow` is gone with it: keeping a row free for help that does not
  exist was the hole itself, and there is nothing left to configure.

## 0.8.0

### Added

- Widgets of the application can come from the container. A fourth generator finds every
  `IArlecchinoWidget` declared in the project and `.AddGeneratedWidgets()` registers each as a
  **singleton**, built by a factory calling its public constructor with the most parameters — so one
  instance is shared by every screen resolving it, state and focus included. Only the project's own
  widgets are registered; the built-in ones live in the package's assembly and are still constructed
  in the view.
- `.AddWidget<T>()` registers one widget by hand, for a widget the generator cannot see. As with
  commands, it is an alternative to the generated call rather than a layer on top.
- `TSR007` names a widget the container cannot build — generic, no public constructor, or `required`
  members — instead of emitting code that would not compile. `ArlecchinoGenerateWidgets` turns the
  generator off.
- `ArlecchinoKeymap` and `ArlecchinoStrings` are resolvable services now, so a widget or store built
  by the container takes the keymap directly instead of reaching through `ArlecchinoOptions`.

### Fixed

- `LogBuffer` could fall below its own capacity: the check for a full buffer and the removal of the
  oldest line were separate steps, so two threads logging at once dropped the same surplus line
  twice. Trimming happens under a lock now.

## 0.7.0

### Added

- Widgets have a contract. `IArlecchinoWidget` is `Draw(SurfaceRegion)` — what a reusable piece of a
  screen does — and `IArlecchinoInteractiveWidget` adds the input half through `IFocusable`, which is
  what a `FocusRing` cycles. Everything built in answers one of the two, and a widget of your own
  implements the same interface rather than following a convention.

### Changed

- **Breaking.** `ProgressBar`, `StatusBar` and `Spinner` take their colour as a `Style` property
  instead of an argument to `Draw`, so every widget is drawn by the same call. `Spinner.Draw` paints
  the top-left cell of the region it is given rather than taking a row and a column — pass it the
  cell, `region.SplitLeft(region.Width - 1).Right` and friends.

## 0.6.0

### Added

- Commands register themselves. A third generator finds every `IArlecchinoCommand` in the project and
  `.AddGeneratedCommands()` puts each one in the container as a singleton, built by a factory calling
  its public constructor with the most parameters. `TSR006` reports a command the container cannot
  build, and `ArlecchinoGenerateCommands` turns the generator off. `AddCommand<T>()` stays for
  commands that come from another assembly — the two are alternatives, and using both for the same
  type lists it twice in the palette.

### Changed

- **Breaking.** The markers an application implements carry the package name now: `IView` is
  `IArlecchinoView`, `IStore` is `IArlecchinoStore`, and `IScopedStore` is
  `IArlecchinoScopedStore`. All three sit in namespaces an application imports, where a bare `IView`
  or `IStore` is the same name half the ecosystem uses. `IViewFactory` and the rest of the
  navigation types are unchanged — nothing outside the package implements them.
- **Breaking.** `Rendering.FontStyle` is now `TextStyle` and `Rendering.Region` is now
  `SurfaceRegion`. Those two were the whole measured overlap of the public surface with anything
  outside it: both collide with `System.Drawing` at the same arity, so a project that also
  references `System.Drawing.Common` could not import `Arlecchino.Rendering` without qualifying
  them. Nothing else among the 112 public types clashes with the .NET reference assemblies.
- **Breaking.** The atoms are called atoms in the API, not only in the prose: `State<T>` is
  `Atom<T>`, `TrackedState<T>` is `TrackedAtom<T>`, and `LocalState<T>` is `LocalAtom<T>`. The base
  type also stops carrying the name of the namespace it lives in. `AsyncState<T>`, `Computed<T>`,
  `StateHistory` and `StateChanges` keep their names — they are not atoms.

## 0.5.0

### Added

- Stores register themselves. A class of atoms marked `IStore` is found by a second generator, and
  `.AddGeneratedStores()` puts every one of them in the container as a singleton — built by a factory
  calling its public constructor with the most parameters, so nothing is resolved by reflection.
  `IScopedStore` registers as scoped instead, living exactly as long as the screen that asked for it.
  `TSR005` reports a store the container cannot build, and `ArlecchinoGenerateStores` turns the whole
  thing off.

### Documentation

- The two atom types are named where an atom is described rather than only in the state chapter: the
  XML documentation of `Field` and `Form`, the page tables, and the opening of
  [State and forms](docs/state-and-forms.md).

### Continuous integration

- `build.yml` ignores `**.md`, `docs/**` and `LICENSE`, and the documentation says how to keep a
  work-in-progress commit off CI entirely (`[skip ci]`, which Actions reads by itself).

## 0.4.0

### Changed

- Whether an atom is undoable is now the type it is created as, not a flag set on it afterwards.
  `State<T>` is abstract; `TrackedState<T>` records its edits on the undo stack and `LocalState<T>`
  never does. Everything that takes an atom still takes `State<T>`, so call sites are unchanged —
  `new State<int>(0) { RecordsHistory = false }` becomes `new LocalState<int>(0)`, and the rest
  becomes `new TrackedState<T>(…)`.
- `State<T>.SetWithoutHistory` is gone with it: the type of the atom is the whole answer, and undo
  restores values through its own path.

## 0.3.0

### Fixed

- The generated view factory named each view by its short name, so a view in a namespace the generated
  file did not sit under failed to compile with `CS0246`. Namespaces of the views are emitted as
  `using` directives now, and views may live anywhere in the project.
- A project with no view yet had nothing generated at all, so `AddGeneratedViews` and `ViewKind` did
  not exist and the error was `cannot resolve symbol` on the first line of the setup. Both are emitted
  from the moment the package is referenced — `ViewKind` simply holds no routes — and the new `TSR004`
  says why.

### Documentation

- The `using` for the generated namespace (`$(RootNamespace).Navigation` by default) is in the README
  and the getting-started example; it was the one line a new application could not guess.
- `IView` is documented with `HandlePaste` and `Commands`, the options table with `BracketedPaste` and
  `EscapeTimeout`, the strings table with `ListPosition`, the form and the log-overlay text, and the
  assembly table with the `Focus`, `Forms`, `Widgets` and `Diagnostics` namespaces and the
  `Arlecchino.Testing` package.

## 0.2.0

First release published on NuGet.

### Added

- Text fields are edited as a real line: a caret drawn where the next character goes, `←`/`→`,
  `Ctrl+←`/`Ctrl+→` by word, `Home`/`End`, `Delete`, `Ctrl+Backspace` and `Ctrl+U`. The logic lives in
  `TextEditing` and applies to the number field too.
- Modals stack. `TuiState.PushModal` opens one over another so a callback can ask a follow-up
  question; closing it uncovers the one underneath, and every level is drawn, offset.
- Bracketed paste. Pasted text arrives as one block through `IView.HandlePaste` or straight into the
  open field, instead of a burst of key presses. On by default (`options.BracketedPaste`).
- Copying a field to the clipboard with `Ctrl+Insert`, encoded as OSC 52 so it works over SSH.
- Scroll bars and a `3/40` position readout in lists, tables, trees and choice modals that hold more
  than fits. `ScrollBar` is public for panes laid out by hand.
- Mouse support on Windows, read from the console's own event queue with `ReadConsoleInput` — the
  platform cannot deliver SGR reports without silencing the keyboard. Quick-edit selection is turned
  off while it runs and restored afterwards.
- A log overlay on `Ctrl+L`. `ArlecchinoLoggerProvider` keeps the last lines in memory rather than
  painting them over the frame, and the overlay scrolls back through them.
- XML documentation on the whole public API of all three packages, enforced by `CS1591` with warnings
  as errors.
- A second sample, `Arlecchino.Processes`: the process list in a sortable table, read in the background,
  filtered from a modal, with a details screen.
- Benchmarks under `benchmarks/Arlecchino.Benchmarks` for frame composition and text measurement.
- The public API of all three packages is now written down in `PublicAPI.*.txt` and enforced by
  `Microsoft.CodeAnalysis.PublicApiAnalyzers`, so a change to the surface cannot land unnoticed.

### Fixed

- `LogBuffer` held its lines in a plain list while logging arrives from any thread; it is a concurrent
  queue now and the overlay draws from a snapshot.
- Editing worked in `char` values, so backspace could cut an emoji or a combining sequence in half.
  Movement and deletion go by symbols, and `TextWidth` exposes the boundary helpers.
- A value longer than the terminal hid the caret; the field scrolls now, with `…` on the side that
  continues.
- An escape sequence split across two reads — normal over ssh — was delivered as `Esc`, `[`, `A`. The
  reader waits `options.EscapeTimeout` for the rest.
- The undo stack grew for the lifetime of the process; it is bounded by `StateHistory.Capacity`.
- Being killed (`SIGTERM`, `SIGHUP`) or suspended (`Ctrl+Z`) left the terminal in the alternate screen
  with no cursor. Both are handled now, and `SIGCONT` restores the modes and repaints.
- `ArlecchinoTestHost` drew whatever colour the machine running the tests happened to allow, so a build
  agent with `NO_COLOR` set produced frames with no styling in them and assertions on colour failed. It
  fixes `TerminalCapabilities.Color` at `TrueColor` instead.

### Changed

- Each screen is built in its own DI scope, so views can take scoped services. `IViewFactory.TryCreate`
  now receives the `IServiceProvider` to build from, and `ViewResolver.Create` returns an `ActiveView`
  that owns the scope.
- `ViewLifetime` ties background work to the screen: `Loading<T>()` cancels with it, `Track` disposes
  with it, `Closing` is the token for work started by hand.
- `AsyncState.Cancel()` drops the status back to `Idle` instead of leaving it `Loading`, so a spinner
  stops when the load is abandoned.

- A validation message now follows the field as it is edited and clears the moment the input becomes
  valid, instead of disappearing on the next keystroke. Nothing is reported before the first attempt
  to submit.
- `ITerminal` gained `MouseAvailable`, `ReadMouse`, `EnablePaste`, `DisablePaste` and
  `CopyToClipboard`. Custom terminals have to implement them.
- `TuiState.CloseModal` closes the top modal rather than all of them; `CloseAllModals` does what it
  used to.

## 0.1.0

First release: cell-grid renderer with diff output, view navigation and history, the modal set, forms
bound to atomic state, focus, the widget set, the command palette and per-view commands, the source
generator, mouse support, theming, localisation through delegates, and the headless test host.
