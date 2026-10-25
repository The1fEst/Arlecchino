---
title: "Bmp"
sidebar_label: "Bmp"
---

# Bmp class

**Namespace:** `Arlecchino.Pictures.Formats.Bmp` &middot; **Assembly:** `Arlecchino.Pictures`

Reads a Windows bitmap. The rows stand bottom to top unless the height is negative, the colors are written blue first, and the rows come either plainly or run-length encoded.

```csharp
public sealed class Bmp : IPictureFormat
```

**Implements** [`IPictureFormat`](../arlecchino.pictures/IPictureFormat.md)

## Constructors

| Member | Summary |
|---|---|
| [`Bmp()`](#bmp) |  |

## Properties

| Member | Summary |
|---|---|
| [`Name`](#name) |  |

## Methods

| Member | Summary |
|---|---|
| [`Read(ReadOnlySpan<byte>, PictureLimits)`](#read-readonlyspan-byte-picturelimits) |  |
| [`Starts(ReadOnlySpan<byte>)`](#starts-readonlyspan-byte) |  |

## Constructors in detail

### `Bmp()` {#bmp}

```csharp
public Bmp();
```

## Properties in detail

### `Name` {#name}

```csharp
public string Name { get; }
```

**Type** `string`

## Methods in detail

### `Read(ReadOnlySpan<byte>, PictureLimits)` {#read-readonlyspan-byte-picturelimits}

```csharp
public Raster? Read(ReadOnlySpan<byte> bytes, PictureLimits limits);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `bytes` | `ReadOnlySpan<T>`&lt;`byte`&gt; |  |
| `limits` | [`PictureLimits`](../arlecchino.pictures/PictureLimits.md) |  |

**Returns** [`Raster`](../arlecchino.pictures/Raster.md)

### `Starts(ReadOnlySpan<byte>)` {#starts-readonlyspan-byte}

```csharp
public bool Starts(ReadOnlySpan<byte> bytes);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `bytes` | `ReadOnlySpan<T>`&lt;`byte`&gt; |  |

**Returns** `bool`

