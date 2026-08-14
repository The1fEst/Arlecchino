---
title: "PictureFormats"
sidebar_label: "PictureFormats"
---

# PictureFormats class

**Namespace:** `Arlecchino.Pictures` &middot; **Assembly:** `Arlecchino.Pictures`

The formats that can be read, and the two questions asked of them: which format a file is, and what it holds. A file is recognized by what is in it rather than by what it is called.

```csharp
public static class PictureFormats
```

## Fields

| Member | Summary |
|---|---|
| [`DefaultPixels`](#defaultpixels) | How many pixels are read at once when a caller does not say. A header states its own size, so a small file can ask for an enormous picture. |

## Properties

| Member | Summary |
|---|---|
| [`All`](#all) | Every format that is read, in the order a file is offered to them. |

## Methods

| Member | Summary |
|---|---|
| [`For(ReadOnlySpan<byte>)`](#for-readonlyspan-byte) | Which format the file is. |
| [`Read(ReadOnlySpan<byte>)`](#read-readonlyspan-byte) | Reads a picture of whichever format it turns out to be. |
| [`Read(ReadOnlySpan<byte>, PictureLimits)`](#read-readonlyspan-byte-picturelimits) | Reads a picture of whichever format it turns out to be. |

## Fields in detail

### `DefaultPixels` {#defaultpixels}

```csharp
public static int DefaultPixels { get; }
```

How many pixels are read at once when a caller does not say. A header states its own size, so a small file can ask for an enormous picture.

**Type** `int`

## Properties in detail

### `All` {#all}

```csharp
public static IReadOnlyList<IPictureFormat> All { get; }
```

Every format that is read, in the order a file is offered to them.

**Type** `IReadOnlyList<T>`&lt;[`IPictureFormat`](../arlecchino.pictures/IPictureFormat.md)&gt;

## Methods in detail

### `For(ReadOnlySpan<byte>)` {#for-readonlyspan-byte}

```csharp
public static IPictureFormat? For(ReadOnlySpan<byte> head);
```

Which format the file is.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `head` | `ReadOnlySpan<T>`&lt;`byte`&gt; | The head of a file; the signatures are all short. |

**Returns** [`IPictureFormat`](../arlecchino.pictures/IPictureFormat.md) — The format, or `null` when none of them claims it.

### `Read(ReadOnlySpan<byte>)` {#read-readonlyspan-byte}

```csharp
public static Raster? Read(ReadOnlySpan<byte> bytes);
```

Reads a picture of whichever format it turns out to be.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `bytes` | `ReadOnlySpan<T>`&lt;`byte`&gt; | The whole file. |

**Returns** [`Raster`](../arlecchino.pictures/Raster.md) — The pixels, or `null` when nothing here can read it.

### `Read(ReadOnlySpan<byte>, PictureLimits)` {#read-readonlyspan-byte-picturelimits}

```csharp
public static Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits);
```

Reads a picture of whichever format it turns out to be.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `bytes` | `ReadOnlySpan<T>`&lt;`byte`&gt; | The whole file. |
| `limits` | [`PictureLimits`](../arlecchino.pictures/PictureLimits.md) | What the caller will hold and what it has a use for. |

**Returns** [`Raster`](../arlecchino.pictures/Raster.md) — The pixels, or `null` when nothing here can read it.

