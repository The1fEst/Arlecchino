---
title: "PathField"
sidebar_label: "PathField"
---

# PathField class

**Namespace:** `Arlecchino.Forms` &middot; **Assembly:** `Arlecchino`

A path on disk. It leaves the form rather than opening a dialog over it, since the picker is a view of its own, and so it has to be told where to come back to.

```csharp
public sealed class PathField : Field<string>, IField
```

**Inherits from** [`Field`](../arlecchino.forms/Field-1.md)&lt;`string`&gt;  
**Implements** [`IField`](../arlecchino.forms/IField.md)

## Constructors

| Member | Summary |
|---|---|
| [`PathField(Atom<string>, ViewRoute, bool)`](#pathfield-atom-string-viewroute-bool) | Creates the row. |

## Properties

| Member | Summary |
|---|---|
| [`PickFolder`](#pickfolder) | Whether a folder is being chosen rather than a file. |
| [`ReturnView`](#returnview) | The view to return to once the picker is done. |
| [`Start`](#start) | Where the picker opens while the row is still empty, asked for as it opens rather than as the form is built. A row already holding a path opens there instead. |

## Methods

| Member | Summary |
|---|---|
| [`WithStart(Func<string>)`](#withstart-func-string) | Says where the picker opens while the row is still empty. |

## Constructors in detail

### `PathField(Atom<string>, ViewRoute, bool)` {#pathfield-atom-string-viewroute-bool}

```csharp
public PathField(Atom<string> value, ViewRoute returnView, bool pickFolder);
```

Creates the row.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `value` | [`Atom`](../arlecchino.atoms/Atom-1.md)&lt;`string`&gt; | The atom to read and write. |
| `returnView` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | The view to return to once the picker is done. |
| `pickFolder` | `bool` | Whether a folder is being chosen rather than a file. |

## Properties in detail

### `PickFolder` {#pickfolder}

```csharp
public bool PickFolder { get; }
```

Whether a folder is being chosen rather than a file.

**Type** `bool`

### `ReturnView` {#returnview}

```csharp
public ViewRoute ReturnView { get; }
```

The view to return to once the picker is done.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

### `Start` {#start}

```csharp
public Func<string>? Start { get; }
```

Where the picker opens while the row is still empty, asked for as it opens rather than as the form is built. A row already holding a path opens there instead.

**Type** `Func<TResult>`&lt;`string`&gt;

## Methods in detail

### `WithStart(Func<string>)` {#withstart-func-string}

```csharp
public PathField WithStart(Func<string> start);
```

Says where the picker opens while the row is still empty.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `start` | `Func<TResult>`&lt;`string`&gt; | The folder to open on. |

**Returns** [`PathField`](../arlecchino.forms/PathField.md) — The same row, to go on building it.

