---
title: "Fields"
sidebar_label: "Fields"
---

# Fields class

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

The rows a form is built out of. Each one gives the row the shape [`Form`](../arlecchino.forms/Form.md) reads it by, so an application says what a row is over and never says which dialog to open.

```csharp
public static class Fields
```

## Methods

| Member | Summary |
|---|---|
| [`Action(Func<string>, Func<ViewRoute>)`](#action-func-string-func-viewroute) | A button rather than a value, for the things a form does once it has been filled in. |
| [`Choice(Func<string>, IReadOnlyList<string>, Atom<string>)`](#choice-func-string-ireadonlylist-string-atom-string) | One option out of a list, chosen in a filterable dialog. |
| [`Color(Func<string>, Atom<RGB>)`](#color-func-string-atom-rgb) | A color, shown as its hex code and picked on three sliders. Reopening the dialog can shift the color by one unit, since it is edited as hue, saturation and lightness. |
| [`Date(Func<string>, Atom<DateOnly>)`](#date-func-string-atom-dateonly) | A calendar date. |
| [`MultiChoice(Func<string>, IReadOnlyList<string>, AtomsList<string>)`](#multichoice-func-string-ireadonlylist-string-atomslist-string) | Any number of options out of a list, held in a list atom. |
| [`Number(Func<string>, Atom<decimal>)`](#number-func-string-atom-decimal) | A number that can be typed or stepped. Without [`Field.Items`](../arlecchino.forms/Field-1.md#items) it takes any number at all; with them, it is held between the lowest and the highest. |
| [`Path(Func<string>, Atom<string>, ViewRoute, bool)`](#path-func-string-atom-string-viewroute-bool) | A path on disk. It leaves the form, since the picker is a view of its own, and so it has to be told where to come back to. |
| [`Secret(Func<string>, Atom<string>)`](#secret-func-string-atom-string) | A secret, shown as dots both in the form and while it is typed. The atom still holds the text as it is, so treat it as sensitive. |
| [`Slider(Func<string>, Atom<decimal>)`](#slider-func-string-atom-decimal) | A number picked on a slider rather than typed, for values where the range matters more than the exact figure. The ends of the track are the lowest and the highest of [`Field.Items`](../arlecchino.forms/Field-1.md#items), or 0 and 100 without them. |
| [`Text(Func<string>, Atom<string>)`](#text-func-string-atom-string) | A line of text, edited in a dialog. |
| [`Time(Func<string>, Atom<TimeOnly>)`](#time-func-string-atom-timeonly) | A time of day. |
| [`Toggle(Func<string>, Atom<bool>)`](#toggle-func-string-atom-bool) | A yes-or-no answer, worded by the application's strings. |

## Methods in detail

### `Action(Func<string>, Func<ViewRoute>)` {#action-func-string-func-viewroute}

```csharp
public static ActionField Action(Func<string> label, Func<ViewRoute> run);
```

A button rather than a value, for the things a form does once it has been filled in.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the button says. |
| `run` | `Func<TResult>`&lt;[`ViewRoute`](../arlecchino.navigation/ViewRoute.md)&gt; | What pressing it does. Returning a route navigates. |

**Returns** [`ActionField`](../arlecchino.forms/ActionField.md) — The field.

### `Choice(Func<string>, IReadOnlyList<string>, Atom<string>)` {#choice-func-string-ireadonlylist-string-atom-string}

```csharp
public static Field<string> Choice(
    Func<string> label,
    IReadOnlyList<string> options,
    Atom<string> value);
```

One option out of a list, chosen in a filterable dialog.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `options` | `IReadOnlyList<T>`&lt;`string`&gt; | Everything that can be chosen. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`string`&gt; — The field.

### `Color(Func<string>, Atom<RGB>)` {#color-func-string-atom-rgb}

```csharp
public static Field<RGB> Color(Func<string> label, Atom<RGB> value);
```

A color, shown as its hex code and picked on three sliders. Reopening the dialog can shift the color by one unit, since it is edited as hue, saturation and lightness.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;[`RGB`](../arlecchino.rendering.colors/RGB.md)&gt; | The atom to read and write. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;[`RGB`](../arlecchino.rendering.colors/RGB.md)&gt; — The field.

### `Date(Func<string>, Atom<DateOnly>)` {#date-func-string-atom-dateonly}

```csharp
public static Field<DateOnly> Date(Func<string> label, Atom<DateOnly> value);
```

A calendar date.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`DateOnly`&gt; | The atom to read and write. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`DateOnly`&gt; — The field.

### `MultiChoice(Func<string>, IReadOnlyList<string>, AtomsList<string>)` {#multichoice-func-string-ireadonlylist-string-atomslist-string}

```csharp
public static Field<string> MultiChoice(
    Func<string> label,
    IReadOnlyList<string> options,
    AtomsList<string> value);
```

Any number of options out of a list, held in a list atom.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `options` | `IReadOnlyList<T>`&lt;`string`&gt; | Everything that can be chosen. |
| `value` | [`AtomsList`](../arlecchino.atoms.collections/AtomsList-1.md)&lt;`string`&gt; | The list to read and write. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`string`&gt; — The field.

### `Number(Func<string>, Atom<decimal>)` {#number-func-string-atom-decimal}

```csharp
public static Field<decimal> Number(Func<string> label, Atom<decimal> value);
```

A number that can be typed or stepped. Without [`Field.Items`](../arlecchino.forms/Field-1.md#items) it takes any number at all; with them, it is held between the lowest and the highest.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`decimal`&gt; | The atom to read and write. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`decimal`&gt; — The field.

### `Path(Func<string>, Atom<string>, ViewRoute, bool)` {#path-func-string-atom-string-viewroute-bool}

```csharp
public static PathField Path(
    Func<string> label,
    Atom<string> value,
    ViewRoute returnView,
    bool pickFolder);
```

A path on disk. It leaves the form, since the picker is a view of its own, and so it has to be told where to come back to.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |
| `returnView` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | The view to return to once the picker is done. |
| `pickFolder` | `bool` | Whether a folder is being chosen rather than a file. |

**Returns** [`PathField`](../arlecchino.forms/PathField.md) — The field.

### `Secret(Func<string>, Atom<string>)` {#secret-func-string-atom-string}

```csharp
public static SecretField Secret(Func<string> label, Atom<string> value);
```

A secret, shown as dots both in the form and while it is typed. The atom still holds the text as it is, so treat it as sensitive.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |

**Returns** [`SecretField`](../arlecchino.forms/SecretField.md) — The field.

### `Slider(Func<string>, Atom<decimal>)` {#slider-func-string-atom-decimal}

```csharp
public static SliderField Slider(Func<string> label, Atom<decimal> value);
```

A number picked on a slider rather than typed, for values where the range matters more than the exact figure. The ends of the track are the lowest and the highest of [`Field.Items`](../arlecchino.forms/Field-1.md#items), or 0 and 100 without them.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`decimal`&gt; | The atom to read and write. |

**Returns** [`SliderField`](../arlecchino.forms/SliderField.md) — The field.

### `Text(Func<string>, Atom<string>)` {#text-func-string-atom-string}

```csharp
public static Field<string> Text(Func<string> label, Atom<string> value);
```

A line of text, edited in a dialog.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`string`&gt; — The field.

### `Time(Func<string>, Atom<TimeOnly>)` {#time-func-string-atom-timeonly}

```csharp
public static Field<TimeOnly> Time(Func<string> label, Atom<TimeOnly> value);
```

A time of day.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`TimeOnly`&gt; | The atom to read and write. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`TimeOnly`&gt; — The field.

### `Toggle(Func<string>, Atom<bool>)` {#toggle-func-string-atom-bool}

```csharp
public static Field<bool> Toggle(Func<string> label, Atom<bool> value);
```

A yes-or-no answer, worded by the application's strings.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `label` | `Func<TResult>`&lt;`string`&gt; | What the field is called. |
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`bool`&gt; | The atom to read and write. |

**Returns** [`Field`](../arlecchino.forms/Field-1.md)&lt;`bool`&gt; — The field.

