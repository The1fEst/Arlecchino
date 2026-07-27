---
title: KeyText
sidebar_label: KeyText
---

# KeyText class

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

Turns a key press into the character it should type. Take it as a constructor parameter instead of reading `ConsoleKeyInfo.KeyChar` yourself — that is what keeps filters and shortcuts working on a non-latin layout.

```csharp
public sealed class KeyText
```

## Constructors

| Member | Summary |
|---|---|
| [`KeyText(TextInputMode)`](#keytext-textinputmode) | Creates a resolver for one mode. |

## Properties

| Member | Summary |
|---|---|
| [`LatinOnly`](#latinonly) | Shared resolver for [`TextInputMode.LatinOnly`](../arlecchino.input/TextInputMode.md). |
| [`Mode`](#mode) | The mode this resolver works in. |
| [`Native`](#native) | Shared resolver for [`TextInputMode.Native`](../arlecchino.input/TextInputMode.md). |

## Methods

| Member | Summary |
|---|---|
| [`For(TextInputMode)`](#for-textinputmode) | Returns the shared resolver for a mode. |
| [`Resolve(ConsoleKeyInfo)`](#resolve-consolekeyinfo) | The character a key press should type, or `null` for keys that type nothing — function keys, arrows, and unmapped combinations. |

## Constructors in detail

### `KeyText(TextInputMode)` {#keytext-textinputmode}

```csharp
public KeyText(TextInputMode mode);
```

Creates a resolver for one mode.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mode` | [`TextInputMode`](../arlecchino.input/TextInputMode.md) | How characters should be resolved. |

## Properties in detail

### `LatinOnly` {#latinonly}

```csharp
public static KeyText LatinOnly { get; }
```

Shared resolver for [`TextInputMode.LatinOnly`](../arlecchino.input/TextInputMode.md).

**Type** [`KeyText`](../arlecchino.input/KeyText.md)

### `Mode` {#mode}

```csharp
public TextInputMode Mode { get; }
```

The mode this resolver works in.

**Type** [`TextInputMode`](../arlecchino.input/TextInputMode.md)

### `Native` {#native}

```csharp
public static KeyText Native { get; }
```

Shared resolver for [`TextInputMode.Native`](../arlecchino.input/TextInputMode.md).

**Type** [`KeyText`](../arlecchino.input/KeyText.md)

## Methods in detail

### `For(TextInputMode)` {#for-textinputmode}

```csharp
public static KeyText For(TextInputMode mode);
```

Returns the shared resolver for a mode.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `mode` | [`TextInputMode`](../arlecchino.input/TextInputMode.md) | The mode wanted. |

**Returns** [`KeyText`](../arlecchino.input/KeyText.md) — The matching resolver.

### `Resolve(ConsoleKeyInfo)` {#resolve-consolekeyinfo}

```csharp
public Nullable<char> Resolve(ConsoleKeyInfo key);
```

The character a key press should type, or `null` for keys that type nothing — function keys, arrows, and unmapped combinations.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | `ConsoleKeyInfo` | The key that was pressed. |

**Returns** `Nullable<T>`&lt;`char`&gt; — The character to insert, or `null`.

