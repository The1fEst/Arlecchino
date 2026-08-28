---
title: "HistoryServiceCollectionExtensions"
sidebar_label: "HistoryServiceCollectionExtensions"
---

# HistoryServiceCollectionExtensions class

**Namespace:** `Arlecchino.Atoms.History.Hosting` &middot; **Assembly:** `Arlecchino.Atoms.History`

Registers the undo history with the host's container.

```csharp
public static class HistoryServiceCollectionExtensions
```

## Methods

| Member | Summary |
|---|---|
| [`UseHistory(IServiceCollection, Action<HistoryOptions>)`](#usehistory-iservicecollection-action-historyoptions) | Adds undo and redo. Without this call an application has atoms and no history: nothing records steps, and the atoms that could feed one allocate nothing for it.  ```csharp builder.Services.UseHistory(options => options.Keep = true);  ``` |

## Methods in detail

### `UseHistory(IServiceCollection, Action<HistoryOptions>)` {#usehistory-iservicecollection-action-historyoptions}

```csharp
public static IServiceCollection UseHistory(
    this IServiceCollection services,
    Action<HistoryOptions>? configure = null);
```

Adds undo and redo. Without this call an application has atoms and no history: nothing records steps, and the atoms that could feed one allocate nothing for it.

```csharp
builder.Services.UseHistory(options => options.Keep = true);

```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `services` | `IServiceCollection` | The container being built. |
| `configure` | `Action<T>`&lt;[`HistoryOptions`](../arlecchino.atoms.history.hosting/HistoryOptions.md)&gt; | Adjusts the settings before anything reads them. |

**Returns** `IServiceCollection` — The container, for chaining.

**Exceptions**

| Type | Thrown when |
|---|---|
| `InvalidOperationException` | Called twice; there is one history per application. |

