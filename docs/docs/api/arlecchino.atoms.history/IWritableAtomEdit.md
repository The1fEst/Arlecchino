---
title: "IWritableAtomEdit"
sidebar_label: "IWritableAtomEdit"
---

# IWritableAtomEdit interface

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

An edit a [`HistoryFile`](../arlecchino.atoms.history/HistoryFile.md) can keep for longer than the run that made it. An edit of an atom that was never named cannot be written, and says so from [`IWritableAtomEdit.WriteTo`](../arlecchino.atoms.history/IWritableAtomEdit.md#writeto-ivaluewriter).

```csharp
public interface IWritableAtomEdit : IAtomEdit
```

**Implements** [`IAtomEdit`](../arlecchino.atoms.history/IAtomEdit.md)

## Properties

| Member | Summary |
|---|---|
| [`Key`](#key) | What the atom is called in the file. Read only once [`IWritableAtomEdit.WriteTo`](../arlecchino.atoms.history/IWritableAtomEdit.md#writeto-ivaluewriter) has agreed. |

## Methods

| Member | Summary |
|---|---|
| [`WriteTo(IValueWriter)`](#writeto-ivaluewriter) | Hands both values over, typed. |

## Properties in detail

### `Key` {#key}

```csharp
public string Key { get; }
```

What the atom is called in the file. Read only once [`IWritableAtomEdit.WriteTo`](../arlecchino.atoms.history/IWritableAtomEdit.md#writeto-ivaluewriter) has agreed.

**Type** `string`

## Methods in detail

### `WriteTo(IValueWriter)` {#writeto-ivaluewriter}

```csharp
public bool WriteTo(IValueWriter writer);
```

Hands both values over, typed.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `writer` | [`IValueWriter`](../arlecchino.atoms.history/IValueWriter.md) | Where the values go. |

**Returns** `bool` — `false` when the writer could not take them.

