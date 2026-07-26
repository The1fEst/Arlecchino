[Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Arlecchino documentation

Arlecchino is a terminal UI framework for .NET. A view is a plain class, navigation keeps a history, and
every part of the machinery is a service in `Microsoft.Extensions.DependencyInjection`.

## Pages

| Page | What it covers |
|---|---|
| [Getting started](getting-started.md) | Installing the package, the smallest app that runs, the first view |
| [Views and navigation](views-and-navigation.md) | `IArlecchinoView`, `ViewRoute`, the navigator, history, view registration |
| [Source generator](source-generator.md) | How `ViewKind`, the view factory and the store registration are emitted, MSBuild switches |
| [Rendering](rendering.md) | `Surface`: the frame lifecycle, flow layout, absolute layout |
| [Theming](theming.md) | `Theme`, `ThemePalette`, `TermColor`, ANSI colours and font styles |
| [Commands and input](commands-and-input.md) | `IArlecchinoCommand`, the command palette, key routing, keyboard layouts |
| [Modals and state](modals-and-state.md) | `ArlecchinoState`, text and choice modals, the output line |
| [File picker](file-picker.md) | Requesting a path, places sidebar, filters, keys |
| [Hosting and options](hosting-and-options.md) | `AddArlecchino`, every option, the builder API, running without the hosted service |
| [State and forms](state-and-forms.md) | `TrackedAtom` and `LocalAtom` atoms, computed values, undo, async loading, and forms built from them |
| [Widgets](widgets.md) | Lists, trees, sortable tables, tabs, progress, spinner, status bar |
| [Localization](localization.md) | `ArlecchinoStrings` and why no user-visible text is hardcoded |
| [Packages and building](packages-and-building.md) | What ships in which package, `pack.cmd`, the local feed |

## Where things live

| Assembly | Namespaces | Contents |
|---|---|---|
| `Arlecchino.Core` | `Arlecchino`, `Arlecchino.Rendering`, `Arlecchino.Input` | `Surface`, `Theme`, `TermColor`, `KeyText`, `IArlecchinoTerminal` — the renderer, no DI |
| `Arlecchino` | `Arlecchino.Hosting`, `Arlecchino.Navigation`, `Arlecchino.Commands`, `Arlecchino.Atoms`, `Arlecchino.Modals`, `Arlecchino.State`, `Arlecchino.Views`, `Arlecchino.Forms`, `Arlecchino.Focus`, `Arlecchino.Widgets`, `Arlecchino.Input`, `Arlecchino.Rendering`, `Arlecchino.Diagnostics` | views, navigation, modals, commands, forms, widgets, hosting, the file picker |
| `Arlecchino.Testing` | `Arlecchino.Testing` | `ArlecchinoTestHost`, `FakeTerminal`, `FrameText` — the headless host for tests |
| `Arlecchino.Generators` | — | the incremental generator, shipped inside the `Arlecchino` package |
