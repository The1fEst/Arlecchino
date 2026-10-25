---
title: Migrating to 2026.9.1
sidebar_label: Migrating to 2026.9.1
description: The input mode that is gone, the static KeyText that replaces the resolved one, and the dialog key that went with them.
---

# Migrating to 2026.9.1

One break, and the compiler names most of it for you: the setting that chose between shortcuts and
typing is gone, because the release stopped making an application choose. An application that never
called `UseTextInput` has nothing to do.

| What changed | What to do |
|---|---|
| `TextInputMode`, `UseTextInput(mode)` | `UseKeysByPosition()`, or `ArlecchinoOptions.KeysByPosition = true` |
| `ArlecchinoOptions.TextInput` | `ArlecchinoOptions.KeysByPosition`, a `bool` |
| `KeyText.For(mode).Resolve(key)` | `KeyText.Resolve(key)` — a static class, nothing to build |
| `KeyText` as a constructor parameter | Take it away; nothing injects it any more |
| `ModalFrame.Keys` | Gone with it; a dialog that reads what was typed calls `KeyText.Resolve(key)` |

## The mode is a switch now

`TextInputMode.ByPosition` bought the shortcuts and sold the language. There is nothing to trade any
more, so what is left is whether the position is filled in at all:

```csharp
// before
builder.UseTextInput(TextInputMode.ByPosition);

// after
builder.UseKeysByPosition();
```

An application that was on `TextInputMode.Native` — the default — says nothing and keeps what it had.

## KeyText is static

It resolved characters for a configured mode, so it was built, registered and handed round. The mode is
gone, so it is three methods on a static class:

```csharp
// before
public MyView(KeyText keys, ArlecchinoOptions options)
{
    _keys = keys;
}

if (_keys.Resolve(key) is { } typed) { … }

// after
public MyView(ArlecchinoOptions options)
{
}

if (KeyText.Resolve(key) is { } typed) { … }
```

`Resolve` is what the press types, `ByPosition` is the character its key sits at, and `Shortcut` is the
one a command is looked up under — which is what a view matching a binding by character wants.

## What a press carries

`KeyPress.ByPosition` is new, and is filled in only where `KeysByPosition` asked for it. A view that
reads it directly should treat `'\0'` as "nothing was filled in" rather than as a character, which is
what `KeyText.Shortcut` does for you.
