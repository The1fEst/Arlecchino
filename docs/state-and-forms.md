[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [Widgets](widgets.md) · [Localization](localization.md) · [Packages and building](packages-and-building.md)

# State and forms

Application state lives in atoms: small observable cells that notify what reads them and mark the
frame stale by themselves. Subscriptions are deliberately coarse — there is no need for fine-grained
ones to keep rendering cheap, because a frame already redraws everything and only changed cells reach
the terminal.

An atom is one of two types — `TrackedAtom<T>` for state the undo stack should carry, `LocalAtom<T>`
for state it should not. `Atom<T>` is the base both share and the type everything else is written
against; it is abstract, so the choice is made once, where the atom is declared.

## Atoms

```csharp
public sealed class SettingsStore : IArlecchinoStore
{
    public Atom<string> Profile { get; } = new TrackedAtom<string>("");
    public Atom<decimal> Volume { get; } = new TrackedAtom<decimal>(60);
    public Atom<bool> Fullscreen { get; } = new TrackedAtom<bool>(true);
    public Atom<int> Cursor { get; } = new LocalAtom<int>(0);
}
```

A class of atoms like this one is a **store**. Marking it `IArlecchinoStore` is all the wiring there is: the
generator finds it and `.AddGeneratedStores()` puts it in the container as a singleton, so views and
commands take it as a constructor parameter like any other service — nothing to register by hand and
nothing to forget when a store is added. `IArlecchinoScopedStore` does the same for state that belongs to one
screen. See [Source generator](source-generator.md#stores).

`Atom<T>` itself is abstract: an atom is created as the kind it is, and the declaration says whether
its edits can be taken back. Everything that consumes an atom — `Field.*`, `Computed<T>`, a view's
constructor — takes `Atom<T>`, so the two are interchangeable at the call site.

| Type | Undo | For |
|---|---|---|
| `TrackedAtom<T>` | Yes | What the user authored: the draft being edited, a setting, the selected item |
| `LocalAtom<T>` | No | What the user did not: a filter, a cursor, a load in progress |

| Member | Meaning |
|---|---|
| `Value` | Reads and writes; writing an equal value changes nothing and notifies nobody |
| `Subscribe(listener)` | Returns an `IDisposable`; dispose it to stop listening |

Every write also requests a repaint, so a screen driven by atoms never needs a manual
`Repaint.Request()`.

## Derived values

`Computed<T>` re-evaluates lazily and tracks whatever it read while doing so — including other
computed values, and including branches taken only sometimes:

```csharp
public Computed<bool> CanImport { get; } = new(() => Profile.Value.Length > 0 && Theme.Value.Length > 0);
```

There is no dependency list to keep in sync: reading `Profile.Value` inside the lambda is the
subscription.

## What belongs in an atom

- **`TrackedAtom<T>`** — state that outlives a view or is read by more than one screen, and that the
  user would expect `Undo` to take back: a draft being edited, settings, the selected mod.
- **`LocalAtom<T>`** — the same reach, but nothing the user authored: what a background load
  produced, which row is selected, the filter a screen keeps between visits.
- **Plain field of the view** — the cursor in a list, the scroll offset, anything that dies with the
  view. Making these atoms buys nothing.

## Undo and redo

`AtomHistory` is registered by `AddArlecchino` and records every `TrackedAtom<T>` there is — there is
no list of atoms to keep in sync, and nothing to register. Take it where you need `Undo()` / `Redo()`:

```csharp
public Atom<string> Profile { get; } = new TrackedAtom<string>("");   // on the undo stack
public Atom<int> Cursor { get; } = new LocalAtom<int>(0);             // never on it
```

```csharp
using (history.Group())
{
    settings.Profile.Value = "fEst";
    settings.Volume.Value = 80;
}

history.Undo();   // both fields go back together
```

Groups nest, and the count is what matters rather than the innermost scope: a helper that groups its
own edits, called from inside a group of yours, joins it instead of closing it early. The step is
committed when the outermost scope is disposed.

Undoing does not record itself, and writing something new after an undo drops the redo branch.

The stack is bounded by `Capacity` (200 steps): the oldest fall off the far end, because a session
that runs all day would otherwise keep every edit — and every value those edits replaced — alive for
as long as it runs. A group counts as one step. Lowering `Capacity` trims immediately.

The history records from the moment it exists — the hosted service resolves it at startup and clears
it once the application is up, so edits made while wiring things together do not end up as the first
undo step. Rendering a frame headlessly (tests, `--frame`) has no hosted service, so resolve
`AtomHistory` yourself before making edits you intend to undo.

## Loading in the background

`AsyncAtom<T>` wraps a load in progress and lands its result on the frame loop through
[`UiDispatcher`](rendering.md):

```csharp
private readonly AsyncAtom<IReadOnlyList<Mod>> _mods;

_mods.Load(async token => await _service.LoadAsync(token));
```

| Member | Meaning |
|---|---|
| `Value` | Last loaded value, `default` until one arrives |
| `Status` | `Idle`, `Loading`, `Loaded`, `Failed` — an atom itself, so a view can show a spinner |
| `Error` | The exception of the last failure |
| `Load(load)` | Starts a load, cancelling the one in flight |
| `Cancel()` | Cancels without starting another |

A failed load is kept as `Failed` + `Error` rather than thrown at the render loop. Cancelling keeps
the last value but drops the status back to `Idle`, so a spinner bound to it stops.

### Tying work to the screen

Work outlives the screen that started it unless something stops it. `ViewLifetime` is that something:
it is scoped, so [each screen gets its own](views-and-navigation.md), and navigating away cancels it.

```csharp
public sealed class ModsView : IArlecchinoView
{
    private readonly AsyncAtom<IReadOnlyList<Mod>> _mods;

    public ModsView(ViewLifetime lifetime, ModService service)
    {
        _mods = lifetime.Loading<IReadOnlyList<Mod>>();
        lifetime.Track(_mods.Subscribe(Redraw));

        _mods.Load(token => service.LoadAsync(token));
    }
}
```

| Member | Does |
|---|---|
| `Loading<T>(initial)` | An `AsyncAtom<T>` that is cancelled when the screen goes away |
| `Track(resource)` | Disposes a subscription, timer or handle with the screen; returns it back |
| `OnClose(action)` | Runs something as the screen goes |
| `Closing` | The token to pass into work you start yourself; readable after the screen has gone |

The view no longer needs `IDisposable` for any of this. What it does still need it for is anything it
wants to do *before* its scope is released — the view is disposed first, then the scope.

Releasing happens once, over a snapshot of what was registered, so a resource whose own `Dispose`
reaches back into the lifetime does not break the screen it is closing. Anything handed to `Track`
after that point is disposed immediately rather than held by a screen that is already gone.

## Forms

A view is the form; `Form` is the part that turns atoms into editable rows, each opening the modal
that matches its type:

```csharp
_form = new Form(state, options)
{
    Fields =
    [
        Field.Text(() => Loc(LocString.Profile), settings.Profile, help: () => Loc(LocString.ProfileHelp)),
        Field.Secret(() => Loc(LocString.Passphrase), settings.Passphrase),
        Field.Choice(() => Loc(LocString.Theme), ["dark", "light"], settings.Theme),
        Field.Slider(() => Loc(LocString.Volume), settings.Volume, 0, 100),
        Field.Toggle(() => Loc(LocString.Fullscreen), settings.Fullscreen, value => value ? Yes : No),
        Field.Path(() => Loc(LocString.Folder), settings.Folder, ViewKind.Settings, pickFolder: true),
        Field.Action(() => Loc(LocString.Apply), Apply, enabled: () => settings.IsComplete.Value),
    ],
};

public void Draw() => _form.Place(_surface.Content);
public ViewRoute Handle(ConsoleKeyInfo key) => _form.Handle(key).Route;
public ViewRoute HandleMouse(MouseEvent mouse) => _form.HandleMouse(mouse).Route;
```

Rendered as `label = value`, labels padded to the longest, the help of the selected field on the line
under it, actions as `> Label`. The help line exists only when the selected field actually has help,
so a form of fields without any is a solid column of rows:

```
  Profile    = empty
    shown in the title bar
  Passphrase = ••••••
  Theme      = dark
  Volume     = 60

  > Apply
```

| Factory | Opens |
|---|---|
| `Field.Text`, `Field.Secret` | Text modal, masked for secrets |
| `Field.Number`, `Field.Slider` | Number and slider modals |
| `Field.Toggle` | Toggle modal |
| `Field.Choice`, `Field.MultiChoice` | Choice and multi-choice modals |
| `Field.Date`, `Field.Time`, `Field.Color` | Segment editors and the colour picker |
| `Field.Path` | The [file picker](file-picker.md); returns its route so the view navigates |
| `Field.Action` | Nothing — runs your delegate and returns a route |

Movement, `Confirm` and `Erase` come from the [keymap](commands-and-input.md); `Erase` resets a field
to its empty value. Clicking a row selects it, clicking the selected row opens it, and the wheel
moves the selection. `Field.Action` takes an `enabled` predicate — usually a `Computed<bool>` — and
draws itself muted while it is false.

Because fields read and write atoms, an edit made through a modal is already undoable if the atom is
tracked, and a value changed from the outside redraws the form without anyone telling it to.

## Views that subscribe

A view that subscribes to an atom has to unsubscribe. Implement `IDisposable` on it — the navigator
disposes a view when it leaves the route:

```csharp
public sealed class SettingsView : IArlecchinoView, IDisposable
{
    private readonly IDisposable _watch;

    public SettingsView(SettingsStore settings, ArlecchinoState state) =>
        _watch = settings.Summary.Subscribe(() => state.Output = settings.Summary.Value);

    public void Dispose() => _watch.Dispose();
}
```

Views that only read atoms in `Draw` need none of this: reading happens fresh every frame.

## Threads

Atoms are not thread-safe. Anything that finishes on another thread writes through
`UiDispatcher.Post` — which is what `AsyncAtom` does internally.

That rule works because there is exactly one thread to be on. The frame loop and the loop reading the
terminal are separate, but the reader does not route what it reads: it queues it, and the frame loop
routes the queue just before drawing. So every view, widget, atom and modal — and the `Surface` they
draw into — is touched by the drawing thread alone, and none of them needs a lock. What you post from
elsewhere runs there too, immediately before the frame that shows it.
