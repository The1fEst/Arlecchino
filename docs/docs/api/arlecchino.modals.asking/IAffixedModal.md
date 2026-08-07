---
title: "IAffixedModal"
sidebar_label: "IAffixedModal"
---

# IAffixedModal interface

**Namespace:** `Arlecchino.Modals.Asking` &middot; **Assembly:** `Arlecchino`

A field that shows something around its value — a currency sign, a unit. Affixes are decoration only: the callback still receives the bare value.

```csharp
public interface IAffixedModal
```

**Implemented by** [`ITextEntryModal`](../arlecchino.modals.asking/ITextEntryModal.md), [`NumberModal`](../arlecchino.modals.asking/NumberModal.md), [`NumericModal`](../arlecchino.modals.asking/NumericModal.md), [`TextModal`](../arlecchino.modals.asking/TextModal.md), [`SliderModal`](../arlecchino.modals.setting/SliderModal.md)

## Properties

| Member | Summary |
|---|---|
| [`Prefix`](#prefix) | Drawn before the value, such as `$`. |
| [`Suffix`](#suffix) | Drawn after the value, such as `%`. |

## Properties in detail

### `Prefix` {#prefix}

```csharp
public string Prefix { get; }
```

Drawn before the value, such as `$`.

**Type** `string`

### `Suffix` {#suffix}

```csharp
public string Suffix { get; }
```

Drawn after the value, such as `%`.

**Type** `string`

