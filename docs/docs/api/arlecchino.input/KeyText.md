---
title: "KeyText"
sidebar_label: "KeyText"
---

# KeyText class

**Namespace:** `Arlecchino.Input` &middot; **Assembly:** `Arlecchino.Core`

Turns a key press into the character it types. Ask this instead of reading `KeyPress.Character` yourself — that is what keeps a field typing on a terminal that names the key and sends no character with it.

```csharp
public static class KeyText
```

## Methods

| Member | Summary |
|---|---|
| [`ByPosition(KeyPress)`](#byposition-keypress) | The character the key would type where it sits, whatever the layout makes of it. A key that sits nowhere on a US keyboard stands for nothing. |
| [`Resolve(KeyPress)`](#resolve-keypress) | The character a key press should type, or `null` for keys that type nothing — function keys, arrows, and unmapped combinations. Whatever the layout made of the key is taken as it is, so any language is typed without the application asking for it. |
| [`Shortcut(KeyPress)`](#shortcut-keypress) | The character a press stands for as a shortcut rather than as text: where its key sits when the application reads keys by position, and what the layout typed otherwise. |

## Methods in detail

### `ByPosition(KeyPress)` {#byposition-keypress}

```csharp
public static Nullable<char> ByPosition(KeyPress key);
```

The character the key would type where it sits, whatever the layout makes of it. A key that sits nowhere on a US keyboard stands for nothing.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** `Nullable<T>`&lt;`char`&gt; — The character its position stands for, or `null`.

### `Resolve(KeyPress)` {#resolve-keypress}

```csharp
public static Nullable<char> Resolve(KeyPress key);
```

The character a key press should type, or `null` for keys that type nothing — function keys, arrows, and unmapped combinations. Whatever the layout made of the key is taken as it is, so any language is typed without the application asking for it.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** `Nullable<T>`&lt;`char`&gt; — The character to insert, or `null`.

### `Shortcut(KeyPress)` {#shortcut-keypress}

```csharp
public static Nullable<char> Shortcut(KeyPress key);
```

The character a press stands for as a shortcut rather than as text: where its key sits when the application reads keys by position, and what the layout typed otherwise.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `key` | [`KeyPress`](../arlecchino.input/KeyPress.md) | The key that was pressed. |

**Returns** `Nullable<T>`&lt;`char`&gt; — The character the shortcut is looked up under, or `null`.

