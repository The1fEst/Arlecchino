---
title: Views and navigation
sidebar_label: Views and navigation
description: IArlecchinoView, ViewRoute, the navigator, history and view registration.
---

# Views and navigation

## IArlecchinoView

```csharp
public interface IArlecchinoView
{
    void Draw();
    ViewRoute Handle(ConsoleKeyInfo key);
    ViewRoute HandleMouse(MouseEvent mouse) => ViewRoute.None;
    ViewRoute HandlePaste(string text) => ViewRoute.None;
    IReadOnlyList<ViewCommand> Commands() => [];
    (string Key, string Description)[] Hints() => [];
    bool UsesLayout => true;
}
```

Only `Draw` and `Handle` have to be written: everything else has a default implementation, so a view
writes what it needs and nothing else — see [Commands](commands.md) and
[a layout around every view](#a-layout-around-every-view).

`Draw` runs once per frame against the shared [`Surface`](rendering.md). `Handle` receives keys that
survived the input router — modal keys and the command palette key never reach a view, see
[Commands](commands.md). `Hints` is optional; whatever it returns is drawn as the
`Keys` box in the bottom-right corner while no modal is open.

A view instance lives as long as it is the current view. Navigating away and back constructs a new
one, so per-view state (selection, filter, scroll offset) can live in fields; anything that must
survive navigation belongs in a service or in [`ArlecchinoState`](state.md).

## ViewRoute

```csharp
public readonly record struct ViewRoute(string Name);
```

A route is a string name in a struct, compared ordinally. `ViewRoute.None` is the empty route: return
it from `Handle` to stay where you are. `IsNone` tells the two apart.

Routes are strings rather than an enum on purpose — the framework has to name a route without seeing
your application's types, and the generated route table lives in your assembly, not in Arlecchino.

## Getting a route

- **Generated.** The analyzer inside the package finds every non-abstract `IArlecchinoView` in your project,
  strips the `View` suffix and emits a `ViewKind` class of `ViewRoute` fields. Turn it on with
  `.AddGeneratedViews()`. See [Source generator](source-generator.md).
- **Explicit.** `.AddView<ModsView>("Mods")` resolves the type through the container, or
  `.AddView("Mods", provider => new ModsView(...))` builds it yourself.

Both may be mixed. `ViewResolver` walks the registered `IArlecchinoViewFactory` instances in registration order
and the explicit registry is added first by `AddArlecchino`, so an explicit registration wins over the
generated factory for the same route name.

If nothing can build the route, `ViewResolver.Create` throws `InvalidOperationException` naming the
route and both ways to register it.

## Navigator

`Navigator` is a singleton holding the current view, a back stack and a forward stack.

| Member | Behaviour |
|---|---|
| `Apply(route)` | Navigates. Ignores `ViewRoute.None` and the current route; pushes the previous route onto the back stack and clears the forward stack |
| `Back()` / `Forward()` | Walk the history, return `false` when the stack is empty |
| `Reload()` | Rebuilds the current view from scratch |
| `CurrentRoute` | The route being shown |
| `CurrentHints` | `Hints()` of the current view |
| `CanGoBack` / `CanGoForward` | Whether the stacks hold anything |
| `Draw()` / `Handle(key)` | Called by the frame loop and the input router |

`Alt+←` and `Alt+→` are handled by the navigator itself and never reach the view. Every other key goes
to `IArlecchinoView.Handle`, and the route it returns is passed to `Apply`.

Navigation is all-or-nothing. The new screen is built before anything is given up, so a view whose
constructor throws — a store that was never registered is the usual reason — leaves the route, the
history and the screen on display exactly as they were. The exception itself is caught by the input
router: it goes to the log and the output row, the way a view that throws while drawing does.

The start route comes from `ArlecchinoOptions.StartRoute` (`.StartAt(...)`), applied in the navigator's
constructor. For a start route that depends on runtime state, implement `IArlecchinoStartup` instead —
see [Hosting and options](hosting-and-options.md).

## A layout around every view

A band along the top, a bar along the bottom, whatever a screen of this application always has around
it. `IArlecchinoLayout` is Razor's `_Layout.cshtml` with `@RenderBody()`: it is handed the room there
is and a delegate that draws the view, and where it calls that delegate is where the view goes.

```csharp
public sealed class Chrome : IArlecchinoLayout
{
    private readonly Tabs _tabs;

    public Chrome(Tabs tabs) => _tabs = tabs;

    public void Draw(SurfaceRegion frame, Action<SurfaceRegion> body)
    {
        _tabs.Draw(frame.Rows(0, 1));

        body(frame.Rows(1, frame.Height - 2));

        frame.WriteLine(frame.Height - 1, "F1 help · F10 quit", Theme.Muted);
    }
}
```

```csharp
builder.Services
    .AddArlecchino()
    .AddGeneratedViews()
    .UseLayout<Chrome>()
    .StartAt(ViewKind.Default);
```

**No view has to be edited.** A view asks the [`Surface`](rendering.md) for its content and is handed
the room the layout left it, so the same view draws correctly with a layout, without one, or under a
different one. A screen that wants the whole terminal — a file being read, a picture — says so:

```csharp
public bool UsesLayout => false;
```

One instance serves the whole application, built from the container like anything else. That is the
point of it: what the layout holds outlives the view, so a row of tabs keeps its scroll position when
a screen is left and come back to, which a header drawn again by every view cannot do. A layout that
needs the [`Navigator`](#navigator) should resolve it from `IServiceProvider` when it draws rather than
take it in the constructor, since the navigator is built after it.

`HandleMouse` sees a click before the view does, for a header that answers to one:

```csharp
public bool HandleMouse(MouseEvent mouse) => _tabs.Clicked(mouse.Row, mouse.Column);
```

Returning `true` means the layout took it and the view never sees it. There is no key equivalent on
purpose: a key that works on every screen is an [application command](commands.md), which the
framework already had, and two ways to say one thing is one too many.

:::note[The framework's own chrome sits on top]

The [hints box](commands.md) and the [output line](state.md#the-output-line) are drawn over the frame
after the layout, so an application with a bar of its own turns them off:

```csharp
options.ShowHints = false;
options.ShowOutputLine = false;
```

:::

## Focus inside a view

A view with more than one pane needs to know which one keys go to. `IArlecchinoFocusable` is that contract —
the input half of [`IArlecchinoInteractiveWidget`](widgets.md), and what the ring actually cycles —
and `FocusRing` is the `Tab` / `Shift+Tab` cycle over them:

```csharp
private readonly FocusRing _panes;
private readonly ListBox<Mod> _list;
private readonly ListBox<string> _sidebar;

_panes = new FocusRing(options.Keymap);
_panes.Add(_list);        // first one added starts focused
_panes.Add(_sidebar);

public ViewRoute Handle(ConsoleKeyInfo key) => _panes.Handle(key);
public ViewRoute HandleMouse(MouseEvent mouse) => _panes.HandleMouse(mouse);
```

An item answers with a `FocusResult`: `Ignored` (the key was not mine), `Handled`, or
`Navigate(route)`. The ring returns the route so the view can hand it straight back to the navigator,
and moves focus to whichever item claims a mouse event.

| Member | Meaning |
|---|---|
| `IsFocused` | Set by the ring; draw the item differently when it is false |
| `Handle(key)` / `HandleMouse(mouse)` | Return `FocusResult` |
| `FocusRing.Add/Focus/FocusNext/FocusPrevious` | Building and moving the cycle |
| `FocusRing.Current` / `Index` / `Items` | What is focused right now |

Every interactive [widget](widgets.md) is focusable by construction, so lists, tables, trees, tabs and
[`Form`](forms.md) go into the ring as they are. `FocusablePane` covers the other case: it
wraps delegates for a pane that draws itself elsewhere and only needs somewhere to route keys — that
is how the file picker holds its list and its places sidebar.

## Custom view factories

```csharp
public interface IArlecchinoViewFactory
{
    bool TryCreate(
        IServiceProvider services,
        ViewRoute route,
        [NotNullWhen(true)] out IArlecchinoView? view);
}
```

Register one with `.AddViewFactory<T>()` to resolve a whole family of routes at once — a plugin
directory, say, or routes carrying an id in the name. Return `false` for routes you do not own so the
next factory gets a turn. Build from the `services` handed in rather than from a container you
captured — that is the screen's own scope.

## Each screen gets a scope

`ViewResolver` opens an `IServiceScope` per screen and builds the view inside it, so a scoped
service — a database context, a unit of work, a connection — belongs to the screen that asked for it:

```csharp
builder.Services.AddScoped<CatalogContext>();

public sealed class ModsView : IArlecchinoView
{
    public ModsView(CatalogContext catalog) { … }
}
```

Navigating away disposes the view first (if it implements `IDisposable`) and then the scope, so the
view can still use what it took during its own `Dispose`. Navigating back builds a fresh view *and* a
fresh scope — going back is not a cache. Singletons behave as they always did: `ArlecchinoState`, `Surface`
and the rest are the same instances everywhere.
