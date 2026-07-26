# Changelog

Notable changes to the `Arlecchino`, `Arlecchino.Core` and `Arlecchino.Testing` packages. The three ship
together and always carry the same version.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and the project follows
[Semantic Versioning](https://semver.org/spec/v2.0.0.html) — with the caveat that on `0.x` a breaking
change only bumps the minor. See [Versioning](docs/packages-and-building.md).

## Unreleased

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
