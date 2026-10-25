---
title: "EntryRuns"
sidebar_label: "EntryRuns"
---

# EntryRuns class

**Namespace:** `Arlecchino.Widgets.Text` &middot; **Assembly:** `Arlecchino`

A line being typed into, cut into the runs it is drawn as: what is plain, what is selected, and the symbol the caret stands on. The caret is written the other way round rather than wedged beside it.

```csharp
public static class EntryRuns
```

## Methods

| Member | Summary |
|---|---|
| [`Of(string, int, ValueTuple<int, int>, EntryLook, Action<string, IArlecchinoColor>)`](#of-string-int-valuetuple-int-int-entrylook-action-string-iarlecchinocolor) | Hands every run of the line to whoever is drawing it, left to right. |

## Methods in detail

### `Of(string, int, ValueTuple<int, int>, EntryLook, Action<string, IArlecchinoColor>)` {#of-string-int-valuetuple-int-int-entrylook-action-string-iarlecchinocolor}

```csharp
public static void Of(
    string text,
    int caret,
    ValueTuple<int, int> selection,
    EntryLook look,
    Action<string, IArlecchinoColor> write);
```

Hands every run of the line to whoever is drawing it, left to right.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `text` | `string` | The line as it is written, which for a secret is the dots. |
| `caret` | `int` | Where the caret is in it, or `-1` for a line that is not being typed into. |
| `selection` | `ValueTuple<T1, T2>`&lt;`int`, `int`&gt; | Where the selection starts and ends in it. |
| `look` | [`EntryLook`](../arlecchino.widgets.text/EntryLook.md) | The colors to write it in. |
| `write` | `Action<T1, T2>`&lt;`string`, [`IArlecchinoColor`](../arlecchino.rendering.colors/IArlecchinoColor.md)&gt; | Takes one run: what it says, and how it is written. |

