[Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Arlecchino documentation

Arlecchino is a terminal UI framework for .NET. A view is a plain class, navigation keeps a history, and
every part of the machinery is a service in `Microsoft.Extensions.DependencyInjection`.

## Pages

| Page | What it covers |
|---|---|
| [Getting started](getting-started.md) | Installing the package, the smallest app that runs, the first view |
| [Views and navigation](views-and-navigation.md) | `IView`, `ViewRoute`, the navigator, history, view registration |
| [Source generator](source-generator.md) | How `ViewKind` and the view factory are emitted, MSBuild switches |
| [Rendering](rendering.md) | `Surface`: the frame lifecycle, flow layout, absolute layout |
| [Theming](theming.md) | `Theme`, `ThemePalette`, `TermColor`, ANSI colours and font styles |
| [Commands and input](commands-and-input.md) | `IArlecchinoCommand`, the command palette, key routing, keyboard layouts |
| [Modals and state](modals-and-state.md) | `TuiState`, text and choice modals, the output line |
| [File picker](file-picker.md) | Requesting a path, places sidebar, filters, keys |
| [Hosting and options](hosting-and-options.md) | `AddArlecchino`, every option, the builder API, running without the hosted service |
| [State and forms](state-and-forms.md) | Atoms, computed values, undo, async loading, and forms built from them |
| [Widgets](widgets.md) | Lists, trees, sortable tables, tabs, progress, spinner, status bar |
| [Localization](localization.md) | `ArlecchinoStrings` and why no user-visible text is hardcoded |
| [Packages and building](packages-and-building.md) | What ships in which package, `pack.cmd`, the local feed |

## Where things live

| Assembly | Namespaces | Contents |
|---|---|---|
| `Arlecchino.Core` | `Arlecchino`, `Arlecchino.Rendering`, `Arlecchino.Input` | `Surface`, `Theme`, `TermColor`, `KeyText`, `ITerminal` — the renderer, no DI |
| `Arlecchino` | `Arlecchino.Hosting`, `Arlecchino.Navigation`, `Arlecchino.Commands`, `Arlecchino.State`, `Arlecchino.Views` | views, navigation, modals, commands, hosting, the file picker |
| `Arlecchino.Generators` | — | the incremental generator, shipped inside the `Arlecchino` package |
