# Contributing

Issues and pull requests are welcome. This page says what the build expects, so that a change does
not bounce off CI for something mechanical.

## Before a pull request

```bash
dotnet build Arlecchino.slnx --configuration Release
dotnet test tests/Arlecchino.Tests
```

Both target frameworks are built and tested, and the whole repository builds with
`TreatWarningsAsErrors` — a warning is a failure, and the fix is the warning rather than a `NoWarn`.

CI also runs `jb inspectcode` against `.editorconfig`, which catches what the compiler has no rule
for. It is worth running before pushing rather than after:

```bash
dotnet tool install --global JetBrains.ReSharper.GlobalTools
jb inspectcode Arlecchino.slnx --severity=WARNING
```

## What the build will insist on

| Rule | Why |
|---|---|
| No `//` comments in `.cs` files | Names and structure carry the meaning; a comment that explains code is a sign the code needs changing |
| `/// <summary>` on everything public in a package | It is the documentation an application sees in its IDE, and the build fails without it |
| `new()` where the type is already written | `IDE0090`, plus the ReSharper half for arguments |
| Braces on every branch, file-scoped namespaces, no unused `using` | `IDE0011`, `IDE0161`, `IDE0005` |
| A new public member recorded in `PublicAPI.Unshipped.txt` | `RS0016`; see [Packages and building](https://the1fest.github.io/Arlecchino.Docs/docs/packages-and-building) |
| Coverage at or above 80% of lines and 65% of branches | Code that arrives without tests fails the run |

Recording a new public member is mechanical:

```bash
dotnet format analyzers src/Arlecchino/Arlecchino.csproj --diagnostics RS0016 --severity warn
```

## Things that are settled

Two questions come up often enough to answer here:

- **No component tree.** Reusable UI is a widget: a class with `Draw(SurfaceRegion)`, and
  `IArlecchinoInteractiveWidget` when it handles keys. There is no layout container hierarchy, and
  adding one is not on the table — see [Widgets](https://the1fest.github.io/Arlecchino.Docs/docs/widgets).
- **No user-visible string in the framework is hardcoded.** Everything goes through
  `ArlecchinoStrings` as a delegate, so an application can translate it — see
  [Localization](https://the1fest.github.io/Arlecchino.Docs/docs/localization).

## The readmes

`README.md` is the page on GitHub. The pages on nuget.org are in `nuget/`, one per package and named
after it — `Arlecchino.md`, `Arlecchino.Core.md`, `Arlecchino.Testing.md` — and each `.csproj` packs
its own as `README.md`. The gallery escapes raw HTML and drops relative image paths, so those three
stay plain Markdown with absolute image URLs and no `<details>`. A change to the quick start, the
packages table or what the framework claims for itself belongs in `README.md` and in
`nuget/Arlecchino.md`.

## Breaking changes

Since `1.0.0` the public surface is a contract: breaking it means a new major, and the API analyzer
will make the break visible in the diff. When one is genuinely due, it lands whole — the old shape is
removed in the same release rather than left behind as an obsolete shim, and the changelog says what
moved.

## Commits

One-line commit messages in the imperative, describing what the change does rather than what was
touched. The changelog is where a change is explained at length.
