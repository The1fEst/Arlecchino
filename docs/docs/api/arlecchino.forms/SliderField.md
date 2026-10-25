---
title: "SliderField"
sidebar_label: "SliderField"
---

# SliderField class

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

A number moved along a track rather than typed. Both kinds of number row are over the same `decimal`, so the row says which it is.

```csharp
public sealed class SliderField : Field<decimal>, IField
```

**Inherits from** [`Field`](../arlecchino.forms/Field-1.md)&lt;`decimal`&gt;  
**Implements** [`IField`](../arlecchino.forms/IField.md)

## Constructors

| Member | Summary |
|---|---|
| [`SliderField(Atom<decimal>)`](#sliderfield-atom-decimal) | Creates the row. |

## Constructors in detail

### `SliderField(Atom<decimal>)` {#sliderfield-atom-decimal}

```csharp
public SliderField(Atom<decimal> value);
```

Creates the row.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`decimal`&gt; | The atom to read and write. |

