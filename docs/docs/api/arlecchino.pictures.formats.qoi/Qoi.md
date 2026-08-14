---
title: "Qoi"
sidebar_label: "Qoi"
---

# Qoi class

**Namespace:** `Arlecchino.Pictures.Formats.Qoi` &middot; **Assembly:** `Arlecchino.Pictures`

Reads a `QOI`, which is one pass over the bytes: a chunk is either a color, a small step from the last one, a run of it, or one of the sixty-four colors seen recently.

```csharp
public sealed class Qoi : IPictureFormat
```

**Implements** [`IPictureFormat`](../arlecchino.pictures/IPictureFormat.md)

## Constructors

| Member | Summary |
|---|---|
| [`Qoi()`](#qoi) |  |

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

### `Qoi()` {#qoi}

```csharp
public Qoi();
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

