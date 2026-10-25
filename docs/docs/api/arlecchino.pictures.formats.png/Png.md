---
title: "Png"
sidebar_label: "Png"
---

# Png class

**Namespace:** `Arlecchino.Pictures.Formats.Png` &middot; **Assembly:** `Arlecchino.Pictures`

Reads a PNG into pixels, dropping the alpha a terminal has nothing to show against. Every color type and depth the format allows is read, interlaced or not.

```csharp
public sealed class Png : IPictureFormat
```

**Implements** [`IPictureFormat`](../arlecchino.pictures/IPictureFormat.md)

## Constructors

| Member | Summary |
|---|---|
| [`Png()`](#png) |  |

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

### `Png()` {#png}

```csharp
public Png();
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

