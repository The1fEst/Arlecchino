## What this changes

<!-- What the change does, and why. If it fixes an issue, link it. -->

## Checks

- [ ] `dotnet build Arlecchino.slnx --configuration Release` — no warnings
- [ ] `dotnet test tests/Arlecchino.Tests` — both frameworks green
- [ ] `jb inspectcode Arlecchino.slnx --severity=WARNING` — nothing reported
- [ ] New public members recorded in `PublicAPI.Unshipped.txt`
- [ ] `CHANGELOG.md` updated under `Unreleased`
- [ ] Documentation updated in [Arlecchino.Docs](https://github.com/The1fEst/Arlecchino.Docs), if behaviour changed

## Breaking change?

<!-- Since 1.0.0 a break means a new major. If this is one, say what moves and what it becomes. -->
