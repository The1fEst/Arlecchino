---
title: Field
sidebar_label: Field
---

# Field class

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

One row of a form: a label, the value beside it, and what happens when it is confirmed. Everything is read through delegates rather than stored, so a field always shows what the state holds now and follows the language the application is running in. The factories bind a field to an atom and pick the dialog that suits its type, which is the usual way to build one. The atom they take is a [`Atom`](../arlecchino.atoms/Atom-1.md), which is either a [`TrackedAtom`](../arlecchino.atoms/TrackedAtom-1.md) — so that editing the field can be undone — or a [`LocalAtom`](../arlecchino.atoms/LocalAtom-1.md) when it should not be.

```csharp
public sealed class Field
```

## Constructors

| Member | Summary |
|---|---|
| [`Field()`](#field) |  |

## Properties

| Member | Summary |
|---|---|
| [`Activate`](#activate) | What confirming the field does, usually opening a dialog. Returning a route navigates; return [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) to stay put. Without this the field is read-only. |
| [`Help`](#help) | A line shown under the field while it is selected. Empty means nothing is shown. |
| [`IsAction`](#isaction) | Whether the row is a button rather than a value. Buttons have no value to align, so they are not counted when working out the label column. |
| [`IsEnabled`](#isenabled) | Whether the field can be used. A disabled field is drawn muted and skipped when moving through the form. |
| [`Label`](#label) | What the field is called. |
| [`Reset`](#reset) | Puts the field back to its empty or lowest value. Fields whose type has no sensible empty value, such as a date, leave this out and simply cannot be cleared. |
| [`Value`](#value) | The value as the user should see it, which is not always what is stored. |

## Methods

| Member | Summary |
|---|---|
| [`Action(Func<string>, Func<ViewRoute>, Func<bool>, Func<string>)`](#action-func-string-func-viewroute-func-bool-func-string) | A button rather than a value, for the things a form does once it has been filled in. |
| [`Choice(Func<string>, IReadOnlyList<string>, Atom<string>, Func<string>)`](#choice-func-string-ireadonlylist-string-atom-string-func-string) | One option out of a list, chosen in a filterable dialog. |
| [`Color(Func<string>, Atom<Rgb>, Func<string>)`](#color-func-string-atom-rgb-func-string) | A colour, shown as its hex code and picked on three sliders. Reopening the dialog can shift the colour by one unit, since it is edited as hue, saturation and lightness. |
| [`Date(Func<string>, Atom<DateOnly>, Func<DateOnly, string>, Func<string>)`](#date-func-string-atom-dateonly-func-dateonly-string-func-string) | A calendar date. There is no empty date, so the field cannot be cleared. |
| [`MultiChoice(Func<string>, IReadOnlyList<string>, Atom<IReadOnlyList<string>>, Func<IReadOnlyList<string>, string>, Func<string>)`](#multichoice-func-string-ireadonlylist-string-atom-ireadonlylist-string-func-ireadonlylist-string-string-func-string) | Any number of options out of a list. |
| [`Number(Func<string>, Atom<decimal>, decimal, decimal, Func<string>)`](#number-func-string-atom-decimal-decimal-decimal-func-string) | A number that can be typed or stepped. Clearing it puts the value back to the lowest allowed. |
| [`Path(Func<string>, Atom<string>, ViewRoute, bool, Func<string>)`](#path-func-string-atom-string-viewroute-bool-func-string) | A path on disk. Unlike the other fields this leaves the form: the picker is a view of its own, which is why it has to be told where to come back to. |
| [`Secret(Func<string>, Atom<string>, Func<string>)`](#secret-func-string-atom-string-func-string) | A secret, shown as dots both in the form and while it is typed. The atom still holds the text as it is, so treat it as sensitive. |
| [`Slider(Func<string>, Atom<decimal>, decimal, decimal, Func<string>)`](#slider-func-string-atom-decimal-decimal-decimal-func-string) | A number picked on a slider rather than typed, for values where the range matters more than the exact figure. |
| [`Text(Func<string>, Atom<string>, Func<string, string>, Func<string>)`](#text-func-string-atom-string-func-string-string-func-string) | A line of text, edited in a dialog. |
| [`Time(Func<string>, Atom<TimeOnly>, Func<TimeOnly, string>, Func<string>)`](#time-func-string-atom-timeonly-func-timeonly-string-func-string) | A time of day. There is no empty time, so the field cannot be cleared. |
| [`Toggle(Func<string>, Atom<bool>, Func<bool, string>, Func<string>)`](#toggle-func-string-atom-bool-func-bool-string-func-string) | A yes-or-no answer. |

## Constructors in detail

### `Field()` {#field}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public Field();
```

## Properties in detail

### `Activate` {#activate}

```csharp
public Func<ArlecchinoState, ViewRoute>? Activate { get; init; }
```

What confirming the field does, usually opening a dialog. Returning a route navigates; return [`ViewRoute.None`](../arlecchino.navigation/ViewRoute.md#none) to stay put. Without this the field is read-only.

**Type** `Func<T, TResult>`&lt;[`ArlecchinoState`](../arlecchino.state/ArlecchinoState.md), [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt;

### `Help` {#help}

```csharp
public Func<string> Help { get; init; }
```

A line shown under the field while it is selected. Empty means nothing is shown.

**Type** `Func<TResult>`&lt;`string`&gt;

### `IsAction` {#isaction}

```csharp
public bool IsAction { get; init; }
```

Whether the row is a button rather than a value. Buttons have no value to align, so they are not counted when working out the label column.

**Type** `bool`

### `IsEnabled` {#isenabled}

```csharp
public Func<bool> IsEnabled { get; init; }
```

Whether the field can be used. A disabled field is drawn muted and skipped when moving through the form.

**Type** `Func<TResult>`&lt;`bool`&gt;

### `Label` {#label}

```csharp
public Func<string> Label { get; init; }
```

What the field is called.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Reset` {#reset}

```csharp
public Action? Reset { get; init; }
```

Puts the field back to its empty or lowest value. Fields whose type has no sensible empty value, such as a date, leave this out and simply cannot be cleared.

**Type** `Action`

### `Value` {#value}

```csharp
public Func<string> Value { get; init; }
```

The value as the user should see it, which is not always what is stored.

**Type** `Func<TResult>`&lt;`string`&gt;

## Methods in detail

### `Action(Func<string>, Func<ViewRoute>, Func<bool>, Func<string>)` {#action-func-string-func-viewroute-func-bool-func-string}

```csharp
public static Field Action(Func<string> label, Func<ViewRoute> run, Func<bool>? enabled = null, Func<string>? help = null);
```

A button rather than a value, for the things a form does once it has been filled in.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the button says. |
| `run` | `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt; | What pressing it does. Returning a route navigates. |
| `enabled` | `Func<TResult>`&lt;`bool`&gt; | Whether it can be pressed; use it to require the form to be complete. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the button while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Choice(Func<string>, IReadOnlyList<string>, Atom<string>, Func<string>)` {#choice-func-string-ireadonlylist-string-atom-string-func-string}

```csharp
public static Field Choice(Func<string> label, IReadOnlyList<string> options, Atom<string> value, Func<string>? help = null);
```

One option out of a list, chosen in a filterable dialog.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `options` | `IReadOnlyList<T>`&lt;`string`&gt; | Everything that can be chosen. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Color(Func<string>, Atom<Rgb>, Func<string>)` {#color-func-string-atom-rgb-func-string}

```csharp
public static Field Color(Func<string> label, Atom<Rgb> value, Func<string>? help = null);
```

A colour, shown as its hex code and picked on three sliders. Reopening the dialog can shift the colour by one unit, since it is edited as hue, saturation and lightness.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;[`Rgb`](../arlecchino.rendering/Rgb.md)&gt; | The atom to read and write. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Date(Func<string>, Atom<DateOnly>, Func<DateOnly, string>, Func<string>)` {#date-func-string-atom-dateonly-func-dateonly-string-func-string}

```csharp
public static Field Date(Func<string> label, Atom<DateOnly> value, Func<DateOnly, string> render, Func<string>? help = null);
```

A calendar date. There is no empty date, so the field cannot be cleared.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`DateOnly`&gt; | The atom to read and write. |
| `render` | `Func<T, TResult>`&lt;`DateOnly`, `string`&gt; | Formats the date for the form, where a local format usually beats the ISO one. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `MultiChoice(Func<string>, IReadOnlyList<string>, Atom<IReadOnlyList<string>>, Func<IReadOnlyList<string>, string>, Func<string>)` {#multichoice-func-string-ireadonlylist-string-atom-ireadonlylist-string-func-ireadonlylist-string-string-func-string}

```csharp
public static Field MultiChoice(Func<string> label, IReadOnlyList<string> options, Atom<IReadOnlyList<string>> value, Func<IReadOnlyList<string>, string> render, Func<string>? help = null);
```

Any number of options out of a list.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `options` | `IReadOnlyList<T>`&lt;`string`&gt; | Everything that can be chosen. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`IReadOnlyList<T>`&lt;`string`&gt;&gt; | The atom to read and write. |
| `render` | `Func<T, TResult>`&lt;`IReadOnlyList<T>`&lt;`string`&gt;, `string`&gt; | Sums up what is chosen for the one row the form has to show it in. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Number(Func<string>, Atom<decimal>, decimal, decimal, Func<string>)` {#number-func-string-atom-decimal-decimal-decimal-func-string}

```csharp
public static Field Number(Func<string> label, Atom<decimal> value, decimal minimum, decimal maximum, Func<string>? help = null);
```

A number that can be typed or stepped. Clearing it puts the value back to the lowest allowed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`decimal`&gt; | The atom to read and write. |
| `minimum` | `decimal` | Lowest value allowed. |
| `maximum` | `decimal` | Highest value allowed. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Path(Func<string>, Atom<string>, ViewRoute, bool, Func<string>)` {#path-func-string-atom-string-viewroute-bool-func-string}

```csharp
public static Field Path(Func<string> label, Atom<string> value, ViewRoute returnView, bool pickFolder, Func<string>? help = null);
```

A path on disk. Unlike the other fields this leaves the form: the picker is a view of its own, which is why it has to be told where to come back to.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |
| `returnView` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | The view to return to once the picker is done. |
| `pickFolder` | `bool` | Whether a folder is being chosen rather than a file. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Secret(Func<string>, Atom<string>, Func<string>)` {#secret-func-string-atom-string-func-string}

```csharp
public static Field Secret(Func<string> label, Atom<string> value, Func<string>? help = null);
```

A secret, shown as dots both in the form and while it is typed. The atom still holds the text as it is, so treat it as sensitive.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Slider(Func<string>, Atom<decimal>, decimal, decimal, Func<string>)` {#slider-func-string-atom-decimal-decimal-decimal-func-string}

```csharp
public static Field Slider(Func<string> label, Atom<decimal> value, decimal minimum, decimal maximum, Func<string>? help = null);
```

A number picked on a slider rather than typed, for values where the range matters more than the exact figure.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`decimal`&gt; | The atom to read and write. |
| `minimum` | `decimal` | Value at the left end. |
| `maximum` | `decimal` | Value at the right end. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Text(Func<string>, Atom<string>, Func<string, string>, Func<string>)` {#text-func-string-atom-string-func-string-string-func-string}

```csharp
public static Field Text(Func<string> label, Atom<string> value, Func<string, string?>? validate = null, Func<string>? help = null);
```

A line of text, edited in a dialog.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |
| `validate` | `Func<T, TResult>`&lt;`string`, `string`&gt; | Checked when the dialog is confirmed; return a message to keep it open. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Time(Func<string>, Atom<TimeOnly>, Func<TimeOnly, string>, Func<string>)` {#time-func-string-atom-timeonly-func-timeonly-string-func-string}

```csharp
public static Field Time(Func<string> label, Atom<TimeOnly> value, Func<TimeOnly, string> render, Func<string>? help = null);
```

A time of day. There is no empty time, so the field cannot be cleared.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`TimeOnly`&gt; | The atom to read and write. |
| `render` | `Func<T, TResult>`&lt;`TimeOnly`, `string`&gt; | Formats the time for the form. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

### `Toggle(Func<string>, Atom<bool>, Func<bool, string>, Func<string>)` {#toggle-func-string-atom-bool-func-bool-string-func-string}

```csharp
public static Field Toggle(Func<string> label, Atom<bool> value, Func<bool, string> render, Func<string>? help = null);
```

A yes-or-no answer.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`bool`&gt; | The atom to read and write. |
| `render` | `Func<T, TResult>`&lt;`bool`, `string`&gt; | Words for the two answers, since "on" and "off" do not suit every question. |
| `help` | `Func<TResult>`&lt;`string`&gt; | A line shown under the field while it is selected. |

**Returns** [`Field`](../arlecchino.forms/Field.md) — The field.

