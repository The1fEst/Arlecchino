![Arlecchino](https://raw.githubusercontent.com/The1fEst/Arlecchino/master/assets/arlecchino-banner.png)

[![NuGet](https://img.shields.io/nuget/v/Arlecchino.Pictures?logo=nuget&label=Arlecchino.Pictures&color=C9382B&labelColor=141317)](https://www.nuget.org/packages/Arlecchino.Pictures)
[![Downloads](https://img.shields.io/nuget/dt/Arlecchino.Pictures?color=C9382B&labelColor=141317)](https://www.nuget.org/packages/Arlecchino.Pictures)
[![Build](https://img.shields.io/github/actions/workflow/status/The1fEst/Arlecchino/build.yml?branch=master&logo=github&labelColor=141317)](https://github.com/The1fEst/Arlecchino/actions/workflows/build.yml)
![Target frameworks](https://img.shields.io/badge/net8.0%20%7C%20net10.0-512BD4?logo=dotnet&logoColor=white&labelColor=141317)
[![MIT](https://img.shields.io/badge/license-MIT-EDE6D9?labelColor=141317)](https://github.com/The1fEst/Arlecchino/blob/master/LICENSE)

Picture files read into the pixels the `Picture` widget draws: PNG, JPEG, BMP, Netpbm, QOI and Targa. Every decoder is written against the format itself, so the package has no dependency
beyond [`Arlecchino.Core`](https://www.nuget.org/packages/Arlecchino.Core) and nothing native to
carry from platform to platform.

## Quick start

```
dotnet add package Arlecchino.Pictures
```

```csharp
using Arlecchino.Pictures;
using Arlecchino.Widgets.Pictures;

var bytes = File.ReadAllBytes(path);

if (PictureFormats.Read(bytes) is { } raster)
{
    var picture = new Picture();

    picture.Show(raster.Pixels, raster.Width, raster.Height);
}
```

A file is recognized by what is in it rather than by what it is called, since a picture opened from a
file manager is as likely to be named wrongly as rightly. `PictureFormats.For` says which format
claimed it, which is what a status line shows.

Nothing here throws. What cannot be read comes back as `null`, and the caller shows the bytes
instead.

## What is read

| Format | What of it |
|---|---|
| **PNG** | every color type and bit depth, interlaced or not |
| **JPEG** | baseline and progressive, at any sampling, with restart markers |
| **BMP** | 1, 4, 8, 16, 24 and 32 bits a pixel, plainly written, run-length encoded, or with named masks |
| **Netpbm** | P1 to P6, as numbers or as bytes, at any depth |
| **QOI** | all of it |
| **Targa** | 8, 15, 16, 24 and 32 bits a pixel, with a color map or without, run-length encoded or not |

Alpha is read and then dropped: a terminal has nothing to show it against.

A header states its own size, so a small file can ask for an enormous picture. `PictureLimits.Most`
is what stands against that: anything larger is refused before a byte is allocated for it.

`PictureLimits.Enough` is the other half — how many pixels you actually have a use for:

```csharp
var raster = PictureFormats.Read(bytes, PictureLimits.For(picture.Detail));
```

A twenty-four megapixel photograph drawn into a terminal pane is twenty-three million pixels nobody
sees. JPEG answers such a request by reading each block at a quarter or an eighth of its side —
a block is a square of waves, and the flattest few of them are a smaller square of samples for a
fraction of the arithmetic. The size it lands on is its own, and never smaller than you asked for.

Every type is trimming- and Native AOT-compatible.

## Packages

| Package | Contents |
|---|---|
| [`Arlecchino.Core`](https://www.nuget.org/packages/Arlecchino.Core) | the renderer, no DI, and atoms with their undo history |
| [`Arlecchino`](https://www.nuget.org/packages/Arlecchino) | views, navigation, modals, commands, hosting, DI, async stores, and the generator |
| [`Arlecchino.Pictures`](https://www.nuget.org/packages/Arlecchino.Pictures) | this one — picture files read into pixels |
| [`Arlecchino.Testing`](https://www.nuget.org/packages/Arlecchino.Testing) | `ArlecchinoTestHost` — the headless host applications write their tests against |

They ship together and always carry the same version.

## Links

[Documentation](https://the1fest.github.io/Arlecchino.Docs/) ·
[Changelog](https://github.com/The1fEst/Arlecchino/blob/master/CHANGELOG.md) ·
[Source and issues](https://github.com/The1fEst/Arlecchino)

MIT.
