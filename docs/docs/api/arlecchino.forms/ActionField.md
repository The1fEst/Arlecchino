---
title: "ActionField"
sidebar_label: "ActionField"
---

# ActionField class

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

A button rather than a value, for the things a form does once it has been filled in. It holds no atom, which is what the form draws it as a button by.

```csharp
public sealed class ActionField : IField
```

**Implements** [`IField`](../arlecchino.forms/IField.md)

## Constructors

| Member | Summary |
|---|---|
| [`ActionField(Func<string>, Func<ViewRoute>)`](#actionfield-func-string-func-viewroute) | Creates the button. |

## Properties

| Member | Summary |
|---|---|
| [`Action`](#action) |  |
| [`Help`](#help) |  |
| [`IsEnabled`](#isenabled) |  |
| [`Label`](#label) |  |
| [`Template`](#template) |  |
| [`Value`](#value) |  |

## Methods

| Member | Summary |
|---|---|
| [`TryReset()`](#tryreset) |  |
| [`WithHelp(Func<string>)`](#withhelp-func-string) | Gives the button a line of help, shown under it while it is selected. |

## Constructors in detail

### `ActionField(Func<string>, Func<ViewRoute>)` {#actionfield-func-string-func-viewroute}

```csharp
public ActionField(Func<string> label, Func<ViewRoute> action);
```

Creates the button.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the button says. |
| `action` | `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt; | What pressing it does. Returning a route navigates. |

## Properties in detail

### `Action` {#action}

```csharp
public Func<ViewRoute>? Action { get; }
```

**Type** `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

### `Help` {#help}

```csharp
public Func<string>? Help { get; }
```

**Type** `Func<TResult>`&lt;`string`&gt;

### `IsEnabled` {#isenabled}

```csharp
public bool IsEnabled { get; set; }
```

**Type** `bool`

### `Label` {#label}

```csharp
public Func<string>? Label { get; }
```

**Type** `Func<TResult>`&lt;`string`&gt;

### `Template` {#template}

```csharp
public Func<string>? Template { get; }
```

**Type** `Func<TResult>`&lt;`string`&gt;

### `Value` {#value}

```csharp
public IReadableAtom? Value { get; }
```

**Type** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md)

## Methods in detail

### `TryReset()` {#tryreset}

```csharp
public bool TryReset();
```

**Returns** `bool`

### `WithHelp(Func<string>)` {#withhelp-func-string}

```csharp
public ActionField WithHelp(Func<string> help);
```

Gives the button a line of help, shown under it while it is selected.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `help` | `Func<TResult>`&lt;`string`&gt; | What to say. |

**Returns** [`ActionField`](../arlecchino.forms/ActionField.md) — The same button, to go on building it.

