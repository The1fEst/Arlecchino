---
title: "CommandKeys"
sidebar_label: "CommandKeys"
---

# CommandKeys class

**Namespace:** `Arlecchino.Commands` &middot; **Assembly:** `Arlecchino`

The keys that reach commands, and the half-typed chord in between two of them.

```csharp
public sealed class CommandKeys
```

## Properties

| Member | Summary |
|---|---|
| [`IsWaiting`](#iswaiting) | Whether a chord has been started, so the next key belongs to it and nothing else. |

## Methods

| Member | Summary |
|---|---|
| [`Hints()`](#hints) | Every key that would finish the chord being typed, under the name of that key alone. This is what a leader is grouped for: the second key is looked up rather than remembered. |

## Properties in detail

### `IsWaiting` {#iswaiting}

```csharp
public bool IsWaiting { get; }
```

Whether a chord has been started, so the next key belongs to it and nothing else.

**Type** `bool`

## Methods in detail

### `Hints()` {#hints}

```csharp
public ValueTuple<string, string>[] Hints();
```

Every key that would finish the chord being typed, under the name of that key alone. This is what a leader is grouped for: the second key is looked up rather than remembered.

**Returns** `ValueTuple<T1, T2>`&lt;`string`, `string`&gt;\[\] — The keys behind the leader, or nothing when no chord is waiting.

