[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# Source generator

`Arlecchino.Generators` ships inside the `Arlecchino` package as `analyzers/dotnet/cs` and holds two
incremental generators. They write one file each into the project that references the package:
`ArlecchinoViewNavigation.g.cs` for the routes and the view factory, and
`ArlecchinoStoreRegistration.g.cs` for the stores. Both land in the same namespace.

## What it looks for

**Views** — every class declaration with a base list, whose symbol is non-abstract and implements
`Arlecchino.Navigation.IView`. The route name is the type name with a trailing `View` stripped:
`ModsView` becomes `Mods`, `Settings` stays `Settings`.

**Stores** — the same, for `Arlecchino.State.IStore`. The name means nothing here; the marker is the
whole declaration. See [Stores](#stores) below.

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
constructor with the most parameters, so a view is built without reflection and stays AOT-friendly.
Namespaces of the views and of those parameter types are emitted as `using` directives, so views may
sit anywhere in the project.

The three types are emitted whether or not the project holds a view yet. A project with none gets an
empty `ViewKind`, a factory that creates nothing and a working `AddGeneratedViews()`, along with
`TSR004` — so the first thing a new application sees is a missing route rather than a missing method.

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

## Stores

A store is a class of atoms that outlives the screens reading it. Marking it with `IStore` is the
whole registration:

```csharp
public sealed class SettingsStore : IStore
{
    public State<string> Profile { get; } = new TrackedState<string>("");
}
```

```csharp
builder.Services
    .AddArlecchino()
    .AddGeneratedViews()
    .AddGeneratedStores()
    .StartAt(ViewKind.Default);
```

`AddGeneratedStores()` registers every store it found, in the container, as a singleton — no
`AddSingleton<SettingsStore>()` line to forget when a store is added, and no list to keep in sync.
Views and commands then take the store as a constructor parameter like any other service.

```csharp
public static class GeneratedStoreRegistration
{
    public static ArlecchinoBuilder AddGeneratedStores(this ArlecchinoBuilder builder)
    {
        builder.Services.AddSingleton(static services => new SettingsStore());
        builder.Services.AddScoped(static services => new DraftStore(services.GetRequiredService<TuiState>()));
        return builder;
    }
}
```

Each registration is a factory calling the public constructor with the most parameters, so nothing is
built by reflection and trimming keeps working — the same deal the view factory gets.

`IScopedStore` is the second marker: a store that belongs to one screen rather than to the
application. It is registered `AddScoped`, so it is built inside the scope
[the resolver opens per screen](views-and-navigation.md), disposed with it, and built afresh when the
screen is opened again. `IScopedStore` extends `IStore`, so it is found the same way.

| Marker | Lifetime | Holds |
|---|---|---|
| `IStore` | Singleton | State the whole application shares: settings, the catalogue, the session |
| `IScopedStore` | Scoped to the screen | State one screen owns but keeps out of the view: an editor's draft, a wizard's answers |

Nothing forces a store to be one or the other; a class with neither marker is simply invisible to the
generator and can still be registered by hand.

## MSBuild switches

The package's `build/Arlecchino.props` marks these properties compiler-visible; set them in your csproj.

| Property | Effect |
|---|---|
| `ArlecchinoViewNamespace` | Namespace `ViewKind`, `GeneratedViewFactory`, `AddGeneratedViews` and `AddGeneratedStores` land in |
| `RootNamespace` | Fallback when `ArlecchinoViewNamespace` is unset: `$(RootNamespace).Navigation`, or `Views` if that is empty too |
| `ArlecchinoGenerateViews` | Set to `false` to emit no routes and no view factory |
| `ArlecchinoGenerateStores` | Set to `false` to emit no store registration |

```xml
<PropertyGroup>
  <ArlecchinoViewNamespace>MyApp.Views</ArlecchinoViewNamespace>
</PropertyGroup>
```

Whichever namespace it lands in, files that navigate have to import it — `using MyApp.Navigation;` by
default — and that is what makes `ViewKind.Mods` read like an enum at the call site. Views may live in
that namespace or anywhere else; the generated file imports what it needs either way.

## Diagnostics

The generator says something instead of quietly doing the wrong thing:

| Id | Severity | Means |
|---|---|---|
| `TSR001` | Warning | Two views produce the same route — `Sample.ModsView` and `Sample.Extra.ModsView` both become `Mods`. The first one wins and the other is unreachable; rename one of them or register it explicitly |
| `TSR002` | Warning | A view implements `IView` but has no public constructor, so the generated factory cannot create it |
| `TSR003` | Info | `ArlecchinoViewNamespace` is not set, so `ViewKind` lands in `$(RootNamespace).Navigation` — the message names the namespace it chose |
| `TSR004` | Info | No class implements `IView`, so `ViewKind` holds no routes and the application has nowhere to start |
| `TSR005` | Warning | A store implements `IStore` but has no public constructor, so the container cannot build it |

Whether a constructor parameter is actually registered in the container is not something the generator
can see; that surfaces at startup as the usual `InvalidOperationException` from the provider.
