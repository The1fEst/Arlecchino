---
title: "FilePickerPlace"
sidebar_label: "FilePickerPlace"
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
| [`Deconstruct(out string, out string, out string)`](#deconstruct-out-string-out-string-out-string) |  |

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

### `Deconstruct(out string, out string, out string)` {#deconstruct-out-string-out-string-out-string}

```csharp
public void Deconstruct(out string Name, out string Path, out string Icon);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Name` | `string` |  |
| `Path` | `string` |  |
| `Icon` | `string` |  |

