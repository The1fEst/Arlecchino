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

[Arlecchino.Commander](https://github.com/The1fEst/Arlecchino.Commander) is a Midnight Commander built
on the framework: two panels over local disks, SFTP and FTP, with the file operations running in the
background and reporting themselves as notifications.

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

## [Documentation](https://the1fest.github.io/Arlecchino.Docs/)

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
