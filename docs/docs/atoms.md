---
title: Atoms
sidebar_label: Atoms
description: Atom and HistoryAtom, computed values, the undo history in a package of its own, and what belongs in an atom rather than in a field.
---

# Atoms

Application state lives in atoms: small observable cells that notify what reads them and mark the
frame stale by themselves. Subscriptions are deliberately coarse — there is no need for fine-grained
ones to keep rendering cheap, because a frame already redraws everything and only changed cells reach
the terminal.

```csharp
public Atom<string> Profile { get; } = new("");
public Atom<int> Cursor { get; } = new(0);
```

## One kind here, another in the undo package

`Atom<T>` holds a value, tells what reads it, and forgets what it held. Nothing in it mentions a
history: an atom whose edits should be undoable is a `HistoryAtom<T>`, which lives in
`Arlecchino.Atoms.History` — a package `Arlecchino` does not reference, so an application that wants no
undo does not carry it.

| Type | Undo | For |
|---|---|---|
| `Atom<T>` | No | What the user did not author: a filter, a cursor, a load in progress |
| `HistoryAtom<T>` | Yes | What the user did: the draft being edited, a setting, the selected item |

```csharp
public HistoryAtom<string> Profile { get; } = new("", "SettingsStore.Profile");
```

A history atom is given a key, and it is required: the key is what carries the atom across a run when
the history [outlives it](#undo-and-redo), and a name worked out from the property it sits in would
quietly orphan a kept history the first time that property was renamed.

Everything that consumes an atom — `Fields.*`, `Computed<T>`, a view's constructor — takes `Atom<T>`,
which `HistoryAtom<T>` is, so the two are interchangeable at the call site. Whether an edit is undoable
is decided once, where the state is declared, rather than by a flag set somewhere else afterward.

| Member | Meaning |
|---|---|
| `Value` | Reads and writes; writing an equal value changes nothing and notifies nobody |
| `Post(value)` | Writes from another thread, applied just before the next frame — see [Threads](#threads) |
| `Subscribe(listener)` | Returns an `IDisposable`; dispose it to stop listening |
| `ToString()` | What the atom holds, written out; one holding nothing reads as `IReadableAtom.UndefinedValue` |

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
a widget's `IsEnabled` — takes a computed value as readily as a plain one.

## What belongs in an atom

| State | Where |
|---|---|
| Outlives a view or is read by more than one screen, and `Undo` should take it back | `HistoryAtom<T>` |
| The same reach, but nothing the user authored | `Atom<T>` |
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
public HistoryAtom<IReadOnlyList<string>> Columns { get; } = new(["Name", "Size"], "Layout.Columns");

Columns.Value = [.. Columns.Value, "Kind"];
```

Every write is a new list, so the undo history holds a before and an after that are genuinely
different, and the whole thing costs one copy per change.

**A list appended to often, or long enough that copying it hurts** — hold an `AtomsList<T>`, which
changes in place and still does everything a write does:

```csharp
public AtomsList<string> Log { get; } = new();
public HistoryAtomsList<Task> Plan { get; } = new("Plan.Tasks");

Log.Add(line);
Plan.Insert(0, task);
```

The two kinds mirror the two atoms: `HistoryAtomsList<T>` goes on the undo stack and takes a key before
what it starts with, `AtomsList<T>` does neither.

| Member | Meaning |
|---|---|
| `Value` | A live, read-only view of the contents. Hand it to a widget once and it draws whatever is in the list on every later frame |
| `Count`, `this[index]`, `IndexOf(item)` | Reading. Writing the indexer an equal item changes nothing |
| `Add(item)`, `Add(items)`, `Insert(index, item)` | Adding. The overload taking a list is one notification and one undo step for the lot |
| `Remove(item)`, `RemoveAt(index)`, `Clear()` | Taking out. Removing something that is not there changes nothing |
| `RemoveRange(index, count)` | Takes out several in a row as one change, which is how a list that has grown too long is trimmed |
| `Reset(items)` | Replaces the contents, for a list that is reloaded rather than edited |
| `Refresh()` | Tells everything watching to read the list again, for when an item changed inside itself |
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

## Maps

The same story for a dictionary, and the same trap: writing into a `Dictionary<TKey, TValue>` held in
an atom notifies nobody, and putting the same instance back is taken for a change of nothing.
`AtomsMap<TKey, TValue>` holds one and routes every change through the atom's path:

```csharp
public AtomsMap<string, ServerState> Servers { get; } = new();
public HistoryAtomsMap<string, string> Overrides { get; } = new("Settings.Overrides", comparer: StringComparer.OrdinalIgnoreCase);

Servers["build-01"] = ServerState.Online;
Overrides.Remove("theme");
```

`HistoryAtomsMap<TKey, TValue>` goes on the undo stack, `AtomsMap<TKey, TValue>` does not.

| Member | Meaning |
|---|---|
| `Value` | A live, read-only view of the contents |
| `Count`, `ContainsKey(key)`, `TryGetValue(key, out value)` | Reading without throwing |
| `this[key]` | Reads, and writes whether or not the key was there. Writing an equal value changes nothing |
| `Add(key, value)` | Puts one in and throws when the key is taken, as a dictionary does |
| `Remove(key)` | Takes one out; a key that is not there changes nothing |
| `Clear()`, `Reset(items)` | Empties it, or replaces the contents in one change |
| `Subscribe(listener)` | Same as an atom's |

It holds a dictionary but is not one — there is no `IDictionary` to write through, which is what keeps
every change on the frame's path. Keys are compared by the comparer given to the constructor, values
by `EqualityComparer<TValue>.Default`, which is what decides that a write changed nothing.

Reading one key is enough to depend on the whole map: `TryGetValue` inside a
[`Computed<T>`](#derived-values) subscribes to it, so a derived value follows an entry that is not
there yet.

## Sets

`AtomsSet<T>` holds what is in and what is out — the files marked, the rows expanded, the hosts that
answered:

```csharp
public HistoryAtomsSet<string> Marked { get; } = new("Panel.Marked", comparer: StringComparer.OrdinalIgnoreCase);

Marked.Add(path);                       // already there? nothing happens
Marked.Add(everythingBelow);            // one change for the lot

if (Marked.TryRemove(path))
{
    Say($"{path} is no longer marked");
}
```

| Member | Meaning |
|---|---|
| `Value` | A live, read-only `IReadOnlySet<T>` — `Contains`, `SetEquals`, `IsSubsetOf` and the rest |
| `Count`, `IsEmpty`, `Contains(item)` | Reading |
| `Add(item)`, `Add(items)` | Putting in. What is already there is not an error and not a change |
| `TryAdd(item)`, `TryRemove(item)` | The same, answering whether anything happened |
| `Remove(item)`, `Clear()`, `Reset(items)` | Taking out, or replacing the contents in one change |
| `Subscribe(listener)` | Same as an atom's |

It follows `HashSet<T>` rather than the map: adding what is already in is idempotent, not an
exception, which is why `Add` answers nothing and `TryAdd` is there when the answer matters. Items
are compared by the comparer given to the constructor.

A set has no order, so a walk hands them back in whatever order the set holds them — sort where the
order is what the reader sees.

## Queues and stacks

The same again for the two orders that matter: `AtomsQueue<T>` takes at the back and gives at the
front, `AtomsStack<T>` takes and gives at the top.

```csharp
public AtomsQueue<FileEntry> ToCopy { get; } = new();
public AtomsStack<string> Been { get; } = new();

ToCopy.Enqueue(entries);              // one notification for the lot
Been.Push(folder);

if (ToCopy.TryDequeue(out var next))
{
    Copy(next);
}
```

| Member | Queue | Stack |
|---|---|---|
| Putting in | `Enqueue(item)`, `Enqueue(items)` | `Push(item)`, `Push(items)` |
| Taking out | `Dequeue()`, `TryDequeue(out item)` | `Pop()`, `TryPop(out item)` |
| Looking | `Peek()`, `TryPeek(out item)` | `Peek()`, `TryPeek(out item)` |
| The rest | `Count`, `IsEmpty`, `Clear()`, `Reset(items)`, `Value`, `Subscribe` | the same |

`Dequeue`, `Pop` and `Peek` throw on an empty one, as the collections they are named after do; the
`Try` members answer instead. `Value` reads in the order the type means — front first for a queue,
top first for a stack, the way `Stack<T>` itself enumerates — so `Value[0]` is what `Peek` answers and
a view draws either by walking it.

:::note[Why not the concurrent ones]

`ConcurrentQueue<T>` and `ConcurrentStack<T>` make each operation atomic, which is not the problem a
frame has. A frame reads state many times; what it needs is for nothing to change while it draws, and
that is thread affinity rather than a thread-safe container — see [Threads](#threads). Background
work hands its item over with `FrameThread.Post`, and several changes that belong together go in one
`Post` so no frame falls between them. `ConcurrentDictionary<TKey, TValue>` maps to
[`AtomsMap`](#maps) — including where it stands in for a set — `ConcurrentBag<T>` to
[`AtomsList`](#lists), and a `HashSet<T>` behind a lock to [`AtomsSet`](#sets).

:::

## Undo and redo

Undo lives in `Arlecchino.Atoms.History`, a package of its own:

```
dotnet add package Arlecchino.Atoms.History
```

`UseHistory` is what turns it on, and nothing records a step until it is called:

```csharp
builder.Services.UseHistory(options =>
{
    options.Resume = true;                    // come up where the last run left off
    options.FileName = "history/session.undo";
    options.Capacity = 1000;                  // steps kept before the oldest fall off
});
```

It registers `AtomHistory`, the file behind it, and the service that readies both before the first
frame. Calling it twice throws: atoms announce their steps to the whole process, so a second history
would collect the same steps into a second stack. The constructor is internal to the package for the
same reason — there is one history, and it comes from the container.

The one history records every `HistoryAtom<T>` there is — there is no list of atoms to keep in sync.
Take it where you need `Undo()` / `Redo()`:

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

The history records from the moment it exists. `UseHistory` adds a hosted service that resolves it at
startup and clears it once the application is up, so edits made while wiring things together do not end
up as the first undo step. Rendering a frame headlessly — tests, `--frame` — has no hosted service, so
resolve `AtomHistory` yourself before making edits you intend to undo.

:::

### A history that outlives the run

`Resume` keeps the file between runs, and starts a named atom holding what the file last saw it hold
rather than the value its store gives it. It is asked as the atom gives its name rather than once at
startup, since a screen's store is built when the screen is — it picks its state up on the way back to
it.

The file is one JSON array of edits per line, written beside the program unless `FileName` has a folder
in it, so a run that is killed leaves everything up to the last complete line readable. Memory holds the
last `Capacity` steps; older ones are seeked to and rebuilt only when an undo asks for them. Without
`Resume` the file is a scratch one, thrown away on the way out and again on the way in.

What crosses a run is the key an atom was given, so an atom without one still undoes and redoes in
memory and is simply not written down. Claiming a key again with the same kind of atom takes the slot
over — a screen's store is rebuilt every time it is navigated back to — while claiming it with another
kind is refused, since a step written by one would not parse into the other.

How a value is written is said once for the kind, not once for the atom. `string`, `bool`, `int`,
`long`, `double`, `decimal`, `Guid`, `DateOnly`, `TimeOnly` and `DateTimeOffset` are known already; a
type of your own is taught by hand, because a trimmed program has no reflection to derive it with:

```csharp
file.Handles<Uri>(
    static (writer, value) => writer.WriteStringValue(value.ToString()),
    static element => new(element.GetString()!));
```

A group holding one atom the file cannot write is left out whole rather than half, which on being read
back would move some atoms and not others.

`HistoryAtom<T>.TryReset()` writes back what the atom held before the first edit the history took. It is
written rather than unwound, so the step it makes is itself the next one to be taken back, and no other
atom moves; an atom nothing was ever written to answers `false`.

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
write does happens then — subscribers are notified, a repaint is asked for, and a `HistoryAtom<T>`
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
