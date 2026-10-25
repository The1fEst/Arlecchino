---
title: "HistoryFile"
sidebar_label: "HistoryFile"
---

# HistoryFile class

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

Where [`AtomHistory`](../arlecchino.atoms.history/AtomHistory.md) writes its steps, as one JSON array of edits per line in a file beside the program. A run that is killed leaves every complete line readable.

```csharp
public sealed class HistoryFile : IDisposable
```

**Implements** `IDisposable`

## Constructors

| Member | Summary |
|---|---|
| [`HistoryFile(string)`](#historyfile-string) | Names the file. Nothing is created or removed until [`HistoryFile.Prepare`](../arlecchino.atoms.history/HistoryFile.md#prepare) or the first step. |

## Fields

| Member | Summary |
|---|---|
| [`DefaultName`](#defaultname) | What the file is called when the application did not name one. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many steps the file holds, counting the ones an earlier run left. |
| [`Exists`](#exists) | Whether anything has been written, which is also whether the file is there. |
| [`Path`](#path) | Where the file is, whether it exists yet or not. |

## Methods

| Member | Summary |
|---|---|
| [`Discard()`](#discard) | Closes and removes the file, losing every step an earlier run wrote down. |
| [`Dispose()`](#dispose) | Closes the file, leaving it where it is for the next run to read. |
| [`Handles<T>(Action<Utf8JsonWriter, T>, Func<JsonElement, T>)`](#handles-t-action-utf8jsonwriter-t-func-jsonelement-t) | Says how a kind of value is written and read, once for the kind rather than once for every atom holding it. Saying it twice replaces what was said the first time. |
| [`Prepare()`](#prepare) | Makes the file ready for a run: what an earlier one left is indexed rather than thrown away, so its steps can be read back and the atoms it names can come up holding what it last saw them hold. |
| [`Read(int)`](#read-int) | Reads one step back, seeking to it rather than holding the file in memory. The atoms it names are the ones running now, found by the names they were given. |
| [`Restore(string, IRestorableAtom)`](#restore-string-irestorableatom) | Puts one atom back to what the file last saw it hold. It does not become a step, since what is put back is what the file already says. |
| [`Write(IReadOnlyList<IAtomEdit>)`](#write-ireadonlylist-iatomedit) | Appends one step. The file is opened by the first step that can be written, so a run that recorded nothing leaves no file. |

## Constructors in detail

### `HistoryFile(string)` {#historyfile-string}

```csharp
public HistoryFile(string? name = null);
```

Names the file. Nothing is created or removed until [`HistoryFile.Prepare`](../arlecchino.atoms.history/HistoryFile.md#prepare) or the first step.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `name` | `string` | File name, placed beside the program. A name with a directory in it is used as it stands, for an application that keeps its history somewhere the program folder cannot be written to. |

## Fields in detail

### `DefaultName` {#defaultname}

```csharp
public static string DefaultName { get; }
```

What the file is called when the application did not name one.

**Type** `string`

## Properties in detail

### `Count` {#count}

```csharp
public int Count { get; }
```

How many steps the file holds, counting the ones an earlier run left.

**Type** `int`

### `Exists` {#exists}

```csharp
public bool Exists { get; }
```

Whether anything has been written, which is also whether the file is there.

**Type** `bool`

### `Path` {#path}

```csharp
public string Path { get; }
```

Where the file is, whether it exists yet or not.

**Type** `string`

## Methods in detail

### `Discard()` {#discard}

```csharp
public void Discard();
```

Closes and removes the file, losing every step an earlier run wrote down.

### `Dispose()` {#dispose}

```csharp
public void Dispose();
```

Closes the file, leaving it where it is for the next run to read.

### `Handles<T>(Action<Utf8JsonWriter, T>, Func<JsonElement, T>)` {#handles-t-action-utf8jsonwriter-t-func-jsonelement-t}

```csharp
public void Handles<T>(Action<Utf8JsonWriter, T> write, Func<JsonElement, T> read);
```

Says how a kind of value is written and read, once for the kind rather than once for every atom holding it. Saying it twice replaces what was said the first time.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `write` | `Action<T1, T2>`&lt;`Utf8JsonWriter`, `T`&gt; | Writes one value as a single JSON value. |
| `read` | `Func<T, TResult>`&lt;`JsonElement`, `T`&gt; | Reads one value back from what was written. |

### `Prepare()` {#prepare}

```csharp
public void Prepare();
```

Makes the file ready for a run: what an earlier one left is indexed rather than thrown away, so its steps can be read back and the atoms it names can come up holding what it last saw them hold.

### `Read(int)` {#read-int}

```csharp
public List<IAtomEdit>? Read(int step);
```

Reads one step back, seeking to it rather than holding the file in memory. The atoms it names are the ones running now, found by the names they were given.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `step` | `int` | Which step, counting from the oldest the file holds. |

**Returns** `List<T>`&lt;[`IAtomEdit`](../arlecchino.atoms.history/IAtomEdit.md)&gt; — The edits, or `null` when the step is unreadable or names an atom nothing answers to.

### `Restore(string, IRestorableAtom)` {#restore-string-irestorableatom}

```csharp
public bool Restore(string key, IRestorableAtom atom);
```

Puts one atom back to what the file last saw it hold. It does not become a step, since what is put back is what the file already says.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `string` | What the atom is called. |
| `atom` | [`IRestorableAtom`](../arlecchino.atoms.history/IRestorableAtom.md) | The atom answering to that name. |

**Returns** `bool` — `false` when no step names it, or the one that does cannot be read.

### `Write(IReadOnlyList<IAtomEdit>)` {#write-ireadonlylist-iatomedit}

```csharp
public bool Write(IReadOnlyList<IAtomEdit> edits);
```

Appends one step. The file is opened by the first step that can be written, so a run that recorded nothing leaves no file.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `edits` | `IReadOnlyList<T>`&lt;[`IAtomEdit`](../arlecchino.atoms.history/IAtomEdit.md)&gt; | The edits of the step, in the order they were made. |

**Returns** `bool` — `true` when the step was written, `false` when it was skipped.

