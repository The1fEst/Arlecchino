---
title: "Jpeg"
sidebar_label: "Jpeg"
---

# Jpeg class

**Namespace:** `Arlecchino.Pictures.Formats.Jpeg` &middot; **Assembly:** `Arlecchino.Pictures`

Reads a JPEG. A baseline file is turned into samples as it is read; a progressive one is held as coefficients until the last of its scans has been read.

```csharp
public sealed class Jpeg : IPictureFormat
```

**Implements** [`IPictureFormat`](../arlecchino.pictures/IPictureFormat.md)

## Constructors

| Member | Summary |
|---|---|
| [`Jpeg()`](#jpeg) |  |

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

### `Jpeg()` {#jpeg}

```csharp
public Jpeg();
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

