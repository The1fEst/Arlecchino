---
title: Packages and building
sidebar_label: Packages and building
description: What ships in which package, the local feed, versioning, CI and benchmarks.
---

# Packages and building

## What ships

| Package | Target | Contents |
|---|---|---|
| `Arlecchino.Core` | `net8.0`, `net10.0` | `Surface`, `Theme`, `TermColor`, `KeyText`, `IArlecchinoTerminal` — the renderer, no DI, no hosting |
| `Arlecchino` | `net8.0`, `net10.0` | Views, navigation, modals, commands, the file picker, hosting and DI; depends on `Arlecchino.Core` |
| `Arlecchino.Pictures` | `net8.0`, `net10.0` | `PictureFormats` — PNG, JPEG, BMP, Netpbm, QOI and Targa read into pixels; depends on `Arlecchino.Core` and on nothing native |
| `Arlecchino.Testing` | `net8.0`, `net10.0` | Headless host for testing an application built on Arlecchino |

`Arlecchino` also carries the generators as `analyzers/dotnet/cs` and a `build/Arlecchino.props` that
makes `RootNamespace`, `ArlecchinoViewNamespace`, `ArlecchinoGenerateViews`,
`ArlecchinoGenerateStores`, `ArlecchinoGenerateCommands` and `ArlecchinoGenerateWidgets` visible to
them. Referencing `Arlecchino` is enough to get everything — see
[Source generator](source-generator.md).

`Arlecchino.Generators` itself targets `netstandard2.0` (Roslyn's requirement for analyzers) and is never
published on its own.

Its reference to `Microsoft.CodeAnalysis.CSharp` is pinned low on purpose, and Dependabot is told to
leave it alone. A generator runs inside the compiler the *application* is built with, not the one it
was built with: raise that reference and the generator stops loading for anyone on an older SDK, so
`AddGeneratedViews` simply is not there and the error they see is `cannot resolve symbol`. The version
is the oldest Roslyn the package supports, and moving it is a deliberate change with a floor to raise
in the documentation, not a dependency bump.

The libraries are marked `IsAotCompatible`, and the whole repository builds with
`TreatWarningsAsErrors`. Generic parameters that reach `ActivatorUtilities` or
`AddSingleton<TService, TImpl>` carry `[DynamicallyAccessedMembers]` so trimming keeps their
constructors.

That mark only turns on the analyzer, so CI publishes the sample natively and runs it — an
application whose registrations the trimmer removed compiles and warns about nothing, and then draws
nothing. Locally the same probe is one switch:

```bash
dotnet publish samples/Arlecchino.Sample -c Release -p:AotProbe=true -o native
./native/Arlecchino.Sample --frame default 100x20
```

The switch is on the sample rather than passed as a plain `-p:PublishAot=true`, which would reach the
generator too — and a `netstandard2.0` analyzer cannot be compiled ahead of time. The binary comes out
around 5 MB with no runtime to install.

## Building

```
dotnet run --project tools/Arlecchino.Tools -- pack
```

Builds all four packages in `Release` and drops the `.nupkg` files into `artifacts/packages`, which is
the local feed a consuming application points its `nuget.config` at:

```xml
<packageSources>
  <add key="arlecchino-local" value="../path/to/Arlecchino/artifacts/packages" />
</packageSources>
```

`Directory.Build.props` holds one version for the whole repository. Because it does not change between
builds, NuGet may serve a cached copy after a repack — clear `~/.nuget/packages/arlecchino*` if a
consumer seems to be building against stale code.

For a plain compile of everything, including the sample:

```
dotnet build Arlecchino.slnx
```

## Tests

```
dotnet test tests/Arlecchino.Tests
```

The suite runs on `Arlecchino.Testing` — the same package applications use, so it is exercised by
every run — and it runs twice, once per target framework. [Testing](testing.md) is what to read for
testing an application of your own.

## What ends up in the package

`Arlecchino.2026.8.1.nupkg` carries `lib/net8.0/Arlecchino.dll` and `lib/net10.0/Arlecchino.dll`, the
generator under `analyzers/dotnet/cs`, `build/Arlecchino.props` and the README shown on the package
page. The two libraries are the same source: `net8.0` is there because that is the long-term support
release most applications sit on, and the code avoids anything newer — that is why `LogBuffer` locks
on a plain object rather than `System.Threading.Lock`. Symbols ship separately as `.snupkg`,
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

`benchmarks/Arlecchino.Benchmarks` measures what a terminal UI can plausibly be slow at: composing a
frame, measuring text, answering a key, and writing state.

```bash
dotnet run --project benchmarks/Arlecchino.Benchmarks -c Release -- --filter "*" --job short
```

On a 120×40 frame with every row written (Ryzen 7 9800X3D, .NET 10, short job):

| What | Mean | Allocated |
|---|---|---|
| Full frame, every cell changed | 124 µs | 89 KB |
| Repeat frame, nothing changed | 122 µs | 0 B |
| Frame with one cell changed | 121 µs | 96 B |
| List of 2000 rows scrolled by one | 381 µs | 20 KB |
| A key through the router | 17 ns | 0 B |
| A click through the router | 11 ns | 0 B |
| A pasted block arriving as an escape sequence | 395 ns | 304 B |
| Write an atom nothing listens to | 1.5 ns | 0 B |
| Write an atom 20 things listen to | 8.3 ns | 0 B |
| Write an atom that records history | 17 ns | 144 B |
| Read a computed value that did not change | 0.5 ns | 0 B |
| Read a computed value after a dependency changed | 47 ns | 304 B |
| `TextWidth.Of` on a latin line | 0.9 µs | 0 B |
| `TextWidth.Wrap` on two paragraphs | 12.9 µs | 2.9 KB |

The useful reading is the second row: a frame where nothing changed costs the same as a full one and
allocates nothing, because the cost is in filling the grid, not in talking to the terminal — the diff
means an unchanged frame writes nothing at all. At 60 frames a second that is under one percent of
the budget, and frames are only built when something asks for one, so an idle application does none
of this. Input is far below anything a person can notice: the router costs tens of nanoseconds, so
what a key costs is whatever the view does with it.

Reading and writing atoms allocates nothing, which is the point: frames read atoms constantly, and a
read that allocated would put the garbage collector on the critical path of drawing. What is left is
paid for on purpose — 144 bytes when an edit enters the undo history, because the step has to be kept
somewhere, and 304 when a `Computed` re-runs and subscribes to whatever it read this time.

### Running them on CI

Numbers from a shared runner say nothing, so nothing is recorded there. Every push executes each
benchmark once as a dry job, which fails on a benchmark that stopped compiling or started throwing;
`.github/workflows/benchmarks.yml` runs them properly on demand — start it from the Actions tab, with
a filter if only some are wanted, and it writes the tables into the run summary and keeps them as an
artifact.

## Versioning

The four packages ship together and always carry the same version — mixing versions between them is
not supported, and there is nothing to gain from it since they are built from one commit.

A version is the year, the month and which release of that month it is: `2026.8.1` is the first
release of August 2026, `2026.8.2` the next one, and a month with nothing to release skips its number
entirely. What the number tells you is how old the code in your `packages` folder is, which is the
question a version is usually read for.

What it does not tell you is whether an upgrade is free. No digit is reserved for a break, so the
[changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md) is what to read before
moving — it says what moved and what the edit is. How a break is delivered has not changed: no
obsolete shims, no duplicate overloads left behind, the old shape removed in the same release that
brings the new one. Breaks stay rare, and the surface recorded in `PublicAPI.Shipped.txt` is what
makes one impossible to ship by accident.

Everything up to and including `5.0.0` was numbered under SemVer, where a break took a new major.
Those numbers stay as they are and still sort below the calendar ones, so an application pinned to
`5.0.0` is not disturbed by any of this. `2026.8.1` is the first calendar version, and the release
that would have been `6.0.0`.

`Directory.Build.props` holds the version for local builds, and `ship` is what sets it. A release
takes its number from the tag instead (`v2026.8.1` → `2026.8.1`), so publishing is a matter of
tagging — and a tag shaped like anything else fails the workflow on its first step rather than
publishing a package nobody can name.

Every change worth a line goes into `CHANGELOG.md` under a heading naming the version. That section is
not only for readers: `release.yml` reads it back out of the file and it becomes the body of the
GitHub release, with the `.nupkg` files attached — so a tag whose version has no section in the
changelog fails the release rather than publishing something undocumented.

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

That writes the new entries. Deliberate removals are recorded by hand — write `*REMOVED*` in front of
the entry when it was already shipped, delete the line when it was not. At release time the contents
of `Unshipped` move into `Shipped` and `Unshipped` is emptied again — which is what `0.2.0`, the first
release on NuGet, did with the whole surface, and what `1.0.0` did with the review that preceded it.

That move is what keeps the record worth having. While an entry sits in `Unshipped` it can be deleted
for nothing; once it is in `Shipped`, taking it away is a build error until the removal is written
down, so a breaking change cannot slip through as an ordinary diff.

### Preparing a release

Three things change together, so one tool does them:

```
dotnet run --project tools/Arlecchino.Tools -- ship
```

It works out the version — this year and month, and the build after the one the repository holds, or
`1` when the month it holds is not this one — sets `<Version>` to it, moves every recorded entry from
`Unshipped` into `Shipped` for all four packages, and points `PackageValidationBaselineVersion` at the
release that came before, after checking that release really is on nuget.org, since a baseline that is
not published fails the pack rather than validating anything. Read the diff, commit, tag.

A version given as an argument is taken instead of the calculated one — `ship 2026.9.4` — which is how
a release is put back where it belongs after a false start.

### And checked against the last release

The API files say what the source declares. `EnablePackageValidation` checks the package that comes
out of it: `dotnet pack` runs APICompat over the two target frameworks, so `net8.0` and `net10.0`
cannot drift apart, and over the previous release once there is one to compare with.

```xml
<PackageValidationBaselineVersion Condition="'$(IsPackable)' != 'false'">5.0.0</PackageValidationBaselineVersion>
```

The baseline is the last release that is on nuget.org, which is the number `ship` advances — a
baseline left behind on an old release still passes, but it says nothing about everything added since.
Packing a version whose baseline is missing from NuGet fails with `NU1102` rather than passing
quietly, which is the behavior worth having: a validation that silently does nothing is worse than
none.

Since no version is allowed to break the surface, a break against the baseline is always deliberate
and always written down: `dotnet pack` refuses it until the difference sits in the project's
`CompatibilitySuppressions.xml`, where the diff of a pull request has to walk past it. A suppression
names the two versions it was written between, so `ship` throws them away when it moves the baseline —
kept, they would hide the next break rather than the last one.

## Continuous integration

`.github/workflows/build.yml` runs on every push to `master`/`main` and on pull requests that change
something other than documentation — on Windows and Linux, because console behavior differs between
them. The Windows leg uploads the packages as a build artifact.

| Step | Catches |
|---|---|
| Build in `Release` with warnings as errors | Everything the compiler and the Roslyn style rules see |
| The test suite, on both target frameworks | Behavior |
| Coverage, with a floor under it (Linux leg) | Code that arrived without tests. The run fails below 85% of lines or 66% of branches, and the figures per assembly are written to the run summary |
| Every benchmark as a dry job (Linux leg) | A benchmark that stopped compiling or started throwing. Numbers from a shared runner are worthless, so none are recorded — this is a check that the code still runs |
| The sample published with `PublishAot` and run (Linux leg) | What `IsAotCompatible` only warns about. The native binary has to draw a frame, so a registration the trimmer removed or a type built by reflection fails the run rather than the user's `publish` |
| `jb inspectcode` (Windows leg) | What the compiler has no rule for — the `resharper_*` half of `.editorconfig`. A warning fails the build and is annotated on the line it came from |
| An application built against the packages, on both SDKs | Whatever only breaks on the way through NuGet: a generator that emits nothing, a missing `build/*.props`, a namespace that does not exist for a consumer. It is built once on the .NET 10 SDK and once on the .NET 8 one, because a generator is loaded by the compiler the *consumer* has |

That last step is the one worth keeping. It creates a console application from scratch, points it at
the freshly packed `.nupkg` files, writes views, a store, a widget and a command in it, and builds —
which is exactly how the source generator is exercised from the outside. Its source lives in
`.github/consumer`, and the project itself is generated in the runner's temporary directory rather
than inside the checkout: a consumer under this repository would inherit `Directory.Build.props` and
the repository `.editorconfig`, and would then be testing our build settings instead of the packages.

### The other two workflows

`codeql.yml` runs GitHub's analysis over the C# on every push, on pull requests, and once a week on a
schedule — the weekly run is the point of it, since a rule added after a commit was merged still finds
what it finds. `dependabot.yml` proposes dependency updates monthly, grouped so that the
`Microsoft.Extensions.*` packages and the testing ones arrive as one pull request each rather than
five, and it watches the versions of the actions in these workflows as well.

### Committing without running it

A half-finished commit does not need the matrix, and neither does a typo in a page of this
documentation. Two ways out, neither of them invented here:

| Way | What happens |
|---|---|
| `[skip ci]`, `[ci skip]`, `[no ci]`, `[skip actions]` in the commit message | GitHub does not start a run at all — Actions reads the message itself |
| Touching only `**.md`, `docs/**` or `LICENSE` | Nothing starts: those paths are ignored by the workflow |

```bash
git commit -m "halfway through the layout [skip ci]"
```

None of this touches `release.yml`: publishing is triggered by a tag, so it happens when a tag is
pushed and never by accident.

`.github/workflows/release.yml` publishes: push a `v2026.8.1` tag and it builds, tests and pushes all
four packages to NuGet with the version taken from the tag. A tag not shaped `year.month.build` stops
the run before anything is built.

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
| `src/Arlecchino.Pictures` | Readers for the picture formats, published as a package |
| `src/Arlecchino.Testing` | Headless test host published as a package |
| `tools/Arlecchino.Tools` | Everything the repository is maintained with, one file to a tool |
| `samples/Arlecchino.Sample` | Gallery of every modal and widget, also the headless `--frame` renderer |
| `samples/Arlecchino.Processes` | A real application: the process list, live-loaded and sortable |
| `benchmarks/Arlecchino.Benchmarks` | Frame composition, text measurement, input and atoms |
| `tests/Arlecchino.Tests` | Test suite: rendering, navigation, every modal, color conversion |
| `artifacts/packages` | Local package feed produced by the `pack` tool |

This documentation is not among them: it lives in
[Arlecchino.Docs](https://github.com/The1fEst/Arlecchino.Docs), and a release asks that repository to
regenerate the API reference from the assemblies it just published.

## Conventions

- No comments in the source; names carry the meaning, and documentation lives in its own repository.
- No user-visible string at a call site — every one of them is a delegate on
  [`ArlecchinoStrings`](localization.md).
- No application domain types in the framework; extension points are interfaces (`IArlecchinoView`,
  `IArlecchinoViewFactory`, `IArlecchinoCommand`, `IArlecchinoStartup`, `IArlecchinoTerminal`).
