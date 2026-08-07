---
title: "FilePickerStrings"
sidebar_label: "FilePickerStrings"
---

# FilePickerStrings class

**Namespace:** `Arlecchino.Hosting` &middot; **Assembly:** `Arlecchino`

Text of the file picker: labels, column headers, and the three formatters.

```csharp
public sealed class FilePickerStrings
```

## Constructors

| Member | Summary |
|---|---|
| [`FilePickerStrings()`](#filepickerstrings) |  |

## Properties

| Member | Summary |
|---|---|
| [`ColumnDateModified`](#columndatemodified) | Header of the modification date column. |
| [`ColumnKind`](#columnkind) | Header of the kind column. |
| [`ColumnName`](#columnname) | Header of the name column. |
| [`ColumnSize`](#columnsize) | Header of the size column. |
| [`DateModified`](#datemodified) | Formats a modification date. The default says `Today at 9:41` and `Yesterday at 9:41` before falling back to a full date. |
| [`Drives`](#drives) | The drive list, shown as a place and as the location above every drive. |
| [`Favorites`](#favorites) | Heading above the user folders in the sidebar. |
| [`FileMode`](#filemode) | Shown beside the title when the picker is choosing a file. |
| [`FolderMode`](#foldermode) | Shown beside the title when the picker is choosing a folder. |
| [`HintCancel`](#hintcancel) | Legend entry for leaving without picking anything. |
| [`HintFilter`](#hintfilter) | Legend entry for filtering by typing. |
| [`HintMove`](#hintmove) | Legend entry for moving through the list. |
| [`HintOpen`](#hintopen) | Legend entry for entering the folder under the cursor. |
| [`HintOpenFolder`](#hintopenfolder) | Legend entry for opening a folder while picking folders. |
| [`HintOpenFolderOrPickFile`](#hintopenfolderorpickfile) | Legend entry for the key that opens a folder or picks a file. |
| [`HintPickCurrentFolder`](#hintpickcurrentfolder) | Legend entry for picking the folder that is currently open. |
| [`HintPlaces`](#hintplaces) | Legend entry for switching to the places sidebar. |
| [`HintUp`](#hintup) | Legend entry for going to the parent folder. |
| [`ItemCount`](#itemcount) | How many entries the current folder shows, on the status row. |
| [`KindFolder`](#kindfolder) | Kind shown for a directory. |
| [`KindOf`](#kindof) | Turns a file extension into a readable kind. The default knows the common ones and falls back to `XYZ file`. |
| [`KindVolume`](#kindvolume) | Kind shown for a drive. |
| [`Locations`](#locations) | Heading above the home folder and the drives. |
| [`Search`](#search) | Label of the filter field in the toolbar. |
| [`Size`](#size) | Formats a file size. The default scales to KB, MB and up, and shows `--` for something with no size, such as a folder. |
| [`Title`](#title) | Title used when a request does not carry one of its own. |

## Constructors in detail

### `FilePickerStrings()` {#filepickerstrings}

```csharp
public FilePickerStrings();
```

## Properties in detail

### `ColumnDateModified` {#columndatemodified}

```csharp
public Func<string> ColumnDateModified { get; set; }
```

Header of the modification date column.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ColumnKind` {#columnkind}

```csharp
public Func<string> ColumnKind { get; set; }
```

Header of the kind column.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ColumnName` {#columnname}

```csharp
public Func<string> ColumnName { get; set; }
```

Header of the name column.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ColumnSize` {#columnsize}

```csharp
public Func<string> ColumnSize { get; set; }
```

Header of the size column.

**Type** `Func<TResult>`&lt;`string`&gt;

### `DateModified` {#datemodified}

```csharp
public Func<DateTime, string> DateModified { get; set; }
```

Formats a modification date. The default says `Today at 9:41` and `Yesterday at 9:41` before falling back to a full date.

**Type** `Func<T, TResult>`&lt;`DateTime`, `string`&gt;

### `Drives` {#drives}

```csharp
public Func<string> Drives { get; set; }
```

The drive list, shown as a place and as the location above every drive.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Favorites` {#favorites}

```csharp
public Func<string> Favorites { get; set; }
```

Heading above the user folders in the sidebar.

**Type** `Func<TResult>`&lt;`string`&gt;

### `FileMode` {#filemode}

```csharp
public Func<string> FileMode { get; set; }
```

Shown beside the title when the picker is choosing a file.

**Type** `Func<TResult>`&lt;`string`&gt;

### `FolderMode` {#foldermode}

```csharp
public Func<string> FolderMode { get; set; }
```

Shown beside the title when the picker is choosing a folder.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintCancel` {#hintcancel}

```csharp
public Func<string> HintCancel { get; set; }
```

Legend entry for leaving without picking anything.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintFilter` {#hintfilter}

```csharp
public Func<string> HintFilter { get; set; }
```

Legend entry for filtering by typing.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintMove` {#hintmove}

```csharp
public Func<string> HintMove { get; set; }
```

Legend entry for moving through the list.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintOpen` {#hintopen}

```csharp
public Func<string> HintOpen { get; set; }
```

Legend entry for entering the folder under the cursor.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintOpenFolder` {#hintopenfolder}

```csharp
public Func<string> HintOpenFolder { get; set; }
```

Legend entry for opening a folder while picking folders.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintOpenFolderOrPickFile` {#hintopenfolderorpickfile}

```csharp
public Func<string> HintOpenFolderOrPickFile { get; set; }
```

Legend entry for the key that opens a folder or picks a file.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintPickCurrentFolder` {#hintpickcurrentfolder}

```csharp
public Func<string> HintPickCurrentFolder { get; set; }
```

Legend entry for picking the folder that is currently open.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintPlaces` {#hintplaces}

```csharp
public Func<string> HintPlaces { get; set; }
```

Legend entry for switching to the places sidebar.

**Type** `Func<TResult>`&lt;`string`&gt;

### `HintUp` {#hintup}

```csharp
public Func<string> HintUp { get; set; }
```

Legend entry for going to the parent folder.

**Type** `Func<TResult>`&lt;`string`&gt;

### `ItemCount` {#itemcount}

```csharp
public Func<int, string> ItemCount { get; set; }
```

How many entries the current folder shows, on the status row.

**Type** `Func<T, TResult>`&lt;`int`, `string`&gt;

### `KindFolder` {#kindfolder}

```csharp
public Func<string> KindFolder { get; set; }
```

Kind shown for a directory.

**Type** `Func<TResult>`&lt;`string`&gt;

### `KindOf` {#kindof}

```csharp
public Func<string, string> KindOf { get; set; }
```

Turns a file extension into a readable kind. The default knows the common ones and falls back to `XYZ file`.

**Type** `Func<T, TResult>`&lt;`string`, `string`&gt;

### `KindVolume` {#kindvolume}

```csharp
public Func<string> KindVolume { get; set; }
```

Kind shown for a drive.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Locations` {#locations}

```csharp
public Func<string> Locations { get; set; }
```

Heading above the home folder and the drives.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Search` {#search}

```csharp
public Func<string> Search { get; set; }
```

Label of the filter field in the toolbar.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Size` {#size}

```csharp
public Func<long, string> Size { get; set; }
```

Formats a file size. The default scales to KB, MB and up, and shows `--` for something with no size, such as a folder.

**Type** `Func<T, TResult>`&lt;`long`, `string`&gt;

### `Title` {#title}

```csharp
public Func<string> Title { get; set; }
```

Title used when a request does not carry one of its own.

**Type** `Func<TResult>`&lt;`string`&gt;

