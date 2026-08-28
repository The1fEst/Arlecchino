---
title: "HistoryFile"
sidebar_label: "HistoryFile"
---

# HistoryFile class

**Namespace:** `Arlecchino.Atoms.History` &middot; **Assembly:** `Arlecchino.Atoms.History`

Where [`AtomHistory`](../arlecchino.atoms.history/AtomHistory.md) writes its steps, as one JSON array of edits per line in a file beside the program. A run that is killed leaves every complete line readable.

```csharp
public sealed class HistoryFile : IDisposable, IValueWriter, IValueReader
```

**Implements** `IDisposable`, [`IValueWriter`](../arlecchino.atoms.history/IValueWriter.md), [`IValueReader`](../arlecchino.atoms.history/IValueReader.md)

## Constructors

| Member | Summary |
|---|---|
| [`HistoryFile(string, bool)`](#historyfile-string-bool) | Names the file. Nothing is created or removed until [`HistoryFile.Prepare`](../arlecchino.atoms.history/HistoryFile.md#prepare) or the first step. |

## Fields

| Member | Summary |
|---|---|
| [`DefaultName`](#defaultname) | What the file is called when the application did not name one. |

## Properties

| Member | Summary |
|---|---|
| [`Count`](#count) | How many steps the file holds, counting the ones an earlier run left. |
| [`Exists`](#exists) | Whether anything has been written, which is also whether the file is there. |
| [`Keep`](#keep) | Whether the file outlives the run rather than being removed on the way out. |
| [`Path`](#path) | Where the file is, whether it exists yet or not. |

## Methods

| Member | Summary |
|---|---|
| [`Discard()`](#discard) | Closes and removes the file, whether the application asked to keep it or not. |
| [`Dispose()`](#dispose) | Closes the file, and removes it unless the application asked to keep it. |
| [`Handles<T>(Action<Utf8JsonWriter, T>, Func<JsonElement, T>)`](#handles-t-action-utf8jsonwriter-t-func-jsonelement-t) | Says how a kind of value is written and read, once for the kind rather than once for every atom holding it. Saying it twice replaces what was said the first time. |
| [`Prepare()`](#prepare) | Makes the file ready for a run. A scratch file is thrown away, since a killed run left it; a file the application asked to keep is indexed instead, so its steps can be read back. |
| [`Read(int)`](#read-int) | Reads one step back, seeking to it rather than holding the file in memory. The atoms it names are the ones running now, found by their [`HistoryAtom.Key`](../arlecchino.atoms.history/HistoryAtom-1.md#key). |
| [`Write(IReadOnlyList<IAtomEdit>)`](#write-ireadonlylist-iatomedit) | Appends one step. The file is opened by the first step that can be written, so a run that recorded nothing leaves no file. |

## Constructors in detail

### `HistoryFile(string, bool)` {#historyfile-string-bool}

```csharp
public HistoryFile(string? name = null, bool keep = false);
```

Names the file. Nothing is created or removed until [`HistoryFile.Prepare`](../arlecchino.atoms.history/HistoryFile.md#prepare) or the first step.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `name` | `string` | File name, placed beside the program. A name with a directory in it is used as it stands, for an application that keeps its history somewhere the program folder cannot be written to. |
| `keep` | `bool` | Whether the file outlives the run. The default removes it on the way out, which makes it a scratch file that only matters while the program is up. |

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

### `Keep` {#keep}

```csharp
public bool Keep { get; }
```

Whether the file outlives the run rather than being removed on the way out.

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

Closes and removes the file, whether the application asked to keep it or not.

### `Dispose()` {#dispose}

```csharp
public void Dispose();
```

Closes the file, and removes it unless the application asked to keep it.

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

Makes the file ready for a run. A scratch file is thrown away, since a killed run left it; a file the application asked to keep is indexed instead, so its steps can be read back.

### `Read(int)` {#read-int}

```csharp
public List<IAtomEdit>? Read(int step);
```

Reads one step back, seeking to it rather than holding the file in memory. The atoms it names are the ones running now, found by their [`HistoryAtom.Key`](../arlecchino.atoms.history/HistoryAtom-1.md#key).

**Parameters**

| Name | Type | Description |
|---|---|---|
| `step` | `int` | Which step, counting from the oldest the file holds. |

**Returns** `List<T>`&lt;[`IAtomEdit`](../arlecchino.atoms.history/IAtomEdit.md)&gt; — The edits, or `null` when the step is unreadable or names an atom nothing answers to.

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

