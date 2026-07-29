---
title: Atoms
sidebar_label: Atoms
description: TrackedAtom and LocalAtom, computed values, the undo history, and what belongs in an atom rather than in a field.
---

# Atoms

Application state lives in atoms: small observable cells that notify what reads them and mark the
frame stale by themselves. Subscriptions are deliberately coarse — there is no need for fine-grained
ones to keep rendering cheap, because a frame already redraws everything and only changed cells reach
the terminal.

```csharp
public Atom<string> Profile { get; } = new TrackedAtom<string>("");
public Atom<int> Cursor { get; } = new LocalAtom<int>(0);
```

## Two kinds, chosen at the declaration

`Atom<T>` is abstract. An atom is created as the kind it is, and the declaration says whether its edits
can be taken back:

| Type | Undo | For |
|---|---|---|
| `TrackedAtom<T>` | Yes | What the user authored: the draft being edited, a setting, the selected item |
| `LocalAtom<T>` | No | What the user did not: a filter, a cursor, a load in progress |

Everything that consumes an atom — `Field.*`, `Computed<T>`, a view's constructor — takes `Atom<T>`,
so the two are interchangeable at the call site. Whether an edit is undoable is decided once, where
the state is declared, rather than by a flag set somewhere else afterwards.

| Member | Meaning |
|---|---|
| `Value` | Reads and writes; writing an equal value changes nothing and notifies nobody |
| `Post(value)` | Writes from another thread, applied just before the next frame — see [Threads](#threads) |
| `Subscribe(listener)` | Returns an `IDisposable`; dispose it to stop listening |
| `RecordsHistory` | Whether edits of this atom enter the undo history |

Every write that actually changed the value also requests a repaint, so a screen driven by atoms never
needs a manual `Repaint.Request()`. Equality is `EqualityComparer<T>.Default` unless a comparer was
passed to the constructor.

## Derived values

`Computed<T>` re-evaluates lazily and tracks whatever it read while doing so — including other
computed values, and including branches taken only sometimes:

```csharp
public Computed<bool> CanImport { get; } =
    new(() => Profile.Value.Length > 0 && Theme.Value.Length > 0);
```

There is no dependency list to keep in sync: reading `Profile.Value` inside the lambda **is** the
subscription. `Computed<T>` implements `IReadableAtom<T>`, so anything that only reads an atom —
`Field.Action(enabled:)`, a widget's `IsEnabled` — takes a computed value as readily as a plain one.

## What belongs in an atom

| State | Where |
|---|---|
| Outlives a view or is read by more than one screen, and `Undo` should take it back | `TrackedAtom<T>` |
| The same reach, but nothing the user authored | `LocalAtom<T>` |
| The cursor in a list, a scroll offset, anything that dies with the view | A plain field |

Making a per-view cursor an atom buys nothing: the view already redraws every frame, and the atom only
adds a subscription nobody reads.

## Lists

An `Atom<List<T>>` looks like the obvious way to hold many things and is a trap. Adding to the list
inside it never goes through `Atom.Value`, so:

- **nothing is notified and no frame is asked for** — the screen changes on the next keystroke or
  resize, which reads as an application that sometimes lags behind itself;
- **the drawing thread is not checked**, so a background task can append while a widget is
  enumerating the list mid-frame;
- **writing the same instance back does not fix it**: an atom compares with
  `EqualityComparer<T>.Default`, a list is compared by reference, and the write is taken for a change
  of nothing and dropped.

There are two right answers, and which one to use is a question of size and rate.

**A set of things that is small, or is replaced rather than edited** — hold a read-only list and swap
it wholesale:

```csharp
public Atom<IReadOnlyList<string>> Columns { get; } = new TrackedAtom<IReadOnlyList<string>>(["Name", "Size"]);

Columns.Value = [.. Columns.Value, "Kind"];
```

Every write is a new list, so the undo history holds a before and an after that are genuinely
different, and the whole thing costs one copy per change.

**A list appended to often, or long enough that copying it hurts** — hold an `AtomsList<T>`, which
changes in place and still does everything a write does:

```csharp
public LocalAtomsList<string> Log { get; } = new();
public TrackedAtomsList<Task> Plan { get; } = new();

Log.Add(line);
Plan.Insert(0, task);
```

The two kinds mirror the two atoms: `TrackedAtomsList<T>` goes on the undo stack, `LocalAtomsList<T>`
does not.

| Member | Meaning |
|---|---|
| `Value` | A live, read-only view of the contents. Hand it to a widget once and it draws whatever is in the list on every later frame |
| `Count`, `this[index]`, `IndexOf(item)` | Reading. Writing the indexer an equal item changes nothing |
| `Add(item)`, `Add(items)`, `Insert(index, item)` | Adding. The overload taking a list is one notification and one undo step for the lot |
| `Remove(item)`, `RemoveAt(index)`, `Clear()` | Taking out. Removing something that is not there changes nothing |
| `RemoveRange(index, count)` | Takes out several in a row as one change, which is how a list that has grown too long is trimmed |
| `Reset(items)` | Replaces the contents, for a list that is reloaded rather than edited |
| `Subscribe(listener)` | Same as an atom's |

One call is one step, which is why `Add(rows)` and `RemoveRange` exist: a loop of `Add(row)` would
come back a row at a time, and a list kept to a length — the last thousand lines of output, say — is
trimmed in one call rather than one line at a time:

```csharp
if (Lines.Count > Kept)
{
    Lines.RemoveRange(0, Lines.Count - Kept);
}
``` `Value` is read-only all the way down — there is no cast back to the list underneath — so every
change is seen by the frame and by the history.

An `AtomsList<T>` has no `Post` of its own, because a change is a call rather than a value. Hand the
whole change over instead:

```csharp
var loaded = await ReadRowsAsync(token);

FrameThread.Post(() => Rows.Reset(loaded));
```

## Undo and redo

`AtomHistory` is registered by `AddArlecchino` and records every `TrackedAtom<T>` there is — there is
no list of atoms to keep in sync, and nothing to register. Take it where you need `Undo()` / `Redo()`:

```csharp
private readonly AtomHistory history;

using (history.Group())
{
    settings.Profile.Value = "fEst";
    settings.Volume.Value = 80;
}

history.Undo();   // both fields go back together
```

| Member | Meaning |
|---|---|
| `Undo()` / `Redo()` | Returns whether there was anything to do |
| `CanUndo` / `CanRedo` | For drawing the state of a menu entry |
| `Group()` | Opens a step that several edits join |
| `Depth` | How many steps are on the stack |
| `Capacity` | How many are kept; 200 by default |
| `Clear()` | Drops everything |

Groups nest, and the count is what matters rather than the innermost scope: a helper that groups its
own edits, called from inside a group of yours, joins it instead of closing it early. The step is
committed when the outermost scope is disposed.

Undoing does not record itself, and writing something new after an undo drops the redo branch.

The stack is bounded because a session that runs all day would otherwise keep every edit — and every
value those edits replaced — alive for as long as it runs. A group counts as one step. Lowering
`Capacity` trims immediately.

:::note[When the history starts recording]

The history records from the moment it exists. The hosted service resolves it at startup and clears it
once the application is up, so edits made while wiring things together do not end up as the first undo
step. Rendering a frame headlessly — tests, `--frame` — has no hosted service, so resolve `AtomHistory`
yourself before making edits you intend to undo.

:::

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

Or hand the subscription to [`ViewLifetime.Track`](views-and-navigation.md) and skip the interface.

Views that only read atoms in `Draw` need none of this: reading happens fresh every frame.

## Threads

Atoms are not thread-safe, and say so: writing one from off the drawing thread throws. Anything that
finishes elsewhere hands the value over instead, and the atom does the handing:

```csharp
private readonly Atom<ScanStep> _step;

public void Report(ScanStep value) => _step.Post(value);
```

`Post` writes the value just before the next frame, in the order it was posted. Everything a plain
write does happens then — subscribers are notified, a repaint is asked for, and a `TrackedAtom<T>`
records an undo step, so a posted edit is taken back by `Undo` like any other.

The write has not happened when `Post` returns, which is why the `Value` setter throws rather than
quietly posting for you: `atom.Value = loaded` followed by reading `atom.Value` back would return the
old value with nothing to say so. The method is named for what it does.

Atoms that have to change together belong in one `FrameThread.Post` with a block rather than a `Post`
each, so that no frame falls between them:

```csharp
private readonly List<string> _log = [];

FrameThread.Post(() =>
{
    _log.Add(line);
    Written.Value = _log.Count;
});
```

That is what [`AsyncAtom`](async-atoms.md) does internally with its value and its status, which is why
a load never draws as finished while the old value is still on screen.
[The frame loop](frame-loop.md#which-thread-draws) is where the rule comes from.
