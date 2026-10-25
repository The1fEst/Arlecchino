---
title: "TestClock"
sidebar_label: "TestClock"
---

# TestClock class

**Namespace:** `Arlecchino.Testing` &middot; **Assembly:** `Arlecchino.Testing`

A clock a test moves by hand. Scheduled work runs when the clock passes its due time, so a second is moved rather than waited for.

```csharp
public sealed class TestClock : TimeProvider
```

**Inherits from** `TimeProvider`

## Constructors

| Member | Summary |
|---|---|
| [`TestClock()`](#testclock) |  |

## Methods

| Member | Summary |
|---|---|
| [`Advance(TimeSpan)`](#advance-timespan) | Moves the clock forward. |
| [`GetUtcNow()`](#getutcnow) | The time the clock reads now. |

## Constructors in detail

### `TestClock()` {#testclock}

```csharp
public TestClock();
```

## Methods in detail

### `Advance(TimeSpan)` {#advance-timespan}

```csharp
public void Advance(TimeSpan amount);
```

Moves the clock forward.

**Parameters**

| Name | Type | Description |
|---|---|---|
| `amount` | `TimeSpan` | How far ahead; a negative amount leaves the clock where it is. |

### `GetUtcNow()` {#getutcnow}

```csharp
public override DateTimeOffset GetUtcNow();
```

The time the clock reads now.

**Returns** `DateTimeOffset` — The current time.

