---
title: Arlecchino.Widgets.Pictures
sidebar_label: Arlecchino.Widgets.Pictures
sidebar_position: 0
---

# Arlecchino.Widgets.Pictures

## Classes

| Type | Summary |
|---|---|
| [`Picture`](Picture.md) | An image drawn in cells. Each cell carries two pixels: the upper half block is painted in the color of the pixel above and its background in the color of the pixel below. A cell is about twice as tall as it is wide, so that comes out roughly square per pixel. That is the default because it needs nothing of the terminal but the color it already draws in: no protocol, no state left behind, nothing to clean up when the picture goes away. Where the terminal speaks a graphics protocol, [`Picture.Protocol`](../arlecchino.widgets.pictures/Picture.md#protocol) sends the pixels themselves instead and the picture is as sharp as the screen allows. The pixels are handed over rather than read from a file: decoding PNG or JPEG belongs to the application, which knows what it wants to depend on, while the framework only draws what it is given.  ```csharp private readonly Picture _preview = new();  _preview.Show(pixels, width, height); _preview.Draw(region);  ``` |

