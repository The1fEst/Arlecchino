---
title: "IValueWriter"
sidebar_label: "IValueWriter"
---

# IValueWriter interface

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

Where an edit hands over the two values it stands between. The edit calls this, since it is the one place the kind of value is known without reflection.

```csharp
public interface IValueWriter
```

## Methods

| Member | Summary |
|---|---|
| [`Write<T>(T, T)`](#write-t-t-t) | Takes both values of one edit. |
| [`WriteEntries<TKey, TValue>(KeyValuePair<TKey, TValue>[], KeyValuePair<TKey, TValue>[])`](#writeentries-tkey-tvalue-keyvaluepair-tkey-tvalue-keyvaluepair-tkey-tvalue) | Takes what a map held either side of one edit, in full, for the same reason. |
| [`WriteItems<T>(T[], T[])`](#writeitems-t-t-t) | Takes what a collection held either side of one edit, in full rather than as the difference it made. A later run holds something else, and only the contents say where the step lands. |

## Methods in detail

### `Write<T>(T, T)` {#write-t-t-t}

```csharp
public bool Write<T>(T undone, T redone);
```

Takes both values of one edit.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `undone` | `T` | The value undoing puts back. |
| `redone` | `T` | The value redoing puts back. |

**Returns** `bool` — `false` when this kind of value cannot be written, which abandons the whole step.

### `WriteEntries<TKey, TValue>(KeyValuePair<TKey, TValue>[], KeyValuePair<TKey, TValue>[])` {#writeentries-tkey-tvalue-keyvaluepair-tkey-tvalue-keyvaluepair-tkey-tvalue}

```csharp
public bool WriteEntries<TKey, TValue>(
    KeyValuePair<TKey, TValue>[] undone,
    KeyValuePair<TKey, TValue>[] redone);
```

Takes what a map held either side of one edit, in full, for the same reason.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `undone` | `KeyValuePair<TKey, TValue>`&lt;`TKey`, `TValue`&gt;\[\] | The entries undoing lands on. |
| `redone` | `KeyValuePair<TKey, TValue>`&lt;`TKey`, `TValue`&gt;\[\] | The entries redoing lands on. |

**Returns** `bool` — `false` when either half of an entry cannot be written, which abandons the whole step.

### `WriteItems<T>(T[], T[])` {#writeitems-t-t-t}

```csharp
public bool WriteItems<T>(T[] undone, T[] redone);
```

Takes what a collection held either side of one edit, in full rather than as the difference it made. A later run holds something else, and only the contents say where the step lands.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `undone` | `T`\[\] | The contents undoing lands on. |
| `redone` | `T`\[\] | The contents redoing lands on. |

**Returns** `bool` — `false` when this kind of item cannot be written, which abandons the whole step.

