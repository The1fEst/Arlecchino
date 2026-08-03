---
title: NumericModal
sidebar_label: NumericModal
---

# NumericModal class

**Namespace:** `Arlecchino.Modals.Asking` &middot; **Assembly:** `Arlecchino`

What the number field and the slider have in common: stepping, precision and affixes.

```csharp
public abstract class NumericModal : Modal, IAffixedModal
```

**Inherits from** [`Modal`](../arlecchino.modals/Modal.md)  
**Implements** [`IAffixedModal`](../arlecchino.modals.asking/IAffixedModal.md)  
**Derived types** [`NumberModal`](../arlecchino.modals.asking/NumberModal.md), [`SliderModal`](../arlecchino.modals.setting/SliderModal.md)

## Constructors

| Member | Summary |
|---|---|
| [`NumericModal()`](#numericmodal) |  |

## Properties

| Member | Summary |
|---|---|
| [`Decimals`](#decimals) | Digits kept after the separator. Zero also means a decimal separator cannot be typed at all. |
| [`LargeStep`](#largestep) | How far the page keys move the value. |
| [`Prefix`](#prefix) | Drawn before the value. |
| [`Step`](#step) | How far the arrow keys move the value. |
| [`Suffix`](#suffix) | Drawn after the value. |

## Methods

| Member | Summary |
|---|---|
| [`Display(decimal)`](#display-decimal) | Formats a value the way the user sees it, affixes included. |
| [`FormatNumber(decimal)`](#formatnumber-decimal) | Formats a value with the configured precision, culture-independently. |

## Constructors in detail

### `NumericModal()` {#numericmodal}

:::warning[Obsolete]

Constructors of types with required members are not supported in this version of your compiler.

:::

```csharp
public NumericModal();
```

## Properties in detail

### `Decimals` {#decimals}

```csharp
public int Decimals { get; init; }
```

Digits kept after the separator. Zero also means a decimal separator cannot be typed at all.

**Type** `int`

### `LargeStep` {#largestep}

```csharp
public decimal LargeStep { get; init; }
```

How far the page keys move the value.

**Type** `decimal`

### `Prefix` {#prefix}

```csharp
public string Prefix { get; init; }
```

Drawn before the value.

**Type** `string`

### `Step` {#step}

```csharp
public decimal Step { get; init; }
```

How far the arrow keys move the value.

**Type** `decimal`

### `Suffix` {#suffix}

```csharp
public string Suffix { get; init; }
```

Drawn after the value.

**Type** `string`

## Methods in detail

### `Display(decimal)` {#display-decimal}

```csharp
public string Display(decimal value);
```

Formats a value the way the user sees it, affixes included.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | `decimal` | The value to format. |

**Returns** `string` — The number as text, with affixes.

### `FormatNumber(decimal)` {#formatnumber-decimal}

```csharp
public string FormatNumber(decimal value);
```

Formats a value with the configured precision, culture-independently.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | `decimal` | The value to format. |

**Returns** `string` — The number as text, without affixes.

