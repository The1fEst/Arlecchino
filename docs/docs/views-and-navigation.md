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
}
```

Only `Draw` and `Handle` have to be written: `HandleMouse`, `HandlePaste`, `Commands` and `Hints`
have default implementations, so a view writes what it needs and nothing else — see
[Commands](commands.md).

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
