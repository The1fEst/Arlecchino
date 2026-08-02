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

## The tools

Everything the repository is maintained with lives in `tools/Arlecchino.Tools`, one file to a tool, and
is named by the first argument:

```bash
dotnet run --project tools/Arlecchino.Tools -- pack
dotnet run --project tools/Arlecchino.Tools -- oracle
dotnet run --project tools/Arlecchino.Tools -- keys
dotnet run --project tools/Arlecchino.Tools -- live
dotnet run --project tools/Arlecchino.Tools -- ship 3.1.0
```

It is a project in the solution rather than a folder of scripts so that the tools are built, inspected
and analysed with everything else — a script beside the repository is checked by nothing.

- **`pack`** builds the three packages into `artifacts/packages`. That local feed is how an application
  is tried against a change before it is released; skip it and the application quietly keeps building
  against the version on nuget.org.
- **`oracle`** holds the screen the tests read frames back from against a real terminal. `ScreenGrid`
  and the code that writes the frames were written by the same head, so a wrong idea about the edge of
  a row or the width of a symbol would be held by both and cancel out, leaving every test green and the
  picture wrong. This draws frames through the real `Surface`, plays what was written into a `tmux` pane
  of the same size, and compares the screen tmux ended up with against the one `ScreenGrid` did — the
  symbols, the colour of every cell, and where the cursor was left. It needs `tmux` on `PATH`, and it is
  not part of the test run — run it after a change to `ScreenGrid` or to how `Surface` writes a frame.
- **`keys`** is the same argument about input. The escape sequences the tests feed the reader were
  written by the same head that wrote the reader, so a key spelled wrong in both places reads correctly
  in every test and does nothing at all in a terminal. This presses each key in a tmux pane, reads the
  bytes it really produced off the pty, and hands them to the reader — along with what
  `Console.ReadKey` made of the same press, which is the shape an application actually meets. Also
  needs `tmux`.
- **`live`** runs a whole application in a pane and holds it to what a terminal application owes the
  person who started it: take the screen, draw on it, give it back as it was found. The fake terminal
  records taking the screen as a flag being set, which is true whether or not the sequence that does it
  was ever written, written correctly, or written back on the way out; tmux says whether the alternate
  screen is really in force, whether the cursor is showing, whether the mouse was released, and the
  shell underneath is still there to compare against. Runs the sample by default, `--app` for anything
  else, `--keys "Down Tab"` to drive it. Also needs `tmux`.
- **`ship`** prepares a release: it sets the version, moves the recorded public API from
  `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt`, and points package validation at the release
  before it. Run it, read the diff, commit, tag.

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
