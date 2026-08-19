---
title: "CompletionAsk"
sidebar_label: "CompletionAsk"
---

# CompletionAsk struct

**Namespace:** `Arlecchino.Editing` &middot; **Assembly:** `Arlecchino`

The half-typed word something is being asked to finish, and the line it stands in. The line goes with it because what a word could turn into depends on what stands in front of it.

```csharp
public readonly struct CompletionAsk : IEquatable<CompletionAsk>
```

**Implements** `IEquatable<T>`&lt;[`CompletionAsk`](../arlecchino.editing/CompletionAsk.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`CompletionAsk(string, int, int)`](#completionask-string-int-int) | The half-typed word something is being asked to finish, and the line it stands in. The line goes with it because what a word could turn into depends on what stands in front of it. |

## Properties

| Member | Summary |
|---|---|
| [`Length`](#length) | How long the word is. It ends where the caret is. |
| [`Line`](#line) | The line as it stands. |
| [`Prefix`](#prefix) | Whatever stands in front of the word. |
| [`Start`](#start) | Where the word begins in it. |
| [`Suffix`](#suffix) | Whatever follows the caret, which finishing the word leaves where it is. |
| [`Word`](#word) | The word itself, which is empty where the caret stands after a space. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out string, out int, out int)`](#deconstruct-out-string-out-int-out-int) |  |

## Constructors in detail

### `CompletionAsk(string, int, int)` {#completionask-string-int-int}

```csharp
public CompletionAsk(string Line, int Start, int Length);
```

The half-typed word something is being asked to finish, and the line it stands in. The line goes with it because what a word could turn into depends on what stands in front of it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Line` | `string` | The line as it stands. |
| `Start` | `int` | Where the word begins in it. |
| `Length` | `int` | How long the word is. It ends where the caret is. |

## Properties in detail

### `Length` {#length}

```csharp
public int Length { get; init; }
```

How long the word is. It ends where the caret is.

**Type** `int`

### `Line` {#line}

```csharp
public string Line { get; init; }
```

The line as it stands.

**Type** `string`

### `Prefix` {#prefix}

```csharp
public string Prefix { get; }
```

Whatever stands in front of the word.

**Type** `string`

### `Start` {#start}

```csharp
public int Start { get; init; }
```

Where the word begins in it.

**Type** `int`

### `Suffix` {#suffix}

```csharp
public string Suffix { get; }
```

Whatever follows the caret, which finishing the word leaves where it is.

**Type** `string`

### `Word` {#word}

```csharp
public string Word { get; }
```

The word itself, which is empty where the caret stands after a space.

**Type** `string`

## Methods in detail

### `Deconstruct(out string, out int, out int)` {#deconstruct-out-string-out-int-out-int}

```csharp
public void Deconstruct(out string Line, out int Start, out int Length);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Line` | `string` |  |
| `Start` | `int` |  |
| `Length` | `int` |  |

