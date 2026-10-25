---
title: "Routes"
sidebar_label: "Routes"
---

# Routes class

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

Routes of the screens that ship with the framework.

```csharp
public static class Routes
```

## Fields

| Member | Summary |
|---|---|
| [`FilePicker`](#filepicker) | The file picker. Fill `ArlecchinoState.FilePicker`, then navigate here. |
| [`Help`](#help) | The screen listing every key and command. |
| [`Notifications`](#notifications) | The notifications screen, listing what the application has said lately. |

## Fields in detail

### `FilePicker` {#filepicker}

```csharp
public static readonly ViewRoute FilePicker { get; }
```

The file picker. Fill `ArlecchinoState.FilePicker`, then navigate here.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

### `Help` {#help}

```csharp
public static readonly ViewRoute Help { get; }
```

The screen listing every key and command.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

### `Notifications` {#notifications}

```csharp
public static readonly ViewRoute Notifications { get; }
```

The notifications screen, listing what the application has said lately.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

