---
title: "IField"
sidebar_label: "IField"
---

# IField interface

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

One row of a form, as the form sees it. Rows over values of different kinds stand in one list through this, and the form asks the atom rather than the row for what is shown.

```csharp
public interface IField
```

**Implemented by** [`ActionField`](../arlecchino.forms/ActionField.md), [`Field`](../arlecchino.forms/Field-1.md), [`PathField`](../arlecchino.forms/PathField.md), [`SecretField`](../arlecchino.forms/SecretField.md), [`SliderField`](../arlecchino.forms/SliderField.md)

## Properties

| Member | Summary |
|---|---|
| [`Action`](#action) | What confirming the row does instead of opening the dialog its value would otherwise get. Returning a route navigates and [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) stays put. |
| [`Help`](#help) | A line shown under the row while it is selected. Nothing means none is shown. |
| [`IsEnabled`](#isenabled) | Whether the row can be used. A disabled row is drawn muted and confirming it does nothing. |
| [`Label`](#label) | What the row is called. |
| [`Template`](#template) | How the value is worded, for the values an atom does not write out the way a form should show them — a secret as dots, a date in the order the reader expects. Nothing leaves it to the atom. |
| [`Value`](#value) | The atom the row shows and edits, or `null` for a button, which has no value to show and is drawn as a button because of it. |

## Methods

| Member | Summary |
|---|---|
| [`TryReset()`](#tryreset) | Puts the value back to what the atom held before anything was written to it. |

## Properties in detail

### `Action` {#action}

```csharp
public Func<ViewRoute>? Action { get; }
```

What confirming the row does instead of opening the dialog its value would otherwise get. Returning a route navigates and [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) stays put.

**Type** `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

### `Help` {#help}

```csharp
public Func<string>? Help { get; }
```

A line shown under the row while it is selected. Nothing means none is shown.

**Type** `Func<TResult>`&lt;`string`&gt;

### `IsEnabled` {#isenabled}

```csharp
public bool IsEnabled { get; set; }
```

Whether the row can be used. A disabled row is drawn muted and confirming it does nothing.

**Type** `bool`

### `Label` {#label}

```csharp
public Func<string>? Label { get; }
```

What the row is called.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Template` {#template}

```csharp
public Func<string>? Template { get; }
```

How the value is worded, for the values an atom does not write out the way a form should show them — a secret as dots, a date in the order the reader expects. Nothing leaves it to the atom.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Value` {#value}

```csharp
public IReadableAtom? Value { get; }
```

The atom the row shows and edits, or `null` for a button, which has no value to show and is drawn as a button because of it.

**Type** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md)

## Methods in detail

### `TryReset()` {#tryreset}

```csharp
public bool TryReset();
```

Puts the value back to what the atom held before anything was written to it.

**Returns** `bool` — `false` when the atom has no starting value to go back to.

