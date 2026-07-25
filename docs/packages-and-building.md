[Home](README.md) · [Getting started](getting-started.md) · [Views and navigation](views-and-navigation.md) · [Source generator](source-generator.md) · [Rendering](rendering.md) · [Theming](theming.md) · [Commands and input](commands-and-input.md) · [Modals and state](modals-and-state.md) · [File picker](file-picker.md) · [Hosting and options](hosting-and-options.md) · [State and forms](state-and-forms.md) · [Widgets](widgets.md) · [Localization](localization.md)

# Packages and building

## What ships

| Package | Target | Contents |
|---|---|---|
| `Arlecchino.Core` | `net10.0` | `Surface`, `Theme`, `TermColor`, `KeyText`, `ITerminal` — the renderer, no DI, no hosting |
| `Arlecchino` | `net10.0` | Views, navigation, modals, commands, the file picker, hosting and DI; depends on `Arlecchino.Core` |
| `Arlecchino.Testing` | `net10.0` | Headless host for testing an application built on Arlecchino |

`Arlecchino` also carries the generator as `analyzers/dotnet/cs` and a `build/Arlecchino.props` that makes
`RootNamespace`, `ArlecchinoViewNamespace` and `ArlecchinoGenerateViews` visible to it. Referencing
`Arlecchino` is enough to get everything — see [Source generator](source-generator.md).

`Arlecchino.Generators` itself targets `netstandard2.0` (Roslyn's requirement for analyzers) and is never
published on its own.

Both libraries are marked `IsAotCompatible`, and the whole repository builds with
`TreatWarningsAsErrors`. Generic parameters that reach `ActivatorUtilities` or
`AddSingleton<TService, TImpl>` carry `[DynamicallyAccessedMembers]` so trimming keeps their
constructors.

## Building

```
pack.cmd
```

Builds all three packages in `Release` and drops the `.nupkg` files into `artifacts/packages`, which is
the local feed a consuming application points its `nuget.config` at:

```xml
<packageSources>
  <add key="arlecchino-local" value="../path/to/Arlecchino/artifacts/packages" />
</packageSources>
```

The version is `0.1.0` for the whole repository. Because it does not change between builds, NuGet may
serve a cached copy after a repack — clear `~/.nuget/packages/arlecchino*` if a consumer seems to be
building against stale code.

For a plain compile of everything, including the sample:

```
dotnet build Arlecchino.slnx
```

## Tests

```
dotnet test tests/Arlecchino.Tests
```

The suite runs on `Arlecchino.Testing` — the same package applications use, so it is exercised by every
run.

## Testing an application

```
dotnet add package Arlecchino.Testing
```

```csharp
using var app = new ArlecchinoTestHost(80, 24,
    builder => builder.AddGeneratedViews().StartAt(ViewKind.Mods));

app.Press(ConsoleKey.DownArrow);
app.Type("wide");
app.Press(ConsoleKey.Enter);

Assert.Contains("Widebody kit", app.Frame());
```

`ArlecchinoTestHost` builds the container exactly as `AddArlecchino` would, minus the hosted service, and
draws into a `FakeTerminal` — a fake `ITerminal` with a fixed size, a queue of keys and a buffer of
everything written. Nothing touches a real console, so the tests run anywhere.

| Member | Use |
|---|---|
| `Press(key, shift, alt, control)` | Sends a key through the input router |
| `Type(text)` | Types characters one by one |
| `Click(row, column)` / `Scroll(row, column, down)` | Mouse events in frame coordinates |
| `ReadFromTerminal(sequence)` | Feeds a raw escape sequence, as a real terminal would |
| `Frame()` / `FrameLines()` / `FrameLineContaining(text)` | The composed frame as plain text |
| `Styles()` | The ANSI style sequences of the frame, for asserting on colour |
| `State`, `Navigator`, `Surface`, `Options`, `History`, `Dispatcher`, `Services` | The wired services |

`FrameText` is the helper behind those: `WithoutStyles`, `Lines`, `StylesIn`, `CursorJumpsIn` and
`BoxWidth` for checking that a box is rectangular.

In this repository `ProbeView` / `OtherView` also keep the source generator under test: the routes
they produce are used by the navigation tests.

## What ends up in the package

`Arlecchino.0.1.0.nupkg` carries `lib/net10.0/Arlecchino.dll`, the generator under `analyzers/dotnet/cs`,
`build/Arlecchino.props` and the README shown on the package page. Symbols ship separately as `.snupkg`,
builds are deterministic, and SourceLink is on — `ContinuousIntegrationBuild` switches itself on when
the build runs in GitHub Actions.

## Code style is part of the build

`.editorconfig` raises four Roslyn style rules to warnings, and `EnforceCodeStyleInBuild` plus
`TreatWarningsAsErrors` turns them into build errors:

| Rule | Catches |
|---|---|
| `IDE0005` | Unused `using` directives |
| `IDE0090` | `Foo x = new Foo()` where `new()` says the same |
| `IDE0011` | A branch body without braces |
| `IDE0161` | A block-scoped `namespace` |
| `CA1822` | A member that touches no instance state and should be `static` |

Two rules are deliberately off: `IDE0290` (primary constructors) and `IDE1006` (naming) — the code is
written that way on purpose. `CA1822` is off for the benchmarks, where the benchmark runner wants
instance methods.

`IDE0005` only runs during a build when the project produces an XML documentation file — a
[long-standing quirk](https://github.com/dotnet/roslyn/issues/41640). The packable projects generate
one anyway; the tests, samples and benchmarks turn it on and silence `CS1591` in the same breath,
since nothing there is public API that needs documenting.

The `resharper_*` half of `.editorconfig` says the same thing to IDEs that read those keys, and covers
what the compiler has no rule for: target-typed `new` in an *argument* (`Apply(new ViewRoute("x"))` —
`IDE0090` only fires where the type is written on the left), braces on every statement kind, and
turning a nested `if` inside out.

Nothing here formats code. It only refuses what is redundant.

## Benchmarks

`benchmarks/Arlecchino.Benchmarks` measures the two things a terminal UI can plausibly be slow at:
composing a frame and measuring text.

```bash
dotnet run --project benchmarks/Arlecchino.Benchmarks -c Release -- --filter "*" --job short
```

On a 120×40 frame with every row written (Ryzen laptop, .NET 10, short job):

| What | Mean | Allocated |
|---|---|---|
| Full frame, every cell changed | 111 µs | 89 KB |
| Repeat frame, nothing changed | 111 µs | 0 B |
| Frame with one cell changed | 109 µs | 96 B |
| List of 2000 rows scrolled by one | 325 µs | 20 KB |
| `TextWidth.Of` on a latin line | 0.9 µs | 0 B |
| `TextWidth.Of` on wide and combining text | 0.7 µs | 0 B |

The useful reading is the second row: a frame where nothing changed costs the same as a full one and
allocates nothing, because the cost is in filling the grid, not in talking to the terminal — the diff
means an unchanged frame writes nothing at all. At 60 frames a second that is under one percent of
the budget, and frames are only built when something asks for one, so an idle application does none
of this.

## Versioning

The three packages ship together and always carry the same version — mixing versions between them is
not supported, and there is nothing to gain from it since they are built from one commit.

Versions follow SemVer with the usual `0.x` caveat: while the major is zero, a breaking change bumps
the **minor**, and the patch is reserved for fixes that keep the API as it is. Breaking changes are
expected at this stage and are not softened with obsolete shims or duplicate overloads — the
[changelog](../CHANGELOG.md) says what moved, and the old shape is removed in the same release.

`Directory.Build.props` holds the version for local builds. A release takes it from the tag instead
(`v0.2.0` → `0.2.0`), so publishing is a matter of tagging: nothing has to be edited in the
repository first, and a tag that does not match the props file is fine and deliberate.

Every change worth a line goes into `CHANGELOG.md` under `Unreleased`, which becomes the release
section when the tag is pushed.

### The public API is written down

Each packable project carries `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`, checked by
`Microsoft.CodeAnalysis.PublicApiAnalyzers`. Adding, removing or changing anything public fails the
build until the change is recorded in `PublicAPI.Unshipped.txt` — which means an accidental break
shows up as a red build rather than as a bug report after release, and the diff of a pull request
says plainly what the API surface did.

Recording the change is mechanical:

```bash
dotnet format analyzers src/Arlecchino/Arlecchino.csproj --diagnostics RS0016 --severity warn
```

That writes the new entries. Deliberate removals are recorded by hand — delete the line, or move it
under `*REMOVED*` when it was already shipped. At release time the contents of `Unshipped` move into
`Shipped` and `Unshipped` is emptied again; until `0.1.0` actually ships, everything lives in
`Unshipped`.

## Continuous integration

`.github/workflows/build.yml` runs on every push to `master`/`main` and on pull requests: restore,
build in `Release` with warnings as errors, the test suite, and a pack — on Windows and Linux, because
console behaviour differs between them. The Windows leg uploads the packages as a build artifact.

`.github/workflows/release.yml` publishes: push a `v0.2.0` tag and it builds, tests and pushes all
three packages to NuGet with the version taken from the tag.

There is no API key anywhere. The workflow asks GitHub for an OIDC token — which is what
`permissions: id-token: write` grants — and `NuGet/login` exchanges that token for a key that lives
only for the length of the job. Nothing long-lived is stored in the repository, and a leaked log
cannot be replayed later.

The other half of that handshake lives on nuget.org, under **Trusted Publishing**: a policy naming
the package owner (`fEst`), this repository (`The1fEst/Arlecchino`) and the workflow file allowed to
publish (`release.yml`). Only a run of that file, in that repository, can get a key. Change the
workflow's name or move the job to another file and publishing stops until the policy is updated —
that is the point of it.

## Repository layout

| Path | Contents |
|---|---|
| `src/Arlecchino.Core` | Renderer and input primitives |
| `src/Arlecchino` | Framework, hosting, built-in views |
| `src/Arlecchino.Generators` | The incremental generator |
| `src/Arlecchino.Testing` | Headless test host published as a package |
| `samples/Arlecchino.Sample` | Gallery of every modal and widget, also the headless `--frame` renderer |
| `samples/Arlecchino.Processes` | A real application: the process list, live-loaded and sortable |
| `benchmarks/Arlecchino.Benchmarks` | Frame composition and text measurement |
| `tests/Arlecchino.Tests` | Test suite: rendering, navigation, every modal, colour conversion |
| `docs` | This documentation |
| `artifacts/packages` | Local package feed produced by `pack.cmd` |

## Conventions

- No comments in the source; names carry the meaning, and documentation lives here in `docs`.
- No user-visible string at a call site — every one of them is a delegate on
  [`ArlecchinoStrings`](localization.md).
- No application domain types in the framework; extension points are interfaces (`IView`,
  `IViewFactory`, `IArlecchinoCommand`, `IArlecchinoStartup`, `ITerminal`).
