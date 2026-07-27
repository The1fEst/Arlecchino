---
title: FilePickerRequest
sidebar_label: FilePickerRequest
---

# FilePickerRequest class

**Namespace:** `Arlecchino.State` &middot; **Assembly:** `Arlecchino`

Everything the file picker needs for one round of picking. Unlike the modals, the picker is a view of its own, so the request also carries where to go once it is done.

```csharp
public sealed class FilePickerRequest : IEquatable<FilePickerRequest>
```

**Implements** `IEquatable<T>`&lt;[`FilePickerRequest`](../arlecchino.state/FilePickerRequest.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`FilePickerRequest(string, bool, string, ViewRoute, Action<string>)`](#filepickerrequest-string-bool-string-viewroute-action-string) | Everything the file picker needs for one round of picking. Unlike the modals, the picker is a view of its own, so the request also carries where to go once it is done. |

## Properties

| Member | Summary |
|---|---|
| [`FileFilter`](#filefilter) | Decides which files are worth showing, by full path. Folders are always listed, since they have to be walked through to reach anything. |
| [`InitialPath`](#initialpath) | Where browsing starts. |
| [`OnPicked`](#onpicked) | Called with the full path that was chosen. |
| [`PickFolder`](#pickfolder) | Whether a folder is being chosen rather than a file. |
| [`Places`](#places) | Shortcuts offered in the sidebar. |
| [`ReturnView`](#returnview) | The view to return to, whether or not anything was picked. |
| [`Title`](#title) | Heading shown above the listing. |

## Methods

| Member | Summary |
|---|---|
| [`<Clone>$()`](#clone) |  |
| [`Deconstruct(String&, Boolean&, String&, ViewRoute&, Action)`](#deconstruct-string-boolean-string-viewroute-action) |  |
| [`Equals(object)`](#equals-object) |  |
| [`Equals(FilePickerRequest)`](#equals-filepickerrequest) |  |
| [`GetHashCode()`](#gethashcode) |  |
| [`ToString()`](#tostring) |  |

## Operators

| Member | Summary |
|---|---|
| [`operator Inequality(FilePickerRequest, FilePickerRequest)`](#operator-inequality-filepickerrequest-filepickerrequest) |  |
| [`operator Equality(FilePickerRequest, FilePickerRequest)`](#operator-equality-filepickerrequest-filepickerrequest) |  |

## Constructors in detail

### `FilePickerRequest(string, bool, string, ViewRoute, Action<string>)` {#filepickerrequest-string-bool-string-viewroute-action-string}

```csharp
public FilePickerRequest(string Title, bool PickFolder, string InitialPath, ViewRoute ReturnView, Action<string> OnPicked);
```

Everything the file picker needs for one round of picking. Unlike the modals, the picker is a view of its own, so the request also carries where to go once it is done.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Title` | `string` | Heading shown above the listing. |
| `PickFolder` | `bool` | Whether a folder is being chosen rather than a file. |
| `InitialPath` | `string` | Where browsing starts. |
| `ReturnView` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) | The view to return to, whether or not anything was picked. |
| `OnPicked` | `Action<T>`&lt;`string`&gt; | Called with the full path that was chosen. |

## Properties in detail

### `FileFilter` {#filefilter}

```csharp
public Func<string, bool>? FileFilter { get; init; }
```

Decides which files are worth showing, by full path. Folders are always listed, since they have to be walked through to reach anything.

**Type** `Func<T, TResult>`&lt;`string`, `bool`&gt;

### `InitialPath` {#initialpath}

```csharp
public string InitialPath { get; init; }
```

Where browsing starts.

**Type** `string`

### `OnPicked` {#onpicked}

```csharp
public Action<string> OnPicked { get; init; }
```

Called with the full path that was chosen.

**Type** `Action<T>`&lt;`string`&gt;

### `PickFolder` {#pickfolder}

```csharp
public bool PickFolder { get; init; }
```

Whether a folder is being chosen rather than a file.

**Type** `bool`

### `Places` {#places}

```csharp
public IReadOnlyList<FilePickerPlace> Places { get; init; }
```

Shortcuts offered in the sidebar.

**Type** `IReadOnlyList<T>`&lt;[`FilePickerPlace`](../arlecchino.state/FilePickerPlace.md)&gt;

### `ReturnView` {#returnview}

```csharp
public ViewRoute ReturnView { get; init; }
```

The view to return to, whether or not anything was picked.

**Type** [`ViewRoute`](../arlecchino.navigation/ViewRoute.md)

### `Title` {#title}

```csharp
public string Title { get; init; }
```

Heading shown above the listing.

**Type** `string`

## Methods in detail

### `<Clone>$()` {#clone}

```csharp
public FilePickerRequest <Clone>$();
```

**Returns** [`FilePickerRequest`](../arlecchino.state/FilePickerRequest.md)

### `Deconstruct(String&, Boolean&, String&, ViewRoute&, Action)` {#deconstruct-string-boolean-string-viewroute-action}

```csharp
public void Deconstruct(out string Title, out bool PickFolder, out string InitialPath, out ViewRoute ReturnView, out Action<string> OnPicked);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Title` | `string` |  |
| `PickFolder` | `bool` |  |
| `InitialPath` | `string` |  |
| `ReturnView` | [`ViewRoute`](../arlecchino.navigation/ViewRoute.md) |  |
| `OnPicked` | `Action<T>`&lt;`string`&gt; |  |

### `Equals(object)` {#equals-object}

```csharp
public override bool Equals(object? obj);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `obj` | `object` |  |

**Returns** `bool`

### `Equals(FilePickerRequest)` {#equals-filepickerrequest}

```csharp
public bool Equals(FilePickerRequest? other);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`FilePickerRequest`](../arlecchino.state/FilePickerRequest.md) |  |

**Returns** `bool`

### `GetHashCode()` {#gethashcode}

```csharp
public override int GetHashCode();
```

**Returns** `int`

### `ToString()` {#tostring}

```csharp
public override string ToString();
```

**Returns** `string`

## Operators in detail

### `operator Inequality(FilePickerRequest, FilePickerRequest)` {#operator-inequality-filepickerrequest-filepickerrequest}

```csharp
public static bool op_Inequality(FilePickerRequest left, FilePickerRequest right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`FilePickerRequest`](../arlecchino.state/FilePickerRequest.md) |  |
| `right` | [`FilePickerRequest`](../arlecchino.state/FilePickerRequest.md) |  |

**Returns** `bool`

### `operator Equality(FilePickerRequest, FilePickerRequest)` {#operator-equality-filepickerrequest-filepickerrequest}

```csharp
public static bool op_Equality(FilePickerRequest left, FilePickerRequest right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`FilePickerRequest`](../arlecchino.state/FilePickerRequest.md) |  |
| `right` | [`FilePickerRequest`](../arlecchino.state/FilePickerRequest.md) |  |

**Returns** `bool`

