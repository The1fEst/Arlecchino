---
title: Source generator
sidebar_label: Source generator
description: How ViewKind, the view factory and the store, command and widget registrations are emitted.
---

# Source generator

`Arlecchino.Generators` ships inside the `Arlecchino` package as `analyzers/dotnet/cs` and holds four
incremental generators. They write one file each into the project that references the package:
`ArlecchinoViewNavigation.g.cs` for the routes and the view factory,
`ArlecchinoStoreRegistration.g.cs` for the stores, and `ArlecchinoCommandRegistration.g.cs` for the
application commands. All three land in the same namespace.

## What it looks for

**Views** — every class declaration with a base list, whose symbol is non-abstract and implements
`Arlecchino.Navigation.IArlecchinoView`. The route name is the type name with a trailing `View` stripped:
`ModsView` becomes `Mods`, `Settings` stays `Settings`.

**Stores** — the same, for `Arlecchino.Atoms.IArlecchinoStore`. The name means nothing here; the marker is the
whole declaration. See [Stores](#stores) below.

**Commands** — the same again, for `Arlecchino.Commands.IArlecchinoCommand`. See [Commands](#commands)
below.

A type nested inside another is found as readily as one at the top level and is named through its
owner — `Screens.ModsView`. One nested *privately* is skipped instead: the generated file lives in the
same assembly but outside that type, so naming it would not compile.

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

public sealed class GeneratedViewFactory : IArlecchinoViewFactory
{
    public bool TryCreate(IServiceProvider services, ViewRoute route, [NotNullWhen(true)] out IArlecchinoView? view) { ... }
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
`ARL004` — so the first thing a new application sees is a missing route rather than a missing method.

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

A store is a class of atoms that outlives the screens reading it. Marking it with `IArlecchinoStore` is the
whole registration:

```csharp
public sealed class SettingsStore : IArlecchinoStore
{
    public Atom<string> Profile { get; } = new TrackedAtom<string>("");
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
        builder.Services.AddScoped(static services => new DraftStore(services.GetRequiredService<ArlecchinoState>()));
        return builder;
    }
}
```

Each registration is a factory calling the public constructor with the most parameters, so nothing is
built by reflection and trimming keeps working — the same deal the view factory gets.

`IArlecchinoScopedStore` is the second marker: a store that belongs to one screen rather than to the
application. It is registered `AddScoped`, so it is built inside the scope
[the resolver opens per screen](views-and-navigation.md), disposed with it, and built afresh when the
screen is opened again. `IArlecchinoScopedStore` extends `IArlecchinoStore`, so it is found the same way.

| Marker | Lifetime | Holds |
|---|---|---|
| `IArlecchinoStore` | Singleton | State the whole application shares: settings, the catalogue, the session |
| `IArlecchinoScopedStore` | Scoped to the screen | State one screen owns but keeps out of the view: an editor's draft, a wizard's answers |

Nothing forces a store to be one or the other; a class with neither marker is simply invisible to the
generator and can still be registered by hand.

## Widgets

`.AddGeneratedWidgets()` does the same for the [widgets](widgets.md) of the project — every class
implementing `IArlecchinoWidget`, registered as a **singleton** built by a factory:

```csharp
builder.Services
    .AddArlecchino()
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddGeneratedWidgets()
    .StartAt(ViewKind.Default);
```

Only widgets declared in your own project are registered — the built-in ones live in the package's
assembly, which the generator never looks at.

A singleton widget is one instance for the life of the application, and a widget holds state: the
selection, the scroll offset, whether it has the focus. Two screens resolving the same widget share
all of it. That is the point when the widget is a shared panel, and a bug when it is not — build the
second kind in the view instead, as before.

Some widgets cannot be registered at all, and the generator says so with `ARL007` rather than
emitting code that would not compile:

| Left out when | Because |
|---|---|
| The class is generic | There is no single closed type to register |
| It has no public constructor | The factory has nothing to call |
| It has `required` members | A factory cannot fill them in — `ListBox<T>.Render` is exactly this |
| It is nested privately in another type | The generated file cannot name it, and code that names it would not compile |

The three built-in reasons cover the built-in widgets too, which is another way of saying the same
thing: `ListBox<T>`, `Table<T>` and `Form` are constructed where they are used, with their
`Render`, `Columns` and `Fields` given at the call site.

`ArlecchinoGenerateWidgets` set to `false` turns the generator off.

`AddWidget<T>()` registers one widget by hand, the same singleton the generator would have made. It
is for a widget the generator cannot see — one from another assembly — and it is an alternative to
`AddGeneratedWidgets()`, not a layer on top: registering the same type both ways puts it in the
container twice, exactly as `AddCommand<T>()` and `AddGeneratedCommands()` do.

```csharp
builder.Services
    .AddArlecchino()
    .AddWidget<SearchPanel>()
    .StartAt(ViewKind.Default);
```

## Commands

An application command is a class implementing `IArlecchinoCommand`, and it registers the way a store
does:

```csharp
public sealed class QuitCommand : IArlecchinoCommand
{
    private readonly IHostApplicationLifetime _lifetime;

    public QuitCommand(IHostApplicationLifetime lifetime) => _lifetime = lifetime;

    public KeyBinding Binding => new(ConsoleKey.Q, control: true);
    public string Icon => "×";
    public string Label => "Quit";

    public ViewRoute Execute()
    {
        _lifetime.StopApplication();
        return ViewRoute.None;
    }
}
```

```csharp
builder.Services
    .AddArlecchino()
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddGeneratedCommands()
    .StartAt(ViewKind.Default);
```

```csharp
public static class GeneratedCommandRegistration
{
    public static ArlecchinoBuilder AddGeneratedCommands(this ArlecchinoBuilder builder)
    {
        builder.Services.AddSingleton<IArlecchinoCommand>(static services =>
            new QuitCommand(services.GetRequiredService<IHostApplicationLifetime>()));
        return builder;
    }
}
```

Every command becomes a singleton `IArlecchinoCommand` built from its public constructor with the most
parameters, so `CommandRegistry` and the palette pick it up with no list to keep in sync.

`AddGeneratedCommands()` and `AddCommand<T>()` are alternatives, not layers: calling both for the same
type registers it twice and it appears twice in the palette. Use the generator, and keep
`AddCommand<T>()` for a command that comes from another assembly or is chosen at runtime.

Screen commands are a different thing — a view returns those from `Commands()` as data and nothing
registers them. See [Commands](commands.md).

## MSBuild switches

The package's `build/Arlecchino.props` marks these properties compiler-visible; set them in your csproj.

| Property | Effect |
|---|---|
| `ArlecchinoViewNamespace` | Namespace `ViewKind`, `GeneratedViewFactory`, `AddGeneratedViews`, `AddGeneratedStores` and `AddGeneratedCommands` land in |
| `RootNamespace` | Fallback when `ArlecchinoViewNamespace` is unset: `$(RootNamespace).Navigation`, or `Views` if that is empty too |
| `ArlecchinoGenerateViews` | Set to `false` to emit no routes and no view factory |
| `ArlecchinoGenerateStores` | Set to `false` to emit no store registration |
| `ArlecchinoGenerateWidgets` | Set to `false` to emit no widget registration |
| `ArlecchinoGenerateCommands` | Set to `false` to emit no command registration |

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
| `ARL001` | Warning | Two views produce the same route — `Sample.ModsView` and `Sample.Extra.ModsView` both become `Mods`. The first one wins and the other is unreachable; rename one of them or register it explicitly |
| `ARL002` | Warning | A view implements `IArlecchinoView` but has no public constructor, so the generated factory cannot create it and leaves it out |
| `ARL003` | Info | `ArlecchinoViewNamespace` is not set, so `ViewKind` lands in `$(RootNamespace).Navigation` — the message names the namespace it chose |
| `ARL004` | Info | No class implements `IArlecchinoView`, so `ViewKind` holds no routes and the application has nowhere to start |
| `ARL005` | Warning | A store implements `IArlecchinoStore` but has no public constructor, so it is left out of `AddGeneratedStores()` |
| `ARL006` | Warning | A command implements `IArlecchinoCommand` but has no public constructor, so it is left out of `AddGeneratedCommands()` |
| `ARL007` | Info | A widget cannot be registered — generic, no public constructor, or `required` members — and is left out of `AddGeneratedWidgets()` |

Whether a constructor parameter is actually registered in the container is not something the generator
can see; that surfaces at startup as the usual `InvalidOperationException` from the provider.
