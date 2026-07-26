[Home](README.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Getting started

## Install

```
dotnet add package Arlecchino
```

`Arlecchino` pulls in `Arlecchino.Core` (the renderer) and carries the source generator inside the package,
so nothing else has to be referenced. `Microsoft.Extensions.Hosting` is what you need for the host
itself.

## The smallest app

```csharp
using MyApp.Navigation;   // ViewKind and AddGeneratedViews are generated here

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddArlecchino(options => options.MinimumWidth = 60)
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddCommand<QuitCommand>()
    .StartAt(ViewKind.Default);

await builder.Build().RunAsync();
```

`AddArlecchino` registers the renderer, the navigator, the input router and a hosted service that owns
the render loop. `AddGeneratedViews` plugs in the factory emitted by the generator,
`AddGeneratedStores` registers every `IStore` it found alongside it, and `StartAt`
picks the route shown on the first frame. Everything after `AddArlecchino` is a call on
`ArlecchinoBuilder` — see [Hosting and options](hosting-and-options.md).

That first `using` is the one thing not visible from the code: `ViewKind` and `AddGeneratedViews` are
written by the generator into `$(RootNamespace).Navigation`, not into a namespace of the package, so
the file wiring the application up has to import it. Both exist from the moment the package is
referenced — before the first view is written `ViewKind` simply holds no routes and the generator
says so as `TSR004`. See [Source generator](source-generator.md) to put them somewhere else.

## The first view

A view is a class implementing `IView`. Constructor parameters are resolved from the container:

```csharp
public class DefaultView : IView
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

`Draw` is called once per frame, `Handle` gets every key the framework did not consume itself, and the
route it returns navigates. `Hints` fills the box in the bottom-right corner. Details in
[Views and navigation](views-and-navigation.md) and [Rendering](rendering.md).

The `DefaultView` class name is what produces `ViewKind.Default`: the generator strips the `View`
suffix. The view itself may live in any namespace — the generated factory imports whatever it needs.
Set `<ArlecchinoViewNamespace>` in your csproj to choose where `ViewKind` lands instead of
`$(RootNamespace).Navigation` — see [Source generator](source-generator.md).

## Running the samples

Two of them ship in the repository. `Arlecchino.Sample` is the gallery — a default view, an about view,
a settings form, the widget page, a command palette and the file picker:

```
dotnet run --project samples/Arlecchino.Sample
```

It also renders a single frame headlessly, which is the fastest way to look at a layout:

```
dotnet run --project samples/Arlecchino.Sample -- --frame picker 130x30
```

The frame goes to stdout as ANSI text; the view name is `default`, `about`, `picker`, or one of
`password`, `number`, `slider`, `toggle`, `multi`, `date`, `time`, `color` to render the matching
modal over the default view, and the size is `<width>x<height>`. `Surface.SetFixedSize` is what makes this possible in your own app — see
[Rendering](rendering.md).

`Arlecchino.Processes` is the other kind of sample: a small application that does real work rather than
showing off widgets. It lists the processes on the machine in a sortable [table](widgets.md), reads
them on a background thread through `AsyncState` with a spinner while it loads, filters them from a
text modal, and opens a details screen for the selected row:

```
dotnet run --project samples/Arlecchino.Processes
```

`r` re-reads the list, `m` and `n` sort, `f` filters, `Enter` opens the details. All four are
[view commands](commands-and-input.md), so they appear in the palette and in the hints box without
being written down twice. It renders headlessly too — `--frame processes 110x26` or
`--frame details 90x18`.
