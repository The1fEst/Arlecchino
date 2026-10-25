---
title: "NotificationAction"
sidebar_label: "NotificationAction"
---

# NotificationAction class

**Namespace:** `Arlecchino.Diagnostics` &middot; **Assembly:** `Arlecchino`

Something the user can do about a notification, offered when the entry is opened: stop the copy that is running, retry what failed, go to what it is about.

```csharp
public sealed class NotificationAction : IEquatable<NotificationAction>
```

**Implements** `IEquatable<T>`&lt;[`NotificationAction`](../arlecchino.diagnostics/NotificationAction.md)&gt;

## Constructors

| Member | Summary |
|---|---|
| [`NotificationAction(Func<string>, Action)`](#notificationaction-func-string-action) | Something the user can do about a notification, offered when the entry is opened: stop the copy that is running, retry what failed, go to what it is about. |

## Properties

| Member | Summary |
|---|---|
| [`Label`](#label) | What the action is called; a delegate, so it can be translated. |
| [`Run`](#run) | What it does. The dialog closes first, so this may open one of its own. |

## Methods

| Member | Summary |
|---|---|
| [`Deconstruct(out Func<string>, out Action)`](#deconstruct-out-func-string-out-action) |  |

## Constructors in detail

### `NotificationAction(Func<string>, Action)` {#notificationaction-func-string-action}

```csharp
public NotificationAction(Func<string> Label, Action Run);
```

Something the user can do about a notification, offered when the entry is opened: stop the copy that is running, retry what failed, go to what it is about.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Label` | `Func<TResult>`&lt;`string`&gt; | What the action is called; a delegate, so it can be translated. |
| `Run` | `Action` | What it does. The dialog closes first, so this may open one of its own. |

## Properties in detail

### `Label` {#label}

```csharp
public Func<string> Label { get; init; }
```

What the action is called; a delegate, so it can be translated.

**Type** `Func<TResult>`&lt;`string`&gt;

### `Run` {#run}

```csharp
public Action Run { get; init; }
```

What it does. The dialog closes first, so this may open one of its own.

**Type** `Action`

## Methods in detail

### `Deconstruct(out Func<string>, out Action)` {#deconstruct-out-func-string-out-action}

```csharp
public void Deconstruct(out Func<string> Label, out Action Run);
```

**Parameters**

| Name | Type | Description |
|---|---|---|
| `Label` | `Func<TResult>`&lt;`string`&gt; |  |
| `Run` | `Action` |  |

