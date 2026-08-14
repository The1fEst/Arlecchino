---
title: Arlecchino.Widgets.Pictures
sidebar_label: Arlecchino.Widgets.Pictures
sidebar_position: 0
---

# Arlecchino.Widgets.Pictures

## Classes

| Type | Summary |
|---|---|
| [`Picture`](Picture.md) | An image drawn in cells, two pixels to each, or in a graphics protocol where [`Picture.Protocol`](../arlecchino.widgets.pictures/Picture.md#protocol) names one. The pixels are handed over rather than read from a file, since decoding belongs to the application.  ```csharp private readonly Picture _preview = new();  _preview.Show(pixels, width, height); _preview.Draw(region);  ``` |

