---
title: "IPictureFormat"
sidebar_label: "IPictureFormat"
---

# IPictureFormat interface

**Namespace:** `Arlecchino.Pictures` &middot; **Assembly:** `Arlecchino.Pictures`

One picture format. It claims a file by the head of it and reads the whole of one into pixels, answering `null` rather than throwing.

```csharp
public interface IPictureFormat
```

**Implemented by** [`Bmp`](../arlecchino.pictures.formats.bmp/Bmp.md), [`Jpeg`](../arlecchino.pictures.formats.jpeg/Jpeg.md), [`Png`](../arlecchino.pictures.formats.png/Png.md), [`Pnm`](../arlecchino.pictures.formats.pnm/Pnm.md), [`Qoi`](../arlecchino.pictures.formats.qoi/Qoi.md), [`Tga`](../arlecchino.pictures.formats.tga/Tga.md)

## Properties

| Member | Summary |
|---|---|
| [`Name`](#name) | What the format is called, in lower case, for a caller that shows it. |

## Methods

| Member | Summary |
|---|---|
| [`Read(ReadOnlySpan<byte>, PictureLimits)`](#read-readonlyspan-byte-picturelimits) | Reads the picture. |
| [`Starts(ReadOnlySpan<byte>)`](#starts-readonlyspan-byte) | Whether the bytes begin the way this format does. |

## Properties in detail

### `Name` {#name}

```csharp
public string Name { get; }
```

What the format is called, in lower case, for a caller that shows it.

**Type** `string`

## Methods in detail

### `Read(ReadOnlySpan<byte>, PictureLimits)` {#read-readonlyspan-byte-picturelimits}

```csharp
public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits);
```

Reads the picture.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `bytes` | `ReadOnlySpan<T>`&lt;`byte`&gt; | The whole file. |
| `limits` | [`PictureLimits`](../arlecchino.pictures/PictureLimits.md) | What the caller will hold and what it has a use for. |

**Returns** [`Raster`](../arlecchino.pictures/Raster.md) — The pixels, or `null` when this is not a file of this format that can be read.

### `Starts(ReadOnlySpan<byte>)` {#starts-readonlyspan-byte}

```csharp
public bool Starts(ReadOnlySpan<byte> bytes);
```

Whether the bytes begin the way this format does.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `bytes` | `ReadOnlySpan<T>`&lt;`byte`&gt; | The head of a file. |

**Returns** `bool` — `true` when the file is worth trying to read.

