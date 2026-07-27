<p align="center">
  <img src="assets/arlecchino-banner.svg" alt="Arlecchino" width="820">
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Arlecchino"><img src="https://img.shields.io/nuget/v/Arlecchino?logo=nuget&color=C9382B&labelColor=141317" alt="NuGet"></a>
  <a href="https://www.nuget.org/packages/Arlecchino"><img src="https://img.shields.io/nuget/dt/Arlecchino?color=C9382B&labelColor=141317" alt="Downloads"></a>
  <a href="https://github.com/The1fEst/Arlecchino/actions/workflows/build.yml"><img src="https://github.com/The1fEst/Arlecchino/actions/workflows/build.yml/badge.svg" alt="Build"></a>
  <img src="https://img.shields.io/badge/net8.0%20%7C%20net10.0-512BD4?logo=dotnet&logoColor=white&labelColor=141317" alt="Target frameworks">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-EDE6D9?labelColor=141317" alt="MIT"></a>
</p>

A terminal UI framework for .NET. Views are plain classes, navigation keeps a history, and everything
is wired through `Microsoft.Extensions.DependencyInjection`.

```
dotnet add package Arlecchino
```

## What it looks like

`samples/Arlecchino.Packages` is a dependency review of a .NET solution, built on the framework and
shown here reading the fixture solution that ships beside it — a sortable table, tabs, a tree, a form
on atoms, every modal, the command palette and the file picker:

![Every package in the solution, coloured by what is wrong with it](assets/screenshots/inventory.png)

<details>
<summary><b>Seventeen more screens</b> — scanning, tabs, the tree, the upgrade form, modals, the palette, the file picker</summary>

### Finding out what is there

Four `dotnet list package` passes, reported while they run:

![Four dotnet list passes, reported while they run](assets/screenshots/scanning.png)

The hints box is off by default and `:h` brings it back:

![The same table with the hints box turned on](assets/screenshots/hints.png)

Tabs narrow the table down to one kind of problem:

![The vulnerable tab](assets/screenshots/vulnerable.png)

Transitive packages fold in, and the list grows a scroll bar:

![Transitive packages folded in, the list scrolling](assets/screenshots/transitive.png)

A text modal filters it:

![The text modal filtering the table](assets/screenshots/filter.png)

### The other screens

One package, its advisories and every project that pulls it in:

![One package: advisories and every project that pulls it in](assets/screenshots/package.png)

The dependency tree beside a per-project table:

![The dependency tree beside a per-project table](assets/screenshots/projects.png)

![The same tree with the transitive branches open](assets/screenshots/projects-expanded.png)

An upgrade form built from atoms, and the commands it would run:

![The upgrade form and the commands it would run](assets/screenshots/upgrade.png)

![The dry run, printing what it would have executed](assets/screenshots/upgrade-run.png)

### Modals and chrome

Choice, multi-choice and confirmation, opened by the form's own fields:

![Picking the target version](assets/screenshots/upgrade-versions.png)

![Picking which projects to rewrite](assets/screenshots/upgrade-projects.png)

![Turning the dry run off asks first](assets/screenshots/upgrade-confirm.png)

The command palette, the keys screen, the file picker and the log overlay all come with the framework:

![The command palette over the inventory](assets/screenshots/palette.png)

![The keys screen](assets/screenshots/help.png)

![The file picker asking for another solution](assets/screenshots/picker.png)

![The log overlay](assets/screenshots/log.png)

</details>

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
| [Migrating to 2.0](docs/migrating-to-2.0.md) | What 2.0 changed and the edits an application written against `1.x` needs |

What changed between versions is in the [changelog](CHANGELOG.md).

## Packages

| Package | Contents |
|---|---|
| `Arlecchino.Core` | `Surface`, `Theme`, `TermColor`, `KeyText`, `IArlecchinoTerminal` — the renderer, no DI |
| `Arlecchino` | views, navigation, modals, commands, hosting, DI, and the generator |
| `Arlecchino.Testing` | `ArlecchinoTestHost` — the headless host applications write their tests against |

## Contributing

What the build expects of a change is in [CONTRIBUTING.md](CONTRIBUTING.md); how to report something
that looks like a security problem is in [SECURITY.md](SECURITY.md).

## License

MIT.
