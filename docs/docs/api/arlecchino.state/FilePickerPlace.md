---
title: FilePickerPlace
sidebar_label: FilePickerPlace
---

# FilePickerPlace class

**Namespace:** `Arlecchino.State` &middot; **Assembly:** `Arlecchino`

A shortcut in the file picker's sidebar, for somewhere the user goes often.

```csharp
public sealed class FilePickerPlace : IEquatable<FilePickerPlace>
```

**Implements** `IEquatable<T>`&lt;[`FilePickerPlace`](../arlecchino.state/FilePickerPlace.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`FilePickerPlace(string, string, string)`](#filepickerplace-string-string-string) | A shortcut in the file picker's sidebar, for somewhere the user goes often. |

## Properties

| Member | Summary |
|---|---|
| [`Icon`](#icon) | An optional glyph drawn before the name. |
| [`Name`](#name) | What the shortcut is called. |
| [`Path`](#path) | Where it leads. |

## Methods

| Member | Summary |
|---|---|
| [`<Clone>$()`](#clone) |  |
| [`Deconstruct(String&, String&, String&)`](#deconstruct-string-string-string) |  |
| [`Equals(object)`](#equals-object) |  |
| [`Equals(FilePickerPlace)`](#equals-filepickerplace) |  |
| [`GetHashCode()`](#gethashcode) |  |
| [`ToString()`](#tostring) |  |

## Operators

| Member | Summary |
|---|---|
| [`operator Inequality(FilePickerPlace, FilePickerPlace)`](#operator-inequality-filepickerplace-filepickerplace) |  |
| [`operator Equality(FilePickerPlace, FilePickerPlace)`](#operator-equality-filepickerplace-filepickerplace) |  |

## Constructors in detail

### `FilePickerPlace(string, string, string)` {#filepickerplace-string-string-string}

```csharp
public FilePickerPlace(string Name, string Path, string Icon = "");
```

A shortcut in the file picker's sidebar, for somewhere the user goes often.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Name` | `string` | What the shortcut is called. |
| `Path` | `string` | Where it leads. |
| `Icon` | `string` | An optional glyph drawn before the name. |

## Properties in detail

### `Icon` {#icon}

```csharp
public string Icon { get; init; }
```

An optional glyph drawn before the name.

**Type** `string`

### `Name` {#name}

```csharp
public string Name { get; init; }
```

What the shortcut is called.

**Type** `string`

### `Path` {#path}

```csharp
public string Path { get; init; }
```

Where it leads.

**Type** `string`

## Methods in detail

### `<Clone>$()` {#clone}

```csharp
public FilePickerPlace <Clone>$();
```

**Returns** [`FilePickerPlace`](../arlecchino.state/FilePickerPlace.md)

### `Deconstruct(String&, String&, String&)` {#deconstruct-string-string-string}

```csharp
public void Deconstruct(out string Name, out string Path, out string Icon);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Name` | `string` |  |
| `Path` | `string` |  |
| `Icon` | `string` |  |

### `Equals(object)` {#equals-object}

```csharp
public override bool Equals(object? obj);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `obj` | `object` |  |

**Returns** `bool`

### `Equals(FilePickerPlace)` {#equals-filepickerplace}

```csharp
public bool Equals(FilePickerPlace? other);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `other` | [`FilePickerPlace`](../arlecchino.state/FilePickerPlace.md) |  |

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

### `operator Inequality(FilePickerPlace, FilePickerPlace)` {#operator-inequality-filepickerplace-filepickerplace}

```csharp
public static bool op_Inequality(FilePickerPlace left, FilePickerPlace right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`FilePickerPlace`](../arlecchino.state/FilePickerPlace.md) |  |
| `right` | [`FilePickerPlace`](../arlecchino.state/FilePickerPlace.md) |  |

**Returns** `bool`

### `operator Equality(FilePickerPlace, FilePickerPlace)` {#operator-equality-filepickerplace-filepickerplace}

```csharp
public static bool op_Equality(FilePickerPlace left, FilePickerPlace right);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `left` | [`FilePickerPlace`](../arlecchino.state/FilePickerPlace.md) |  |
| `right` | [`FilePickerPlace`](../arlecchino.state/FilePickerPlace.md) |  |

**Returns** `bool`

