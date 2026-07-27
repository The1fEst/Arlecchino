---
title: ViewRoute
sidebar_label: ViewRoute
---

# ViewRoute struct

**Namespace:** `Arlecchino.Navigation` &middot; **Assembly:** `Arlecchino`

Where to navigate: a name in a struct, compared ordinally. Routes are strings rather than an enum because the framework has to name a screen without seeing the application's types — the generated `ViewKind` lives in your assembly, not in Arlecchino.

```csharp
public readonly struct ViewRoute : IEquatable<ViewRoute>
```

**Implements** `IEquatable<T>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`ViewRoute(string)`](#viewroute-string) | Where to navigate: a name in a struct, compared ordinally. Routes are strings rather than an enum because the framework has to name a screen without seeing the application's types — the generated `ViewKind` lives in your assembly, not in Arlecchino. |

## Properties

| Member | Summary |
|---|---|
| [`IsNone`](#isnone) | Whether this is the empty route. |
| [`Name`](#name) | Name of the route. |
| [`None`](#none) | The empty route: returned from a handler to stay where you are. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out string)`](#deconstruct-out-string) |  |
| [`Equals(ViewRoute)`](#equals-viewroute) | Compares routes by name, case-sensitively. |
| [`GetHashCode()`](#gethashcode) | Hash of the route name. |
| [`ToString()`](#tostring) | The route name, or `None` for the empty route. |

## Constructors in detail

### `ViewRoute(string)` {#viewroute-string}

```csharp
public ViewRoute(string Name);
```

Where to navigate: a name in a struct, compared ordinally. Routes are strings rather than an enum because the framework has to name a screen without seeing the application's types — the generated `ViewKind` lives in your assembly, not in Arlecchino.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | Name of the route. |

## Properties in detail

### `IsNone` {#isnone}

```csharp
public bool IsNone { get; }
```

Whether this is the empty route.

**Type** `bool`

### `Name` {#name}

```csharp
public string Name { get; init; }
```

Name of the route.

**Type** `string`

### `None` {#none}

```csharp
public static ViewRoute None { get; }
```

The empty route: returned from a handler to stay where you are.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

## Methods in detail

### `Deconstruct(out string)` {#deconstruct-out-string}

```csharp
public void Deconstruct(out string Name);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Name` | `string` |  |

### `Equals(ViewRoute)` {#equals-viewroute}

```csharp
public bool Equals(ViewRoute other);
```

Compares routes by name, case-sensitively.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | The route to compare with. |

**Returns** `bool` — `true` when both name the same screen.

### `GetHashCode()` {#gethashcode}

```csharp
public override int GetHashCode();
```

Hash of the route name.

**Returns** `int` — The hash code.

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

The route name, or `None` for the empty route.

**Returns** `string` — Readable form of the route.

