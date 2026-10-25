---
title: "SecretField"
sidebar_label: "SecretField"
---

# SecretField class

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

A line of text that is never shown or typed in the clear. Nothing about a string says it is a secret, so the row says it instead; the atom still holds the text as it is.

```csharp
public sealed class SecretField : Field<string>, IField
```

**Inherits from** [`Field`](../arlecchino.forms/Field-1.md)&lt;`string`&gt;  
**Implements** [`IField`](../arlecchino.forms/IField.md)

## Constructors

| Member | Summary |
|---|---|
| [`SecretField(Atom<string>)`](#secretfield-atom-string) | Creates the row. |

## Constructors in detail

### `SecretField(Atom<string>)` {#secretfield-atom-string}

```csharp
public SecretField(Atom<string> value);
```

Creates the row.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |

