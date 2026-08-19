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
| [`AddArlecchino(IServiceCollection, Action<ArlecchinoOptions>)`](#addarlecchino-iservicecollection-action-arlecchinooptions) | Registers everything an application needs and returns the builder that describes it. The look and the hold on the console are installed here, before any thread has claimed the drawing, and a terminal already registered stands. |

## Methods in detail

### `AddArlecchino(IServiceCollection, Action<ArlecchinoOptions>)` {#addarlecchino-iservicecollection-action-arlecchinooptions}

```csharp
public static ArlecchinoBuilder AddArlecchino(
    this IServiceCollection services,
    Action<ArlecchinoOptions>? configure = null);
```

Registers everything an application needs and returns the builder that describes it. The look and the hold on the console are installed here, before any thread has claimed the drawing, and a terminal already registered stands.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `services` | `IServiceCollection` | The container being built. |
| `configure` | `Action<T>`&lt;[`ArlecchinoOptions`](../arlecchino.hosting/ArlecchinoOptions.md)&gt; | Adjusts the settings before anything reads them. |

**Returns** [`ArlecchinoBuilder`](../arlecchino.hosting/ArlecchinoBuilder.md) — The builder, for describing views, commands and the rest.

