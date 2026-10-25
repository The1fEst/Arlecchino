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
| [`UseHistory(IServiceCollection, Action<HistoryOptions>)`](#usehistory-iservicecollection-action-historyoptions) | Adds undo and redo; without this call nothing records a step. Naming a file makes the history outlive the run, and without one it is kept in memory.  ```csharp builder.Services.UseHistory(options => options.FileName = "session.undo");  ``` |

## Methods in detail

### `UseHistory(IServiceCollection, Action<HistoryOptions>)` {#usehistory-iservicecollection-action-historyoptions}

```csharp
public static IServiceCollection UseHistory(
    this IServiceCollection services,
    Action<HistoryOptions>? configure = null);
```

Adds undo and redo; without this call nothing records a step. Naming a file makes the history outlive the run, and without one it is kept in memory.

```csharp
builder.Services.UseHistory(options => options.FileName = "session.undo");

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

