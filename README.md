# Arlecchino

A terminal UI framework for .NET. Views are plain classes, navigation keeps a history, and everything
is wired through `Microsoft.Extensions.DependencyInjection`.

```
dotnet add package Arlecchino
```

## The shortest app

```csharp
using MyApp.Navigation;   // where the generator puts ViewKind and AddGeneratedViews

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddArlecchino(options => options.MinimumWidth = 60)
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddGeneratedCommands()
    .StartAt(ViewKind.Default);

await builder.Build().RunAsync();
```

`ViewKind` and `AddGeneratedViews` are written by the source generator into
`$(RootNamespace).Navigation` — `MyApp.Navigation` above — so the file that starts the application
needs that `using`. Both appear as soon as the package is referenced, and `ViewKind` fills up with a
route per view.

A view is a class implementing `IArlecchinoView`. Constructor parameters come from the container:

```csharp
public class DefaultView : IArlecchinoView
{
    private readonly Surface _surface;

    public DefaultView(Surface surface) => _surface = surface;

    public void Draw()
    {
        _surface.AppendLine("hello", Theme.Header, Align.Center);
    }

    public ViewRoute Handle(ConsoleKeyInfo key) =>
        key.Key == ConsoleKey.A ? ViewKind.About : ViewRoute.None;

    public (string Key, string Description)[] Hints() => [("a", "about")];
}
```

Routes come from a source generator that finds every `IArlecchinoView` in the project, so `ViewKind.About` reads
like an enum while staying a plain string route the framework can name. Modals for text, passwords,
email and links, numbers, sliders, toggles, single and multiple choice, dates, times and colours come
with the framework, along with a command palette, a hints box and a file picker — and
every string any of them draws is a delegate the application can point at its own translations.

## Documentation

Full documentation lives in [docs](docs/README.md).

| Page | What it covers |
|---|---|
| [Getting started](docs/getting-started.md) | Installing the package, the smallest app that runs, the first view |
| [Views and navigation](docs/views-and-navigation.md) | `IArlecchinoView`, `ViewRoute`, the navigator, history, view registration |
| [Source generator](docs/source-generator.md) | How `ViewKind`, the view factory and the store registration are emitted, MSBuild switches |
| [Rendering](docs/rendering.md) | `Surface`: the frame lifecycle, flow layout, absolute layout |
| [Theming](docs/theming.md) | `Theme`, `ThemePalette`, `TermColor`, ANSI colours and font styles |
| [Commands and input](docs/commands-and-input.md) | `IArlecchinoCommand`, the command palette, key routing, keyboard layouts |
| [Modals and state](docs/modals-and-state.md) | `ArlecchinoState`, text and choice modals, the output line |
| [File picker](docs/file-picker.md) | Requesting a path, places sidebar, filters, keys |
| [Hosting and options](docs/hosting-and-options.md) | `AddArlecchino`, every option, the builder API, running without the hosted service |
| [State and forms](docs/state-and-forms.md) | `TrackedAtom` and `LocalAtom` atoms, computed values, undo, async loading, and forms built from them |
| [Widgets](docs/widgets.md) | Lists, trees, sortable tables, tabs, progress, spinner, status bar |
| [Localization](docs/localization.md) | `ArlecchinoStrings` and why no user-visible text is hardcoded |
| [Packages and building](docs/packages-and-building.md) | What ships in which package, `pack.cmd`, the local feed, versioning, testing an application, CI and benchmarks |

What changed between versions is in the [changelog](CHANGELOG.md).

## Packages

| Package | Contents |
|---|---|
| `Arlecchino.Core` | `Surface`, `Theme`, `TermColor`, `KeyText`, `IArlecchinoTerminal` — the renderer, no DI |
| `Arlecchino` | views, navigation, modals, commands, hosting, DI, and the generator |
| `Arlecchino.Testing` | `ArlecchinoTestHost` — the headless host applications write their tests against |

## License

MIT.
