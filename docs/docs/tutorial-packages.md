---
title: 'Tutorial: a dependency review'
sidebar_label: 'Tutorial: build an app'
description: Build Arlecchino.Packages from an empty project — a real terminal application with a background scan, a sortable table behind tabs, a dependency tree and a form that runs dotnet add package.
---

# Tutorial: a dependency review

This builds a dependency review from an empty project. It is a real application rather than a widget
gallery: it runs `dotnet list package` four times, merges the reports, and shows what came out across
four screens.

:::note

The application built here shipped in the framework's repository as `samples/Arlecchino.Packages`
until 2.6.0, and the sample that ships now is
[Arlecchino.Commander](https://github.com/The1fEst/Arlecchino.Commander) instead. Every line below
still builds and runs — the project is simply one you create yourself.

:::

By the end you will have used almost everything the framework has — a
[store](stores.md) of [atoms](atoms.md), an [async atom](async-atoms.md) with a spinner and a progress
bar, a [table](table.md) behind [tabs](tabs.md), a [tree](tree.md), a [form](forms.md), several
[modals](modals.md), a [focus ring](focus.md), [view commands](commands.md#commands-of-a-view) and
[application commands](commands.md).

The finished code is in the repository, so read this beside it if you get lost.

:::tip[Before you start]

[Getting started](getting-started.md) is the ten-line version of steps 1 and 2. If you have not run
anything yet, do that first — it takes a minute and this page assumes it worked.

:::

## Step 1 — the project

```bash
dotnet new console -n Arlecchino.Packages
cd Arlecchino.Packages
dotnet add package Arlecchino
dotnet add package Microsoft.Extensions.Hosting
```

One property is worth setting now, because it decides where the generated `ViewKind` lands:

```xml
<PropertyGroup>
  <RootNamespace>Arlecchino.Packages</RootNamespace>
  <ArlecchinoViewNamespace>Arlecchino.Packages.Views</ArlecchinoViewNamespace>
</PropertyGroup>
```

Without it the generator writes into `$(RootNamespace).Navigation`. Putting `ViewKind` beside the views
means a view referring to another route needs no extra `using` — see
[Source generator](source-generator.md).

## Step 2 — the host

`Program.cs` is the whole of the wiring:

```csharp
using Arlecchino.Hosting;
using Arlecchino.Packages.Stores;
using Arlecchino.Packages.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddArlecchino(options =>
    {
        options.MinimumWidth = 84;
        options.MinimumHeight = 20;
        options.ShowOutputLine = false;
        options.ShowHints = false;
    })
    .AddGeneratedViews()
    .AddGeneratedStores()
    .AddGeneratedCommands()
    .UseMouse()
    .StartAt(ViewKind.Inventory);

await builder.Build().RunAsync();
```

Four decisions are already made here:

| Choice | Why |
|---|---|
| `ShowOutputLine = false` | This application wants the last row for its own [status bar](status-bar.md) |
| `ShowHints = false` | The screens are dense; `:h` turns the box back on in step 10 |
| `UseMouse()` | Tabs, table rows and the tree all answer clicks — see [Mouse](mouse.md) |
| `MinimumWidth = 84` | Below that the table is not worth drawing, so a [size notice](frame-loop.md#what-one-frame-is-made-of) replaces it |

`ViewKind.Inventory` does not compile yet. It will as soon as there is a class called `InventoryView`.

## Step 3 — the model

Nothing here is framework-specific; it is the shape the screens will draw. A package is an id, the
places it is used, what is wrong with it, and what version is available:

```csharp
public enum PackageHealth { Ok, Outdated, Drift, Deprecated, Vulnerable }

public sealed record PackageUse(
    string Id,
    string Project,
    string Framework,
    string Requested,
    string Resolved,
    bool Transitive);

public sealed record Advisory(string Severity, string Url);
```

The one piece worth designing carefully is **health**, because it drives the tabs, the colours and the
text in the `State` column all at once:

```csharp
public PackageHealth Health
{
    get
    {
        if (Advisories.Count > 0)
        {
            return PackageHealth.Vulnerable;
        }

        if (DeprecationReasons.Count > 0)
        {
            return PackageHealth.Deprecated;
        }

        if (ResolvedVersions().Count > 1)
        {
            return PackageHealth.Drift;
        }

        return IsOutdated ? PackageHealth.Outdated : PackageHealth.Ok;
    }
}
```

One enum, computed once, read by five things. That is the shape to look for whenever a screen is about
to grow a second `switch` over the same data.

`Catalog` is the result of one scan — the packages, the projects, and any report that failed:

```csharp
public sealed class Catalog
{
    public static readonly Catalog Empty = new()
    {
        Solution = "",
        Packages = [],
        Projects = [],
        Notes = [],
    };

    public required string Solution { get; init; }
    public required IReadOnlyList<PackageRow> Packages { get; init; }
    public required IReadOnlyList<ProjectSummary> Projects { get; init; }
    public required IReadOnlyList<string> Notes { get; init; }

    public DateTimeOffset ScannedAt { get; init; }
}
```

`Empty` earns its keep in step 6: the first frame is drawn before any scan has finished, and a screen
that draws an empty catalogue needs no null checks.

## Step 4 — running dotnet

```csharp
public static async Task<DotnetResult> RunAsync(
    IReadOnlyList<string> arguments,
    string workingDirectory,
    CancellationToken token)
{
    var info = new ProcessStartInfo("dotnet")
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        WorkingDirectory = workingDirectory,
    };

    foreach (var argument in arguments)
    {
        info.ArgumentList.Add(argument);
    }

    using var process = Process.Start(info)
        ?? throw new InvalidOperationException("dotnet could not be started");

    var output = process.StandardOutput.ReadToEndAsync(token);
    var error = process.StandardError.ReadToEndAsync(token);

    await process.WaitForExitAsync(token).ConfigureAwait(false);

    return new(
        process.ExitCode,
        await output.ConfigureAwait(false),
        await error.ConfigureAwait(false));
}
```

Two details matter for a terminal application:

- **`CreateNoWindow` and redirected streams.** A child process that writes to the console writes into
  the middle of your frame. Everything it says has to be captured.
- **The token.** It comes from the screen that started the scan, so navigating away stops it — step 5.

The scanner runs four passes and reports which one it is on:

```csharp
public sealed record ScanStep(int Done, int Total, string Title);

public static async Task<Catalog> ScanAsync(
    string solution,
    IProgress<ScanStep> progress,
    CancellationToken token)
```

`IProgress<T>` rather than an event, because the caller is going to have to get the value back onto the
drawing thread and that is easier to do in one place.

## Step 5 — the store

This is the first framework-shaped piece. Everything the four screens share lives in one
[store](stores.md):

```csharp
public sealed class Inventory : IArlecchinoStore
{
    private readonly AsyncAtom<Catalog> _catalog = new(Catalog.Empty);

    public AsyncAtom<Catalog> Scan => _catalog;

    public Atom<string> Solution { get; } = new LocalAtom<string>("");
    public Atom<string> Filter { get; } = new LocalAtom<string>("");
    public Atom<int> Tab { get; } = new LocalAtom<int>(0);
    public Atom<bool> ShowTransitive { get; } = new LocalAtom<bool>(false);
    public Atom<PackageRow?> Selected { get; } = new LocalAtom<PackageRow?>(null);
    public Atom<ScanStep> Step { get; } = new LocalAtom<ScanStep>(new(0, 4, ""));
}
```

Marking it `IArlecchinoStore` is the whole registration: `.AddGeneratedStores()` finds it and puts it in
the container as a singleton. Every atom here is `LocalAtom` — none of it is something the user
authored, so none of it belongs on the [undo stack](atoms.md#undo-and-redo).

Starting a scan is one line:

```csharp
public void Rescan()
{
    if (Solution.Value.Length == 0)
    {
        return;
    }

    var progress = new StepProgress(Step);
    _catalog.Load(token => PackageScanner.ScanAsync(Solution.Value, progress, token));
}
```

[`AsyncAtom.Load`](async-atoms.md) cancels whatever is in flight, runs the work off the drawing thread,
and lands the result back on it. Pressing `r` twice cannot leave two scans racing.

The progress reports come back from a thread that is not allowed to touch an atom, so they take the
door:

```csharp
private sealed class StepProgress : IProgress<ScanStep>
{
    private readonly Atom<ScanStep> _step;

    public StepProgress(Atom<ScanStep> step) => _step = step;

    public void Report(ScanStep value) => _step.Post(value);
}
```

Writing `_step.Value` here instead would throw, by design — the scan runs on another thread. See
[The frame loop](frame-loop.md#which-thread-draws).

Finally, the filtering the tabs and the filter modal both need, in the store rather than in a view:

```csharp
public IReadOnlyList<PackageRow> Visible()
{
    var rows = new List<PackageRow>();
    var lens = (Lens)Tab.Value;

    foreach (var package in _catalog.Value?.Packages ?? [])
    {
        if (!package.Matches(Filter.Value))
        {
            continue;
        }

        if (package.IsTransitive && !ShowTransitive.Value)
        {
            continue;
        }

        if (Keeps(lens, package))
        {
            rows.Add(package);
        }
    }

    return rows;
}
```

## Step 6 — the first screen

Now the part that draws. A view is a plain class; the constructor takes what it needs from the
container:

```csharp
public sealed class InventoryView : IArlecchinoView
{
    private const int HeaderRows = 2;

    private readonly Surface _surface;
    private readonly Inventory _inventory;
    private readonly ArlecchinoState _state;
    private readonly FocusRing _focus;
    private readonly Tabs _tabs;
    private readonly Table<PackageRow> _table;
    private readonly StatusBar _status;
    private readonly ProgressBar _progress;
    private readonly Spinner _spinner = new();

    public InventoryView(
        Surface surface,
        Inventory inventory,
        ArlecchinoState state,
        ArlecchinoOptions options,
        ViewLifetime lifetime)
    {
        // …
    }
}
```

Naming the class `InventoryView` is what produces `ViewKind.Inventory` — the generator strips the
suffix. `Program.cs` compiles now.

### The table

```csharp
_table = new Table<PackageRow>(options.Keymap)
{
    Columns =
    [
        new()
        {
            Header = static () => "Package",
            Cell = static row => row.IsTransitive ? $"{row.Id} (transitive)" : row.Id,
            Sort = static (first, second) => string.CompareOrdinal(first.Id, second.Id),
        },
        new()
        {
            Header = static () => "Resolved",
            Cell = static row => row.Resolved(),
            Width = 24,
            Sort = static (first, second) =>
                VersionOrder.Compare(first.Highest(), second.Highest()),
        },
        new()
        {
            Header = static () => "Latest", Cell = static row => row.Latest ?? "—", Width = 10,
        },
        new()
        {
            Header = static () => "Used by",
            Cell = static row => row.Uses.Count.ToString(),
            Width = 8,
            AlignRight = true,
            Sort = static (first, second) => first.Uses.Count.CompareTo(second.Uses.Count),
        },
        new()
        {
            Header = static () => "State",
            Cell = Describe,
            Width = 26,
            Sort = static (first, second) => second.Health.CompareTo(first.Health),
        },
    ],
    ItemStyle = Paint,
    OnActivate = Open,
};
```

The first column has no `Width`, so it takes what the fixed ones leave. `ItemStyle` is where health
becomes colour:

```csharp
private static IArlecchinoColor Paint(PackageRow row) => row.Health switch
{
    PackageHealth.Vulnerable => Theme.Error,
    PackageHealth.Deprecated => Theme.Warning,
    PackageHealth.Drift => Theme.Accent,
    PackageHealth.Outdated => Theme.Default,
    _ => Theme.Muted,
};
```

Roles, not colours — so [swapping the palette](theming.md) restyles the whole screen.

### The tabs

```csharp
_tabs = new Tabs(options.Keymap)
{
    Titles =
    [
        () => $"All {Total()}",
        () => $"Outdated {Count(PackageHealth.Outdated)}",
        () => $"Vulnerable {Count(PackageHealth.Vulnerable)}",
        () => $"Deprecated {Count(PackageHealth.Deprecated)}",
        () => $"Drift {Count(PackageHealth.Drift)}",
    ],
    OnSelected = index =>
    {
        _inventory.Tab.Value = index;
        _table.Selected = 0;
    },
};
```

The titles are delegates, so the counts follow the scan without anyone refreshing them. `OnSelected`
writes the tab into the **store**, not into a field — which is why leaving the screen and coming back
lands on the same tab.

### The focus ring

```csharp
_focus = new FocusRing(options.Keymap);
_focus.Add(_tabs);
_focus.Add(_table);
_focus.Focus(_table);
```

Two lines of routing, and `Tab` now moves between the strip and the rows:

```csharp
public ViewRoute Handle(ConsoleKeyInfo key) => _focus.Handle(key);
public ViewRoute HandleMouse(MouseEvent mouse) => _focus.HandleMouse(mouse);
```

### Redrawing while the scan runs

A scan finishing does not touch an atom the view reads in `Draw`, so nothing would ask for a frame:

```csharp
lifetime.Track(_inventory.Scan.SubscribeToStatus(() => _state.Invalidate()));
lifetime.Track(_inventory.Step.Subscribe(() => _state.Invalidate()));
```

[`ViewLifetime.Track`](async-atoms.md#tying-work-to-the-screen) disposes both when the screen goes away,
so the view needs no `IDisposable`.

And the screen kicks off the first scan itself, once:

```csharp
if (_inventory.Scan.Value is { Packages.Count: 0 } && !_inventory.Scan.IsLoading)
{
    _inventory.Rescan();
}
```

### Draw

```csharp
public void Draw()
{
    var content = _surface.Content;
    var (header, rest) = content.SplitTop(HeaderRows);

    header.WriteLine(0, Title(), Theme.Header);
    header.WriteLine(1, Headline(), Failed() ? Theme.Error : Theme.Muted);

    if (_inventory.Scan.IsLoading)
    {
        _spinner.Advance();
        _spinner.Draw(header.SplitLeft(header.Width - 1).Right);
    }

    var body = _tabs.Draw(rest);
    var (room, status) = body.SplitTop(body.Height - 1);

    _table.Rows = _inventory.Visible();

    if (_inventory.Scan.IsLoading)
    {
        var (rows, bar) = room.SplitTop(room.Height - 2);

        _table.Draw(rows);
        _progress.Value = _inventory.Step.Value.Done;
        _progress.Draw(bar.Rows(1, 1));
    }
    else
    {
        _table.Draw(room);
    }

    _status.Draw(status);
}
```

Read that from the top: split a header off, let the tabs take their row and hand back the rest, keep a
row for the status bar, give what is left to the table — and squeeze a progress bar in only while a
scan is running. Nothing counts rows, and nothing knows the terminal size.
[`Draw` returning a region](widgets.md#draw-returns-what-is-left) is what makes that possible.

## Step 7 — keys the screen owns

None of this goes in `Handle`:

```csharp
public IReadOnlyList<ViewCommand> Commands() =>
[
    ViewCommand.Navigating(
        ConsoleKey.Enter,
        static () => "details",
        () => Open(_table.SelectedRow)),
    ViewCommand.For(ConsoleKey.R, static () => "rescan", _inventory.Rescan),
    ViewCommand.For(ConsoleKey.F, static () => "filter", Filter),
    ViewCommand.For(ConsoleKey.T, static () => "transitive", Transitives),
    ViewCommand.Navigating(ConsoleKey.P, static () => "projects", static () => ViewKind.Projects),
    new()
    {
        Binding = new(ConsoleKey.U),
        Label = static () => "upgrade",
        IsEnabled = () => _table.SelectedRow is not null,
        Run = Upgrade,
    },
];
```

Declaring them as data buys three things at once: they appear in the
[command palette](commands.md#the-command-palette), they fill the hints box, and they show up on the
`F1` [keys screen](keyboard.md#the-keys-screen) under this route. The last one is disabled while
nothing is selected, and a disabled command swallows its key rather than letting `u` fall through.

The filter is a [modal](modals.md#text) and one callback:

```csharp
private void Filter() =>
    _state.RequestText("Filter packages", _inventory.Filter.Value, null, typed =>
    {
        _inventory.Filter.Value = typed;
        _table.Selected = 0;
    });
```

Writing the atom is enough — `Visible()` reads it on the next frame, and the atom asked for that frame
itself.

## Step 8 — the second screen: a tree

`ProjectsView` puts a [tree](tree.md) beside a [table](table.md), both in one ring:

```csharp
private readonly Tree<Branch> _tree;
private readonly Table<ProjectRow> _projects;
private readonly FocusRing _focus;

_tree = new Tree<Branch>(options.Keymap)
{
    Render = static branch => branch.Label,
    ItemStyle = Paint,
    OnActivate = Open,
    Roots = Build(inventory.Scan.Value ?? Catalog.Empty),
};

_focus = new FocusRing(options.Keymap);
_focus.Add(_tree);
_focus.Add(_projects);
```

and the layout is two splits:

```csharp
var (panes, status) = rest.SplitTop(rest.Height - 1);
var (tree, side) = panes.SplitLeft(panes.Width - SidebarWidth);

_tree.Draw(tree.Border(Theme.Info, "Dependency tree"));
_projects.Draw(side.Border(Theme.Info, "Per project"));
_status.Draw(status);
```

`Border` draws the box and hands back the space inside it, so a bordered pane is one call.

Two more view commands are worth having on a tree:

```csharp
ViewCommand.For(ConsoleKey.E, static () => "expand all", () => _tree.ExpandAll()),
ViewCommand.For(ConsoleKey.C, static () => "collapse all", () => _tree.CollapseAll()),
```

## Step 9 — the form that does something

The upgrade screen is where the application stops reporting and starts acting. Its state is a second
store — and this one **is** tracked, because the user authors it:

```csharp
public sealed class UpgradePlan : IArlecchinoStore
{
    public Atom<string> Target { get; } = new TrackedAtom<string>("");
    public Atom<IReadOnlyList<string>> Projects { get; } =
        new TrackedAtom<IReadOnlyList<string>>([]);
    public Atom<bool> DryRun { get; } = new TrackedAtom<bool>(true);

    public Atom<bool> Running { get; } = new LocalAtom<bool>(false);
    public Atom<int> Written { get; } = new LocalAtom<int>(0);
}
```

`Running` and `Written` are local: nobody wants `Undo` to take back "a command was run".

The [form](forms.md) is four fields over those atoms:

```csharp
private readonly Form _form;
private readonly UpgradePlan plan;

_form = new Form(state, options)
{
    Fields =
    [
        Field.Choice(static () => "Version", Versions(), plan.Target,
            static () => "what every selected project will ask for"),
        Field.MultiChoice(static () => "Projects", Names(), plan.Projects,
            static picked => picked.Count == 0 ? "none" : string.Join(", ", picked),
            static () => "Space marks a project, Enter confirms"),
        Field.Toggle(static () => "Dry run", plan.DryRun, static value => value ? "yes" : "no",
            static () => "yes prints the commands, no runs dotnet add package"),
        Field.Action(static () => "Apply", Apply,
            () => !plan.Running.Value && plan.Projects.Value.Count > 0),
    ],
};
```

Each field opens the modal that matches its type, and `Field.Action` is disabled until there is
something to do.

`Apply` asks before it writes:

```csharp
private readonly Inventory _inventory;
private readonly UpgradePlan _plan;
private readonly ViewLifetime _lifetime;
private readonly ArlecchinoState _state;

private ViewRoute Apply()
{
    if (_inventory.Selected.Value is not { } package || _inventory.Scan.Value is not { } catalog)
    {
        return ViewRoute.None;
    }

    if (_plan.DryRun.Value)
    {
        _plan.Run(package, catalog, _lifetime.Closing);
        return ViewRoute.None;
    }

    _state.RequestConfirmation(
        $"Rewrite {_plan.Projects.Value.Count} project file(s)?",
        () => _plan.Run(package, catalog, _lifetime.Closing));

    return ViewRoute.None;
}
```

[`RequestConfirmation`](modals.md#message-and-confirmation) starts on **No**, so a stray `Enter`
cancels rather than rewrites project files. `_lifetime.Closing` is the token: leave the screen and the
run stops.

Running is a background task that posts every line back:

```csharp
Task.Run(async () =>
{
    foreach (var step in steps)
    {
        Write(Dotnet.Describe(step));

        if (dry)
        {
            continue;
        }

        var result = await Dotnet.RunAsync(step, root, token).ConfigureAwait(false);
        Write(result.Failed ? Tail(result.Error) : "  updated");
    }

    FrameThread.Post(() => Running.Value = false);
}, token);

private void Write(string line) => FrameThread.Post(() =>
{
    _log.Add(line);
    Written.Value = _log.Count;
});
```

`Written` is an atom nobody displays. Its whole job is to ask for a frame when a line lands.

The output goes in a [`ScrollPane`](scrolling.md), which shows the planned commands until there is
real output to show instead:

```csharp
private readonly ScrollPane _log;

_log = new ScrollPane(options.Keymap)
{
    ContentHeight = () => Lines().Count,
    Content = region =>
    {
        var lines = Lines();
        for (var row = 0; row < lines.Count; row++)
        {
            region.WriteLine(row, lines[row], Style(lines[row]));
        }
    },
};
```

## Step 10 — application commands

Three keys belong to the application rather than to any screen:

```csharp
public sealed class HintsCommand : IArlecchinoCommand
{
    private readonly ArlecchinoOptions _options;

    public HintsCommand(ArlecchinoOptions options) => _options = options;

    public KeyBinding Binding => new(ConsoleKey.H);

    public string Icon => "?";

    public string Label => "Hints";

    public ViewRoute Execute()
    {
        _options.ShowHints = !_options.ShowHints;
        return ViewRoute.None;
    }
}
```

`.AddGeneratedCommands()` already registers it — there is nothing to add to `Program.cs`. The binding
is a plain `h`, so it does **not** fire globally; it is reachable through the palette as `:h`, which is
exactly right for something you press once a session. `RescanCommand` uses `Ctrl+R` instead, and that
one does fire anywhere.

## Step 11 — looking at it without running it

Add a `--frame` branch before the host is built and every screen becomes one composed frame on stdout:

```csharp
if (args is ["--frame", ..])
{
    HeadlessFrame.Render(
        args.Length >= 2 ? args[1] : "inventory",
        args.Length >= 3 ? args[2] : "120x34",
        Option(args, "--keys") ?? "",
        solution);

    return;
}
```

```bash
dotnet run -- --frame inventory 120x30
dotnet run -- --frame upgrade 120x24
```

The wiring behind `HeadlessFrame` is `WithoutHostedService()`, `Surface.SetFixedSize` and
`Screen.DrawOnce()` — see
[Hosting and options](hosting-and-options.md#running-without-the-hosted-service). It is the fastest way
to check a layout, and it is what produced the screenshots on this page.

The same three calls, with assertions instead of stdout, are [Testing](testing.md):

```csharp
using var app = new ArlecchinoTestHost(120, 30,
    builder => builder.AddGeneratedViews().AddGeneratedStores().StartAt(ViewKind.Inventory));

app.Press(ConsoleKey.RightArrow);
Assert.Contains("Vulnerable", app.FrameLineContaining("All"));
```

## What to read next

| If you want to | Read |
|---|---|
| Understand why the `Post` calls are mandatory | [The frame loop](frame-loop.md) |
| Lay a screen out differently | [Layout](layout.md) |
| Write a widget the framework does not have | [Widgets](widgets.md#writing-your-own) |
| Translate every string in it | [Localization](localization.md) |
| Find a member you saw here | [API reference](api/index.md) |
