---
title: "IArlecchinoCommand"
sidebar_label: "IArlecchinoCommand"
---

# IArlecchinoCommand interface

**Namespace:** `Arlecchino.Commands` &middot; **Assembly:** `Arlecchino`

An application-wide command. It appears in the command palette from every screen, and fires globally when its binding carries a modifier — a plain letter would swallow typing, so those are only reachable through the palette.

```csharp
public interface IArlecchinoCommand
```

## Properties

| Member | Summary |
|---|---|
| [`Binding`](#binding) | Key that runs the command. |
| [`Icon`](#icon) | Short marker to draw beside the label; yours to use or ignore. |
| [`Label`](#label) | Name shown in the palette. |

## Methods

| Member | Summary |
|---|---|
| [`Execute()`](#execute) | Runs the command. |

## Properties in detail

### `Binding` {#binding}

```csharp
public KeyBinding Binding { get; }
```

Key that runs the command.

**Type** [`KeyBinding`](../arlecchino.input/KeyBinding.md)

### `Icon` {#icon}

```csharp
public string Icon { get; }
```

Short marker to draw beside the label; yours to use or ignore.

**Type** `string`

### `Label` {#label}

```csharp
public string Label { get; }
```

Name shown in the palette.

**Type** `string`

## Methods in detail

### `Execute()` {#execute}

```csharp
public ViewRoute Execute();
```

Runs the command.

**Returns** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) — A route to navigate to, or [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) to stay.

