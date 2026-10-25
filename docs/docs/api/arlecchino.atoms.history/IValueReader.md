---
title: "IValueReader"
sidebar_label: "IValueReader"
---

# IValueReader interface

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

Where an atom takes back the two values a step stood between. The atom calls this, since it is the one place the kind of value is known without reflection.

```csharp
public interface IValueReader
```

## Methods

| Member | Summary |
|---|---|
| [`Read<T>(out T, out T)`](#read-t-out-t-out-t) | Reads both values of one step. |
| [`ReadEntries<TKey, TValue>(out KeyValuePair<TKey, TValue>[], out KeyValuePair<TKey, TValue>[])`](#readentries-tkey-tvalue-out-keyvaluepair-tkey-tvalue-out-keyvaluepair-tkey-tvalue) | Reads what a map held either side of one step. |
| [`ReadItems<T>(out T[], out T[])`](#readitems-t-out-t-out-t) | Reads what a collection held either side of one step. |

## Methods in detail

### `Read<T>(out T, out T)` {#read-t-out-t-out-t}

```csharp
public bool Read<T>(out T undone, out T redone);
```

Reads both values of one step.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `undone` | `T` | The value undoing puts back. |
| `redone` | `T` | The value redoing puts back. |

**Returns** `bool` — `false` when this kind of value has no reader, or the step does not parse.

### `ReadEntries<TKey, TValue>(out KeyValuePair<TKey, TValue>[], out KeyValuePair<TKey, TValue>[])` {#readentries-tkey-tvalue-out-keyvaluepair-tkey-tvalue-out-keyvaluepair-tkey-tvalue}

```csharp
public bool ReadEntries<TKey, TValue>(
    out KeyValuePair<TKey, TValue>[] undone,
    out KeyValuePair<TKey, TValue>[] redone);
```

Reads what a map held either side of one step.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `undone` | `KeyValuePair<TKey, TValue>`&lt;`TKey`, `TValue`&gt;\[\] | The entries undoing lands on. |
| `redone` | `KeyValuePair<TKey, TValue>`&lt;`TKey`, `TValue`&gt;\[\] | The entries redoing lands on. |

**Returns** `bool` — `false` when either half of an entry has no reader, or the step does not parse.

### `ReadItems<T>(out T[], out T[])` {#readitems-t-out-t-out-t}

```csharp
public bool ReadItems<T>(out T[] undone, out T[] redone);
```

Reads what a collection held either side of one step.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `undone` | `T`\[\] | The contents undoing lands on. |
| `redone` | `T`\[\] | The contents redoing lands on. |

**Returns** `bool` — `false` when this kind of item has no reader, or the step does not parse.

