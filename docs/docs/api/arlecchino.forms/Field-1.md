---
title: "Field<T>"
sidebar_label: "Field<T>"
---

# Field&lt;T&gt; class

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

One row of a form over a value of a known kind. It holds no value of its own, so a row shows what the state holds now; build one through [`Fields`](../arlecchino.forms/Fields.md) and add the rest with the `With` members.

```csharp
public class Field<T> : IField
```

**Implements** [`IField`](../arlecchino.forms/IField.md)

## Constructors

| Member | Summary |
|---|---|
| [`Field(IReadableAtom)`](#field-ireadableatom) | Creates a row over an atom. |

## Properties

| Member | Summary |
|---|---|
| [`Action`](#action) |  |
| [`Help`](#help) |  |
| [`IsEnabled`](#isenabled) |  |
| [`Items`](#items) | What the value may be, which is what turns a row into one that is picked from rather than typed into. For a number the lowest and the highest of them are the ends of its range. |
| [`Label`](#label) |  |
| [`Template`](#template) |  |
| [`Validation`](#validation) | Whether a value is allowed, answered when the dialog is confirmed; `true` lets it through. Nothing accepts whatever was typed. |
| [`Value`](#value) |  |

## Methods

| Member | Summary |
|---|---|
| [`TryReset()`](#tryreset) |  |
| [`WithAction(Func<ViewRoute>)`](#withaction-func-viewroute) | Makes confirming the row do this rather than open the dialog its value would otherwise get. |
| [`WithHelp(Func<string>)`](#withhelp-func-string) | Gives the row some help, shown under it while it is selected. It wraps over as many rows as it takes, and a line break in it is kept. |
| [`WithItems(Func<T[]>)`](#withitems-func-t) | Gives the row the values it is picked from, or the range it moves along. |
| [`WithLabel(Func<string>)`](#withlabel-func-string) | Names the row. |
| [`WithTemplate(Func<string>)`](#withtemplate-func-string) | Words the value for the form, where the atom does not write itself out the way it reads. |
| [`WithValidation(Func<T, bool>)`](#withvalidation-func-t-bool) | Decides which values the dialog will accept. |

## Constructors in detail

### `Field(IReadableAtom)` {#field-ireadableatom}

```csharp
public Field(IReadableAtom value);
```

Creates a row over an atom.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md) | The atom to read and write. |

## Properties in detail

### `Action` {#action}

```csharp
public Func<ViewRoute> Action { get; }
```

**Type** `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

### `Help` {#help}

```csharp
public Func<string> Help { get; }
```

**Type** `Func<TResult>`&lt;`string`&gt;

### `IsEnabled` {#isenabled}

```csharp
public bool IsEnabled { get; set; }
```

**Type** `bool`

### `Items` {#items}

```csharp
public Func<IReadOnlyList<T>> Items { get; }
```

What the value may be, which is what turns a row into one that is picked from rather than typed into. For a number the lowest and the highest of them are the ends of its range.

**Type** `Func<TResult>`&lt;`IReadOnlyList<T>`&lt;`T`&gt;&gt;

### `Label` {#label}

```csharp
public Func<string> Label { get; }
```

**Type** `Func<TResult>`&lt;`string`&gt;

### `Template` {#template}

```csharp
public Func<string> Template { get; }
```

**Type** `Func<TResult>`&lt;`string`&gt;

### `Validation` {#validation}

```csharp
public Func<T, bool> Validation { get; }
```

Whether a value is allowed, answered when the dialog is confirmed; `true` lets it through. Nothing accepts whatever was typed.

**Type** `Func<T, TResult>`&lt;`T`, `bool`&gt;

### `Value` {#value}

```csharp
public IReadableAtom Value { get; }
```

**Type** [`IReadableAtom`](../arlecchino.atoms/IReadableAtom.md)

## Methods in detail

### `TryReset()` {#tryreset}

```csharp
public bool TryReset();
```

**Returns** `bool`

### `WithAction(Func<ViewRoute>)` {#withaction-func-viewroute}

```csharp
public Field<T> WithAction(Func<ViewRoute> action);
```

Makes confirming the row do this rather than open the dialog its value would otherwise get.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `action` | `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt; | What to run. Returning a route navigates. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`T`&gt; — The same row, to go on building it.

### `WithHelp(Func<string>)` {#withhelp-func-string}

```csharp
public Field<T> WithHelp(Func<string> help);
```

Gives the row some help, shown under it while it is selected. It wraps over as many rows as it takes, and a line break in it is kept.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `help` | `Func<TResult>`&lt;`string`&gt; | What to say. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`T`&gt; — The same row, to go on building it.

### `WithItems(Func<T[]>)` {#withitems-func-t}

```csharp
public Field<T> WithItems(Func<T[]> items);
```

Gives the row the values it is picked from, or the range it moves along.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `items` | `Func<TResult>`&lt;`T`\[\]&gt; | What the value may be, in the order they are offered. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`T`&gt; — The same row, to go on building it.

### `WithLabel(Func<string>)` {#withlabel-func-string}

```csharp
public Field<T> WithLabel(Func<string> label);
```

Names the row.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the row is called. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`T`&gt; — The same row, to go on building it.

### `WithTemplate(Func<string>)` {#withtemplate-func-string}

```csharp
public Field<T> WithTemplate(Func<string> template);
```

Words the value for the form, where the atom does not write itself out the way it reads.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `template` | `Func<TResult>`&lt;`string`&gt; | What to show instead. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`T`&gt; — The same row, to go on building it.

### `WithValidation(Func<T, bool>)` {#withvalidation-func-t-bool}

```csharp
public Field<T> WithValidation(Func<T, bool> validation);
```

Decides which values the dialog will accept.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `validation` | `Func<T, TResult>`&lt;`T`, `bool`&gt; | Answers whether a value is allowed. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`T`&gt; — The same row, to go on building it.

