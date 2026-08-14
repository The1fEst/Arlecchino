---
title: Arlecchino.Pictures
sidebar_label: Arlecchino.Pictures
sidebar_position: 0
---

# Arlecchino.Pictures

## Classes

| Type | Summary |
|---|---|
| [`PictureFormats`](PictureFormats.md) | The formats that can be read, and the two questions asked of them: which format a file is, and what it holds. A file is recognized by what is in it rather than by what it is called. |
| [`Raster`](Raster.md) | What a picture turned out to hold, ready to be handed to a widget that draws pixels. |

## Structs

| Type | Summary |
|---|---|
| [`PictureLimits`](PictureLimits.md) | What a caller will hold, and what it has a use for. A format that can read itself at a smaller size does so rather than decoding pixels that will never be drawn. |

## Interfaces

| Type | Summary |
|---|---|
| [`IPictureFormat`](IPictureFormat.md) | One picture format. It claims a file by the head of it and reads the whole of one into pixels, answering `null` rather than throwing. |

