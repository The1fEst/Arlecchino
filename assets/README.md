# Arlecchino — brand assets

Original design in a dark commedia dell'arte register: a bone harlequin mask,
swept jester horns with bell diamonds, and a rhombus lattice that doubles as a
nod to the character grid of a terminal. The banner and social card carry the
wordmark only — no tagline, no command line.

## Palette

| Role | Hex |
| --- | --- |
| Ink (plate, eyes) | `#141317` |
| Bone (mask, wordmark) | `#EDE6D9` |
| Crimson (accent, bells) | `#C9382B` |
| Hairline (borders) | `#2E2B33` |

Type: any monospace stack. The wordmark is set in spaced capitals — letter
spacing is doing as much work as the letterforms, so keep it generous.

## Files

| File | Use |
| --- | --- |
| `arlecchino-banner.svg` / `.png` | 1280×520 hero for the top of the README |
| `arlecchino-social-card.svg` / `.png` | 1280×640 Open Graph / social preview |
| `arlecchino-icon.svg` | 512×512 icon on the dark plate |
| `arlecchino-icon-transparent.svg` | same mask, no plate |
| `arlecchino-glyph.svg` | single-colour silhouette, inherits `currentColor` |
| `arlecchino-icon-{16,32,64,128,256,512}.png` | raster icon sizes, favicon set |

## README snippet

```html
<p align="center">
  <img src="assets/arlecchino-banner.svg" alt="Arlecchino" width="820">
</p>
```

Point GitHub at the social card under Settings → General → Social preview.

## Favicon

```html
<link rel="icon" type="image/png" sizes="32x32" href="/arlecchino-icon-32.png">
<link rel="icon" type="image/svg+xml" href="/arlecchino-glyph.svg">
```

The glyph inherits `currentColor`, so it flips with the surrounding theme —
useful for docs sites and inline navbar marks.
