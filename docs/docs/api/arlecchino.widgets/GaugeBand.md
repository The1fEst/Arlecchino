---
title: GaugeBand
sidebar_label: GaugeBand
---

# GaugeBand struct

**Namespace:** `Arlecchino.Widgets` &middot; **Assembly:** `Arlecchino`

Where a band of a [`Gauge`](../arlecchino.widgets/Gauge.md) starts and how it is drawn. A band runs from [`GaugeBand.From`](../arlecchino.widgets/GaugeBand.md#from) up to the start of the next one, so the bands are given in order and the first of them decides the colour of everything below it.

```csharp
public readonly struct GaugeBand : IEquatable<GaugeBand>
```

**Implements** `IEquatable<T>`&lt;[`GaugeBand`](../arlecchino.widgets/GaugeBand.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`GaugeBand(decimal, IArlecchinoColor)`](#gaugeband-decimal-iarlecchinocolor) | Where a band of a [`Gauge`](../arlecchino.widgets/Gauge.md) starts and how it is drawn. A band runs from [`GaugeBand.From`](../arlecchino.widgets/GaugeBand.md#from) up to the start of the next one, so the bands are given in order and the first of them decides the colour of everything below it. |

## Properties

| Member | Summary |
|---|---|
| [`From`](#from) | The value the band starts at. |
| [`Style`](#style) | How the part of the track inside the band is drawn. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out decimal, out IArlecchinoColor)`](#deconstruct-out-decimal-out-iarlecchinocolor) |  |

## Constructors in detail

### `GaugeBand(decimal, IArlecchinoColor)` {#gaugeband-decimal-iarlecchinocolor}

```csharp
public GaugeBand(decimal From, IArlecchinoColor Style);
```

Where a band of a [`Gauge`](../arlecchino.widgets/Gauge.md) starts and how it is drawn. A band runs from [`GaugeBand.From`](../arlecchino.widgets/GaugeBand.md#from) up to the start of the next one, so the bands are given in order and the first of them decides the colour of everything below it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `From` | `decimal` | The value the band starts at. |
| `Style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) | How the part of the track inside the band is drawn. |

## Properties in detail

### `From` {#from}

```csharp
public decimal From { get; init; }
```

The value the band starts at.

**Type** `decimal`

### `Style` {#style}

```csharp
public IArlecchinoColor Style { get; init; }
```

How the part of the track inside the band is drawn.

**Type** [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md)

## Methods in detail

### `Deconstruct(out decimal, out IArlecchinoColor)` {#deconstruct-out-decimal-out-iarlecchinocolor}

```csharp
public void Deconstruct(out decimal From, out IArlecchinoColor Style);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `From` | `decimal` |  |
| `Style` | [`IArlecchinoColor`](../arlecchino.rendering/IArlecchinoColor.md) |  |

