<p align="center">
  <img src="static/img/arlecchino-banner.svg" alt="Arlecchino" width="820">
</p>

<p align="center">
  <a href="https://the1fest.github.io/Arlecchino.Docs/"><img src="https://img.shields.io/badge/read-the%20docs-C9382B?labelColor=141317" alt="Documentation"></a>
  <a href="https://github.com/The1fEst/Arlecchino"><img src="https://img.shields.io/badge/framework-Arlecchino-EDE6D9?labelColor=141317" alt="Arlecchino"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-EDE6D9?labelColor=141317" alt="MIT"></a>
</p>

The documentation site for [Arlecchino](https://github.com/The1fEst/Arlecchino), a terminal UI
framework for .NET. Published at **https://the1fest.github.io/Arlecchino.Docs/**.

## Layout

| Path | Holds |
|---|---|
| `docs/` | The written pages, as plain CommonMark |
| `docs/api/` | The API reference, generated — do not edit by hand |
| `apidocs/ApiDocs.cs` | The generator that writes `docs/api` from the built assemblies |
| `src/` | The landing page and the theme |
| `static/img/` | Brand assets and screenshots |
| `sidebars.ts` | The order the pages appear in |

## Running it

```bash
npm install
npm start
```

Node 20 or newer. `npm run build` produces the static site in `build/`, and `npm run serve` shows what
was built.

## Regenerating the API reference

The API pages are generated from the three Arlecchino assemblies and the XML documentation they ship
with, then committed — so building the site needs nothing but Node.

With the framework checked out beside this repository:

```bash
dotnet build ../Arlecchino/Arlecchino.slnx -c Release
npm run api
```

`npm run api` is `dotnet run apidocs/ApiDocs.cs -- --repo ../Arlecchino --out docs/api`; `--repo`
points it somewhere else. It needs the .NET 10 SDK, and it deletes and rewrites `docs/api` every time.

The `api` workflow does the same thing on a schedule, on demand, and on a `repository_dispatch` of
type `arlecchino-released`.

## Writing a page

Pages are CommonMark — `markdown.format` is `md`, not MDX — with front matter for the title and the
description. Add the new page to `sidebars.ts`; nothing is picked up automatically except `docs/api`.

Links between pages are relative and keep the `.md` suffix, so they work both on the site and when
reading the file on GitHub:

```markdown
See [Layout](layout.md) and [`Surface`](api/arlecchino.rendering/Surface.md).
```

The build runs with `onBrokenLinks: 'throw'`, so a link that does not resolve fails it.

## Deploying

Pushing to `master` builds and publishes through GitHub Pages. Set **Settings → Pages → Source** to
*GitHub Actions* once, and nothing else is needed.

## License

MIT, the same as the framework.
