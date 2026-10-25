---
title: "Pnm"
sidebar_label: "Pnm"
---

# Pnm class

**Namespace:** `Arlecchino.Pictures.Formats.Pnm` &middot; **Assembly:** `Arlecchino.Pictures`

Reads the `Netpbm` family: a bitmap, a gray picture or a color one, written either as numbers or as the bytes themselves. The header is text, and a comment may stand between any two of its words.

```csharp
public sealed class Pnm : IPictureFormat
```

**Implements** [`IPictureFormat`](../arlecchino.pictures/IPictureFormat.md)

## Constructors

| Member | Summary |
|---|---|
| [`Pnm()`](#pnm) |  |

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

### `Pnm()` {#pnm}

```csharp
public Pnm();
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

