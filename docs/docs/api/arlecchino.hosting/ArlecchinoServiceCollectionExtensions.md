---
title: "ArlecchinoServiceCollectionExtensions"
sidebar_label: "ArlecchinoServiceCollectionExtensions"
---

# ArlecchinoServiceCollectionExtensions class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Registers Arlecchino with the host's container.

```csharp
public static class ArlecchinoServiceCollectionExtensions
```

## Methods

| Member | Summary |
|---|---|
| [`AddArlecchino(IServiceCollection, Action<ArlecchinoOptions>)`](#addarlecchino-iservicecollection-action-arlecchinooptions) | Registers everything an application needs and returns the builder that describes it. The console terminal is only registered if nothing else claimed the role, so a terminal registered beforehand is left in place. |

## Methods in detail

### `AddArlecchino(IServiceCollection, Action<ArlecchinoOptions>)` {#addarlecchino-iservicecollection-action-arlecchinooptions}

```csharp
public static ArlecchinoBuilder AddArlecchino(
    this IServiceCollection services,
    Action<ArlecchinoOptions>? configure = null);
```

Registers everything an application needs and returns the builder that describes it. The console terminal is only registered if nothing else claimed the role, so a terminal registered beforehand is left in place.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `services` | `IServiceCollection` | The container being built. |
| `configure` | `Action<T>`&lt;[`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md)&gt; | Adjusts the settings before anything reads them. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder, for describing views, commands and the rest.

The look — [`Theme.Palette`](../arlecchino.rendering.colors/Theme.md#palette) and [`Glyphs`](../arlecchino.rendering.text/Glyphs.md) — is installed here rather than when the container hands the options out. Those are read by a frame and so written on the drawing thread, and a container resolves on whichever thread asked first; installing at registration happens before anything has claimed a thread to draw on.

