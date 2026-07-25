[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Source generator

`Arlecchino.Generators` is an incremental generator shipped inside the `Arlecchino` package as
`analyzers/dotnet/cs`. It writes one file, `ArlecchinoViewNavigation.g.cs`, into the project that
references the package.

## What it looks for

Every class declaration with a base list, whose symbol is non-abstract and implements
`Arlecchino.Navigation.IView`. The route name is the type name with a trailing `View` stripped:
`ModsView` becomes `Mods`, `Settings` stays `Settings`.

Duplicate route names collapse to the first declaration seen. Routes are emitted with `Default` first,
then the rest ordered ordinally.

## What it emits

```csharp
public static class ViewKind
{
    public static ViewRoute None => ViewRoute.None;
    public static readonly ViewRoute Default = new ViewRoute("Default");
    public static readonly ViewRoute About = new ViewRoute("About");
}

public sealed class GeneratedViewFactory : IViewFactory
{
    public bool TryCreate(IServiceProvider services, ViewRoute route, [NotNullWhen(true)] out IView? view) { ... }
}

public static class GeneratedViewRegistration
{
    public static ArlecchinoBuilder AddGeneratedViews(this ArlecchinoBuilder builder) { ... }
}
```

The factory switches on `route.Name` and news each view up directly. Constructor arguments come from
`services.GetRequiredService<T>()` — the scope the resolver opened for that screen — using the public
constructor with the most parameters, so a view is built without reflection and stays AOT-friendly. Namespaces of those parameter types are
emitted as `using` directives.

Nothing is generated when the project contains no views.

## Turning it on

`AddGeneratedViews()` is an extension on `ArlecchinoBuilder`, so it sits in the same chain as the rest of
the setup:

```csharp
builder.Services
    .AddArlecchino()
    .AddGeneratedViews()
    .StartAt(ViewKind.Default);
```

Without that call the generated factory is not registered and only explicit `AddView` registrations
resolve — see [Views and navigation](views-and-navigation.md).

## MSBuild switches

The package's `build/Arlecchino.props` marks these properties compiler-visible; set them in your csproj.

| Property | Effect |
|---|---|
| `ArlecchinoViewNamespace` | Namespace `ViewKind`, `GeneratedViewFactory` and `AddGeneratedViews` land in |
| `RootNamespace` | Fallback when `ArlecchinoViewNamespace` is unset: `$(RootNamespace).Navigation`, or `Views` if that is empty too |
| `ArlecchinoGenerateViews` | Set to `false` to emit nothing |

```xml
<PropertyGroup>
  <ArlecchinoViewNamespace>MyApp.Views</ArlecchinoViewNamespace>
</PropertyGroup>
```

Pick a namespace your views themselves do not sit in, and add it to the `using` list of files that
navigate — that is what makes `ViewKind.Mods` read like an enum at the call site.

## Diagnostics

The generator says something instead of quietly doing the wrong thing:

| Id | Severity | Means |
|---|---|---|
| `TSR001` | Warning | Two views produce the same route — `Sample.ModsView` and `Sample.Extra.ModsView` both become `Mods`. The first one wins and the other is unreachable; rename one of them or register it explicitly |
| `TSR002` | Warning | A view implements `IView` but has no public constructor, so the generated factory cannot create it |
| `TSR003` | Info | `ArlecchinoViewNamespace` is not set, so `ViewKind` lands in `$(RootNamespace).Navigation` — the message names the namespace it chose |

Whether a constructor parameter is actually registered in the container is not something the generator
can see; that surfaces at startup as the usual `InvalidOperationException` from the provider.
